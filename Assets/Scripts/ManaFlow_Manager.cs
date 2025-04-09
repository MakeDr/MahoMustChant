using UnityEngine;

public class ManaFlow_Manager : MonoBehaviour
{
    public int width = 20;
    public int height = 20;

    private Mana_Cell[,] manaGrid;

    void Awake()
    {
        InitializeGrid();
    }

    void Update()
    {
        SimulateManaFlow();
    }

    void InitializeGrid()
    {
        manaGrid = new Mana_Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                manaGrid[x, y] = new Mana_Cell(new Vector2Int(x, y));
            }
        }

        manaGrid[width / 2, height / 2].manaPower = 10f;
        manaGrid[width / 2, height / 2].flowDirection = Vector2.right;
    }

    void SimulateManaFlow()
    {
        float flowRate = 0.1f;

        Mana_Cell[,] nextState = new Mana_Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextState[x, y] = new Mana_Cell(new Vector2Int(x, y));
                nextState[x, y].manaPower = manaGrid[x, y].manaPower;
                nextState[x, y].flowDirection = manaGrid[x, y].flowDirection;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Mana_Cell cell = manaGrid[x, y];
                if (cell.manaPower > 0 && cell.flowDirection != Vector2.zero)
                {
                    Vector2 targetPos = new Vector2(x, y) + cell.flowDirection;
                    int tx = Mathf.FloorToInt(targetPos.x);
                    int ty = Mathf.FloorToInt(targetPos.y);

                    if (tx >= 0 && tx < width && ty >= 0 && ty < height)
                    {
                        float flowAmount = cell.manaPower * flowRate;
                        nextState[x, y].manaPower -= flowAmount;
                        nextState[tx, ty].manaPower += flowAmount;
                    }
                }
            }
        }

        manaGrid = nextState;
    }

    void OnDrawGizmos()
    {
        if (manaGrid == null) return;

        Vector3 origin = new Vector3(-width / 2f, -height / 2f, 0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Mana_Cell cell = manaGrid[x, y];
                Vector3 pos = origin + new Vector3(x, y, 0);

                float intensity = Mathf.Clamp01(cell.manaPower / 10f);
                Gizmos.color = new Color(intensity, 0f, 1f - intensity, 0.5f);
                Gizmos.DrawCube(pos, Vector3.one * 0.9f);

                if (cell.flowDirection != Vector2.zero)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(pos, pos + (Vector3)cell.flowDirection * 0.5f);
                }
            }
        }
    }
}
