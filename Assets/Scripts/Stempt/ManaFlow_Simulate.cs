using UnityEngine;

/// <summary>
/// ���� Ȯ�� �ùķ��̼�: ���� ����ġ ��� Ȯ��
/// </summary>
public class ManaFlow_Simulate : MonoBehaviour
{
    public WorldGridManager gridManager;

    [Header("Ȯ�� ����")]
    [Range(0f, 0.125f)] public float diffusionRate = 0.03f;

    [Tooltip("���⺰ Ȯ�� ����ġ (��: �߷� ����)")]
    public float weightRight = 1.0f;
    public float weightLeft = 1.0f;
    public float weightUp = 1.0f;
    public float weightDown = 1.0f;

    private Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    /// <summary>
    /// Ȯ�� ��� (SimulationTickManager�� ���� ȣ���)
    /// </summary>
    public void CalculateDiffusionChanges()
    {
        if (gridManager == null || gridManager.WorldGrid == null) return;
        var grid = gridManager.WorldGrid;
        var size = gridManager.GridSize;

        float[] dirWeights = new float[] { weightRight, weightLeft, weightUp, weightDown };

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var fromTile = grid[x, y];
                if (fromTile?.Mana == null || fromTile.Diffusivity <= 0f) continue;

                float fromMana = fromTile.Mana.CurrentMana;
                if (fromMana <= 0f) continue;

                float[] flows = new float[directions.Length];
                float totalOutflow = 0f;

                for (int i = 0; i < directions.Length; i++)
                {
                    int nx = x + directions[i].x;
                    int ny = y + directions[i].y;

                    if (nx < 0 || ny < 0 || nx >= size.x || ny >= size.y) continue;

                    var toTile = grid[nx, ny];
                    if (toTile?.Mana == null || toTile.Diffusivity <= 0f) continue;

                    float toMana = toTile.Mana.CurrentMana;
                    float diff = fromMana - toMana;
                    if (diff <= 0f) continue;

                    float harmonicK = 2f * fromTile.Diffusivity * toTile.Diffusivity /
                                      (fromTile.Diffusivity + toTile.Diffusivity);

                    float flow = diff * diffusionRate * harmonicK * dirWeights[i];
                    flows[i] = flow;
                    totalOutflow += flow;
                }

                if (totalOutflow < 0.0001f) continue;

                float available = Mathf.Min(totalOutflow, fromMana);
                float scale = available / totalOutflow;

                for (int i = 0; i < directions.Length; i++)
                {
                    if (flows[i] <= 0f) continue;

                    int nx = x + directions[i].x;
                    int ny = y + directions[i].y;
                    var toTile = grid[nx, ny];

                    float actual = flows[i] * scale;
                    fromTile.Mana.AddPendingChange(-actual);
                    toTile.Mana.AddPendingChange(+actual);
                }
            }
        }
    }
}
