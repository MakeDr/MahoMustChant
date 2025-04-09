// ManaWater_Source.cs
using UnityEngine;

/// <summary>
/// ������ �����ϴ� �ҽ� ������ �ϴ� Ŭ�����Դϴ�.
/// </summary>
[System.Serializable]
public class ManaWater_Source
{
    public Vector2Int position; // ���� �ҽ��� ��ġ
    public float regenRate; // ���� ���� �ӵ�
    public float maxMana; // ���� �ִ� ������

    // ������ �߰�
    public ManaWater_Source(Vector2Int position, float regenRate = 1.0f, float maxMana = 15.0f)
    {
        this.position = position;
        this.regenRate = regenRate;
        this.maxMana = maxMana;
    }

    public void Generate(Mana_Cell[,] manaGrid, float deltaTime)
    {
        if (manaGrid == null || position.x < 0 || position.y < 0 || position.x >= manaGrid.GetLength(0) || position.y >= manaGrid.GetLength(1))
        {
            Debug.LogWarning("���� �ҽ� ��ġ�� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        // ���� ���� ������ �����ɴϴ�.
        Mana_Cell cell = manaGrid[position.x, position.y];

        // �ִ� �������� ����Ͽ� ������ �߰��մϴ�.
        cell.AddMana(regenRate * deltaTime, maxMana);
    }
}
