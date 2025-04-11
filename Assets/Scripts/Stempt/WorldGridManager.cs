using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGridManager : MonoBehaviour
{
    public Tilemap groundTilemap;    // 지면 타일맵
    public Tilemap wallTilemap;      // 벽 타일맵
    public Tilemap manaWaterTilemap; // 마나 물 타일맵
    public Vector2Int gridSize;

    [Header("Gizmo 설정")]
    public bool showGizmos = true;   // Gizmo 표시 여부를 제어하는 토글

    private WorldTile[,] worldGrid;

    private void Start()
    {
        InitializeGrid();
    }

    /// <summary>
    /// 타일맵 데이터를 기반으로 그리드를 초기화합니다.
    /// </summary>
    public void InitializeGrid()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("groundTilemap이 설정되지 않았습니다.");
            return;
        }

        Vector2Int gridOffset = GetGridOffset(groundTilemap);
        gridSize = GetGridSize(groundTilemap);

        Debug.Log($"Grid initialized with size: {gridSize}, offset: {gridOffset}");

        worldGrid = new WorldTile[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int worldPos = new Vector2Int(x + gridOffset.x, y + gridOffset.y);
                TerrainType terrain = ResolveTerrainAt(worldPos);
                worldGrid[x, y] = new WorldTile(worldPos, terrain);
            }
        }
    }

    /// <summary>
    /// 월드 좌표에 해당하는 타일의 TerrainType을 판별합니다.
    /// </summary>
    private TerrainType ResolveTerrainAt(Vector2Int worldPos)
    {
        Vector3Int cell = new Vector3Int(worldPos.x, worldPos.y, 0);

        // 마나 물 타일맵 우선 처리
        if (manaWaterTilemap != null && manaWaterTilemap.HasTile(cell))
            return TerrainType.ManaWater;

        // 벽 타일맵 처리
        if (wallTilemap != null && wallTilemap.HasTile(cell))
            return TerrainType.Wall;

        // 지면 타일맵 처리
        if (groundTilemap != null && groundTilemap.HasTile(cell))
            return TerrainType.Ground;

        // 타일이 없는 경우
        return TerrainType.Empty;
    }

    /// <summary>
    /// Gizmo를 항상 그리며, 토글로 제어할 수 있습니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return; // Gizmo 표시가 꺼져 있으면 종료

        if (worldGrid == null)
        {
            Debug.LogWarning("worldGrid가 초기화되지 않았습니다. InitializeGrid를 호출하세요.");
            return;
        }

        if (groundTilemap == null)
        {
            Debug.LogError("groundTilemap이 설정되지 않았습니다.");
            return;
        }

        // 타일맵의 범위를 가져옵니다.
        BoundsInt bounds = GetTileBounds(groundTilemap);
        Vector2Int gridOffset = GetGridOffset(groundTilemap);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                WorldTile tile = worldGrid[x, y];
                if (tile == null) continue;

                Vector3 worldPos = groundTilemap.GetCellCenterWorld(new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0)); // gridOffset 적용

                // 색상 설정
                switch (tile.Terrain)
                {
                    case TerrainType.ManaWater:
                        Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // 민트색 (Cyan) 반투명
                        break;
                    case TerrainType.Ground:
                        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // 녹색 (Green) 반투명
                        break;
                    case TerrainType.Wall:
                        Gizmos.color = new Color(0f, 0f, 0.5f, 0.5f); // 군청색 (Navy) 반투명
                        break;
                    case TerrainType.Empty:
                    default:
                        Gizmos.color = new Color(1f, 1f, 1f, 0.5f); // 흰색 (White) 반투명
                        break;
                }

                // 타일 시각화
                Gizmos.DrawCube(worldPos, groundTilemap.cellSize * 0.9f);
            }
        }
    }

    /// <summary>
    /// 타일맵에서 실제 존재하는 셀 범위를 반환합니다.
    /// </summary>
    private BoundsInt GetTileBounds(Tilemap tilemap)
    {
        return tilemap.cellBounds;
    }

    /// <summary>
    /// 타일맵의 왼쪽 아래 모서리 위치(=offset)를 반환합니다.
    /// </summary>
    private Vector2Int GetGridOffset(Tilemap tilemap)
    {
        BoundsInt bounds = GetTileBounds(tilemap);
        return new Vector2Int(bounds.xMin, bounds.yMin);
    }

    /// <summary>
    /// 타일맵의 유효한 범위 크기(=배열 크기)를 반환합니다.
    /// </summary>
    private Vector2Int GetGridSize(Tilemap tilemap)
    {
        BoundsInt bounds = GetTileBounds(tilemap);
        return new Vector2Int(bounds.size.x, bounds.size.y);
    }
}
