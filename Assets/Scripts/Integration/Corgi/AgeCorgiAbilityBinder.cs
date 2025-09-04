using UnityEngine;
using WWIII.SideScroller.Aging;
using MoreMountains.CorgiEngine;

namespace WWIII.SideScroller.Integration.Corgi
{
    [DisallowMultipleComponent]
    public class AgeCorgiAbilityBinder : MonoBehaviour
    {
        [Tooltip("AgeManager driving ability unlocks. If empty, first in scene will be used.")]
        public AgeManager ageManager;

        [Tooltip("Optional explicit Character reference. If null, will search in parents.")]
        public Character character;

        [Header("Mappings")]
        [Tooltip("Apply AgeProfile.movement.maxRunSpeed to Corgi CharacterHorizontalMovement.WalkSpeed")] 
        public bool mapWalkSpeed = true;
        [Tooltip("Apply AgeProfile.abilities.maxNumberOfJumps to Corgi CharacterJump.NumberOfJumps")] 
        public bool mapJumps = true;

        private CharacterHorizontalMovement _hMove;
        private CharacterJump _jump;
        private CharacterDash _dash;
        private CharacterCrouch _crouch;
        private CharacterWallClinging _wallCling;
        private CharacterHandleWeapon _handleWeapon;

        private void Reset()
        {
            character = GetComponentInParent<Character>();
            ageManager = FindFirstObjectByType<AgeManager>();
        }

        private void Awake()
        {
            if (character == null) character = GetComponentInParent<Character>();
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();

            if (character != null)
            {
                _hMove = character.FindAbility<CharacterHorizontalMovement>();
                _jump = character.FindAbility<CharacterJump>();
                _dash = character.FindAbility<CharacterDash>();
                _crouch = character.FindAbility<CharacterCrouch>();
                _wallCling = character.FindAbility<CharacterWallClinging>();
                _handleWeapon = character.FindAbility<CharacterHandleWeapon>();
            }
        }

        private void OnEnable()
        {
            if (ageManager != null)
            {
                ageManager.OnAgeChanged += Apply;
                if (ageManager.CurrentAge != null) Apply(ageManager.CurrentAge);
            }
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.OnAgeChanged -= Apply;
        }

        private void Apply(AgeProfile profile)
        {
            if (profile == null || character == null) return;

            // Movement speed
            if (mapWalkSpeed && _hMove != null)
            {
                _hMove.WalkSpeed = Mathf.Max(0.5f, profile.movement.maxRunSpeed);
                _hMove.ResetHorizontalSpeed();
            }

            // Jumps count
            if (mapJumps && _jump != null)
            {
                _jump.NumberOfJumps = Mathf.Max(0, profile.abilities.maxNumberOfJumps);
            }

            // Abilities
            if (_dash != null) _dash.AbilityPermitted = profile.abilities.canDash;
            if (_crouch != null) _crouch.AbilityPermitted = profile.abilities.canCrouch;
            if (_wallCling != null) _wallCling.AbilityPermitted = profile.abilities.canWallCling;
            if (_handleWeapon != null) _handleWeapon.AbilityPermitted = profile.abilities.canShoot;
        }
    }
}

