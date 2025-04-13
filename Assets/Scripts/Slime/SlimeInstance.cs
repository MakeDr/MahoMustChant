using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics; // For float4

public class SlimeInstance : MonoBehaviour
{
    public ComputeShader slimeComputeShader;
    public SlimeRenderer slimeRenderer; // Assign in Inspector

    [Header("Simulation Params")]
    public int nodeCount = 128; // Ensure this matches SlimeRenderer if needed there
    public float stiffness = 10.0f;
    public float damping = 0.9f;
    public float neighborStiffness = 5.0f; // Add if using neighbor springs
    public float centerRadius = 1.0f;

    // --- Core Data ---
    private ComputeBuffer coreBuffer;
    private Vector2 corePosition;
    private float coreRotation;
    private float coreMana;

    // --- Node Buffers (Double Buffering) ---
    private ComputeBuffer nodeBufferA;
    private ComputeBuffer nodeBufferB;
    private bool isA_CurrentReadBuffer = true; // Tracks which buffer holds the *latest* data for reading

    private int kernel;
    private int nodeStride;

    void Start() // Use Start instead of Awake if SlimeRenderer needs initialization first
    {
        if (slimeRenderer == null)
        {
            Debug.LogError("SlimeRenderer not assigned to SlimeInstance!");
            this.enabled = false;
            return;
        }
        // Make sure node counts align if SlimeRenderer reads it directly
        // slimeRenderer.SetNodeCount(nodeCount); // Add such a method if needed

        kernel = slimeComputeShader.FindKernel("CSMain");

        // Calculate stride (ensure SlimeNode struct is defined correctly)
        nodeStride = Marshal.SizeOf(typeof(SlimeNode)); // Make sure SlimeNode struct is accessible or defined here

        InitializeCoreBuffer();
        InitializeNodeBuffers();
    }

    void InitializeCoreBuffer()
    {
        coreBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(float4)), ComputeBufferType.Structured);
        // Initial core data can be set here or in the first SyncCoreData call
        SyncCoreData(transform.position, 0f, 100f); // Example initial sync
    }

    void InitializeNodeBuffers()
    {
        nodeBufferA = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);
        nodeBufferB = new ComputeBuffer(nodeCount, nodeStride, ComputeBufferType.Structured);

        // Initialize starting node data (e.g., in a circle)
        SlimeNode[] initialNodes = new SlimeNode[nodeCount];
        float angleStep = Mathf.PI * 2f / nodeCount;
        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * angleStep;
            initialNodes[i] = new SlimeNode
            {
                position = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * centerRadius,
                velocity = Vector2.zero,
                // force = Vector2.zero, // Force is often transient, might not need initialization
                mass = 1f
            };
        }

        // Put initial data into Buffer A, mark it as the first Read buffer
        nodeBufferA.SetData(initialNodes);
        isA_CurrentReadBuffer = true;

        // Pass the initial buffer to the renderer
        slimeRenderer.SetNodeBuffer(nodeBufferA, nodeCount);
    }

    // Called by SlimeCore (e.g., in FixedUpdate)
    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        // Only update if data actually changed to avoid unnecessary SetData calls (optional optimization)
        if (corePosition == position && coreRotation == rotation && coreMana == mana && coreBuffer != null && coreBuffer.IsValid()) return;

        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        float4 coreData = new float4(corePosition.x, corePosition.y, coreRotation, coreMana);
        // Use try-catch or IsValid() check for safety if buffer might be released elsewhere
        if (coreBuffer != null && coreBuffer.IsValid())
        {
            coreBuffer.SetData(new float4[] { coreData });
        }
        else
        {
            Debug.LogWarning("CoreBuffer is null or invalid during SyncCoreData.");
        }

    }

    // Use FixedUpdate for physics simulation consistency
    void FixedUpdate()
    {
        if (nodeBufferA == null || nodeBufferB == null || coreBuffer == null || !coreBuffer.IsValid()) return;

        // 1. Determine Read/Write buffers for THIS dispatch
        ComputeBuffer readBuffer = isA_CurrentReadBuffer ? nodeBufferA : nodeBufferB;
        ComputeBuffer writeBuffer = isA_CurrentReadBuffer ? nodeBufferB : nodeBufferA;

        // 2. Set Shader Uniforms (Parameters)
        slimeComputeShader.SetInt("nodeCount", nodeCount);
        slimeComputeShader.SetFloat("deltaTime", Time.fixedDeltaTime); // Use fixedDeltaTime
        slimeComputeShader.SetFloat("stiffness", stiffness);
        slimeComputeShader.SetFloat("damping", damping);
        slimeComputeShader.SetFloat("neighborStiffness", neighborStiffness); // If using
        slimeComputeShader.SetFloat("centerRadius", centerRadius);

        // 3. Set Buffers for the Kernel
        // *** Bind to the CORRECT HLSL names for double buffering ***
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferRead", readBuffer);
        slimeComputeShader.SetBuffer(kernel, "_NodeBufferWrite", writeBuffer);

        // 4. Dispatch the Compute Shader ONCE
        int threadGroups = Mathf.CeilToInt((float)nodeCount / 64.0f); // Ensure 64 matches [numthreads(x,y,z)] in HLSL
        slimeComputeShader.Dispatch(kernel, threadGroups, 1, 1);

        // 5. SWAP the roles for the NEXT frame
        // The buffer we just WROTE TO (writeBuffer) now holds the latest data
        // and must become the READ buffer for the next FixedUpdate.
        isA_CurrentReadBuffer = !isA_CurrentReadBuffer;

        // 6. Provide the LATEST data buffer to the renderer
        // The buffer that was just written to is now the most current.
        slimeRenderer.SetNodeBuffer(writeBuffer, nodeCount);

        // Remove the AsyncGPUReadback from here unless strictly needed for debugging.
        // Continuous readback adds overhead.
        // ReadFromGPU(writeBuffer);
    }

    // Optional Debug Readback
    // void ReadFromGPU(ComputeBuffer bufferToRead)
    // {
    //     AsyncGPUReadback.Request(bufferToRead, (res) => { /* ... */ });
    // }

    void OnDestroy()
    {
        // Release all buffers
        nodeBufferA?.Release();
        nodeBufferB?.Release();
        coreBuffer?.Release();
        nodeBufferA = null;
        nodeBufferB = null;
        coreBuffer = null;
    }

    // Define SlimeNode struct here or ensure it's accessible
    [StructLayout(LayoutKind.Sequential)] // Good practice for Compute Buffers
    public struct SlimeNode
    {
        public Vector2 position;
        public Vector2 velocity;
        // Consider removing force if only used transiently in the shader
        // public Vector2 force;
        public float mass;
        // Add padding here if needed to match HLSL struct size/alignment (e.g., float padding;)
    }
}