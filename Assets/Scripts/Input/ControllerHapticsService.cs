using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WWIII.SideScroller.Input
{
    [DefaultExecutionOrder(-5000)]
    public class ControllerHapticsService : MonoBehaviour
    {
        public static ControllerHapticsService Instance { get; private set; }

        [Tooltip("Global multiplier applied to all rumble intensities (0..1)")]
        [Range(0f,1f)] public float intensityScale = 1f;

        [Tooltip("If true, enables haptics in the Editor for testing.")]
        public bool enableInEditor = true;

        private Coroutine _patternCo;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Pulse(float low, float high, float duration)
        {
            if (!CanHaptic()) return;
            if (_patternCo != null) StopCoroutine(_patternCo);
            _patternCo = StartCoroutine(PulseCo(Mathf.Clamp01(low*intensityScale), Mathf.Clamp01(high*intensityScale), Mathf.Max(0f, duration)));
        }

        public struct Segment { public float low; public float high; public float duration; }

        public void Pattern(params Segment[] segments)
        {
            if (!CanHaptic()) return;
            if (_patternCo != null) StopCoroutine(_patternCo);
            _patternCo = StartCoroutine(PatternCo(segments));
        }

        private bool CanHaptic()
        {
#if UNITY_EDITOR
            if (!enableInEditor) return false;
#endif
#if ENABLE_INPUT_SYSTEM
            return Gamepad.current != null;
#else
            return false;
#endif
        }

        private IEnumerator PulseCo(float low, float high, float duration)
        {
#if ENABLE_INPUT_SYSTEM
            var gp = Gamepad.current;
            if (gp == null) yield break;
            gp.SetMotorSpeeds(low, high);
            yield return new WaitForSeconds(duration);
            gp.SetMotorSpeeds(0f, 0f);
#else
            yield break;
#endif
        }

        private IEnumerator PatternCo(IList<Segment> segments)
        {
#if ENABLE_INPUT_SYSTEM
            var gp = Gamepad.current;
            if (gp == null || segments == null) yield break;
            foreach (var s in segments)
            {
                gp.SetMotorSpeeds(Mathf.Clamp01(s.low*intensityScale), Mathf.Clamp01(s.high*intensityScale));
                yield return new WaitForSeconds(Mathf.Max(0f, s.duration));
            }
            gp.SetMotorSpeeds(0f, 0f);
#else
            yield break;
#endif
        }
    }
}

