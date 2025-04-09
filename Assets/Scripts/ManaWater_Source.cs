// ManaWater_Source.cs
using UnityEngine;

[System.Serializable]
public class ManaWater_Source
{
    public Vector2Int position;
    public float regenRate = 0.2f; // 초당 재생률
    public float maxMana = 10f;    // 해당 셀에서의 최대 마나 (생성 제한용)

    /// <summary>
    /// 지정된 그리드 셀에 마나를 생성합니다. deltaTime은 경과 시간을 나타냅니다 (보통 Time.fixedDeltaTime).
    /// </summary>
    public void Generate(Mana_Cell[,] manaGrid, float deltaTime)
    {
        // 그리드 범위 확인
        int width = manaGrid.GetLength(0);
        int height = manaGrid.GetLength(1);
        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            return; // 유효하지 않은 위치면 중단

        var cell = manaGrid[position.x, position.y];

        // 셀의 현재 마나가 최대치 미만일 때만 재생
        if (cell.ManaPower < maxMana)
        {
            // Mana_Cell의 AddMana 메서드를 사용하여 안전하게 값 추가 및 클램핑
            cell.AddMana(regenRate * deltaTime);
        }

        // 역할이 불분명한 ManaWaterHeight 관련 로직 제거
        // // cell.manaWaterHeight = Mathf.Min(1f, cell.manaWaterHeight + 0.1f * Time.deltaTime);
        // 만약 이 기능(예: 소스 활성화 시각화)이 필요하다면,
        // Mana_Cell에 관련 상태(예: bool IsSourceActive)나 메서드를 추가하고 관리해야 합니다.
    }
}