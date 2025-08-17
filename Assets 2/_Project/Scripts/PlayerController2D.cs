using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float jumpForce = 12f;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform feet;  // optional child transform for precise checks

    Rigidbody2D rb;
    BoxCollider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    bool IsGrounded()
    {
        Vector2 checkCenter = feet ? (Vector2)feet.position
                                   : new Vector2(col.bounds.center.x, col.bounds.min.y - 0.02f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.05f);
        return Physics2D.OverlapBox(checkCenter, size, 0f, groundLayer) != null;
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        Vector2 checkCenter = feet ? (Vector2)feet.position
                                   : new Vector2(col.bounds.center.x, col.bounds.min.y - 0.02f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(checkCenter, size);
    }
}
