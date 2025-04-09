// Mana_Cell.cs
using UnityEngine;

public class Mana_Cell
{
    // Position은 생성 시 설정되므로 public get만 있어도 충분할 수 있습니다.
    public Vector2Int Position { get; private set; }

    // 외부에서 값을 읽을 수만 있고, 변경은 내부 메서드를 통해서만 가능하도록 합니다.
    public float ManaPower { get; private set; }

    // ManaWaterHeight의 역할이 불분명하여 일단 주석 처리합니다.
    // 필요하다면 역할을 명확히 하고 다시 구현해야 합니다.
    // public float ManaWaterHeight { get; private set; }

    public Mana_Cell(Vector2Int pos)
    {
        Position = pos;
        ManaPower = 0f;
        // ManaWaterHeight = 0f;
    }

    /// <summary>
    /// 셀의 마나 파워를 설정합니다. 값은 0 이상 999 이하로 제한됩니다.
    /// </summary>
    public void SetMana(float amount)
    {
        ManaPower = Mathf.Clamp(amount, 0f, 999f); // 클램핑 로직 포함
    }

    /// <summary>
    /// 셀에 마나를 추가합니다. (내부적으로 SetMana를 호출하여 클램핑 보장 가능)
    /// </summary>
    public void AddMana(float amount)
    {
        SetMana(ManaPower + amount);
    }

    // ManaWaterHeight 관련 메서드가 필요하다면 여기에 추가 (예: SetWaterHeight, ResetWaterHeight 등)
}