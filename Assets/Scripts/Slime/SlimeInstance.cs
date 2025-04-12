using UnityEngine;
using UnityEngine.Rendering;

public class SlimeInstance : MonoBehaviour
{
    public SlimeRenderer slimeRenderer;


    [Header("GPU Sync")]
    public ComputeShader slimeComputeShader;
    private ComputeBuffer nodeBuffer;
    private int kernel;

    private Vector2 corePosition;
    private float coreRotation;
    private float coreMana;

    void Awake()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        int stride = SlimeNodeUtility.GetStride(); // SlimeNode의 stride를 가져옴
        nodeBuffer = new ComputeBuffer(slimeRenderer.NodeCount, stride);  // 기본 노드 수 12로 설정
    }

    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        // Debug: CorePosition이 제대로 전달되고 있는지 확인
        //Debug.Log($"Setting CorePosition in compute shader: {position}");

        // ComputeShader에 데이터 전달
        slimeComputeShader.SetVector("CorePosition", corePosition);
        slimeComputeShader.SetFloat("CoreRotation", coreRotation);
        slimeComputeShader.SetFloat("CoreMana", coreMana);
    }

    void Update()
    {
        if (nodeBuffer == null) return;

        slimeComputeShader.Dispatch(kernel, Mathf.CeilToInt(slimeRenderer.NodeCount / 64.0f), 1, 1);

        // GPU에서 계산된 데이터를 읽어옴(GPU는 유니티내에서 로그 못봄ㅋㅋㅅㅂ)
        ReadFromGPU();
    }
    
    void OnDestroy()
    {
        nodeBuffer.Release();
    }

    void ReadFromGPU()
    {
        AsyncGPUReadback.Request(nodeBuffer, (res) =>
        {
            if (res.hasError)
            {
                Debug.LogError("Error reading from GPU.");
                return;
            }

            var data = res.GetData<SlimeNode>();
            for (int i = 0; i < slimeRenderer.NodeCount; i++)
            {
                var node = data[i];
                Debug.Log($"Node {i} | Pos: {node.position} | Vel: {node.velocity} | CorePos(on GPU): {node.debugCorePos}");
            }
        });
    }
}
