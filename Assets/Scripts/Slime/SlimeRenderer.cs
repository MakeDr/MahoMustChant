using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{
    public ComputeShader slimeComputeShader;
    public int nodeCount = 12;

    private ComputeBuffer nodeBuffer;
    private int kernel;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    struct SlimeNode
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 force;
        public float mass;
    }

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
            meshFilter = gameObject.AddComponent<MeshFilter>(); // MeshFilter 추가
        }

        // MeshRenderer 확인 후 없으면 추가
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>(); // MeshRenderer 추가
        }

        meshFilter.mesh = mesh;
    }

    void InitializeNodes()
    {
        int stride = sizeof(float) * (2 + 2 + 2 + 1);
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
        slimeComputeShader.SetBuffer(kernel, "NodeBuffer", nodeBuffer);
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

        // 메쉬의 정점 배열과 삼각형의 인덱스 배열
        Vector3[] vertices = new Vector3[nodeCount];
        int[] triangles = new int[(nodeCount - 2) * 3]; // 기본적으로 n-2개의 삼각형이 존재

        for (int i = 0; i < nodeCount; i++)
        {
            vertices[i] = new Vector3(nodes[i].position.x, nodes[i].position.y, 0);
        }

        // 삼각형 인덱스를 연결
        int index = 0;
        for (int i = 1; i < nodeCount - 1; i++)
        {
            triangles[index++] = 0; // 첫 번째 정점
            triangles[index++] = i; // 현재 정점
            triangles[index++] = i + 1; // 다음 정점
        }

        // 메시 업데이트
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // 메시의 노멀을 계산
        mesh.RecalculateNormals();
    }

    void OnDestroy()
    {
        if (nodeBuffer != null)
        {
            nodeBuffer.Release();
        }
    }
}
