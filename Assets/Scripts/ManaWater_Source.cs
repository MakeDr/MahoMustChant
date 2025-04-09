// ManaWater_Source.cs
using UnityEngine;

[System.Serializable]
public class ManaWater_Source
{
    public Vector2Int position;
    public float regenRate = 0.2f; // �ʴ� �����
    public float maxMana = 10f;    // �ش� �������� �ִ� ���� (���� ���ѿ�)

    /// <summary>
    /// ������ �׸��� ���� ������ �����մϴ�. deltaTime�� ��� �ð��� ��Ÿ���ϴ� (���� Time.fixedDeltaTime).
    /// </summary>
    public void Generate(Mana_Cell[,] manaGrid, float deltaTime)
    {
        // �׸��� ���� Ȯ��
        int width = manaGrid.GetLength(0);
        int height = manaGrid.GetLength(1);
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return; // ��ȿ���� ���� ��ġ�� �ߴ�

        var cell = manaGrid[position.x, position.y];

        // ���� ���� ������ �ִ�ġ �̸��� ���� ���
        if (cell.ManaPower < maxMana)
        {
            // Mana_Cell�� AddMana �޼��带 ����Ͽ� �����ϰ� �� �߰� �� Ŭ����
            cell.AddMana(regenRate * deltaTime);
        }

        // ������ �Һи��� ManaWaterHeight ���� ���� ����
        // // cell.manaWaterHeight = Mathf.Min(1f, cell.manaWaterHeight + 0.1f * Time.deltaTime);
        // ���� �� ���(��: �ҽ� Ȱ��ȭ �ð�ȭ)�� �ʿ��ϴٸ�,
        // Mana_Cell�� ���� ����(��: bool IsSourceActive)�� �޼��带 �߰��ϰ� �����ؾ� �մϴ�.
    }
}