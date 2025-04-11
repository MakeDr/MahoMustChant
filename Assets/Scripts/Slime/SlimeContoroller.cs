using UnityEngine;

/// <summary>
/// 슬라임의 핵심 제어 역할: 구성 요소 초기화 및 관리
/// </summary>
[RequireComponent(typeof(SlimeMovement), typeof(SlimeMana), typeof(SlimeVisual))]
public class SlimeController : MonoBehaviour
{
    private SlimeMovement movement;
    private SlimeMana mana;
    private SlimeVisual visual;

    void Awake()
    {
        movement = GetComponent<SlimeMovement>();
        mana = GetComponent<SlimeMana>();
        visual = GetComponent<SlimeVisual>();

        Init();
    }

    /// <summary>
    /// 구성 요소에 슬라임 컨텍스트 연결 (옵션)
    /// </summary>
    void Init()
    {
        movement.Init(this);
        mana.Init(this);
        visual.Init(this);
    }

    // 외부에서 접근 가능한 정보 예시
    public float GetManaRatio()
    {
        return mana.GetRatio();
    }

    public void GainMana(float amount)
    {
        mana.Gain(amount);
    }

    public void ConsumeMana(float amount)
    {
        mana.Consume(amount);
    }
}
