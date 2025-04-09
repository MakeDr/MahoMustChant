// TerrainCell.cs
using UnityEngine;

/// <summary>
/// 지형 셀을 나타내는 클래스입니다. 위치와 TileType을 관리합니다.
/// </summary>
public class TerrainCell
{
    public Vector2Int Position { get; private set; }
    public TileType Type { get; private set; }

    // 타일 타입 정의 (지형의 시각적 표현)
    public enum TileType
    {
        Empty,
        Ground,
        Wall,
        ManaWater
    }

    public TerrainCell(Vector2Int pos, TileType type)
    {
        Position = pos;
        Type = type;
    }
}

