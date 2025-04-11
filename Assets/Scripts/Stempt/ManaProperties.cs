

/// <summary>
/// 타일의 마나 속성을 정의하는 클래스입니다.
/// </summary>
public class ManaProperties
{
    public float Power;              // 실제 확산에 쓰이는 마나 값
    public float ApparentPower;      // 감각용: 슬라임, 벌레 등 생물이 인식하는 값

    /// <summary>
    /// 마나 속성을 초기화합니다.
    /// </summary>
    public void Clear()
    {
        Power = 0f;
        ApparentPower = 0f;
    }
}
