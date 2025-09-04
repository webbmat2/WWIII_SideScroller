using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Input;

namespace WWIII.SideScroller.DevTools
{
    public class ControllerSandboxHUD : MonoBehaviour
    {
        public Text info;
        public AgeManager ageManager;
#if ENABLE_INPUT_SYSTEM
        public InputActionAsset actions;
#endif
        private float _timer;

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < 0.1f) return; // 10 Hz
            _timer = 0f;

            if (info == null) return;
            var age = ageManager != null && ageManager.CurrentAge != null ? ageManager.CurrentAge.ageYears.ToString() : "?";
            string lines = $"Age: {age}\n";
#if ENABLE_INPUT_SYSTEM
            if (actions != null)
            {
                lines += "Actions:\n";
                var move = actions.FindAction("Move", false);
                if (move != null) lines += $"  Move: {move.ReadValue<Vector2>()}\n";
                AddActionState(ref lines, "Jump");
                AddActionState(ref lines, "Dash");
                AddActionState(ref lines, "Interact");
                AddActionState(ref lines, "Shoot");
                AddActionState(ref lines, "Album");
                AddActionState(ref lines, "Pause");
            }
#endif
            info.text = lines;
        }

#if ENABLE_INPUT_SYSTEM
        private void AddActionState(ref string lines, string action)
        {
            var a = actions.FindAction(action, false);
            if (a == null) { lines += $"  {action}: (n/a)\n"; return; }
            bool pressed = a.triggered || (a.type == InputActionType.Button && a.ReadValue<float>() > 0.5f);
            lines += $"  {action}: {(pressed ? "Pressed" : "-" )}\n";
        }
#endif

        public void HapticTestLight()
        {
            ControllerHapticsService.Instance?.Pulse(0.1f, 0.15f, 0.1f);
        }
        public void HapticTestFirm()
        {
            ControllerHapticsService.Instance?.Pulse(0.25f, 0.35f, 0.12f);
        }
    }
}
