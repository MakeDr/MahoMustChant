

/// <summary>
/// Ÿ���� ���� �Ӽ��� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class ManaProperties
{
    public float Power;              // ���� Ȯ�꿡 ���̴� ���� ��
    public float ApparentPower;      // ������: ������, ���� �� ������ �ν��ϴ� ��

    /// <summary>
    /// ���� �Ӽ��� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void Clear()
    {
        Power = 0f;
        ApparentPower = 0f;
    }
}
