using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics; // For float4

#if UNITY_EDITOR // Import necessary for Handles
using UnityEditor;
#endif

public class SlimeInstance : MonoBehaviour
{
    [Header("References")]
    public ComputeShader slimeComputeShader;
    public SlimeRenderer slimeRenderer; // Assign in Inspector

    [Header("Simulation Params")]
    public int nodeCount = 128;         // Number of nodes
    public float stiffness = 10.0f;       // Spring force towards target position
    public float damping = 0.9f;        // Velocity damping factor
    public float neighborStiffness = 5.0f; // Spring force between adjacent nodes
    public float centerRadius = 1.0f;     // Ideal radius of the node circle
    [Tooltip("The maximum distance a node can stretch radially outward from the core beyond centerRadius.")]
    public float maxNodeDistanceExtension = 0.5f; // Max radial stretch
    [Range(0f, 180f)]
    public float minNodeAngleDeg = 5.0f; // Min angle between adjacent nodes (degrees)
    [Range(0f, 180f)]
    public float maxNodeAngleDeg = 45.0f; // Max angle between adjacent nodes (degrees)
    public float angleConstraintStiffness = 1.0f; // Stiffness for angle constraints
    [Tooltip("Y-coordinate of the ground plane used in the Compute Shader simulation.")]
    public float groundY = -2.0f; // Ground level for compute shader collision
    [Tooltip("Bounciness for ground collision in Compute Shader (0=stick, 1=perfect bounce). Currently set for sticking.")]
    [Range(0f, 1f)]
    public float groundRestitution = 0.0f; // Bounciness (set near 0 for sticking)

    // --- Core Data ---
    private ComputeBuffer coreBuffer;       // Buffer holding core position, rotation, mana
    private Vector2 corePosition;         // Cached core position for gizmos/updates
    private float coreRotation;           // Cached core rotation
    private float coreMana;               // Cached core mana

    // --- Node Buffers (Double Buffering) ---
    private ComputeBuffer nodeBufferA;      // One of the two buffers for nodes
    private ComputeBuffer nodeBufferB;      // The other node buffer
    private bool isA_CurrentReadBuffer = true; // Tracks which buffer holds the *latest* data for reading

    // --- Internals ---
    private int kernel;                   // Handle for the CSMain kernel
    private int nodeStride;               // Size of the SlimeNode struct in bytes

    // --- Gizmo Settings ---
    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public float nodeGizmoRadius = 0.05f;
    public float coreGizmoRadius = 0.08f;
    public float maxTensionLineLength = 0.5f; // Visual length of the tension line gizmo
    [Tooltip("Select the layer(s) considered as ground for gizmo visualization.")]
    public LayerMask groundLayer;         // Layer mask for Gizmo ground check
    public Color constraintViolationColor = Color.magenta;
    public Color defaultTensionLineColor = new Color(0.6f, 0.6f, 1.0f); // Default gizmo line color

    private Color groundedTensionLineColor; // Complementary color for grounded gizmo line
    private SlimeNode[] gizmoNodeDataCache; // Cache for reading node data for Gizmos

    // Define SlimeNode struct matching HLSL (Ensure LayoutKind.Sequential for safety)
    [StructLayout(LayoutKind.Sequential)]
    public struct SlimeNode
    {
        public Vector2 position;
        public Vector2 velocity;
        public float mass;
        // Add padding here if needed based on HLSL struct size/alignment
    }

    void Start()
    {
        if (slimeRenderer == null)
        {
            Debug.LogError("SlimeRenderer not assigned to SlimeInstance!", this);
            this.enabled = false;
            return;
        }
        if (slimeComputeShader == null)
        {
            Debug.LogError("SlimeComputeShader not assigned to SlimeInstance!", this);
            this.enabled = false;
            return;
        }

        kernel = slimeComputeShader.FindKernel("CSMain");

        // Calculate stride (ensure SlimeNode struct definition is correct)
        nodeStride = Marshal.SizeOf(typeof(SlimeNode));
        if (nodeStride == 0)
        {
            Debug.LogError("Failed to calculate SlimeNode stride. Struct might be empty or invalid.", this);
            this.enabled = false;
            return;
        }

        // Calculate complementary color for gizmos
        groundedTensionLineColor = new Color(
            1.0f - defaultTensionLineColor.r,
            1.0f - defaultTensionLineColor.g,
            1.0f - defaultTensionLineColor.b,
            defaultTensionLineColor.a
        );

        InitializeCoreBuffer();
        InitializeNodeBuffers();
    }

