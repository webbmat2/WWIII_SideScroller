using UnityEngine;
using System.Collections;

[AddComponentMenu("Controllers/Player Controller 2D")]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float jumpForce = 12f;

    [Header("Jump Tuning")]
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] bool  variableJump = true;
    [SerializeField, Range(0.1f, 1f)] float jumpCutMultiplier = 0.5f;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;

    [Header("Startup")]
    [SerializeField] bool  snapToGroundOnStart = true;
    [SerializeField] float snapMaxDistance    = 5f;

    [Header("Debug")]
    [SerializeField] bool showDebugOverlay = true;

    [Header("Health")]
    [SerializeField, Min(1)] int maxLives = 3;

    [Header("Damage / I-Frames")]
    [SerializeField, Min(0f)] float invulnSeconds     = 0.5f;
    [SerializeField, Min(0f)] float controlLockSeconds = 0.35f;

    [Header("Damage FX")]
    [SerializeField] Color hurtColor   = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] float flashSeconds = 0.2f;

    float invulnUntil;
    float controlUnlockAt;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;

    float coyoteCounter, jumpBufferCounter;
    int frames; float fpsTimer; float fps;

    Vector3 respawnPoint;
    int lives;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr  = GetComponentInChildren<SpriteRenderer>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    void Start()
    {
        if (snapToGroundOnStart)
        {
            ResolveInitialOverlap();
            SnapToGround();
        }
        respawnPoint = transform.position;
        lives = Mathf.Max(1, maxLives);
    }

    void ResolveInitialOverlap()
    {
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(groundLayer);
        Collider2D[] hits = new Collider2D[1];
        int steps = 0;
        while (col != null && col.Overlap(filter, hits) > 0 && steps++ < 60)
            transform.position += Vector3.up * 0.02f;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        bool controlsLocked = Time.time < controlUnlockAt;

        if (!controlsLocked)
        {
            float x = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);
        }

        if (IsGrounded()) coyoteCounter = coyoteTime; else coyoteCounter -= dt;
        if (!controlsLocked && Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBuffer; else jumpBufferCounter -= dt;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f; coyoteCounter = 0f;
        }

        if (variableJump && Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);

        frames++; fpsTimer += dt; if (fpsTimer >= 0.5f) { fps = frames / fpsTimer; frames = 0; fpsTimer = 0f; }
    }

    void SnapToGround()
    {
        const float skin = 0.01f;
        float halfHeight = col.bounds.extents.y;
        Vector2 origin = new Vector2(transform.position.x, col.bounds.center.y + 10f);
        float rayDistance = Mathf.Max(snapMaxDistance, 0.1f) + 10f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);
        if (hit.collider != null)
        {
            float targetY = hit.point.y + halfHeight + skin;
            var p = transform.position; p.y = targetY; transform.position = p;
        }
    }

    bool IsGrounded()
    {
        Vector2 c = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.02f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.05f);
        return Physics2D.OverlapBox(c, size, 0f, groundLayer) != null;
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.green;
        Vector2 c = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.02f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.05f);
        Gizmos.DrawWireCube(c, size);
    }

    void OnGUI()
    {
        if (!showDebugOverlay) return;
        var rect = new Rect(8, 8, 320, 84);
        string vel = (rb != null) ? $"{rb.velocity.x:F2}, {rb.velocity.y:F2}" : "n/a";
        string text = $"FPS: {fps:0}\nGrounded: {IsGrounded()}\nVel: {vel}\nLives: {lives}/{Mathf.Max(1, maxLives)}";
        GUI.Box(rect, GUIContent.none);
        GUI.Label(new Rect(rect.x + 6, rect.y + 4, rect.width - 12, rect.height - 8), text);
    }

    public void SetRespawnPoint(Vector3 position) => respawnPoint = position;

    public void Respawn()
    {
        transform.position = respawnPoint;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    IEnumerator Flash()
    {
        if (sr == null) yield break;
        var original = sr.color;
        sr.color = hurtColor;
        yield return new WaitForSeconds(flashSeconds);
        sr.color = original;
    }

    public void ApplyKnockback(Vector2 kb)
    {
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.AddForce(kb, ForceMode2D.Impulse);
        controlUnlockAt = Time.time + controlLockSeconds;
    }

    public void ApplyDamage(Vector2 knockback)
    {
        ApplyKnockback(knockback);
        if (Time.time < invulnUntil) return;
        if (lives > 1)
        {
            lives--;
            if (gameObject.activeInHierarchy && sr != null)
                StartCoroutine(Flash());
            invulnUntil = Time.time + invulnSeconds;
        }
        else
        {
            lives = Mathf.Max(1, maxLives);
            Respawn();
            invulnUntil = Time.time + 0.1f;
        }
    }
}