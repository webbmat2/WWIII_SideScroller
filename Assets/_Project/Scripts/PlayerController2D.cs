using UnityEngine;

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
    [SerializeField] bool variableJump = true; 
    [SerializeField, Range(0.1f, 1f)] float jumpCutMultiplier = 0.5f;

    [Header("Grounding")] 
    [SerializeField] LayerMask groundLayer;

    [Header("Startup")] 
    [SerializeField] bool snapToGroundOnStart = true; 
    [SerializeField] float snapMaxDistance = 5f; // how far to search downwards

    [Header("Debug")] 
    [SerializeField] bool showDebugOverlay = true;

    Rigidbody2D rb; 
    BoxCollider2D col; 
    float coyoteCounter, jumpBufferCounter; 
    int frames; float fpsTimer; float fps;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // Safe defaults
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true; // keep the square upright
    }

    void Start()
    {
        if (snapToGroundOnStart)
        {
            ResolveInitialOverlap();
            SnapToGround();
        }
    }

    void ResolveInitialOverlap()
    {
        // If we spawned intersecting Ground, nudge upward until clear
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(groundLayer);
        Collider2D[] hits = new Collider2D[1];
        int steps = 0;
        while (col != null && col.Overlap(filter, hits) > 0 && steps++ < 60)
        {
            transform.position += Vector3.up * 0.02f; // 2 cm per step
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Horizontal move (old Input)
        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // Timers
        if (IsGrounded()) coyoteCounter = coyoteTime; else coyoteCounter -= dt;
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBuffer; else jumpBufferCounter -= dt;

        // Jump
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f; coyoteCounter = 0f;
        }

        // Variable jump cut
        if (variableJump && Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // FPS calc
        frames++; fpsTimer += dt; if (fpsTimer >= 0.5f) { fps = frames / fpsTimer; frames = 0; fpsTimer = 0f; }
    }

    void SnapToGround()
    {
        // Cast from well above the player so we always hit the ground even if starting inside it
        const float skin = 0.01f;
        float halfHeight = col.bounds.extents.y;

        // Start the ray above the player to avoid starting inside colliders
        Vector2 origin = new Vector2(transform.position.x, col.bounds.center.y + 10f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 100f, groundLayer);

        if (hit.collider != null)
        {
            float targetY = hit.point.y + halfHeight + skin;
            Vector3 p = transform.position;
            p.y = targetY;
            transform.position = p;
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
        var rect = new Rect(8, 8, 320, 64);
        string vel = (rb != null) ? $"{rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2}" : "n/a";
        string text = $"FPS: {fps:0}\nGrounded: {IsGrounded()}\nVel: {vel}";
        GUI.Box(rect, GUIContent.none);
        GUI.Label(new Rect(rect.x + 6, rect.y + 4, rect.width - 12, rect.height - 8), text);
    }
}