using UnityEngine;

public class SlimeCore : MonoBehaviour
{
    [Header("Core Settings")]
    public float mana = 10f;

    [Header("GPU Sync")]
    public Vector2 corePosition;  // GPU ���޿�
    public float coreRotation;    // GPU ���޿�
    public float coreMana;        // GPU ���޿�

    private Rigidbody2D rb;
    private SlimeInstance slimeInstance;  // SlimeInstance ���� �߰�

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeInstance = GetComponentInParent<SlimeInstance>();  // �θ𿡼� ��������

        if (slimeInstance == null)
        {
            Debug.LogWarning("SlimeInstance not found in parent hierarchy.");
        }
    }


    void FixedUpdate()
    {
        // ��ġ �� ȸ�� ������Ʈ
        corePosition = rb.position;
        coreRotation = rb.rotation;
        coreMana = mana;

        // slimeInstance�� null�� �ƴ� ��쿡�� ������ ����
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
