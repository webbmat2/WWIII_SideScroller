using UnityEngine;

[AddComponentMenu("Controllers/Player Controller 2D")]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField, Min(0f)] private float groundCheckInset = 0.02f;   // shrink bounds
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.05f; // ray distance
    [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
    [SerializeField, Min(0f)] private float jumpBuffer = 0.1f;

    [Header("Respawn")]
    [SerializeField] private Transform initialRespawnPoint;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Vector3 _respawn;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _grounded;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.freezeRotation = true;
        if (initialRespawnPoint != null) _respawn = initialRespawnPoint.position;
        else _respawn = transform.position;
    }

    void Update()
    {
        // input
        float x = Input.GetAxisRaw("Horizontal");

        // timers
        _grounded = IsGrounded();
        if (_grounded) _coyoteTimer = coyoteTime;
        else _coyoteTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            _jumpBufferTimer = jumpBuffer;
        else
            _jumpBufferTimer -= Time.deltaTime;

        // horizontal control
        var v = GetVel();
        v.x = x * moveSpeed;

        // jump
        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            v.y = jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }

        SetVel(v);
    }

    public bool IsGrounded()
    {
        Bounds b = _col.bounds;
        b.Expand(new Vector3(-groundCheckInset * 2f, -groundCheckInset * 2f, 0f));
        Vector2 origin = new Vector2(b.center.x, b.min.y);
        float dist = groundCheckDistance;
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(b.size.x, groundCheckInset * 2f), 0f, Vector2.down, dist, groundMask);
        return hit.collider != null;
    }

    public void SetRespawnPoint(Vector3 p) => _respawn = p;

    public void Respawn()
    {
        transform.position = _respawn;
        var v = GetVel();
        v.y = 0f;
        SetVel(v);
    }

    public void ApplyDamage(Vector2 knockback)
    {
        var v = GetVel();
        v = knockback;
        SetVel(v);
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (!col) return;
        Bounds b = col.bounds;
        b.Expand(new Vector3(-groundCheckInset * 2f, -groundCheckInset * 2f, 0f));
        Vector2 origin = new Vector2(b.center.x, b.min.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(origin.x, origin.y - groundCheckDistance, transform.position.z),
            new Vector3(b.size.x, groundCheckInset * 2f, 0f));
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 220, 20), "Grounded: " + (_grounded ? "True" : "False"));
    }

    // Unity 6 vs older velocity API compatibility
    Vector2 GetVel()
    {
    #if UNITY_6000_0_OR_NEWER
        return _rb.linearVelocity;
    #else
        return _rb.velocity;
    #endif
    }

    void SetVel(Vector2 v)
    {
    #if UNITY_6000_0_OR_NEWER
        _rb.linearVelocity = v;
    #else
        _rb.velocity = v;
    #endif
    }
}