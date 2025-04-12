using UnityEngine;

public class SlimeInstance : MonoBehaviour
{
    [Header("GPU Sync")]
    public ComputeShader slimeComputeShader;
    private ComputeBuffer nodeBuffer;
    private int kernel;

    private Vector2 corePosition;
    private float coreRotation;
    private float coreMana;

    struct SlimeNode
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 force;
        public float mass;
    }

    void Awake()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        nodeBuffer = new ComputeBuffer(12, sizeof(float) * (2 + 2 + 2 + 1));  // 기본 노드 수 12로 설정
    }

    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        // ComputeShader에 데이터 전달
        slimeComputeShader.SetVector("CorePosition", corePosition);
        slimeComputeShader.SetFloat("CoreRotation", coreRotation);
        slimeComputeShader.SetFloat("CoreMana", coreMana);
    }

    void Update()
    {
        if (nodeBuffer == null) return;

        slimeComputeShader.Dispatch(kernel, Mathf.CeilToInt(12 / 64.0f), 1, 1);
    }

    void OnDestroy()
    {
        nodeBuffer.Release();
    }
}
