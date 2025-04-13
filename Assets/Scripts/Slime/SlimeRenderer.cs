using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{
    // Remove ComputeShader reference if it's not used for rendering itself
    // public ComputeShader slimeComputeShader;

    // No longer owns the buffer
    private ComputeBuffer currentActiveNodeBuffer;
    private int currentNodeCount;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer; // Ensure you have a material assigned that can use the node data or just for visualization

    void Awake() // Changed to Awake
    {
        // Don't initialize nodes or buffer here
        InitializeMeshComponents();
        InitializeMesh();
    }

    // Called by SlimeInstance to provide the latest buffer
    public void SetNodeBuffer(ComputeBuffer buffer, int count)
    {
        currentActiveNodeBuffer = buffer;
        currentNodeCount = count;
    }

    // Update the mesh based on the buffer provided by SlimeInstance
    void Update() // Update mesh rendering in Update (visuals)
    {
        if (currentActiveNodeBuffer == null || !currentActiveNodeBuffer.IsValid()) return;

        // --- Option A: Readback for CPU Mesh Update (Less Performant) ---
        UpdateMeshCPU();

        // --- Option B: Use Buffer Directly in Shader (More Performant) ---
        // If you have a rendering shader that reads the node buffer directly:
        // meshRenderer.material.SetBuffer("_NodeBuffer", currentActiveNodeBuffer); // Use the correct name for the *rendering* shader
        // Graphics.DrawMeshInstancedProcedural or similar might be used here if not using a standard MeshFilter/Renderer setup.
    }

    void OnDestroy()
    {
        // Don't release the buffer here - SlimeInstance owns it
        currentActiveNodeBuffer = null;
    }

    private void InitializeMeshComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        // Ensure a material is assigned in the inspector
        if (meshRenderer.sharedMaterial == null) Debug.LogWarning("SlimeRenderer needs a material assigned!");
    }

    private void InitializeMesh()
    {
        mesh = new Mesh
        {
            name = "Slime Mesh"
            // Mark dynamic for frequent updates
            // markDynamic = true; // Obsolete, use GraphicsBuffer instead for high perf
        };
        meshFilter.mesh = mesh;
    }

    // CPU-based mesh update (causes GPU->CPU sync)
    private void UpdateMeshCPU()
    {
        if (currentNodeCount <= 0) return;

        // *** This GetData call forces synchronization and can stall ***
        SlimeInstance.SlimeNode[] nodes = new SlimeInstance.SlimeNode[currentNodeCount]; // Use struct definition from SlimeInstance
        currentActiveNodeBuffer.GetData(nodes);

        // Check if mesh needs resizing (vertices)
        if (mesh.vertexCount != currentNodeCount)
        {
            mesh.Clear(); // Clear old data if resizing
            mesh.vertices = new Vector3[currentNodeCount]; // Allocate new array
        }

        Vector3[] vertices = mesh.vertices; // Get potentially existing array to modify
        for (int i = 0; i < currentNodeCount; i++)
        {
            vertices[i] = new Vector3(nodes[i].position.x, nodes[i].position.y, 0);
        }

        // Only recalculate triangles if the count changed
        if (mesh.triangles.Length != (currentNodeCount - 2) * 3 && currentNodeCount >= 3)
        {
            int[] triangles = new int[(currentNodeCount - 2) * 3];
            int index = 0;
            // Simple fan triangulation from the first vertex
            for (int i = 1; i < currentNodeCount - 1; i++)
            {
                triangles[index++] = 0;
                triangles[index++] = i;
                triangles[index++] = i + 1;
            }
            mesh.triangles = triangles;
        }


        mesh.vertices = vertices; // Assign potentially modified array back
        mesh.RecalculateBounds(); // Important for visibility
        // RecalculateNormals might be slow, consider alternatives if needed
        mesh.RecalculateNormals();
    }
}

// Make sure SlimeNode definition is consistent or accessible
// [StructLayout(LayoutKind.Sequential)]
// public struct SlimeNode { ... } // Defined in SlimeInstance now