using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{
    private ComputeBuffer activeNodeBuffer; // Reference to the buffer with latest data
    private int activeNodeCount;
    private SlimeNode[] nodeDataCache; // CPU cache for node data

    // Optional: Mesh related variables if drawing mesh/lines
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material lineMaterial; // Simple material for lines/points

    void Awake()
    {
        // Example: Setup for drawing simple debug points/lines
        // You might replace this with your mesh generation later
        lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // Simple unlit shader
    }


    // Called by SlimeInstance to provide the latest data buffer
    public void SetNodeBuffer(ComputeBuffer buffer, int count)
    {
        activeNodeBuffer = buffer;
        activeNodeCount = count;

        // Resize CPU cache if necessary
        if (nodeDataCache == null || nodeDataCache.Length != activeNodeCount)
        {
            nodeDataCache = new SlimeNode[activeNodeCount];
        }
    }

    void Update()
    {
        // Ensure we have a valid buffer and node data
        if (activeNodeBuffer == null || !activeNodeBuffer.IsValid() || activeNodeCount <= 0)
        {
            return;
        }

        // --- Read Data from GPU ---
        // WARNING: GetData() stalls CPU waiting for GPU. Use AsyncGPUReadback for better performance later.
        try
        {
            activeNodeBuffer.GetData(nodeDataCache);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading node buffer in Renderer: {e.Message}", this);
            return; // Stop if data read fails
        }

        // --- Simple Visualization (Example: Draw Debug Lines/Points) ---
        // Replace this with your actual mesh rendering logic in later phases
        for (int i = 0; i < activeNodeCount; i++)
        {
            Vector3 pos = nodeDataCache[i].position;
            // Draw a small cross at each node position
            Debug.DrawLine(pos - Vector3.up * 0.1f, pos + Vector3.up * 0.1f, Color.cyan);
            Debug.DrawLine(pos - Vector3.right * 0.1f, pos + Vector3.right * 0.1f, Color.cyan);

            // Optional: Draw lines connecting nodes
            // int nextIndex = (i + 1) % activeNodeCount;
            // Vector3 nextPos = nodeDataCache[nextIndex].position;
            // Debug.DrawLine(pos, nextPos, Color.magenta);
        }

        // --- Mesh Rendering (Alternative - Implement Later) ---
        /*
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (mesh == null) { mesh = new Mesh(); meshFilter.mesh = mesh; }
        // Update mesh vertices based on nodeDataCache
        // UpdateMesh();
        */
    }

    // Placeholder for mesh update logic
    // void UpdateMesh() { /* ... Vertex and triangle updates ... */ }
}