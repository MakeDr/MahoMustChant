using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SlimeInstance : MonoBehaviour
{
    [Header("References")]
    public ComputeShader slimeComputeShader; // Assign compute shader asset
    public SlimeRenderer slimeRenderer;     // Assign renderer component

    [Header("Simulation Parameters")]
    [Range(8, 256)] public int nodeCount = 16;
    public float centerRadius = 0.8f;
    // --- Phase 3 Parameters ---
    [Range(0.1f, 50.0f)] public float stiffness = 15.0f; // Spring stiffness towards target
    [Range(0.8f, 1.0f)] public float damping = 0.95f;   // Velocity damping (closer to 1 = less damping)
    // --- Parameters for Later Phases (Declare them now if you like) ---
    public float neighborStiffness = 10.0f;
    public float maxNodeDistanceExtension = 0.4f;
    [Range(0f, 90f)] public float minNodeAngleDeg = 10.0f;
    [Range(0f, 180f)] public float maxNodeAngleDeg = 50.0f;
    public float angleConstraintStiffness = 2.0f;
    public float collisionPushForce = 0.5f;
    public float coreColliderRadius = 0.5f;

    // --- Core Data (Cached) ---
    private ComputeBuffer coreBuffer;
    private Vector2 corePosition;
    private float coreRotation;
    private float coreMana;
    private uint coreCollisionFlags;
    private Vector2 coreRelevantNormal;

    // --- Node Buffers (Double Buffering) ---
    private ComputeBuffer nodeBufferA;
    private ComputeBuffer nodeBufferB;
    private bool isA_CurrentReadBuffer = true;

    // --- Internals ---
    private int kernel;
    private int nodeStride;
    private int coreStride;

    // --- Gizmo Settings & Cache ---
    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public float nodeGizmoRadius = 0.05f;
    public float coreGizmoRadius = 0.08f;
    public float maxTensionLineLength = 0.4f;
    public LayerMask groundLayer;
    public Color constraintViolationColor = Color.magenta;
    public Color defaultTensionLineColor = new Color(0.6f, 0.6f, 1.0f);
    public Color coreCollisionGizmoColor = Color.cyan;
    private Color groundedTensionLineColor;
    private SlimeNode[] gizmoNodeDataCache;


    void Start()
    {
        if (slimeComputeShader == null) { Debug.LogError("Compute Shader not assigned!", this); enabled = false; return; }
        if (slimeRenderer == null) { Debug.LogError("Slime Renderer not assigned!", this); enabled = false; return; }

        nodeStride = Marshal.SizeOf(typeof(SlimeNode));
        coreStride = Marshal.SizeOf(typeof(CoreBufferData));

        if (nodeStride == 0 || (nodeStride % 16 != 0)) { Debug.LogError($"Invalid SlimeNode stride: {nodeStride}", this); enabled = false; return; }
        if (coreStride == 0 || (coreStride % 16 != 0)) { Debug.LogError($"Invalid CoreBufferData stride: {coreStride}", this); enabled = false; return; }

        kernel = slimeComputeShader.FindKernel("CSMain");
        if (kernel < 0) { Debug.LogError("CSMain kernel not found!", this); enabled = false; return; }

        groundedTensionLineColor = new Color(1f - defaultTensionLineColor.r, 1f - defaultTensionLineColor.g, 1f - defaultTensionLineColor.b, defaultTensionLineColor.a);
        InitBuffers();

        if (slimeRenderer != null && nodeBufferA != null && nodeBufferA.IsValid())
        {
            slimeRenderer.SetNodeBuffer(nodeBufferA, nodeCount);
        }
    }

    void InitBuffers()
    {
        nodeBufferA?.Release();
        nodeBufferB?.Release();
        coreBuffer?.Release();

        try
        {
            // Initialize buffers based on current nodeCount
            // Consider using a fixed max size later if nodeCount changes dynamically
            nodeBufferA = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);
            nodeBufferB = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);
            coreBuffer = new ComputeBuffer(1, coreStride, ComputeBufferType.Structured);

            SlimeNode[] initialNodes = new SlimeNode[nodeCount];
            float angleStep = (nodeCount > 0) ? (Mathf.PI * 2f / nodeCount) : 0;
            Vector2 initialCorePos = transform.position;
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
            nodeBufferA.SetData(initialNodes);

            CoreBufferData initialCoreData = new CoreBufferData
            {
                posRotFlags = new Vector4(initialCorePos.x, initialCorePos.y, 0, math.asfloat(0u)),
                normalMana = new Vector4(0, 0, 100f, 0)
            };
            coreBuffer.SetData(new CoreBufferData[] { initialCoreData });

            isA_CurrentReadBuffer = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating compute buffers: {e.Message}", this);
            enabled = false;
        }
    }

    public void SyncCoreData(Vector2 position, float rotationRadians, float mana, uint collisionFlags, Vector2 relevantNormal)
    {
        corePosition = position;
        coreRotation = rotationRadians;
        coreMana = mana;
        coreCollisionFlags = collisionFlags;
        coreRelevantNormal = relevantNormal;

        CoreBufferData dataToSend;
        dataToSend.posRotFlags = new Vector4(corePosition.x, corePosition.y, coreRotation, math.asfloat(coreCollisionFlags));
        dataToSend.normalMana = new Vector4(coreRelevantNormal.x, coreRelevantNormal.y, coreMana, 0);

        if (coreBuffer != null && coreBuffer.IsValid())
        {
            coreBuffer.SetData(new CoreBufferData[] { dataToSend });
        }
    }

    void FixedUpdate()
    {
        if (nodeBufferA == null || !nodeBufferA.IsValid() || nodeBufferB == null || !nodeBufferB.IsValid() ||
            coreBuffer == null || !coreBuffer.IsValid() || slimeComputeShader == null || kernel < 0)
        {
            return;
        }

        ComputeBuffer readBuffer = isA_CurrentReadBuffer ? nodeBufferA : nodeBufferB;
        ComputeBuffer writeBuffer = isA_CurrentReadBuffer ? nodeBufferB : nodeBufferA;

        // --- Set Uniforms for Phase 3 ---
        slimeComputeShader.SetInt("nodeCount", nodeCount);
        slimeComputeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        slimeComputeShader.SetFloat("stiffness", stiffness); // <<< Set Phase 3 uniform
        slimeComputeShader.SetFloat("damping", damping);     // <<< Set Phase 3 uniform
        slimeComputeShader.SetFloat("centerRadius", centerRadius);
        // --- Set other uniforms for later phases when needed ---
        // slimeComputeShader.SetFloat("neighborStiffness", neighborStiffness);
        // float actualMaxRadialDistance = centerRadius + maxNodeDistanceExtension;
        // slimeComputeShader.SetFloat("maxRadialDistance", actualMaxRadialDistance);
        // slimeComputeShader.SetFloat("minAngleRad", minNodeAngleDeg * Mathf.Deg2Rad);
        // slimeComputeShader.SetFloat("maxAngleRad", maxNodeAngleDeg * Mathf.Deg2Rad);
        // slimeComputeShader.SetFloat("angleConstraintStiffness", angleConstraintStiffness);
        // slimeComputeShader.SetFloat("collisionPushForce", collisionPushForce);
        // slimeComputeShader.SetFloat("coreColliderRadius", coreColliderRadius);

        // --- Set Buffers ---
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferRead", readBuffer);
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferWrite", writeBuffer);

        // --- Dispatch ---
        int threadGroups = Mathf.CeilToInt((float)nodeCount / 64.0f);
        if (threadGroups > 0 && nodeCount > 0)
        {
            try { slimeComputeShader.Dispatch(kernel, threadGroups, 1, 1); }
            catch (System.Exception e) { Debug.LogError($"Error dispatching shader: {e.Message}", this); return; }
        }

        // --- Swap Buffers ---
        isA_CurrentReadBuffer = !isA_CurrentReadBuffer;

        // --- Update Renderer ---
        if (slimeRenderer != null)
        {
            slimeRenderer.SetNodeBuffer(writeBuffer, nodeCount);
        }
    }

    private ComputeBuffer GetLatestDataBuffer()
    {
        if (!Application.isPlaying || nodeBufferA == null || nodeBufferB == null) return null;
        return isA_CurrentReadBuffer ? nodeBufferB : nodeBufferA;
    }

    void OnDestroy()
    {
        nodeBufferA?.Release();
        nodeBufferB?.Release();
        coreBuffer?.Release();
        nodeBufferA = null; nodeBufferB = null; coreBuffer = null;
    }

    // --- GIZMO DRAWING --- (Full implementation from previous answer)
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Vector3 corePos3D = new Vector3(corePosition.x, corePosition.y, transform.position.z);
        Gizmos.color = Color.red; Gizmos.DrawSphere(corePos3D, coreGizmoRadius);
        if (Application.isPlaying && coreRelevantNormal.sqrMagnitude > 0.001f) { Gizmos.color = coreCollisionGizmoColor; Gizmos.DrawLine(corePos3D, corePos3D + (Vector3)coreRelevantNormal * 0.5f); }
        if (!Application.isPlaying) return;
        ComputeBuffer latestBuffer = GetLatestDataBuffer();
        if (latestBuffer == null || !latestBuffer.IsValid() || nodeCount <= 0) return;
        if (gizmoNodeDataCache == null || gizmoNodeDataCache.Length != nodeCount) { gizmoNodeDataCache = new SlimeNode[nodeCount]; }
        try { latestBuffer.GetData(gizmoNodeDataCache); } catch { return; }
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 nodePos3D = new Vector3(gizmoNodeDataCache[i].position.x, gizmoNodeDataCache[i].position.y, corePos3D.z);
            Gizmos.color = Color.green; Gizmos.DrawSphere(nodePos3D, nodeGizmoRadius);
            Gizmos.color = Color.blue; Gizmos.DrawLine(corePos3D, nodePos3D);
            Vector3 radialDir = (nodePos3D - corePos3D).normalized; if (radialDir.sqrMagnitude < 0.0001f) radialDir = transform.up; Vector3 tensionLineEndPoint = nodePos3D + radialDir * maxTensionLineLength;
            RaycastHit2D hit = Physics2D.Linecast(nodePos3D, tensionLineEndPoint, groundLayer); bool isGroundedVisually = hit.collider != null;
            Gizmos.color = isGroundedVisually ? groundedTensionLineColor : defaultTensionLineColor; Gizmos.DrawLine(nodePos3D, tensionLineEndPoint); Gizmos.DrawSphere(tensionLineEndPoint, nodeGizmoRadius * 0.5f);
            int nextIndex = (i + 1) % nodeCount; Vector3 nextNodePos3D = new Vector3(gizmoNodeDataCache[nextIndex].position.x, gizmoNodeDataCache[nextIndex].position.y, corePos3D.z); Gizmos.color = Color.black; Gizmos.DrawLine(nodePos3D, nextNodePos3D);
            if (nodeCount >= 2) { int prevIndex = (i + nodeCount - 1) % nodeCount; Vector3 prevNodePos3D = new Vector3(gizmoNodeDataCache[prevIndex].position.x, gizmoNodeDataCache[prevIndex].position.y, corePos3D.z); Vector3 vecToPrev = prevNodePos3D - corePos3D; Vector3 vecToCurrent = nodePos3D - corePos3D; if (vecToPrev.sqrMagnitude > 0.0001f && vecToCurrent.sqrMagnitude > 0.0001f) { float angle = Vector3.Angle(vecToPrev, vecToCurrent); bool violatesConstraints = angle < minNodeAngleDeg || angle > maxNodeAngleDeg; if (violatesConstraints) { Handles.color = constraintViolationColor; float violationRadius = centerRadius * 0.9f; Handles.DrawWireArc(corePos3D, Vector3.forward, vecToPrev, angle, violationRadius); Gizmos.color = constraintViolationColor; Gizmos.DrawWireSphere(nodePos3D, nodeGizmoRadius * 1.5f); } } }
        }
    }
#endif
}