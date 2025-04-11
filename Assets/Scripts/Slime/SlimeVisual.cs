using UnityEngine;

public class SlimeVisual : MonoBehaviour
{
    public Transform innerFill;
    public float maxScale = 1.3f;
    private SlimeMana mana;

    // SlimeVisual.cs
    public void Init(SlimeController controller) { /* 필요 시 참조 저장 */ }
    
    void Start()
    {
        mana = GetComponent<SlimeMana>();
    }

    void Update()
    {
        float ratio = mana.GetRatio();
        float scale = Mathf.Lerp(0.3f, maxScale, ratio);
        innerFill.localScale = new Vector3(scale, scale, 1f);
    }
}
