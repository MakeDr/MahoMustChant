using UnityEngine;

/// <summary>
/// �������� �ٽ� ���� ����: ���� ��� �ʱ�ȭ �� ����
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
    /// ���� ��ҿ� ������ ���ؽ�Ʈ ���� (�ɼ�)
    /// </summary>
    void Init()
    {
        movement.Init(this);
        mana.Init(this);
        visual.Init(this);
    }

    // �ܺο��� ���� ������ ���� ����
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
