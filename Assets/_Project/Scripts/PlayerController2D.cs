// Assets/_Project/Scripts/PlayerController2D.cs
using UnityEngine;

[AddComponentMenu("Controllers/Player Controller 2D")]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed = 8f; public float jumpForce = 12f; public float jumpCutMultiplier = 0.5f;
    [Header("Grounding")] public LayerMask groundLayer; public float coyoteTime = 0.12f; public float jumpBuffer = 0.12f;
    [Header("Debug")] public bool showDebug = true;

    Rigidbody2D rb; BoxCollider2D col; SpriteRenderer sr;
    float coyoteUntil; float jumpBufferedUntil; bool grounded; Vector3 respawnPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        respawnPoint = transform.position;
#if UNITY_6000_0_OR_NEWER
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
#endif
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
#if UNITY_6000_0_OR_NEWER
        var v = rb.linearVelocity; v.x = x * moveSpeed; rb.linearVelocity = v;
#else
        var v = rb.velocity; v.x = x * moveSpeed; rb.velocity = v;
#endif
        grounded = IsGrounded(); if (grounded) coyoteUntil = Time.time + coyoteTime;

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            jumpBufferedUntil = Time.time + jumpBuffer;

        if (jumpBufferedUntil > Time.time && coyoteUntil > Time.time)
        { DoJump(); jumpBufferedUntil = 0f; }

        bool jumpReleased = Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space);
#if UNITY_6000_0_OR_NEWER
        if (jumpReleased && rb.linearVelocity.y > 0f) { var vv = rb.linearVelocity; vv.y *= jumpCutMultiplier; rb.linearVelocity = vv; }
#else
        if (jumpReleased && rb.velocity.y > 0f) { var vv = rb.velocity; vv.y *= jumpCutMultiplier; rb.velocity = vv; }
#endif
    }

    void DoJump()
    {
#if UNITY_6000_0_OR_NEWER
        var v = rb.linearVelocity; v.y = jumpForce; rb.linearVelocity = v;
#else
        var v = rb.velocity; v.y = jumpForce; rb.velocity = v;
#endif
    }

    bool IsGrounded()
    {
        var b = col.bounds; float extra = 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(b.center, b.size - new Vector3(0.02f, 0.02f, 0f), 0f, Vector2.down, extra, groundLayer);
        return hit.collider != null;
    }

    public void SetRespawnPoint(Vector3 p) { respawnPoint = p; }
    public void Respawn() { transform.position = respawnPoint; }

    public void ApplyDamage(Vector2 knockback)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector2.zero; rb.AddForce(knockback, ForceMode2D.Impulse);
#else
        rb.velocity = Vector2.zero; rb.AddForce(knockback, ForceMode2D.Impulse);
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<BoxCollider2D>(); if (!col) return;
        var b = col.bounds; float extra = 0.05f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center + Vector3.down * extra, b.size - new Vector3(0.02f, 0.02f, 0f));
    }
    void OnGUI() { if (!showDebug) return; GUI.Label(new Rect(8, 24, 200, 20), $"Grounded: {grounded}"); }
#endif
}