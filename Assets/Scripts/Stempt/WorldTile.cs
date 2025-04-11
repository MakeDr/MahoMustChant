using UnityEngine;

/// <summary>
/// 개별 타일의 정보를 저장하는 클래스입니다.
/// </summary>
public class WorldTile
{
    public Vector2Int Position;     // 타일의 그리드 좌표
    public TerrainType Terrain;     // 타일의 지형 타입
    public bool IsWalkable;         // 타일이 이동 가능한지 여부
    public bool IsManaBlocked;      // 마나 확산이 차단되는지 여부
    public ManaProperties Mana;     // 타일의 마나 속성

    /// <summary>
    /// WorldTile 생성자
    /// </summary>
    /// <param name="position">타일의 그리드 좌표</param>
    /// <param name="terrain">타일의 지형 타입</param>
    public WorldTile(Vector2Int position, TerrainType terrain)
    {
        Position = position;
        Terrain = terrain;
        IsWalkable = terrain == TerrainType.Ground || terrain == TerrainType.ManaWater;
        IsManaBlocked = terrain == TerrainType.Wall;
        Mana = new ManaProperties();
    }
}
