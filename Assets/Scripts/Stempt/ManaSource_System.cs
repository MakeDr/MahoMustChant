using UnityEngine;

public class ManaSource_System : MonoBehaviour
{
    public WorldGridManager gridManager;

    [Header("마나 생성 설정")]
    public float baseGenerationRate = 10f;
    public float decayStartThreshold = 10f;
    [Min(0.01f)] public float decaySharpness = 2f;

    public void CalculateGenerationChanges()
    {
        if (gridManager == null || gridManager.WorldGrid == null) return;

        var grid = gridManager.WorldGrid;
        var size = gridManager.GridSize;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var tile = grid[x, y];
                if (tile?.Mana == null || !tile.IsManaSource) continue;

                float currentMana = tile.Mana.CurrentMana;
                float decayFactor = CalculateGenerationFactor(currentMana);
                float manaToGenerate = baseGenerationRate * decayFactor;

                if (manaToGenerate > 0.0001f)
                {
                    tile.Mana.AddPendingChange(manaToGenerate);
                }
            }
        }
    }

    private float CalculateGenerationFactor(float currentMana)
    {
        if (decayStartThreshold <= 0f) return 1f;

        float ratio = Mathf.Max(0, currentMana / decayStartThreshold);
        float factor = 1f / (1f + Mathf.Pow(ratio, decaySharpness));
        return Mathf.Clamp01(factor);
    }
}
