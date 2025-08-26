using UnityEngine;
using WWIII.Data;
using WWIII.Core;

namespace WWIII.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Animator animator;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool enableDebugLog = false;
        
        // Components
        private Rigidbody2D rb;
        private Collider2D col;
        private InputManager inputManager;
        
        // Movement state
        private Vector2 velocity;
        private float horizontalInput;
        private bool isGrounded;
        private bool wasGrounded;
        
        // Jump mechanics
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private int airJumpsRemaining;
        private bool isJumping;
        private bool jumpInputReleased = true;
        
        // Physics
        private float currentGravity;
        private bool isFastFalling;
        
        // Events
        public System.Action OnJump;
        public System.Action OnLand;
        public System.Action<float> OnMove;
        
        private void Awake()
        {
            InitializeComponents();
            ValidateConfiguration();
        }
        
        private void Start()
        {
            SetupPlayer();
        }
        
        private void Update()
        {
            HandleInput();
            UpdateGroundedState();
            UpdateTimers();
            UpdatePhysics();
        }
        
        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyGravity();
            ClampVelocity();
            
            rb.linearVelocity = velocity;
        }
        
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            inputManager = InputManager.Instance;
            
            if (groundCheck == null)
            {
                // Create ground check if not assigned
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -col.bounds.extents.y, 0);
                groundCheck = groundCheckObj.transform;
            }
        }
        
        private void ValidateConfiguration()
        {
            if (playerData == null)
            {
                Debug.LogError($"PlayerData not assigned to {gameObject.name}! Creating default PlayerData.");
                playerData = ScriptableObject.CreateInstance<PlayerData>();
            }
            
            if (inputManager == null)
            {
                Debug.LogWarning($"InputManager not found! Player input may not work properly.");
            }
        }
        
        private void SetupPlayer()
        {
            // Configure Rigidbody2D
            rb.gravityScale = 0f; // We handle gravity manually
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Initialize jump state
            airJumpsRemaining = playerData.MaxAirJumps;
            currentGravity = playerData.Gravity;
            
            Debug.Log($"Player initialized with data: {playerData.name}");
        }
        
        private void HandleInput()
        {
            if (inputManager == null) return;
            
            // Get horizontal input
            horizontalInput = inputManager.MoveInput.x;
            
            // Handle jump input
            HandleJumpInput();
            
            // Handle fast fall
            if (inputManager.MoveInput.y < -0.5f && !isGrounded && velocity.y < 0)
            {
                isFastFalling = true;
            }
            else
            {
                isFastFalling = false;
            }
        }
        
        private void HandleJumpInput()
        {
            // Jump buffer - register jump input even if not grounded
            if (inputManager.JumpPressed)
            {
                jumpBufferCounter = playerData.JumpBufferTime;
                jumpInputReleased = false;
                
                if (enableDebugLog)
                    Debug.Log("Jump input registered");
            }
            
            // Track jump input release for variable jump height
            if (inputManager.JumpReleased)
            {
                jumpInputReleased = true;
                
                if (enableDebugLog)
                    Debug.Log("Jump input released");
            }
            
            // Execute jump if conditions are met
            if (jumpBufferCounter > 0 && (coyoteTimeCounter > 0 || airJumpsRemaining > 0))
            {
                ExecuteJump();
            }
            
            // Variable jump height - cut jump short if button released
            if (jumpInputReleased && isJumping && velocity.y > playerData.GetMinJumpVelocity())
            {
                velocity.y = playerData.GetMinJumpVelocity();
                isJumping = false;
                
                if (enableDebugLog)
                    Debug.Log("Jump cut short - variable height");
            }
        }
        
        private void ExecuteJump()
        {
            bool isGroundJump = coyoteTimeCounter > 0;
            bool isAirJump = !isGroundJump && airJumpsRemaining > 0;
            
            if (isGroundJump)
            {
                // Ground jump
                velocity.y = playerData.JumpVelocity;
                airJumpsRemaining = playerData.MaxAirJumps; // Reset air jumps
                
                if (enableDebugLog)
                    Debug.Log("Ground jump executed");
            }
            else if (isAirJump)
            {
                // Air jump
                velocity.y = playerData.GetAirJumpVelocity();
                airJumpsRemaining--;
                
                if (enableDebugLog)
                    Debug.Log($"Air jump executed. Remaining: {airJumpsRemaining}");
            }
            
            if (isGroundJump || isAirJump)
            {
                isJumping = true;
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
                currentGravity = playerData.JumpGravity;
                
                // Play jump sound
                if (WWIII.Audio.AudioManager.Instance != null)
                {
                    WWIII.Audio.AudioManager.Instance.PlayPlayerJump();
                }
                
                // Haptic feedback
                if (playerData.EnableHapticFeedback && MobileOptimizer.Instance != null)
                {
                    MobileOptimizer.Instance.TriggerHapticFeedback(HapticFeedbackType.LightImpact);
                }
                
                // Trigger events
                OnJump?.Invoke();
                
                // Trigger animator
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }
        
        private void UpdateGroundedState()
        {
            wasGrounded = isGrounded;
            
            // Ground check using overlap
            Vector2 checkPosition = groundCheck.position;
            isGrounded = Physics2D.OverlapBox(
                checkPosition, 
                playerData.GroundCheckSize, 
                0f, 
                playerData.GroundLayerMask
            );
            
            // Landing detection
            if (!wasGrounded && isGrounded)
            {
                OnLanding();
            }
            
            // Leaving ground detection
            if (wasGrounded && !isGrounded && velocity.y <= 0)
            {
                coyoteTimeCounter = playerData.CoyoteTime;
            }
        }
        
        private void OnLanding()
        {
            isJumping = false;
            airJumpsRemaining = playerData.MaxAirJumps;
            currentGravity = playerData.Gravity;
            
            // Play landing sound
            if (WWIII.Audio.AudioManager.Instance != null)
            {
                WWIII.Audio.AudioManager.Instance.PlayPlayerLand();
            }
            
            // Haptic feedback
            if (playerData.EnableHapticFeedback && MobileOptimizer.Instance != null)
            {
                MobileOptimizer.Instance.TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
            
            // Trigger events
            OnLand?.Invoke();
            
            // Trigger animator
            if (animator != null)
            {
                animator.SetBool("IsGrounded", true);
                animator.SetTrigger("Land");
            }
            
            if (enableDebugLog)
                Debug.Log("Player landed");
        }
        
        private void UpdateTimers()
        {
            // Update coyote time
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
            
            // Update jump buffer
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }
        
        private void UpdatePhysics()
        {
            // Switch to fall gravity when falling
            if (velocity.y < 0 && !isJumping)
            {
                currentGravity = playerData.FallGravity;
            }
            
            // Update animator parameters
            if (animator != null)
            {
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetFloat("VelocityX", Mathf.Abs(velocity.x));
                animator.SetFloat("VelocityY", velocity.y);
            }
        }
        
        private void ApplyMovement()
        {
            float targetSpeed = horizontalInput * playerData.MoveSpeed;
            float acceleration = isGrounded ? playerData.Acceleration : playerData.AirAcceleration;
            float deceleration = isGrounded ? playerData.Deceleration : playerData.AirDeceleration;
            
            // Determine if accelerating or decelerating
            float accelRate;
            if (Mathf.Abs(targetSpeed) > 0.01f)
            {
                accelRate = acceleration;
            }
            else
            {
                accelRate = deceleration;
            }
            
            // Apply movement
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            
            // Trigger move event
            OnMove?.Invoke(horizontalInput);
            
            // Handle sprite flipping
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(horizontalInput) * Mathf.Abs(transform.localScale.x), 
                    transform.localScale.y, 
                    transform.localScale.z
                );
            }
        }
        
        private void ApplyGravity()
        {
            float gravityToApply = currentGravity;
            
            // Fast fall
            if (isFastFalling)
            {
                gravityToApply *= playerData.FastFallMultiplier;
            }
            
            velocity.y -= gravityToApply * Time.fixedDeltaTime;
        }
        
        private void ClampVelocity()
        {
            // Clamp fall speed
            if (velocity.y < -playerData.MaxFallSpeed)
            {
                velocity.y = -playerData.MaxFallSpeed;
            }
        }
        
        // Public methods for external control
        public void SetHorizontalInput(float input)
        {
            horizontalInput = input;
        }
        
        public void Jump()
        {
            jumpBufferCounter = playerData.JumpBufferTime;
            jumpInputReleased = false;
        }
        
        public void ReleaseJump()
        {
            jumpInputReleased = true;
        }
        
        public void ResetPlayer(Vector3 position)
        {
            transform.position = position;
            velocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            isJumping = false;
            jumpInputReleased = true;
            airJumpsRemaining = playerData.MaxAirJumps;
            coyoteTimeCounter = 0;
            jumpBufferCounter = 0;
        }
        
        // Getters for external systems
        public bool IsGrounded => isGrounded;
        public bool IsJumping => isJumping;
        public Vector2 Velocity => velocity;
        public float HorizontalInput => horizontalInput;
        public int AirJumpsRemaining => airJumpsRemaining;
        
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || groundCheck == null) return;
            
            // Draw ground check
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 checkPos = groundCheck.position;
            
            if (playerData != null)
            {
                Gizmos.DrawWireCube(checkPos, playerData.GroundCheckSize);
            }
            else
            {
                Gizmos.DrawWireCube(checkPos, Vector2.one * 0.2f);
            }
            
            // Draw velocity
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)velocity * 0.1f);
            }
        }
    }
}