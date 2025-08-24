using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[AddComponentMenu("Player/Player Controller 2D (AI Fix)")]
public class PlayerController2D : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private bool flipSpriteByScaleX = true;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private int _currentHealth;
    private Vector3 _respawnPoint;
    private Transform _currentPlatform;
    private float _inputX;
    private bool _jumpQueued;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _currentHealth = Mathf.Max(1, maxHealth);
        _respawnPoint = transform.position;
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Horizontal input (axes + fallback keys)
        float axis = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(axis, 0f))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) axis -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) axis += 1f;
        }
        _inputX = Mathf.Clamp(axis, -1f, 1f);

        // Queue jump (space/W/Up or "Jump" button)
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _jumpQueued = true;
        }

        // Flip sprite by localScale.x
        if (flipSpriteByScaleX && _sr != null && Mathf.Abs(_inputX) > 0.01f)
        {
            var s = _sr.transform.localScale;
            s.x = Mathf.Abs(s.x) * (_inputX < 0 ? -1 : 1);
            _sr.transform.localScale = s;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        var v = _rb.linearVelocity;
        // Horizontal movement
        v.x = _inputX * moveSpeed;

        // Jump (only when grounded)
        if (_jumpQueued && IsGrounded())
        {
            if (v.y < 0f) v.y = 0f; // clean takeoff
            v.y = jumpForce;
        }

        _rb.linearVelocity = v;
        _jumpQueued = false; // consume jump
    }

    public void SetRespawnPoint(Vector3 p) { _respawnPoint = p; }

    public void Respawn()
    {
        transform.position = _respawnPoint;
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.position = (Vector2)_respawnPoint;
        }
    }

    public void ApplyDamage(int amount)
    {
        int dmg = Mathf.Max(0, amount);
        _currentHealth -= dmg;
        if (_currentHealth <= 0)
        {
            _currentHealth = Mathf.Max(1, maxHealth);
            Respawn();
        }
    }

    public bool IsGrounded()
    {
        if (_col == null) return false;
        Bounds b = _col.bounds;
        Vector3 size = new Vector3(Mathf.Max(0.01f, b.size.x - 0.02f), Mathf.Max(0.01f, b.size.y - 0.02f), 1f);
        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, groundCheckDistance, groundMask);
        return hit.collider != null;
    }

    public void AttachToPlatform(Transform platform)
    {
        _currentPlatform = platform;
        if (platform != null) transform.SetParent(platform);
    }

    public void DetachFromPlatform()
    {
        if (_currentPlatform != null)
        {
            transform.SetParent(null);
            _currentPlatform = null;
        }
    }

    // Overloads to satisfy callers that pass a platform reference
    public void DetachFromPlatform(Transform platform)
    {
        if (platform == null || platform == _currentPlatform)
        {
            DetachFromPlatform();
        }
    }

    public void DetachFromPlatform(GameObject platformGO)
    {
        DetachFromPlatform(platformGO != null ? platformGO.transform : null);
    }

    private void OnDisable() { DetachFromPlatform(); }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider2D>();
        if (c == null) return;
        Bounds b = c.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center + Vector3.down * groundCheckDistance, b.size);
    }
#endif
}