    void InitializeCoreBuffer()
    {
        // Buffer for 1 float4 element
        coreBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Vector4)), ComputeBufferType.Structured);
        // Sync initial core data (e.g., starting position)
        SyncCoreData(transform.position, transform.eulerAngles.z * Mathf.Deg2Rad, 100f); // Use initial transform
    }

    void InitializeNodeBuffers()
    {
        nodeBufferA = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);
        nodeBufferB = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);

        // Initialize starting node data (e.g., in a circle)
        SlimeNode[] initialNodes = new SlimeNode[nodeCount];
        float angleStep = Mathf.PI * 2f / nodeCount;
        Vector2 initialCorePos = transform.position; // Start nodes around initial position

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * angleStep;
            initialNodes[i] = new SlimeNode
            {
                position = initialCorePos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * centerRadius,
                velocity = Vector2.zero,
                mass = 1f
            };
        }

        // Put initial data into Buffer A, mark it as the first Read buffer
        nodeBufferA.SetData(initialNodes);
        isA_CurrentReadBuffer = true;

        // Pass the initial buffer to the renderer
        slimeRenderer.SetNodeBuffer(nodeBufferA, nodeCount);
    }

    // Called by SlimeCore (e.g., in FixedUpdate) to update core's state
    public void SyncCoreData(Vector2 position, float rotationRadians, float mana)
    {
        // Cache values for local use (like Gizmos)
        corePosition = position;
        coreRotation = rotationRadians;
        coreMana = mana;

        float4 coreData = new float4(corePosition.x, corePosition.y, coreRotation, coreMana);

        // Set data to the GPU buffer if it's valid
        if (coreBuffer != null && coreBuffer.IsValid())
        {
            coreBuffer.SetData(new float4[] { coreData });
        }
        else
        {
            Debug.LogWarning("CoreBuffer is null or invalid during SyncCoreData.", this);
        }
    }

    // Use FixedUpdate for physics simulation consistency
    void FixedUpdate()
    {
        // Ensure buffers are valid before proceeding
        if (nodeBufferA == null || nodeBufferB == null || !nodeBufferA.IsValid() || !nodeBufferB.IsValid() ||
            coreBuffer == null || !coreBuffer.IsValid() || slimeComputeShader == null || kernel < 0)
        {
            return; // Stop if simulation prerequisites are not met
        }

        // 1. Determine Read/Write buffers for THIS dispatch
        ComputeBuffer readBuffer = isA_CurrentReadBuffer ? nodeBufferA : nodeBufferB;
        ComputeBuffer writeBuffer = isA_CurrentReadBuffer ? nodeBufferB : nodeBufferA;

        // 2. Set Shader Uniforms (Parameters)
        slimeComputeShader.SetInt("nodeCount", nodeCount);
        slimeComputeShader.SetFloat("deltaTime", Time.fixedDeltaTime); // Use fixedDeltaTime for physics loop
        slimeComputeShader.SetFloat("stiffness", stiffness);
        slimeComputeShader.SetFloat("damping", damping);
        slimeComputeShader.SetFloat("neighborStiffness", neighborStiffness);
        slimeComputeShader.SetFloat("centerRadius", centerRadius);
        float actualMaxRadialDistance = centerRadius + maxNodeDistanceExtension;
        slimeComputeShader.SetFloat("maxRadialDistance", actualMaxRadialDistance);
        slimeComputeShader.SetFloat("minAngleRad", minNodeAngleDeg * Mathf.Deg2Rad); // Convert to Radians
        slimeComputeShader.SetFloat("maxAngleRad", maxNodeAngleDeg * Mathf.Deg2Rad); // Convert to Radians
        slimeComputeShader.SetFloat("angleConstraintStiffness", angleConstraintStiffness);
        slimeComputeShader.SetFloat("groundY", groundY);
        slimeComputeShader.SetFloat("groundRestitution", groundRestitution);

        // 3. Set Buffers for the Kernel
        // Bind the core data buffer
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);
        // Bind the node buffers for double buffering read/write
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferRead", readBuffer);
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferWrite", writeBuffer);

        // 4. Dispatch the Compute Shader
        int threadGroups = Mathf.CeilToInt((float)nodeCount / 64.0f); // Match [numthreads(64, ...)] in HLSL
        if (threadGroups > 0) // Only dispatch if there are nodes
        {
            slimeComputeShader.Dispatch(kernel, threadGroups, 1, 1);
        }

        // 5. SWAP the roles for the NEXT frame
        // The buffer we just WROTE TO (writeBuffer) now holds the latest data
        // and must become the READ buffer for the next FixedUpdate.
        isA_CurrentReadBuffer = !isA_CurrentReadBuffer;

        // 6. Provide the LATEST data buffer to the renderer
        // Pass the buffer that was just written to.
        if (slimeRenderer != null)
        {
            slimeRenderer.SetNodeBuffer(writeBuffer, nodeCount);
        }
    }

    // Helper to get the buffer that currently holds the LATEST simulation data
    private ComputeBuffer GetLatestDataBuffer()
    {
        if (!Application.isPlaying || nodeBufferA == null || nodeBufferB == null) return null;

        // If A is marked as the next read buffer, B holds the latest written data, and vice versa.
        return isA_CurrentReadBuffer ? nodeBufferB : nodeBufferA;
    }

    void OnDestroy()
    {
        // IMPORTANT: Release buffers when the object is destroyed to prevent memory leaks
        nodeBufferA?.Release();
        nodeBufferB?.Release();
        coreBuffer?.Release();
        nodeBufferA = null;
        nodeBufferB = null;
        coreBuffer = null;
    }

    // --- GIZMO DRAWING ---
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw core position even if not playing
        Vector3 corePos3D = new Vector3(corePosition.x, corePosition.y, transform.position.z);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(corePos3D, coreGizmoRadius);

        // Gizmos requiring node data only run in play mode when buffers are valid
        if (!Application.isPlaying) return;

        ComputeBuffer latestBuffer = GetLatestDataBuffer();
        if (latestBuffer == null || !latestBuffer.IsValid() || nodeCount <= 0)
        {
            // Indicate buffer issue visually
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(corePos3D, centerRadius * 1.1f);
            return;
        }

        // Ensure cache array exists and is the correct size
        if (gizmoNodeDataCache == null || gizmoNodeDataCache.Length != nodeCount)
        {
            gizmoNodeDataCache = new SlimeNode[nodeCount];
        }

        // --- Read Node Data from GPU (Performance cost - use for debug) ---
        latestBuffer.GetData(gizmoNodeDataCache);
        // ---

        // Draw gizmos for each node
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 nodePos3D = new Vector3(gizmoNodeDataCache[i].position.x, gizmoNodeDataCache[i].position.y, corePos3D.z);

            // 1. Draw Node (Vertex)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nodePos3D, nodeGizmoRadius);

            // 2. Draw Radial Connection (Core to Node)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(corePos3D, nodePos3D);

            // --- 3. Draw Max Tension / Ground Line (Uses Physics2D.Linecast) ---
            Vector3 radialDir = (nodePos3D - corePos3D).normalized;
            if (radialDir.sqrMagnitude < 0.0001f) radialDir = transform.up; // Fallback if node is at core
            Vector3 tensionLineEndPoint = nodePos3D + radialDir * maxTensionLineLength; // Use visual length

            // Check for ground contact using Physics2D.Linecast against the selected layer mask
            RaycastHit2D hit = Physics2D.Linecast(nodePos3D, tensionLineEndPoint, groundLayer);
            bool isGroundedVisually = hit.collider != null; // True if the line hit a collider on the groundLayer

            // Set color based on visual ground contact status
            Gizmos.color = isGroundedVisually ? groundedTensionLineColor : defaultTensionLineColor;

            // Draw the line itself and a small sphere at the end
            Gizmos.DrawLine(nodePos3D, tensionLineEndPoint);
            Gizmos.DrawSphere(tensionLineEndPoint, nodeGizmoRadius * 0.5f);

            // 4. Draw Circumferential Connection (Node to Next Node)
            int nextIndex = (i + 1) % nodeCount; // Wrap around using modulo
            Vector3 nextNodePos3D = new Vector3(gizmoNodeDataCache[nextIndex].position.x, gizmoNodeDataCache[nextIndex].position.y, corePos3D.z);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(nodePos3D, nextNodePos3D);

            // --- 5. Draw Angle Constraints / Violation Visualization ---
            if (nodeCount >= 2) // Need at least two nodes for angles
            {
                int prevIndex = (i + nodeCount - 1) % nodeCount; // Wrap around for previous index
                Vector3 prevNodePos3D = new Vector3(gizmoNodeDataCache[prevIndex].position.x, gizmoNodeDataCache[prevIndex].position.y, corePos3D.z);

                Vector3 vecToPrev = prevNodePos3D - corePos3D;
                Vector3 vecToCurrent = nodePos3D - corePos3D;

                // Check if vectors are valid before calculating angle
                if (vecToPrev.sqrMagnitude > 0.0001f && vecToCurrent.sqrMagnitude > 0.0001f)
                {
                    float angle = Vector3.Angle(vecToPrev, vecToCurrent); // Angle in degrees

                    // Check if the angle violates the constraints defined in the Inspector (in degrees)
                    bool violatesConstraints = angle < minNodeAngleDeg || angle > maxNodeAngleDeg;

                    // Draw violation indicators if constraints are violated
                    if (violatesConstraints)
                    {
                        Handles.color = constraintViolationColor; // Use Handles for arcs
                        float violationRadius = centerRadius * 0.9f; // Draw arc slightly inside the main radius
                        // Draw an arc between the previous and current node vectors
                        Handles.DrawWireArc(corePos3D, Vector3.forward, vecToPrev, angle, violationRadius);

                        // Also highlight the node itself
                        Gizmos.color = constraintViolationColor;
                        Gizmos.DrawWireSphere(nodePos3D, nodeGizmoRadius * 1.5f);
                    }
                }
            }
        }
    }
#endif // UNITY_EDITOR
}