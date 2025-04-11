using System.Collections;
using UnityEngine;
public class SlimeCollisionResponder : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 originalScale;
    public float collisionSquashIntensity = 0.2f;
    public float squashDuration = 0.2f;
    private bool isSquashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSquashing) StartCoroutine(SquashOnImpact());
    }

    private IEnumerator SquashOnImpact()
    {
        isSquashing = true;
        Vector3 squashedScale = new Vector3(originalScale.x + collisionSquashIntensity, originalScale.y - collisionSquashIntensity, 1);
        transform.localScale = squashedScale;
        yield return new WaitForSeconds(squashDuration);
        transform.localScale = originalScale;
        isSquashing = false;
    }
}
