using UnityEngine;

public class Mana_Cell
{
    public Vector2Int position;
    public float manaPower;
    public Vector2 flowDirection;
    
    public Mana_Cell(Vector2Int pos)
    {
        position = pos;
        manaPower = 0f;
        flowDirection = Vector2.zero;
    }
}
