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
        int stride = SlimeNodeUtility.GetStride(); // SlimeNode�� stride�� ������
        nodeBuffer = new ComputeBuffer(slimeRenderer.NodeCount, stride);  // �⺻ ��� �� 12�� ����
    }

    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        // Debug: CorePosition�� ����� ���޵ǰ� �ִ��� Ȯ��
        //Debug.Log($"Setting CorePosition in compute shader: {position}");

        // ComputeShader�� ������ ����
        slimeComputeShader.SetVector("CorePosition", corePosition);
        slimeComputeShader.SetFloat("CoreRotation", coreRotation);
        slimeComputeShader.SetFloat("CoreMana", coreMana);
    }

    void Update()
    {
        if (nodeBuffer == null) return;

        slimeComputeShader.Dispatch(kernel, Mathf.CeilToInt(slimeRenderer.NodeCount / 64.0f), 1, 1);

        // GPU���� ���� �����͸� �о��(GPU�� ����Ƽ������ �α� ������������)
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
