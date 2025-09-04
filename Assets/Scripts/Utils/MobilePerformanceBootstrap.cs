using UnityEngine;

namespace WWIII.SideScroller.Utils
{
    public class MobilePerformanceBootstrap : MonoBehaviour
    {
        [Tooltip("Target FPS for mobile. iPhone 16+ can sustain 60 easily in 2D.")]
        public int targetFps = 60;

        [Tooltip("Reduce 2D physics iterations for mobile.")]
        public int velocityIterations = 6;
        public int positionIterations = 2;

        [Tooltip("Disable VSync to honor targetFrameRate.")]
        public bool disableVSync = true;

        private void Awake()
        {
            if (disableVSync) QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;

            Physics2D.velocityIterations = Mathf.Clamp(velocityIterations, 1, 16);
            Physics2D.positionIterations = Mathf.Clamp(positionIterations, 1, 8);

#if UNITY_6000_0_OR_NEWER
            TryConfigureURP();
#endif
        }

#if UNITY_6000_0_OR_NEWER
        private void TryConfigureURP()
        {
            // Best-effort adjustments; safe no-ops if URP not active
            var pipe = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipe == null) return;

            // Ensure MSAA is minimal for 2D (usually disabled)
            pipe.msaaSampleCount = 1;

            // Keep render scale at 1 on high-end iPhones; expose overrides in asset for QA.
            // Not changing renderScale here to avoid surprises.
        }
#endif
    }
}

