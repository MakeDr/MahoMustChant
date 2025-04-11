// WorldTile.cs
using UnityEngine;

/// <summary>
/// 월드 그리드의 개별 타일 정보를 저장하고 관리하는 클래스.
/// </summary>
public class WorldTile
{
    [Tooltip("타일의 그리드 좌표 (월드 타일맵 기준)")]
    public Vector2Int GridCoords { get; private set; }

    [Tooltip("타일의 지형 타입")]
    public TerrainType Terrain { get; private set; }

    [Tooltip("이 타일을 통과하여 이동할 수 있는지 여부")]
    public bool IsWalkable { get; private set; }

    [Tooltip("이 타일을 통해 마나가 확산될 수 없는지 여부 (Diffusivity 0과 동일 효과)")]
    public bool IsManaBlocked { get; private set; }
    
    [Tooltip("마나 생성 자격")]
    public bool IsManaSource => Terrain == TerrainType.ManaWater;


    [Tooltip("타일의 마나 관련 상태 정보")]
    public ManaState Mana { get; private set; }

    [Tooltip("타일의 마나 확산성 (0: 확산 안됨, 1: 기본(빈공간), 1<: 더 잘 확산). 지형 타입에 따라 자동 설정됨.")]
    public float Diffusivity { get; private set; }

    /// <summary> WorldTile 생성자 </summary>
    public WorldTile(Vector2Int gridCoords, TerrainType terrain)
    {
        GridCoords = gridCoords;
        Terrain = terrain;
        Mana = new ManaState(); // ManaState 초기화

        IsWalkable = terrain == TerrainType.Ground || terrain == TerrainType.ManaWater;
        IsManaBlocked = terrain == TerrainType.Wall;

        // 지형 타입에 따른 확산성(Diffusivity) 설정 (사용자 요청 값 반영, Wall=0f)
        switch (terrain)
        {
            case TerrainType.ManaWater: Diffusivity = 2f; break;
            case TerrainType.Ground: Diffusivity = 0.01f; break;
            case TerrainType.Wall: Diffusivity = 1.0f; break; // 벽은 확산성 0
            case TerrainType.Empty: Diffusivity = 1.2f; break;
            default: Diffusivity = 0.05f; break; // 기타 기본값
        }
        if (Diffusivity < 0) Diffusivity = 0f; // 혹시 모를 음수 값 방지
    }
}

