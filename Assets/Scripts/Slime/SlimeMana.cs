using UnityEngine;

public class SlimeMana : MonoBehaviour
{
    public float current = 5f;
    public float max = 10f;

    // SlimeMana.cs
    public void Init(SlimeController controller) { /* �ʿ� �� ���� ���� */ }


    public void Gain(float amount)
    {
        current = Mathf.Min(current + amount, max);
    }

    public void Consume(float amount)
    {
        current = Mathf.Max(current - amount, 0f);
    }

    public float GetRatio()
    {
        return current / max;
    }
}
