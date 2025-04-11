using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // 딕셔너리 사용을 위한 네임스페이스 추가

/// <summary>
/// 그리드 구조를 관리하는 클래스입니다. 마나 셀과 지형 셀을 초기화하고, Tilemap에 타일을 렌더링합니다.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("그리드 설정")]
    public int width = 20;
    public int height = 20;

    [Header("타일맵 설정")]
    public Tilemap tilemap;       // Unity Tilemap 컴포넌트
    public Tile groundTile;       // Ground 타입 타일
    public Tile wallTile;         // Wall 타입 타일
    public Tile manaWaterTile;    // ManaWater 타입 타일

    private Mana_Cell[,] manaGrid;       // 마나 관련 로직을 위한 그리드
    private TerrainCell[,] terrainGrid; // 지형 관련 로직을 위한 그리드

    // TileType과 CellType 간의 매핑 정의
    private readonly Dictionary<TileBase, TerrainCell.TileType> tileToTerrainMapping = new Dictionary<TileBase, TerrainCell.TileType>();

    private readonly Dictionary<TerrainCell.TileType, Mana_Cell.CellType> tileToCellMapping = new Dictionary<TerrainCell.TileType, Mana_Cell.CellType>
    {
        { TerrainCell.TileType.Empty, Mana_Cell.CellType.Empty },
        { TerrainCell.TileType.ManaWater, Mana_Cell.CellType.Empty },
        { TerrainCell.TileType.Ground, Mana_Cell.CellType.Blocked },
        { TerrainCell.TileType.Wall, Mana_Cell.CellType.Blocked }
    };

    void Awake()
    {
        // 타일 매핑 등록
        RegisterTileMappings();

        // 타일맵 데이터를 기반으로 그리드 초기화
        InitializeGridsFromTilemap();

        // ManaFlow_Manager에 manaGrid 전달
        var manaFlowManager = Object.FindFirstObjectByType<ManaFlow_Manager>();
        if (manaFlowManager != null)
        {
            manaFlowManager.SetManaGrid(manaGrid);
        }
        else
        {
            Debug.LogWarning("ManaFlow_Manager를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 타일과 TerrainCell.TileType 간의 매핑을 등록합니다.
    /// </summary>
    void RegisterTileMappings()
    {
        tileToTerrainMapping[groundTile] = TerrainCell.TileType.Ground;
        tileToTerrainMapping[wallTile] = TerrainCell.TileType.Wall;
        tileToTerrainMapping[manaWaterTile] = TerrainCell.TileType.ManaWater;
    }

    /// <summary>
    /// Tilemap 데이터를 기반으로 manaGrid와 terrainGrid를 초기화합니다.
    /// </summary>
    /// <summary>
    /// Tilemap 데이터를 기반으로 manaGrid와 terrainGrid를 초기화합니다.
    /// </summary>
    void InitializeGridsFromTilemap()
    {
        // Tilemap의 경계(bounds)를 가져옵니다.
        BoundsInt bounds = tilemap.cellBounds;

        // 경계 크기가 유효하지 않은 경우 초기화를 건너뜁니다.
        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogError("Tilemap 경계가 유효하지 않습니다.");
            return;
        }

        // manaGrid와 terrainGrid를 Tilemap의 크기에 맞게 초기화합니다.
        manaGrid = new Mana_Cell[bounds.size.x, bounds.size.y];
        terrainGrid = new TerrainCell[bounds.size.x, bounds.size.y];

        // Tilemap의 모든 타일을 순회하며 그리드를 초기화합니다.
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // 현재 타일의 위치를 가져옵니다.
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // Tilemap에서 현재 위치의 타일을 가져옵니다.
                TileBase tile = tilemap.GetTile(tilePosition);

                // 그리드 좌표를 계산합니다 (Tilemap의 경계 기준으로 0부터 시작).
                var position = new Vector2Int(x - bounds.xMin, y - bounds.yMin);

                if (tile == null)
                {
                    // 타일이 없는 경우, Empty로 처리합니다.
                    terrainGrid[position.x, position.y] = new TerrainCell(position, TerrainCell.TileType.Empty);
                    manaGrid[position.x, position.y] = new Mana_Cell(position, Mana_Cell.CellType.Empty);
                }
                else if (tileToTerrainMapping.TryGetValue(tile, out var terrainType))
                {
                    // 타일이 있는 경우, 매핑된 TerrainCell.TileType을 가져옵니다.
                    terrainGrid[position.x, position.y] = new TerrainCell(position, terrainType);

                    // TileType에 따라 Mana_Cell의 CellType을 설정합니다.
                    if (tileToCellMapping.TryGetValue(terrainType, out var cellType))
                    {
                        manaGrid[position.x, position.y] = new Mana_Cell(position, cellType);

                        // ManaWater 타일인 경우, ManaWater_Source를 생성합니다.
                        if (terrainType == TerrainCell.TileType.ManaWater)
                        {
                            CreateManaWaterSource(position);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"TileType '{terrainType}'에 대한 CellType 매핑이 없습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Tile '{tile}'에 대한 TerrainCell.TileType 매핑이 없습니다.");
                }
            }
        }
    }

    /// <summary>
    /// ManaWater_Source를 생성하고 초기화합니다.
    /// </summary>
    /// <param name="position">ManaWater_Source의 위치</param>
    void CreateManaWaterSource(Vector2Int position)
    {
        // ManaWater_Source를 생성합니다.
        var source = new ManaWater_Source(position);

        // 해당 위치의 Mana_Cell에 초기 마나를 설정합니다.
        manaGrid[position.x, position.y].SetMana(source.maxMana, source.maxMana);

        // ManaFlow_Manager를 찾습니다.
        var manaFlowManager = Object.FindFirstObjectByType<ManaFlow_Manager>();
        if (manaFlowManager != null)
        {
            // manaWaterSources 리스트가 초기화되지 않았다면 초기화합니다.
            if (manaFlowManager.manaWaterSources == null)
            {
                manaFlowManager.manaWaterSources = new List<ManaWater_Source>();
            }

            // ManaWater_Source를 리스트에 추가합니다.
            manaFlowManager.manaWaterSources.Add(source);
            Debug.Log($"ManaWater_Source가 위치 {position}에 생성되었으며, ManaFlow_Manager에 반영되었습니다.");
        }
        else
        {
            Debug.LogWarning("ManaFlow_Manager를 찾을 수 없습니다. ManaWater_Source가 반영되지 않았습니다.");
        }
    }

    /// <summary>
    /// 지형 그리드를 Tilemap에 렌더링합니다.
    /// </summary>
    public void RenderTileMap()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap이 할당되지 않았습니다.");
            return;
        }

        tilemap.ClearAllTiles();

        for (int x = 0; x < terrainGrid.GetLength(0); x++)
        {
            for (int y = 0; y < terrainGrid.GetLength(1); y++)
            {
                var terrainCell = terrainGrid[x, y];
                var position = new Vector3Int(x, y, 0);
                var tile = TileMappingUtility.GetTile(terrainCell.Type);

                tilemap.SetTile(position, tile);
            }
        }
    }

    /// <summary>
    /// 마나 그리드를 반환합니다.
    /// </summary>
    public Mana_Cell[,] GetManaGrid() => manaGrid;

    /// <summary>
    /// 지형 그리드를 반환합니다.
    /// </summary>
    public TerrainCell[,] GetTerrainGrid() => terrainGrid;
}
