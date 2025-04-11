// TerrainCell.cs
using UnityEngine;

/// <summary>
/// ���� ���� ��Ÿ���� Ŭ�����Դϴ�. ��ġ�� TileType�� �����մϴ�.
/// </summary>
public class TerrainCell
{
    public Vector2Int Position { get; private set; }
    public TileType Type { get; private set; }

    // Ÿ�� Ÿ�� ���� (������ �ð��� ǥ��)
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

