using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("Player/Player Movement")]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private bool flipSpriteByScaleX = true;
    
    [Header("Advanced Movement")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float variableJumpMultiplier = 0.5f;
    
    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask = 1 << 3;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    
    [Header("Crouch/Duck")]
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private float crouchScale = 0.7f;
    [SerializeField] private float crouchSpeedMultiplier = 0.6f;
    
    [Header("Input Actions (Optional)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    
    [Header("📱 Mobile Input Settings")]
    [SerializeField] private bool useMobileInput = true;
    [SerializeField] private float mobileSpeedBoost = 1.1f;
    [SerializeField] private float analogDeadzone = 0.2f;

    // Components
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    private MobileInputManager mobileInputManager;
    
    // Movement state
    private float inputX;
    private bool inputJump;
    private bool inputCrouch;
    private bool isCrouching;
    private Vector3 originalScale;
    
    // Grounding
    private bool isGrounded;
    private bool wasGrounded;
    private float lastGroundedTime;
    private float jumpBufferTimeLeft;
    
    // Platform support
    private Transform currentPlatform;
    
    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;
    public float HorizontalInput => inputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // Get or create mobile input manager for mobile platforms
        if (useMobileInput && Application.isMobilePlatform)
        {
            mobileInputManager = FindFirstObjectByType<MobileInputManager>();
            if (mobileInputManager == null)
            {
                var inputGO = new GameObject("MobileInputManager");
                mobileInputManager = inputGO.AddComponent<MobileInputManager>();
                Debug.Log("📱 Created MobileInputManager for mobile input");
            }
        }
        
        if (spriteRenderer != null)
            originalScale = spriteRenderer.transform.localScale;
        
        SetupGroundCheck();
        SetupPhysics();
    }

    private void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.SetParent(transform);
            groundCheckGO.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckGO.transform;
        }
    }

    private void SetupPhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 3.8f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.WakeUp();
    }

    private void Update()
    {
        if (playerHealth != null && !playerHealth.CanMove())
        {
            inputX = 0f;
            jumpBufferTimeLeft = 0f;
            return;
        }

        HandleInput();
        UpdateGrounding();
        HandleSpriteFlipping();
        HandleCrouching();
    }

    private void HandleInput()
    {
        // Movement input - prioritize mobile input manager if available
        if (mobileInputManager != null && useMobileInput)
        {
            Vector2 mobileInput = mobileInputManager.MovementInput;
            
            // Apply deadzone and sensitivity
            if (mobileInput.magnitude > analogDeadzone)
            {
                inputX = mobileInput.x;
            }
            else
            {
                inputX = 0f;
            }
        }
        else if (moveAction != null && moveAction.action != null)
        {
            inputX = moveAction.action.ReadValue<Vector2>().x;
        }
        else
        {
            inputX = Input.GetAxisRaw("Horizontal");
            if (Mathf.Approximately(inputX, 0f))
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX += 1f;
            }
        }

        // Jump input - mobile input manager first
        bool jumpPressed = false;
        if (mobileInputManager != null && useMobileInput)
        {
            jumpPressed = mobileInputManager.JumpPressed;
            if (jumpPressed)
            {
                mobileInputManager.ConsumeJumpInput();
            }
        }
        else if (jumpAction != null && jumpAction.action != null)
        {
            jumpPressed = jumpAction.action.WasPressedThisFrame();
        }
        else
        {
            jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || 
                         Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        // Crouch input
        if (canCrouch)
        {
            if (crouchAction != null && crouchAction.action != null)
            {
                inputCrouch = crouchAction.action.IsPressed();
            }
            else
            {
                inputCrouch = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            }
        }

        // Jump buffering
        if (jumpPressed)
        {
            jumpBufferTimeLeft = jumpBufferTime;
        }
        else
        {
            jumpBufferTimeLeft -= Time.deltaTime;
        }
    }

    private void UpdateGrounding()
    {
        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();
        
        if (isGrounded && !wasGrounded)
        {
            lastGroundedTime = Time.time;
        }
        else if (!isGrounded && wasGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void HandleSpriteFlipping()
    {
        if (flipSpriteByScaleX && spriteRenderer != null && Mathf.Abs(inputX) > 0.01f && !isCrouching)
        {
            var scale = spriteRenderer.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (inputX < 0 ? -1 : 1);
            spriteRenderer.transform.localScale = scale;
        }
    }

    private void HandleCrouching()
    {
        if (!canCrouch) return;

        bool shouldCrouch = inputCrouch && isGrounded;
        
        if (shouldCrouch != isCrouching)
        {
            isCrouching = shouldCrouch;
            
            if (spriteRenderer != null)
            {
                var scale = spriteRenderer.transform.localScale;
                scale.y = isCrouching ? originalScale.y * crouchScale : originalScale.y;
                spriteRenderer.transform.localScale = scale;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerHealth != null && !playerHealth.CanMove())
        {
            return;
        }

        ApplyMovement();
        HandleJumping();
    }

    private void ApplyMovement()
    {
        var velocity = rb.linearVelocity;
        
        float currentMoveSpeed = moveSpeed;
        
        // Apply mobile speed boost if using mobile input
        if (mobileInputManager != null && useMobileInput)
        {
            currentMoveSpeed *= mobileSpeedBoost;
        }
        
        if (isCrouching)
        {
            currentMoveSpeed *= crouchSpeedMultiplier;
        }
        
        velocity.x = inputX * currentMoveSpeed;
        rb.linearVelocity = velocity;
    }

    private void HandleJumping()
    {
        bool canJump = isGrounded || (Time.time - lastGroundedTime) <= coyoteTime;
        bool wantsToJump = jumpBufferTimeLeft > 0f;

        if (wantsToJump && canJump && !isCrouching)
        {
            var velocity = rb.linearVelocity;
            if (velocity.y < 0f) velocity.y = 0f;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
            
            jumpBufferTimeLeft = 0f;
            DetachFromPlatform();
        }
        
        // Variable jump height
        bool jumpHeld = false;
        if (mobileInputManager != null && useMobileInput)
        {
            jumpHeld = mobileInputManager.JumpHeld;
        }
        else if (jumpAction != null && jumpAction.action != null)
        {
            jumpHeld = jumpAction.action.IsPressed();
        }
        else
        {
            jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.Space) || 
                      Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }
        
        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            var velocity = rb.linearVelocity;
            velocity.y *= variableJumpMultiplier;
            rb.linearVelocity = velocity;
        }
    }

    public void AttachToPlatform(Transform platform)
    {
        currentPlatform = platform;
        if (platform != null) transform.SetParent(platform);
    }

    public void DetachFromPlatform()
    {
        if (currentPlatform != null)
        {
            transform.SetParent(null);
            currentPlatform = null;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            inputX = 0f;
            jumpBufferTimeLeft = 0f;
        }
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