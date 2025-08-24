using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[AddComponentMenu("Enemies/Patrol Enemy")]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private bool flipSpriteOnTurn = true;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundMask = 1 << 3; // Ground layer

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 8f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Vector3 _startPosition;
    private Vector3 _leftBound;
    private Vector3 _rightBound;
    private int _direction = 1; // 1 = right, -1 = left
    private float _waitTimer;
    private bool _isWaiting;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
        
        _leftBound = _startPosition + Vector3.left * patrolDistance;
        _rightBound = _startPosition + Vector3.right * patrolDistance;
    }

    private void Start()
    {
        // Setup physics
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.gravityScale = 3f;
    }

    private void FixedUpdate()
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.fixedDeltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                _direction *= -1; // Change direction
                
                if (flipSpriteOnTurn && _sr != null)
                {
                    _sr.flipX = _direction < 0;
                }
            }
            return;
        }

        // Check if at patrol bounds or edge
        bool atLeftBound = transform.position.x <= _leftBound.x;
        bool atRightBound = transform.position.x >= _rightBound.x;
        bool atEdge = !IsGroundAhead();

        if ((atLeftBound && _direction < 0) || (atRightBound && _direction > 0) || atEdge)
        {
            StartWaiting();
            return;
        }

        // Move in current direction
        var velocity = _rb.linearVelocity;
        velocity.x = _direction * moveSpeed;
        _rb.linearVelocity = velocity;
    }

    private void StartWaiting()
    {
        _isWaiting = true;
        _waitTimer = waitTime;
        
        var velocity = _rb.linearVelocity;
        velocity.x = 0f;
        _rb.linearVelocity = velocity;
    }

    private bool IsGroundAhead()
    {
        Vector3 checkPosition = transform.position + Vector3.right * _direction * 0.6f;
        Vector3 rayStart = checkPosition + Vector3.up * 0.1f;
        
        return Physics2D.Raycast(rayStart, Vector3.down, groundCheckDistance, groundMask);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        // Calculate knockback direction
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        Vector2 knockback = new Vector2(direction * knockbackForce, knockbackForce * 0.5f);

        player.ApplyDamage(damage, knockback, 0.15f, 0.75f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? _startPosition : transform.position;
        Vector3 left = start + Vector3.left * patrolDistance;
        Vector3 right = start + Vector3.right * patrolDistance;

        // Draw patrol bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);

        // Draw ground check
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 checkPos = transform.position + Vector3.right * _direction * 0.6f + Vector3.up * 0.1f;
            Gizmos.DrawRay(checkPos, Vector3.down * groundCheckDistance);
        }
    }
#endif
}