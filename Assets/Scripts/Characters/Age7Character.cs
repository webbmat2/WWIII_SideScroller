using UnityEngine;
using MoreMountains.CorgiEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Characters
{
    [RequireComponent(typeof(Character))]
    public class Age7Character : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Age 7 Character Settings")]
        [SerializeField, Range(0.5f, 1.0f)] private float childScale = 0.7f;
        [SerializeField, Range(5f, 15f)] private float defaultWalkSpeed = 8f;
        [SerializeField, Range(8f, 20f)] private float defaultRunSpeed = 12f;
        [SerializeField, Range(8f, 25f)] private float defaultJumpHeight = 12f;

        [Header("Child Development")]
        [SerializeField] private bool enableAdvancedAbilities = false;

        private Character character;
        private CharacterHorizontalMovement movement;
        private CharacterRun run;
        private Animator animator;
        private CharacterJump jump;
        
        private void Awake()
        {
            character = GetComponent<Character>();
            movement = GetComponent<CharacterHorizontalMovement>();
            jump = GetComponent<CharacterJump>();
            run = GetComponent<CharacterRun>();
            animator = GetComponentInChildren<Animator>();

            InitializeAge7Character();
        }
        
        private void InitializeAge7Character()
        {
            // Set child proportions
            transform.localScale = Vector3.one * childScale;
            
            // Configure age-appropriate movement
            ApplyDefaultMovementSettings();

            // disable/enable advanced abilities depending on flag
            ConfigureChildAbilities();
            
            // Set correct layer and tag
            gameObject.layer = LayerMask.NameToLayer("Player");
            gameObject.tag = "Player";
            
            Debug.Log("[Age7Character] Initialized with child-appropriate settings");
        }

        private void ApplyDefaultMovementSettings()
        {
            if (movement != null)
            {
                movement.WalkSpeed = defaultWalkSpeed;
            }
            if (run != null)
            {
                run.RunSpeed = defaultRunSpeed;
            }
            if (jump != null)
            {
                jump.JumpHeight = defaultJumpHeight;
            }
        }

        private void ConfigureChildAbilities()
        {
            var doubleJump = GetComponent<CharacterDoubleJump>();
            if (doubleJump != null) doubleJump.enabled = enableAdvancedAbilities;

            var wallJump = GetComponent<CharacterWallJump>();
            if (wallJump != null) wallJump.enabled = enableAdvancedAbilities;

            var dash = GetComponent<CharacterDash>();
            if (dash != null) dash.enabled = enableAdvancedAbilities;

            var wallClinging = GetComponent<CharacterWallClinging>();
            if (wallClinging != null) wallClinging.enabled = enableAdvancedAbilities;
        }
        
        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            // Map AgeProfile values to Corgi movement/jump (project uses maxRunSpeed/jumpForce)
            if (movement != null)
            {
                // Use max run speed and derive walk speed
                movement.WalkSpeed = Mathf.Max(1f, config.maxRunSpeed * 0.67f);
            }
            if (run != null)
            {
                run.RunSpeed = config.maxRunSpeed;
            }
            if (jump != null)
            {
                // Map jumpForce directly to height for simplicity
                jump.JumpHeight = config.jumpForce;
            }
        }
        
        public void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            
            // Scale character based on age (7-50 range)
            float ageScale = Mathf.Lerp(0.6f, 1.0f, (profile.ageYears - 7f) / 43f);
            transform.localScale = Vector3.one * ageScale;
            // enable more abilities after 12
            enableAdvancedAbilities = profile.ageYears >= 12;
            ConfigureChildAbilities();

            Debug.Log($"[Age7Character] Age changed to {profile.ageYears}: Scale={ageScale}");
        }
    }
}
