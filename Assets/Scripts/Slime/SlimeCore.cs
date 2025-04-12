using UnityEngine;

public class SlimeCore : MonoBehaviour
{
    [Header("Core Settings")]
    public float mana = 10f;

    [Header("GPU Sync")]
    public Vector2 corePosition;  // GPU 전달용
    public float coreRotation;    // GPU 전달용
    public float coreMana;        // GPU 전달용

    private Rigidbody2D rb;
    private SlimeInstance slimeInstance;  // SlimeInstance 참조 추가

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeInstance = GetComponentInParent<SlimeInstance>();  // 부모에서 가져오기

        if (slimeInstance == null)
        {
            Debug.LogWarning("SlimeInstance not found in parent hierarchy.");
        }
    }


    void FixedUpdate()
    {
        // 위치 및 회전 업데이트
        corePosition = rb.position;
        coreRotation = rb.rotation;
        coreMana = mana;

        // slimeInstance가 null이 아닌 경우에만 데이터 전송
        if (slimeInstance != null)
        {
            slimeInstance.SyncCoreData(corePosition, coreRotation, coreMana);
        }
        else
        {
            Debug.LogWarning("SlimeInstance is null, data synchronization skipped.");
        }
    }
}
