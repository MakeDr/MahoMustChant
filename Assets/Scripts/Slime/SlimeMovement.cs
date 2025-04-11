using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 5f;

    public Transform core;
    public Transform membrane;
    public Transform inner;

    private Rigidbody2D rb;
    private Vector3 baseScale;

    // SlimeMovement.cs
    public void Init(SlimeController controller) { /* 필요 시 참조 저장 */ }



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = membrane.localScale;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        AnimateSquashStretch();
    }

    void AnimateSquashStretch()
    {
        float stretchX = 1 + Mathf.Abs(rb.linearVelocity.x) * 0.05f;
        float squashY = 1 - Mathf.Abs(rb.linearVelocity.y) * 0.03f;

        stretchX = Mathf.Clamp(stretchX, 0.8f, 1.3f);
        squashY = Mathf.Clamp(squashY, 0.7f, 1.1f);

        membrane.localScale = new Vector3(stretchX, squashY, 1);
        inner.localScale = new Vector3(stretchX * 0.9f, squashY * 0.9f, 1);
    }
}
