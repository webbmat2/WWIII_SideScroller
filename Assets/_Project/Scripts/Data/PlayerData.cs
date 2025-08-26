using UnityEngine;

namespace WWIII.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "WWIII/Player Data", order = 1)]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 60f;
        [SerializeField] private float airAcceleration = 30f;
        [SerializeField] private float airDeceleration = 20f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float jumpTimeToPeak = 0.4f;
        [SerializeField] private float jumpTimeToDescend = 0.35f;
        [SerializeField] private float minJumpHeight = 1f;
        
        [Header("Advanced Jump Mechanics")]
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        [SerializeField] private int maxAirJumps = 0;
        [SerializeField] private float airJumpHeightMultiplier = 0.8f;
        
        [Header("Physics")]
        [SerializeField] private float maxFallSpeed = 20f;
        [SerializeField] private float fastFallMultiplier = 2f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        
        [Header("Mobile Optimizations")]
        [SerializeField] private bool enableTouchJumpBurst = true;
        [SerializeField] private float touchJumpSensitivity = 1.2f;
        [SerializeField] private bool enableHapticFeedback = true;
        
        // Calculated properties
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float AirDeceleration => airDeceleration;
        
        public float JumpHeight => jumpHeight;
        public float MinJumpHeight => minJumpHeight;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public int MaxAirJumps => maxAirJumps;
        public float AirJumpHeightMultiplier => airJumpHeightMultiplier;
        
        public float MaxFallSpeed => maxFallSpeed;
        public float FastFallMultiplier => fastFallMultiplier;
        public LayerMask GroundLayerMask => groundLayerMask;
        public float GroundCheckDistance => groundCheckDistance;
        public Vector2 GroundCheckSize => groundCheckSize;
        
        public bool EnableTouchJumpBurst => enableTouchJumpBurst;
        public float TouchJumpSensitivity => touchJumpSensitivity;
        public bool EnableHapticFeedback => enableHapticFeedback;
        
        // Calculated jump physics
        private float jumpVelocity;
        private float gravity;
        private float jumpGravity;
        private float fallGravity;
        
        public float JumpVelocity 
        { 
            get 
            { 
                if (jumpVelocity == 0) CalculateJumpPhysics();
                return jumpVelocity; 
            } 
        }
        
        public float Gravity 
        { 
            get 
            { 
                if (gravity == 0) CalculateJumpPhysics();
                return gravity; 
            } 
        }
        
        public float JumpGravity 
        { 
            get 
            { 
                if (jumpGravity == 0) CalculateJumpPhysics();
                return jumpGravity; 
            } 
        }
        
        public float FallGravity 
        { 
            get 
            { 
                if (fallGravity == 0) CalculateJumpPhysics();
                return fallGravity; 
            } 
        }
        
        private void OnValidate()
        {
            CalculateJumpPhysics();
        }
        
        private void CalculateJumpPhysics()
        {
            // Calculate gravity and jump velocity based on jump height and timing
            jumpGravity = (2 * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak);
            fallGravity = (2 * jumpHeight) / (jumpTimeToDescend * jumpTimeToDescend);
            gravity = fallGravity; // Default gravity
            jumpVelocity = jumpGravity * jumpTimeToPeak;
        }
        
        public float GetMinJumpVelocity()
        {
            return Mathf.Sqrt(2 * jumpGravity * minJumpHeight);
        }
        
        public float GetAirJumpVelocity()
        {
            return jumpVelocity * airJumpHeightMultiplier;
        }
    }
}