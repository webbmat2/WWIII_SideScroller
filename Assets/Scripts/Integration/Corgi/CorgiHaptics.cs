using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using WWIII.SideScroller.Input;

namespace WWIII.SideScroller.Integration.Corgi
{
    public class CorgiHaptics : MonoBehaviour, MMEventListener<MMCharacterEvent>
    {
        [Range(0f,1f)] public float jumpIntensity = 0.18f;
        [Range(0f,1f)] public float dashIntensity = 0.25f;
        [Range(0f,1f)] public float wallIntensity = 0.12f;
        public bool enableInEditor = true;

        private void OnEnable()
        {
            this.MMEventStartListening<MMCharacterEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMCharacterEvent>();
        }

        public void OnMMEvent(MMCharacterEvent e)
        {
            // We don’t know all event types across versions; handle common ones safely
            EnsureService();
            var svc = ControllerHapticsService.Instance; if (svc == null) return;
            switch (e.EventType)
            {
                case MMCharacterEventTypes.Jump:
                    svc.Pulse(jumpIntensity*0.6f, jumpIntensity, 0.08f);
                    break;
                case MMCharacterEventTypes.Dash:
                    svc.Pulse(dashIntensity*0.8f, dashIntensity, 0.10f);
                    break;
                case MMCharacterEventTypes.WallCling:
                    svc.Pulse(wallIntensity*0.6f, wallIntensity, 0.06f);
                    break;
                case MMCharacterEventTypes.WallJump:
                    svc.Pulse(wallIntensity*0.7f, wallIntensity*1.1f, 0.08f);
                    break;
                default:
                    break;
            }
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
