using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlimeCore : MonoBehaviour
{
    [Header("Core Settings")]
    public float mana = 100f;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Collision Settings")]
    public LayerMask groundLayer; // Inspector���� Ground ���̾� ����

    [Header("Collision State (Readonly Debug)")]
    [SerializeField] private bool isGrounded;
    // Wall/Ceiling ������ OnCollisionStay2D ����� ������ �� ������ �� ����
    [SerializeField] private bool isTouchingWallLeft;
    [SerializeField] private bool isTouchingWallRight;
    [SerializeField] private bool isTouchingCeiling;
    [SerializeField] private Vector2 wallNormal;
    [SerializeField] private Vector2 groundNormal; // �ʿ��ϴٸ� Enter/Stay���� ������Ʈ

    // --- Internals ---
    private Rigidbody2D rb;
    private SlimeInstance slimeInstance;
    private int groundContactCount = 0; // ���� ���� ���� �ݶ��̴� ���� ī��Ʈ

    // Collision detection thresholds (OnCollisionStay��)
    private const float GROUND_NORMAL_Y_MIN = 0.7f;
    private const float WALL_NORMAL_X_MAX_ABS = 0.7f;
    private bool _isTouchingWallLeftThisFrame = false;
    private bool _isTouchingWallRightThisFrame = false;
    private bool _isTouchingCeilingThisFrame = false;
    private Vector2 _accumulatedWallNormal = Vector2.zero;
    private int _wallContactsCount = 0;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeInstance = GetComponentInParent<SlimeInstance>();

        if (slimeInstance == null) { Debug.LogWarning("SlimeInstance not found in parent.", this); }
        if (groundLayer == 0) { Debug.LogWarning("GroundLayer is not set in SlimeCore Inspector.", this); }
    }

    void Update()
    {
        // --- Input Reading ---
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            // ���� ���� isGrounded�� false�� �� �ʿ� ����, Exit���� ó���� ����
        }
    }

    void FixedUpdate()
    {
        // --- ��/õ�� ���� ������Ʈ (OnCollisionStay ���) ---
        // ��/õ�� ������ Stay�� �� ���� �� ���� (�������� Enter/Exit���� �������� ���� �ľ�)
        isTouchingWallLeft = _isTouchingWallLeftThisFrame;
        isTouchingWallRight = _isTouchingWallRightThisFrame;
        isTouchingCeiling = _isTouchingCeilingThisFrame;
        if (_wallContactsCount > 0) { wallNormal = (_accumulatedWallNormal / _wallContactsCount).normalized; }
        else { wallNormal = Vector2.zero; }


        // --- isGrounded ���� ������Ʈ (Enter/Exit ���) ---
        // groundContactCount�� 0���� ũ�� ���� ����ִ� ������ ����
        isGrounded = groundContactCount > 0;

        // groundNormal�� Enter/Stay���� ������ ���� �����ϰų�, �ʿ�� ���⼭ ����
        if (!isGrounded) groundNormal = Vector2.zero;


        // --- Prepare Data & Sync ---
        float rotationRadians = rb.rotation * Mathf.Deg2Rad;
        uint collisionFlags = 0;
        if (isGrounded) collisionFlags |= (1u << 0);
        if (isTouchingWallLeft) collisionFlags |= (1u << 1);
        if (isTouchingWallRight) collisionFlags |= (1u << 2);
        if (isTouchingCeiling) collisionFlags |= (1u << 3);
        // relevantNormal�� ��Ȳ�� �°� ���� (Ground �켱, ������ Wall)
        Vector2 relevantNormal = isGrounded ? groundNormal : (isTouchingWallLeft || isTouchingWallRight ? wallNormal : Vector2.zero);

        if (slimeInstance != null)
        {
            slimeInstance.SyncCoreData(rb.position, rotationRadians, mana, collisionFlags, relevantNormal);
        }

        // --- Reset Per-Frame Wall/Ceiling Detection Flags ---
        _isTouchingWallLeftThisFrame = false;
        _isTouchingWallRightThisFrame = false;
        _isTouchingCeilingThisFrame = false;
        _accumulatedWallNormal = Vector2.zero;
        _wallContactsCount = 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // �浹�� ������Ʈ�� ���̾ groundLayer�� ���ϴ��� Ȯ��
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            groundContactCount++; // �� ���� ī��Ʈ ����
            // �ʿ��, ���⼭ �浹 ���� ������ ����Ͽ� groundNormal ������Ʈ ����
            // groundNormal = CalculateAverageNormal(collision); // ����
        }
        // ��/õ�� Enter ���� ������ �߰� ���� (���� ����)
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // �浹�� ���� ������Ʈ�� ���̾ groundLayer�� ���ϴ��� Ȯ��
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            groundContactCount--; // �� ���� ī��Ʈ ����
            // Clamp count to non-negative values just in case
            if (groundContactCount < 0) groundContactCount = 0;
        }
        // ��/õ�� Exit ���� ������ �߰� ���� (���� ����)
    }

    // OnCollisionStay�� ��/õ�� ������ ���� ������ ������ �� ����
    void OnCollisionStay2D(Collision2D collision)
    {
        // --- ��/õ�� ���� ������ ���� ---
        // (ground ������ Enter/Exit���� ó��)

        // Iterate through contact points to check for wall/ceiling normals
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            Vector2 normal = contact.normal;

            // Check for Ceiling (mostly pointing down)
            if (normal.y < -GROUND_NORMAL_Y_MIN)
            {
                _isTouchingCeilingThisFrame = true;
            }
            // Check for Walls (mostly horizontal)
            else if (Mathf.Abs(normal.x) > WALL_NORMAL_X_MAX_ABS && normal.y <= GROUND_NORMAL_Y_MIN) // Avoid counting ground as wall
            {
                _accumulatedWallNormal += normal;
                _wallContactsCount++;
                if (normal.x > 0) _isTouchingWallLeftThisFrame = true;
                else if (normal.x < 0) _isTouchingWallRightThisFrame = true;
            }
            // Option: Update groundNormal here too if more precision is needed than just Enter
            else if (normal.y > GROUND_NORMAL_Y_MIN)
            {
                // Could potentially update groundNormal here using Stay contacts if needed
            }
        }
    }

    // Helper function to calculate average normal (optional)
    Vector2 CalculateAverageNormal(Collision2D collision)
    {
        Vector2 avgNormal = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            avgNormal += collision.GetContact(i).normal;
        }
        return (collision.contactCount > 0) ? (avgNormal / collision.contactCount).normalized : Vector2.zero;
    }
}