using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TileType�� Unity Tile ��ü ���� ������ �����ϴ� ��ƿ��Ƽ Ŭ�����Դϴ�.
/// </summary>
public static class TileMappingUtility
{
    private static readonly Dictionary<TerrainCell.TileType, Tile> TileMapping = new Dictionary<TerrainCell.TileType, Tile>();

    /// <summary>
    /// TileType�� Unity Tile�� �����մϴ�.
    /// </summary>
    public static void RegisterTile(TerrainCell.TileType tileType, Tile tile)
    {
        if (TileMapping.ContainsKey(tileType))
        {
            TileMapping[tileType] = tile;
        }
        else
        {
            TileMapping.Add(tileType, tile);
        }
    }

    /// <summary>
    /// TileType�� �ش��ϴ� Unity Tile�� ��ȯ�մϴ�.
    /// </summary>
    public static Tile GetTile(TerrainCell.TileType tileType)
    {
        if (TileMapping.TryGetValue(tileType, out var tile))
        {
            return tile;
        }

        Debug.LogWarning($"No Unity Tile registered for TileType: {tileType}");
        return null;
    }
}
