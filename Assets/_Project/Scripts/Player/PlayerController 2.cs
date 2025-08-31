using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity AI optimized PlayerController for Unity 6 and mobile performance
/// Features: Coyote time, jump buffering, variable jump height, 60 FPS optimized
/// Optimized by Unity AI Assistant for Unity 6 mobile-first development
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isJumping;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"🔍 PlayerController Awake - Rigidbody2D Constraints: {rb.constraints}");
        Application.targetFrameRate = 60; // Ensure 60 FPS on mobile
    }

    private void Update()
    {
        HandleCoyoteTime();
        HandleJumpBuffer();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        
        if (jumpBufferCounter > 0 && (isGrounded || coyoteTimeCounter > 0))
        {
            Jump();
        }
    }

    private void HandleMovement()
    {
        // Calculate target velocity
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDifference = targetSpeed - rb.linearVelocity.x;

        // Determine acceleration or deceleration
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        // Apply movement
        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, 0.9f) * Mathf.Sign(speedDifference);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement * Time.fixedDeltaTime, rb.linearVelocity.y);

        // Clamp horizontal velocity
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed), rb.linearVelocity.y);
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter = Mathf.Clamp(coyoteTimeCounter - Time.deltaTime, 0, coyoteTime);
        }
    }

    private void HandleJumpBuffer()
    {
        if (isJumping)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter = Mathf.Clamp(jumpBufferCounter - Time.deltaTime, 0, jumpBufferTime);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isJumping = false;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveInput = new Vector2(moveInput.x, 0); // Ignore vertical input for 2D side-scroller
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isJumping = true;
        }
        else if (context.canceled)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // Cut jump height
            }
            isJumping = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check radius in the editor
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}