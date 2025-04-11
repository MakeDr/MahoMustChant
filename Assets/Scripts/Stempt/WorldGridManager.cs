// WorldGridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGridManager : MonoBehaviour
{
    [Header("타일맵 참조")]
    [Tooltip("기준이 되는 지면 타일맵")]
    public Tilemap groundTilemap;
    [Tooltip("벽 정보를 담은 타일맵 (선택 사항)")]
    public Tilemap wallTilemap;
    [Tooltip("마나 물 정보를 담은 타일맵 (선택 사항)")]
    public Tilemap manaWaterTilemap;

    [Header("Gizmo 설정")]
    public bool showGizmos = true;

    private WorldTile[,] _worldGrid; // 내부 그리드 데이터
    private Vector2Int _gridSize;    // 그리드 크기 (타일 수)
    private Vector2Int _gridOrigin;  // 그리드의 월드 좌표 원점 (타일맵의 bottom-left)

    // --- Public Accessors ---
    /// <summary>월드 그리드 데이터 (읽기 전용)</summary>
    public WorldTile[,] WorldGrid => _worldGrid;
    /// <summary>그리드 크기 (타일 개수)</summary>
    public Vector2Int GridSize => _gridSize;
    /// <summary>그리드 원점의 월드 좌표</summary>
    public Vector2Int GridOrigin => _gridOrigin;


    private void Awake()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("WorldGridManager: groundTilemap 참조가 없습니다!");
            return;
        }

        // 기준 타일맵에서 경계 계산
        groundTilemap.CompressBounds(); // 실제 타일 영역만 계산하도록 압축
        BoundsInt bounds = groundTilemap.cellBounds;

        _gridOrigin = new Vector2Int(bounds.xMin, bounds.yMin);
        _gridSize = new Vector2Int(bounds.size.x, bounds.size.y);

        if (_gridSize.x <= 0 || _gridSize.y <= 0)
        {
            Debug.LogError($"WorldGridManager: 유효하지 않은 그리드 크기({_gridSize}). groundTilemap 확인 필요.");
            return;
        }

        Debug.Log($"WorldGridManager: 그리드 초기화 - 크기: {_gridSize}, 원점: {_gridOrigin}");
        _worldGrid = new WorldTile[_gridSize.x, _gridSize.y];

        // 그리드 배열 인덱스 (ix, iy) 와 월드 그리드 좌표 (gridCoords) 구분
        for (int ix = 0; ix < _gridSize.x; ix++) // ix: 내부 배열의 x 인덱스
        {
            for (int iy = 0; iy < _gridSize.y; iy++) // iy: 내부 배열의 y 인덱스
            {
                // 배열 인덱스를 실제 월드 그리드 좌표로 변환
                Vector2Int gridCoords = new Vector2Int(ix + _gridOrigin.x, iy + _gridOrigin.y);

                TerrainType terrain = ResolveTerrainAt(gridCoords);

                // WorldTile 생성 (생성자에서 Diffusivity 등 자동 설정됨)
                _worldGrid[ix, iy] = new WorldTile(gridCoords, terrain);
            }
        }
        Debug.Log("WorldGridManager: 그리드 초기화 완료.");
    }

    private TerrainType ResolveTerrainAt(Vector2Int gridCoords)
    {
        // Vector2Int 좌표를 Tilemap에서 사용하는 Vector3Int로 변환
        Vector3Int cellPosition = new Vector3Int(gridCoords.x, gridCoords.y, 0);

        // 우선순위: 벽 > 마나물 > 지면
        if (wallTilemap != null && wallTilemap.HasTile(cellPosition))
            return TerrainType.Wall;
        if (manaWaterTilemap != null && manaWaterTilemap.HasTile(cellPosition))
            return TerrainType.ManaWater;
        if (groundTilemap.HasTile(cellPosition)) // 기준 타일맵은 null 체크 불필요 (위에서 함)
            return TerrainType.Ground;

        return TerrainType.Empty;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || _worldGrid == null) return;

        // Gizmo 그리기 시에도 배열 인덱스(ix, iy)와 타일 좌표(tile.GridCoords) 구분
        for (int ix = 0; ix < _gridSize.x; ix++)
        {
            for (int iy = 0; iy < _gridSize.y; iy++)
            {
                WorldTile tile = _worldGrid[ix, iy];
                if (tile == null) continue;

                // 타일의 그리드 좌표를 사용하여 월드 중심 위치 얻기
                Vector3 worldCenterPos = groundTilemap != null ?
                    groundTilemap.GetCellCenterWorld((Vector3Int)tile.GridCoords) :
                    new Vector3(tile.GridCoords.x + 0.5f, tile.GridCoords.y + 0.5f, 0); // Fallback

                // 지형 타입 또는 마나 양에 따라 Gizmo 그리기 (이전과 동일)
                Gizmos.color = GetGizmoColor(tile); // 색상 결정 로직 분리 가능
                Vector3 gizmoSize = (groundTilemap != null ? groundTilemap.cellSize : Vector3.one) * 0.9f;
                Gizmos.DrawCube(worldCenterPos, gizmoSize);
            }
        }
    }

    // Gizmo 색상 결정 로직 (예시)
    private Color GetGizmoColor(WorldTile tile)
    {
        // 마나 양에 따라 색상 변경 (우선순위 높게)
        if (tile.Mana != null && tile.Mana.CurrentMana > 0.1f)
        {
            float manaRatio = Mathf.Clamp01(tile.Mana.CurrentMana / tile.Mana.MaxMana); // MaxMana 기준 비율
            // 예: 파란색(0) -> 청록색(0.5) -> 노란색(1)
            if (manaRatio < 0.5f) return Color.Lerp(Color.blue, Color.cyan, manaRatio * 2f);
            else return Color.Lerp(Color.cyan, Color.yellow, (manaRatio - 0.5f) * 2f);
        }

        // 기본 지형 색상
        return tile.Terrain switch
        {
            TerrainType.ManaWater => new Color(0.1f, 0.7f, 1f, 0.4f),
            TerrainType.Ground => new Color(0.4f, 0.8f, 0.2f, 0.4f),
            TerrainType.Wall => new Color(0.3f, 0.3f, 0.3f, 0.6f),
            TerrainType.Empty => new Color(1f, 1f, 1f, 0.1f),
            _ => Color.clear
        };
    }
}