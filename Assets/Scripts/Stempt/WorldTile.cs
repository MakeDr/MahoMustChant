using UnityEngine;

/// <summary>
/// ���� Ÿ���� ������ �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class WorldTile
{
    public Vector2Int Position;     // Ÿ���� �׸��� ��ǥ
    public TerrainType Terrain;     // Ÿ���� ���� Ÿ��
    public bool IsWalkable;         // Ÿ���� �̵� �������� ����
    public bool IsManaBlocked;      // ���� Ȯ���� ���ܵǴ��� ����
    public ManaProperties Mana;     // Ÿ���� ���� �Ӽ�

    /// <summary>
    /// WorldTile ������
    /// </summary>
    /// <param name="position">Ÿ���� �׸��� ��ǥ</param>
    /// <param name="terrain">Ÿ���� ���� Ÿ��</param>
    public WorldTile(Vector2Int position, TerrainType terrain)
    {
        Position = position;
        Terrain = terrain;
        IsWalkable = terrain == TerrainType.Ground || terrain == TerrainType.ManaWater;
        IsManaBlocked = terrain == TerrainType.Wall;
        Mana = new ManaProperties();
    }
}
