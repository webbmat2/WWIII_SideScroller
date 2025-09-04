using UnityEngine;
using MoreMountains.CorgiEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Characters
{
    [RequireComponent(typeof(Character))]
    public class Age7Character : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Age 7 Settings")]
        [SerializeField] private float childScale = 0.7f;
        [SerializeField] private float walkSpeed = 8f;
        [SerializeField] private float runSpeed = 12f;
        [SerializeField] private float jumpHeight = 12f;
        
        private Character character;
        private CharacterHorizontalMovement movement;
        private CharacterJump jump;
        
        private void Awake()
        {
            character = GetComponent<Character>();
            movement = GetComponent<CharacterHorizontalMovement>();
            jump = GetComponent<CharacterJump>();
            
            InitializeAge7Character();
        }
        
        private void InitializeAge7Character()
        {
            // Set child proportions
            transform.localScale = Vector3.one * childScale;
            
            // Configure age-appropriate movement
            if (movement != null)
            {
                movement.WalkSpeed = walkSpeed;
                movement.RunSpeed = runSpeed;
            }
            
            if (jump != null)
            {
                jump.JumpHeight = jumpHeight;
            }
            
            // Set correct layer and tag
            gameObject.layer = LayerMask.NameToLayer("Player");
            gameObject.tag = "Player";
            
            Debug.Log("[Age7Character] Initialized with child-appropriate settings");
        }
        
        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            // Map AgeProfile values to Corgi movement/jump (project uses maxRunSpeed/jumpForce)
            if (movement != null)
            {
                // Use provided maxRunSpeed as the run speed, and derive walk speed
                movement.RunSpeed = config.maxRunSpeed;
                movement.WalkSpeed = Mathf.Max(1f, config.maxRunSpeed * 0.66f);
            }
            
            if (jump != null)
            {
                // Convert jump force heuristically to height if needed (keep simple assignment)
                jump.JumpHeight = config.jumpForce;
            }
        }
        
        public void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            
            // Scale character based on age (7-50 range)
            float ageScale = Mathf.Lerp(0.6f, 1.0f, (profile.ageYears - 7f) / 43f);
            transform.localScale = Vector3.one * ageScale;
            
            Debug.Log($"[Age7Character] Age changed to {profile.ageYears}: Scale={ageScale}");
        }
    }
}

