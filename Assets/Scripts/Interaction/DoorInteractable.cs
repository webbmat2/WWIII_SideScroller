using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WWIII.SideScroller.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public class DoorInteractable : MonoBehaviour
    {
        [Tooltip("Root to enable when entering (secret area / interior)")] public GameObject targetRoot;
        [Tooltip("Optional object to disable when entering (exterior)")] public GameObject hideRoot;
        public string prompt = "Up to enter";

        private bool _inside;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<MonoBehaviour>() != null) _inside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _inside = false;
        }

        private void Update()
        {
            if (!_inside) return;
#if ENABLE_INPUT_SYSTEM
            if ((Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame))
            {
                Enter();
            }
#else
            if (Input.GetKeyDown(KeyCode.UpArrow)) Enter();
#endif
        }

        private void Enter()
        {
            if (hideRoot != null) hideRoot.SetActive(false);
            if (targetRoot != null) targetRoot.SetActive(true);
        }
    }
}
