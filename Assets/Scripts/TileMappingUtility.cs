using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TileType과 Unity Tile 객체 간의 매핑을 관리하는 유틸리티 클래스입니다.
/// </summary>
public static class TileMappingUtility
{
    private static readonly Dictionary<TerrainCell.TileType, Tile> TileMapping = new Dictionary<TerrainCell.TileType, Tile>();

    /// <summary>
    /// TileType에 Unity Tile을 매핑합니다.
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
    /// TileType에 해당하는 Unity Tile을 반환합니다.
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
