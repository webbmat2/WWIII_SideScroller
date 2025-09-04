using UnityEngine;
using WWIII.SideScroller.Input;

namespace WWIII.SideScroller.Integration.Corgi
{
    [DisallowMultipleComponent]
    public class LandingHaptics : MonoBehaviour
    {
        [Range(0f,1f)] public float baseIntensity = 0.12f;
        [Tooltip("Max fall speed expected for scaling (abs Y speed)")]
        public float maxFallSpeedForScale = 18f;
        public float minInterval = 0.15f;
        public bool enableInEditor = true;

        private MoreMountains.CorgiEngine.CorgiController _controller;
        private bool _wasGrounded;
        private float _lastTime;

        private void Awake()
        {
            _controller = GetComponentInParent<MoreMountains.CorgiEngine.CorgiController>();
        }

        private void Update()
        {
            if (_controller == null) return;
            bool g = _controller.State.IsGrounded;
            if (g && !_wasGrounded && (Time.time - _lastTime) > minInterval)
            {
                EnsureService();
                // Scale intensity by fall speed
                float vy = Mathf.Abs(_controller.Speed.y);
                float scale = Mathf.Clamp01(vy / Mathf.Max(0.01f, maxFallSpeedForScale));
                float i = Mathf.Clamp01(baseIntensity * (0.5f + 0.5f*scale));
                ControllerHapticsService.Instance?.Pulse(i*0.6f, i, 0.06f);
                _lastTime = Time.time;
            }
            _wasGrounded = g;
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
    }
}
