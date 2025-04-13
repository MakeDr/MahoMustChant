using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SlimeInstance : MonoBehaviour
{
    public SlimeRenderer slimeRenderer;

    [Header("GPU Sync")]
    public ComputeShader slimeComputeShader;
    private int kernel;

    private Vector2 corePosition;
    private float coreRotation;
    private float coreMana;

    private ComputeBuffer coreBuffer;  // CoreBuffer �߰�

    void Awake()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        InitializeCoreBuffer();  // CoreBuffer �ʱ�ȭ
    }

    // CoreBuffer �ʱ�ȭ �Լ�
    private void InitializeCoreBuffer()
    {
        coreBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(float4))); // 1���� float4 ũ��
    }

    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        // Core �����͸� float4�� ��ȯ
        float4 coreData = new float4
        {
            x = corePosition.x,
            y = corePosition.y,
            z = coreRotation,
            w = coreMana
        };

        // coreBuffer�� float4 ������ ����
        coreBuffer.SetData(new float4[] { coreData });

        // ���̴��� CoreBuffer ����
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);
    }




    void Update()
    {
        var nodeBuffer = slimeRenderer.NodeBuffer;
        if (nodeBuffer == null || coreBuffer == null) return;

        // ���̴��� NodeBuffer�� CoreBuffer ����
        slimeComputeShader.SetBuffer(kernel, "_NodeBuffer", nodeBuffer);
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);

        slimeComputeShader.Dispatch(kernel, Mathf.CeilToInt(slimeRenderer.NodeCount / 64.0f), 1, 1);

        ReadFromGPU(nodeBuffer);
    }

    void ReadFromGPU(ComputeBuffer nodeBuffer)
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

    void OnDestroy()
    {
        if (coreBuffer != null)
        {
            coreBuffer.Release();
            coreBuffer = null;
        }
    }
}
