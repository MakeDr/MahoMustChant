﻿// Mana_Cell.cs
using UnityEngine;

public class Mana_Cell
{
    public Vector2Int Position { get; private set; }
    public float ManaPower { get; private set; }

    // 셀 타입 정의
    public enum CellType
    {
        Empty,  // 마나가 흐를 수 있는 빈 셀
        Blocked // 마나가 흐를 수 없는 막힌 셀
    }

    public CellType Type { get; private set; }

    public Mana_Cell(Vector2Int pos, CellType type = CellType.Empty)
    {
        Position = pos;
        Type = type;
        ManaPower = 0f;
    }

    /// <summary>
    /// 셀의 마나 파워를 설정합니다. 값은 0 이상 999 이하로 제한됩니다.
    /// </summary>
    public void SetMana(float amount)
    {
        ManaPower = Mathf.Clamp(amount, 0f, 999f);
    }

    /// <summary>
    /// 셀에 마나를 추가합니다. (내부적으로 SetMana를 호출하여 클램핑 보장 가능)
    /// </summary>
    public void AddMana(float amount)
    {
        SetMana(ManaPower + amount);
    }

    /// <summary>
    /// 셀의 타입을 설정합니다.
    /// </summary>
    public void SetType(CellType type)
    {
        Type = type;
    }
}
