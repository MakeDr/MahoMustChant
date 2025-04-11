// WorldTile.cs
using UnityEngine;

/// <summary>
/// ���� �׸����� ���� Ÿ�� ������ �����ϰ� �����ϴ� Ŭ����.
/// </summary>
public class WorldTile
{
    [Tooltip("Ÿ���� �׸��� ��ǥ (���� Ÿ�ϸ� ����)")]
    public Vector2Int GridCoords { get; private set; }

    [Tooltip("Ÿ���� ���� Ÿ��")]
    public TerrainType Terrain { get; private set; }

    [Tooltip("�� Ÿ���� ����Ͽ� �̵��� �� �ִ��� ����")]
    public bool IsWalkable { get; private set; }

    [Tooltip("�� Ÿ���� ���� ������ Ȯ��� �� ������ ���� (Diffusivity 0�� ���� ȿ��)")]
    public bool IsManaBlocked { get; private set; }
    
    [Tooltip("���� ���� �ڰ�")]
    public bool IsManaSource => Terrain == TerrainType.ManaWater;


    [Tooltip("Ÿ���� ���� ���� ���� ����")]
    public ManaState Mana { get; private set; }

    [Tooltip("Ÿ���� ���� Ȯ�꼺 (0: Ȯ�� �ȵ�, 1: �⺻(�����), 1<: �� �� Ȯ��). ���� Ÿ�Կ� ���� �ڵ� ������.")]
    public float Diffusivity { get; private set; }

    /// <summary> WorldTile ������ </summary>
    public WorldTile(Vector2Int gridCoords, TerrainType terrain)
    {
        GridCoords = gridCoords;
        Terrain = terrain;
        Mana = new ManaState(); // ManaState �ʱ�ȭ

        IsWalkable = terrain == TerrainType.Ground || terrain == TerrainType.ManaWater;
        IsManaBlocked = terrain == TerrainType.Wall;

        // ���� Ÿ�Կ� ���� Ȯ�꼺(Diffusivity) ���� (����� ��û �� �ݿ�, Wall=0f)
        switch (terrain)
        {
            case TerrainType.ManaWater: Diffusivity = 2f; break;
            case TerrainType.Ground: Diffusivity = 0.01f; break;
            case TerrainType.Wall: Diffusivity = 1.0f; break; // ���� Ȯ�꼺 0
            case TerrainType.Empty: Diffusivity = 1.2f; break;
            default: Diffusivity = 0.05f; break; // ��Ÿ �⺻��
        }
        if (Diffusivity < 0) Diffusivity = 0f; // Ȥ�� �� ���� �� ����
    }
}

