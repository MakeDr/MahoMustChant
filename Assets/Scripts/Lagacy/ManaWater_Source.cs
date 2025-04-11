// ManaWater_Source.cs
using UnityEngine;

/// <summary>
/// 마나를 생성하는 소스 역할을 하는 클래스입니다.
/// </summary>
[System.Serializable]
public class ManaWater_Source
{
    public Vector2Int position; // 마나 소스의 위치
    public float regenRate; // 마나 생성 속도
    public float maxMana; // 마나 최대 적층량

    // 생성자 추가
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
            Debug.LogWarning("마나 소스 위치가 유효하지 않습니다.");
            return;
        }

        // 현재 셀의 마나를 가져옵니다.
        Mana_Cell cell = manaGrid[position.x, position.y];

        // 최대 적층량을 고려하여 마나를 추가합니다.
        cell.AddMana(regenRate * deltaTime, maxMana);
    }
}
