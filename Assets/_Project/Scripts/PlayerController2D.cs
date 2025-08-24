using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[AddComponentMenu("Player/Player Controller 2D (AI Fix)")]
public class PlayerController2D : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Hit Feedback")]
    [SerializeField] private float flashSeconds = 0.25f;
    [SerializeField] private Color flashColor = Color.red;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask = 1 << 3; // Ground layer
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private bool flipSpriteByScaleX = true;

    [Header("Advanced Movement (Optional)")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Input Actions (Optional - leave null to use legacy input)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private int _currentHealth;
    private Vector3 _respawnPoint;
    private Transform _currentPlatform;
    private float _inputX;
    private bool _jumpQueued;
    private SpriteRenderer _sr;

    private bool _invulnerable;
    private float _invulnUntil;
    private Color _baseColor;

    // Advanced movement variables
    private bool _isGrounded;
    private bool _wasGrounded;
    private float _lastGroundedTime;
    private float _jumpBufferTimeLeft;

    // Public properties
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => _invulnerable && Time.time < _invulnUntil;
    public Vector3 RespawnPoint => _respawnPoint;

    // Healing method
    public void Heal(int amount)
    {
        int oldHealth = _currentHealth;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        
        if (_currentHealth > oldHealth)
        {
            Debug.Log($"Player healed {_currentHealth - oldHealth} HP. Current: {_currentHealth}/{maxHealth}");
            
            // Could add healing visual/audio effects here
            if (_sr != null)
            {
                StartCoroutine(HealFlash());
            }
        }
    }

    private System.Collections.IEnumerator HealFlash()
    {
        Color originalColor = _sr.color;
        _sr.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        _sr.color = originalColor;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();

        // Store base color for flash effect
        if (_sr != null) _baseColor = _sr.color;

        // Create GroundCheck if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.SetParent(transform);
            groundCheckGO.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckGO.transform;
        }

        // Ensure physics is active & sane
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.simulated = true;
        _rb.gravityScale = 3.5f; 
        
        // CRITICAL FIX: Only freeze rotation, NOT position
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.WakeUp();
        
        // Debug log to verify constraints are set correctly
        Debug.Log($"Player constraints set to: {_rb.constraints}");

        _currentHealth = Mathf.Max(1, maxHealth);
        _respawnPoint = transform.position;
    }

    private void Start()
    {
        // CRITICAL: Force constraints fix at runtime in case Inspector has wrong values
        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"Start: Forced Player constraints to: {_rb.constraints}");
        }
    }

    private void Update()
    {
        // Update grounding state
        _wasGrounded = _isGrounded;
        _isGrounded = IsGrounded();
        
        if (_isGrounded && !_wasGrounded)
        {
            _lastGroundedTime = Time.time;
            
            // Audio feedback for landing
            AudioFXManager.PlayLandSound();
            
            // Screen shake for heavy landings
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.y < -8f)
            {
                var cameraManager = FindFirstObjectByType<CameraManager>();
                if (cameraManager != null)
                {
                    cameraManager.TriggerScreenShake(0.2f, 0.1f);
                }
            }
        }
        else if (!_isGrounded && _wasGrounded)
        {
            _lastGroundedTime = Time.time;
        }

        // Read input from Input System if actions are assigned, otherwise fallback to legacy
        float axis = 0f;
        bool jumpPressed = false;

        if (moveAction != null && moveAction.action != null)
        {
            axis = moveAction.action.ReadValue<Vector2>().x;
        }
        else
        {
            // Legacy input fallback
            axis = Input.GetAxisRaw("Horizontal");
            if (Mathf.Approximately(axis, 0f)) {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) axis -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) axis += 1f;
            }
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            jumpPressed = jumpAction.action.WasPressedThisFrame();
        }
        else
        {
            // Legacy input fallback
            jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || 
                         Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        _inputX = Mathf.Clamp(axis, -1f, 1f);
        
        // Jump buffering
        if (jumpPressed)
        {
            _jumpBufferTimeLeft = jumpBufferTime;
        }
        else
        {
            _jumpBufferTimeLeft -= Time.deltaTime;
        }

        if (flipSpriteByScaleX && _sr != null && Mathf.Abs(_inputX) > 0.01f) {
            var s = _sr.transform.localScale;
            s.x = Mathf.Abs(s.x) * (_inputX < 0 ? -1 : 1);
            _sr.transform.localScale = s;
        }
    }

    private void FixedUpdate()
    {
        var v = _rb.linearVelocity;
        v.x = _inputX * moveSpeed;

        // Enhanced jump logic with coyote time and jump buffering
        bool canJump = _isGrounded || (Time.time - _lastGroundedTime) <= coyoteTime;
        bool wantsToJump = _jumpBufferTimeLeft > 0f;

        if (wantsToJump && canJump) {
            if (v.y < 0f) v.y = 0f;
            v.y = jumpForce;
            _jumpBufferTimeLeft = 0f; // Consume the jump buffer
            
            // Audio feedback for jump
            AudioFXManager.PlayJumpSound();
        }

        _rb.linearVelocity = v;
    }

    public void SetRespawnPoint(Vector3 p) { _respawnPoint = p; }

    public void Respawn()
    {
        // Reset health to full before respawning
        _currentHealth = maxHealth;
        
        // Clear invulnerability
        _invulnerable = false;
        CancelInvoke(nameof(ClearInvuln));
        
        // Reset position and physics
        transform.position = _respawnPoint;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.position = (Vector2)_respawnPoint;
        
        // Ensure sprite color is normal
        if (_sr != null) _sr.color = _baseColor;
        
        Debug.Log($"Player respawned with {_currentHealth}/{maxHealth} health");
    }

    public void ApplyDamage(int amount)
    {
        _currentHealth -= Mathf.Max(0, amount);
        Debug.Log($"Player took {amount} damage. Health: {_currentHealth}/{maxHealth}");
        
        if (_currentHealth <= 0) {
            Debug.Log("Player died, respawning...");
            Respawn();
        }
    }

    /// <summary>
    /// New overload used by hazards: applies damage, knockback impulse, brief hit-stun and i-frames.
    /// </summary>
    public void ApplyDamage(int amount, Vector2 knockback, float hitStunSeconds = 0.15f, float invulnSeconds = 0.75f)
    {
        if (_invulnerable && Time.time < _invulnUntil) 
        {
            Debug.Log("ApplyDamage: Player is invulnerable, ignoring damage");
            return;
        }

        Debug.Log($"ApplyDamage: Applying damage {amount}, knockback {knockback}");
        Debug.Log($"ApplyDamage: Current constraints before: {_rb.constraints}");
        
        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Abs(amount));

        // Audio feedback
        AudioFXManager.PlayDamageSound();

        // Screen shake
        var cameraManager = FindFirstObjectByType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.TriggerScreenShake(0.3f, 0.15f);
        }

        // Detach from moving platform if any (safe if not parented)
        try { DetachFromPlatform(); } catch {}

        // Knockback: zero horizontal first for consistency, then impulse
        if (_rb != null)
        {
            Debug.Log($"ApplyDamage: Current velocity before knockback: {_rb.linearVelocity}");
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            _rb.AddForce(knockback, ForceMode2D.Impulse);
            Debug.Log($"ApplyDamage: Velocity after knockback: {_rb.linearVelocity}");
            Debug.Log($"ApplyDamage: Constraints after knockback: {_rb.constraints}");
        }

        // Invulnerability window
        _invulnerable = true;
        _invulnUntil = Time.time + invulnSeconds;
        Invoke(nameof(ClearInvuln), invulnSeconds);

        // Flash feedback + optional short control dampening via hit-stun
        if (_sr != null) StartCoroutine(FlashRoutine());
        if (hitStunSeconds > 0f) StartCoroutine(HitStunRoutine(hitStunSeconds));

        if (_currentHealth <= 0)
        {
            Respawn(); // use existing respawn logic
        }
    }

    private void ClearInvuln() => _invulnerable = false;

    private System.Collections.IEnumerator FlashRoutine()
    {
        float tEnd = Time.time + flashSeconds;
        // simple two-step blink (keeps URP material intact)
        while (Time.time < tEnd)
        {
            _sr.color = flashColor;
            yield return new WaitForSeconds(flashSeconds * 0.5f);
            _sr.color = _baseColor;
            yield return new WaitForSeconds(flashSeconds * 0.5f);
        }
        _sr.color = _baseColor;
    }

    private System.Collections.IEnumerator HitStunRoutine(float duration)
    {
        // If you have an input gate flag, disable it here (example: _canControl = false)
        // For minimal intrusion, just wait out the stun window.
        yield return new WaitForSeconds(duration);
        // _canControl = true;
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    public void AttachToPlatform(Transform platform)
    {
        _currentPlatform = platform;
        if (platform != null) transform.SetParent(platform);
    }

    public void DetachFromPlatform()
    {
        if (_currentPlatform != null) {
            transform.SetParent(null);
            _currentPlatform = null;
        }
    }

    public void DetachFromPlatform(Transform platform)
    {
        if (platform == null || platform == _currentPlatform) DetachFromPlatform();
    }

    public void DetachFromPlatform(GameObject platformGO)
    {
        DetachFromPlatform(platformGO != null ? platformGO.transform : null);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
}