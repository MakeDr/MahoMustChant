using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{
    public ComputeShader slimeComputeShader;

    [SerializeField]
    private int nodeCount = 12;
    public int NodeCount => nodeCount; // 읽기 전용 프로퍼티

    private ComputeBuffer nodeBuffer;
    public ComputeBuffer NodeBuffer => nodeBuffer; // 외부 접근 허용

    private int kernel;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        InitializeNodes();

        // Mesh 초기화
        mesh = new Mesh();

        // MeshFilter가 없으면 자동으로 추가
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // MeshRenderer 확인 후 없으면 추가
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        meshFilter.mesh = mesh;
    }

    void InitializeNodes()
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

    void Update()
    {
        if (nodeBuffer == null) return;

        slimeComputeShader.Dispatch(kernel, Mathf.CeilToInt(nodeCount / 64.0f), 1, 1);
        UpdateMesh();
    }

    void UpdateMesh()
    {
        SlimeNode[] nodes = new SlimeNode[nodeCount];
        nodeBuffer.GetData(nodes);

        Vector3[] vertices = new Vector3[nodeCount];
        int[] triangles = new int[(nodeCount - 2) * 3];

        for (int i = 0; i < nodeCount; i++)
        {
            vertices[i] = new Vector3(nodes[i].position.x, nodes[i].position.y, 0);
        }

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

    void OnDestroy()
    {
        if (nodeBuffer != null)
        {
            nodeBuffer.Release();
            nodeBuffer = null;
        }
    }
}
