using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // ��ųʸ� ����� ���� ���ӽ����̽� �߰�

/// <summary>
/// �׸��� ������ �����ϴ� Ŭ�����Դϴ�. ���� ���� ���� ���� �ʱ�ȭ�ϰ�, Tilemap�� Ÿ���� �������մϴ�.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("�׸��� ����")]
    public int width = 20;
    public int height = 20;

    [Header("Ÿ�ϸ� ����")]
    public Tilemap tilemap;       // Unity Tilemap ������Ʈ
    public Tile groundTile;       // Ground Ÿ�� Ÿ��
    public Tile wallTile;         // Wall Ÿ�� Ÿ��
    public Tile manaWaterTile;    // ManaWater Ÿ�� Ÿ��

    private Mana_Cell[,] manaGrid;       // ���� ���� ������ ���� �׸���
    private TerrainCell[,] terrainGrid; // ���� ���� ������ ���� �׸���

    // TileType�� CellType ���� ���� ����
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
        // Ÿ�� ���� ���
        RegisterTileMappings();

        // Ÿ�ϸ� �����͸� ������� �׸��� �ʱ�ȭ
        InitializeGridsFromTilemap();

        // ManaFlow_Manager�� manaGrid ����
        var manaFlowManager = Object.FindFirstObjectByType<ManaFlow_Manager>();
        if (manaFlowManager != null)
        {
            manaFlowManager.SetManaGrid(manaGrid);
        }
        else
        {
            Debug.LogWarning("ManaFlow_Manager�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// Ÿ�ϰ� TerrainCell.TileType ���� ������ ����մϴ�.
    /// </summary>
    void RegisterTileMappings()
    {
        tileToTerrainMapping[groundTile] = TerrainCell.TileType.Ground;
        tileToTerrainMapping[wallTile] = TerrainCell.TileType.Wall;
        tileToTerrainMapping[manaWaterTile] = TerrainCell.TileType.ManaWater;
    }

    /// <summary>
    /// Tilemap �����͸� ������� manaGrid�� terrainGrid�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <summary>
    /// Tilemap �����͸� ������� manaGrid�� terrainGrid�� �ʱ�ȭ�մϴ�.
    /// </summary>
    void InitializeGridsFromTilemap()
    {
        // Tilemap�� ���(bounds)�� �����ɴϴ�.
        BoundsInt bounds = tilemap.cellBounds;

        // ��� ũ�Ⱑ ��ȿ���� ���� ��� �ʱ�ȭ�� �ǳʶݴϴ�.
        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogError("Tilemap ��谡 ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        // manaGrid�� terrainGrid�� Tilemap�� ũ�⿡ �°� �ʱ�ȭ�մϴ�.
        manaGrid = new Mana_Cell[bounds.size.x, bounds.size.y];
        terrainGrid = new TerrainCell[bounds.size.x, bounds.size.y];

        // Tilemap�� ��� Ÿ���� ��ȸ�ϸ� �׸��带 �ʱ�ȭ�մϴ�.
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                // ���� Ÿ���� ��ġ�� �����ɴϴ�.
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // Tilemap���� ���� ��ġ�� Ÿ���� �����ɴϴ�.
                TileBase tile = tilemap.GetTile(tilePosition);

                // �׸��� ��ǥ�� ����մϴ� (Tilemap�� ��� �������� 0���� ����).
                var position = new Vector2Int(x - bounds.xMin, y - bounds.yMin);

                if (tile == null)
                {
                    // Ÿ���� ���� ���, Empty�� ó���մϴ�.
                    terrainGrid[position.x, position.y] = new TerrainCell(position, TerrainCell.TileType.Empty);
                    manaGrid[position.x, position.y] = new Mana_Cell(position, Mana_Cell.CellType.Empty);
                }
                else if (tileToTerrainMapping.TryGetValue(tile, out var terrainType))
                {
                    // Ÿ���� �ִ� ���, ���ε� TerrainCell.TileType�� �����ɴϴ�.
                    terrainGrid[position.x, position.y] = new TerrainCell(position, terrainType);

                    // TileType�� ���� Mana_Cell�� CellType�� �����մϴ�.
                    if (tileToCellMapping.TryGetValue(terrainType, out var cellType))
                    {
                        manaGrid[position.x, position.y] = new Mana_Cell(position, cellType);

                        // ManaWater Ÿ���� ���, ManaWater_Source�� �����մϴ�.
                        if (terrainType == TerrainCell.TileType.ManaWater)
                        {
                            CreateManaWaterSource(position);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"TileType '{terrainType}'�� ���� CellType ������ �����ϴ�.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Tile '{tile}'�� ���� TerrainCell.TileType ������ �����ϴ�.");
                }
            }
        }
    }

    /// <summary>
    /// ManaWater_Source�� �����ϰ� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="position">ManaWater_Source�� ��ġ</param>
    void CreateManaWaterSource(Vector2Int position)
    {
        // ManaWater_Source�� �����մϴ�.
        var source = new ManaWater_Source(position);

        // �ش� ��ġ�� Mana_Cell�� �ʱ� ������ �����մϴ�.
        manaGrid[position.x, position.y].SetMana(source.maxMana, source.maxMana);

        // ManaFlow_Manager�� ã���ϴ�.
        var manaFlowManager = Object.FindFirstObjectByType<ManaFlow_Manager>();
        if (manaFlowManager != null)
        {
            // manaWaterSources ����Ʈ�� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ�մϴ�.
            if (manaFlowManager.manaWaterSources == null)
            {
                manaFlowManager.manaWaterSources = new List<ManaWater_Source>();
            }

            // ManaWater_Source�� ����Ʈ�� �߰��մϴ�.
            manaFlowManager.manaWaterSources.Add(source);
            Debug.Log($"ManaWater_Source�� ��ġ {position}�� �����Ǿ�����, ManaFlow_Manager�� �ݿ��Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("ManaFlow_Manager�� ã�� �� �����ϴ�. ManaWater_Source�� �ݿ����� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� �׸��带 Tilemap�� �������մϴ�.
    /// </summary>
    public void RenderTileMap()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap�� �Ҵ���� �ʾҽ��ϴ�.");
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
    /// ���� �׸��带 ��ȯ�մϴ�.
    /// </summary>
    public Mana_Cell[,] GetManaGrid() => manaGrid;

    /// <summary>
    /// ���� �׸��带 ��ȯ�մϴ�.
    /// </summary>
    public TerrainCell[,] GetTerrainGrid() => terrainGrid;
}
