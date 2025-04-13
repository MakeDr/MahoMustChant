using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{
    public ComputeShader slimeComputeShader;

    [SerializeField]
    private int nodeCount = 12;
    public int NodeCount => nodeCount;

    private ComputeBuffer nodeBuffer;
    public ComputeBuffer NodeBuffer => nodeBuffer;

    private int kernel;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        InitializeNodes();
        InitializeMeshComponents();
        InitializeMesh();
    }

    void Update()
    {
        if (nodeBuffer == null) return;

        DispatchShader();
        UpdateMesh();
    }

    void OnDestroy()
    {
        nodeBuffer?.Release();
        nodeBuffer = null;
    }

    private void InitializeNodes()
    {
        int stride = SlimeNodeUtility.GetStride();
        nodeBuffer = new ComputeBuffer(nodeCount, stride);

        SlimeNode[] nodes = new SlimeNode[nodeCount];
        float angleStep = Mathf.PI * 2f / nodeCount;

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * angleStep;
            nodes[i] = new SlimeNode
            {
                position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)),
                velocity = Vector2.zero,
                force = Vector2.zero,
                mass = 1f
            };
        }

        nodeBuffer.SetData(nodes);
        slimeComputeShader.SetBuffer(kernel, "_NodeBuffer", nodeBuffer);
    }

    private void InitializeMeshComponents()
    {
        meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }

    private void InitializeMesh()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    private void DispatchShader()
    {
        slimeComputeShader.SetInt("nodeCount", nodeCount);
        slimeComputeShader.SetFloat("deltaTime", Time.deltaTime);
        slimeComputeShader.SetFloat("stiffness", 10.0f);
        slimeComputeShader.SetFloat("damping", 0.9f);
        slimeComputeShader.SetFloat("centerRadius", 1.0f);
        slimeComputeShader.SetBuffer(kernel, "_NodeBuffer", nodeBuffer);

        int threadGroups = Mathf.CeilToInt(nodeCount / 64f);
        slimeComputeShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    private void UpdateMesh()
    {
        SlimeNode[] nodes = new SlimeNode[nodeCount];
        nodeBuffer.GetData(nodes);

        Vector3[] vertices = new Vector3[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            vertices[i] = new Vector3(nodes[i].position.x, nodes[i].position.y, 0);
        }

        int[] triangles = new int[(nodeCount - 2) * 3];
        int index = 0;
        for (int i = 1; i < nodeCount - 1; i++)
        {
            triangles[index++] = 0;
            triangles[index++] = i;
            triangles[index++] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
