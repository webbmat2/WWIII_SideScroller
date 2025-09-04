using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Utils
{
    public class SessionPerformanceMonitor : MonoBehaviour
    {
        [Tooltip("Rolling window (seconds) for average FPS computation.")]
        public float windowSeconds = 10f;

        [Tooltip("Enable lightweight overlay via CodeStage.AFPSCounter if present.")]
        public bool enableOverlay = true;

        [Header("Warnings")]
        [Tooltip("If true, will emit a warning when FPS stays below warnBelowFPS for sustainSeconds.")]
        public bool enableLowFpsWarnings = false;
        public float warnBelowFPS = 55f;
        public float sustainSeconds = 10f;
        private float _belowTimer;

        private readonly Queue<float> _frameTimes = new();
        private float _accum = 0f;

        public float AverageFPS { get; private set; }

        private void Start()
        {
            if (enableOverlay)
            {
                var afpsType = System.Type.GetType("CodeStage.AdvancedFPSCounter.AFPSCounter, CodeStage.AFPSCounter.Runtime");
                if (afpsType != null && FindFirstObjectByType(afpsType) == null)
                {
                    var go = new GameObject("AFPSCounter");
                    go.AddComponent(afpsType);
                    DontDestroyOnLoad(go);
                }
            }
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            _frameTimes.Enqueue(dt);
            _accum += dt;
            while (_accum > windowSeconds && _frameTimes.Count > 0)
            {
                _accum -= _frameTimes.Dequeue();
            }
            if (_accum > 0f)
            {
                AverageFPS = _frameTimes.Count / _accum;
            }

            if (enableLowFpsWarnings)
            {
                if (AverageFPS < warnBelowFPS)
                {
                    _belowTimer += Time.unscaledDeltaTime;
                    if (_belowTimer >= sustainSeconds)
                    {
                        Debug.LogWarning($"Low FPS sustained: ~{AverageFPS:F1} for {sustainSeconds:F1}s");
                        _belowTimer = 0f;
                    }
                }
                else
                {
                    _belowTimer = 0f;
                }
            }
        }
    }
}
