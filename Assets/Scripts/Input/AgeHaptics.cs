using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Input
{
    [DisallowMultipleComponent]
    public class AgeHaptics : MonoBehaviour
    {
        public AgeManager ageManager;
        [Range(0f,1f)] public float intensity = 1f;
        public bool enableInEditor = true;

        private void Reset()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
        }

        private void OnEnable()
        {
            if (ageManager == null) ageManager = FindFirstObjectByType<AgeManager>();
            if (ageManager != null) ageManager.OnAgeChanged += OnAgeChanged;
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.OnAgeChanged -= OnAgeChanged;
        }

        private void EnsureService()
        {
            if (ControllerHapticsService.Instance == null)
            {
                var go = new GameObject("ControllerHapticsService");
                var svc = go.AddComponent<ControllerHapticsService>();
                svc.enableInEditor = enableInEditor;
            }
        }

        private void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            EnsureService();
            var svc = ControllerHapticsService.Instance;
            if (svc == null) return;

            // Soften child ages, intensify adult
            float lo, hi, dur;
            if (profile.ageYears <= 11)
            {
                lo = 0.1f * intensity; hi = 0.15f * intensity; dur = 0.12f;
                svc.Pulse(lo, hi, dur);
            }
            else if (profile.ageYears <= 17)
            {
                svc.Pattern(
                    new ControllerHapticsService.Segment { low = 0.15f*intensity, high = 0.2f*intensity, duration = 0.08f },
                    new ControllerHapticsService.Segment { low = 0.0f, high = 0.0f, duration = 0.04f },
                    new ControllerHapticsService.Segment { low = 0.15f*intensity, high = 0.2f*intensity, duration = 0.08f }
                );
            }
            else
            {
                lo = 0.2f * intensity; hi = 0.35f * intensity; dur = 0.14f;
                svc.Pulse(lo, hi, dur);
            }
        }
    }
}

