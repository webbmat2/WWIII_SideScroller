using UnityEngine;

[AddComponentMenu("Controllers/Player Controller 2D")]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed = 8f; 
    public float jumpForce = 12f;

    [Header("Jump Tuning")] public float coyoteTime = 0.12f; 
    public float jumpBuffer = 0.12f; 
    public bool variableJump = true; 
    [Range(0.1f,1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Grounding")] public LayerMask groundLayer;

    [Header("Damage/Health")] public int maxLives = 3; 
    public float invulnSeconds = 0.6f; 
    public float controlLockSeconds = 0.25f;

    [Header("Debug")] public bool showDebugOverlay = false;

    [Header("Visuals")]
    [SerializeField] SpriteRenderer body;
    [SerializeField] float hitFlashSeconds = 0.12f;

    Rigidbody2D rb; BoxCollider2D col;
    float coyoteCounter, jumpBufferCounter;
    int lives; float invulnUntil; float controlUnlockAt;
    Vector3 respawnPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        if (body == null) body = GetComponent<SpriteRenderer>();
        lives = Mathf.Max(1, maxLives);
        respawnPoint = transform.position;
    }

    void Update()
    {
        // lock input briefly after damage
        bool locked = Time.time < controlUnlockAt;

        if (!locked)
        {
            float x = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
        }

        // timers
        if (IsGrounded()) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime;
        if (!locked && Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBuffer; else jumpBufferCounter -= Time.deltaTime;

        // jump
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f; coyoteCounter = 0f;
        }

        // variable jump
        if (variableJump && Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.95f, 0.05f);
        Vector2 center = new Vector2(b.center.x, b.min.y + size.y * 0.5f);
        return Physics2D.OverlapBox(center, size, 0f, groundLayer) != null;
    }

    // API used by other scripts
    public void SetRespawnPoint(Vector3 position) { respawnPoint = position; }

    public void Respawn()
    {
        transform.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyDamage(Vector2 knockback)
    {
        if (Time.time < invulnUntil) return;
        if (lives > 1)
        {
            lives--;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
            StartCoroutine(HitFlash());
            invulnUntil = Time.time + invulnSeconds;
            controlUnlockAt = Time.time + controlLockSeconds;
            Debug.Log($"Damage taken. Lives remaining: {lives}");
        }
        else
        {
            Debug.Log("No lives remaining. Respawning.");
            lives = Mathf.Max(1, maxLives);
            Respawn();
            StartCoroutine(HitFlash());
            invulnUntil = Time.time + 0.1f;
        }
    }

    System.Collections.IEnumerator HitFlash()
    {
        if (body == null) yield break;
        var original = body.color;
        body.color = Color.red;
        yield return new WaitForSeconds(hitFlashSeconds);
        body.color = original;
    }

    void OnGUI()
    {
        if (!showDebugOverlay) return;
        int fps = (int)(1f / Time.unscaledDeltaTime);
        var rect = new Rect(8, 8, 320, 84);
        string vel = (rb != null) ? $"{rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2}" : "n/a";
        string text = $"FPS: {fps:0}\nGrounded: {IsGrounded()}\nVel: {vel}\nLives: {lives}/{Mathf.Max(1, maxLives)}";
        GUI.Box(rect, text);
    }
}
