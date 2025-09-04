using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Input
{
    [DisallowMultipleComponent]
    public class AgeInputProfileSwitcher : MonoBehaviour
    {
        public AgeManager ageManager;
#if ENABLE_INPUT_SYSTEM
        [Tooltip("Input Actions asset with 'Player' and optional 'Child' maps")]
        public InputActionAsset actions;
#endif
        [Tooltip("Ages <= this use the Child map if present")] public int childMaxYears = 11;

        private void Reset()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
        }

        private void OnEnable()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
            if (ageManager != null) ageManager.OnAgeChanged += Apply;
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.OnAgeChanged -= Apply;
        }

        private void Apply(AgeProfile profile)
        {
            if (profile == null) return;
#if ENABLE_INPUT_SYSTEM
            if (actions == null) return;

            var child = actions.FindActionMap("Child", throwIfNotFound: false);
            var player = actions.FindActionMap("Player", throwIfNotFound: false);

            if (profile.ageYears <= childMaxYears && child != null)
            {
                if (player != null) player.Disable();
                child.Enable();
            }
            else
            {
                if (child != null) child.Disable();
                player?.Enable();
            }

            // Gate abilities by age flags
            ToggleAction("Dash", profile.abilities.canDash);
            ToggleAction("Shoot", profile.abilities.canShoot);
            ToggleAction("Interact", true);
            ToggleAction("Album", true);
            ToggleAction("Pause", true);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void ToggleAction(string actionName, bool enabled)
        {
            var a = actions.FindAction(actionName, throwIfNotFound: false);
            if (a == null) return;
            if (enabled && !a.enabled) a.Enable();
            if (!enabled && a.enabled) a.Disable();
        }
#endif
    }
}

