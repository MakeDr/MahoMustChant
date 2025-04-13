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

    private ComputeBuffer coreBuffer;  // CoreBuffer 추가

    void Awake()
    {
        kernel = slimeComputeShader.FindKernel("CSMain");
        InitializeCoreBuffer();  // CoreBuffer 초기화
    }

    // CoreBuffer 초기화 함수
    private void InitializeCoreBuffer()
    {
        coreBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(float4))); // 1개의 float4 크기
    }

    public void SyncCoreData(Vector2 position, float rotation, float mana)
    {
        corePosition = position;
        coreRotation = rotation;
        coreMana = mana;

        // Core 데이터를 float4로 변환
        float4 coreData = new float4
        {
            x = corePosition.x,
            y = corePosition.y,
            z = coreRotation,
            w = coreMana
        };

        // coreBuffer에 float4 데이터 전달
        coreBuffer.SetData(new float4[] { coreData });

        // 셰이더에 CoreBuffer 전달
        slimeComputeShader.SetBuffer(kernel, "_CoreBuffer", coreBuffer);
    }




    void Update()
    {
        var nodeBuffer = slimeRenderer.NodeBuffer;
        if (nodeBuffer == null || coreBuffer == null) return;

        // 셰이더에 NodeBuffer와 CoreBuffer 전달
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
