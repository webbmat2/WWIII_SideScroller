using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace WWIII.SideScroller.Characters
{
    [RequireComponent(typeof(InputManager))]
    public class Age7InputHandler : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;
        [Header("Debug")]
        [SerializeField] private bool debugInput = false;
        
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction runAction;
        private InputManager corgiInputManager;
        
        private void Awake()
        {
            corgiInputManager = GetComponent<InputManager>();

            // Optional auto-load (assign manually if null)
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("Age7InputActions");
            }

            if (inputActions != null)
            {
                moveAction = inputActions.FindAction("Move");
                jumpAction = inputActions.FindAction("Jump");
                runAction = inputActions.FindAction("Run");
            }
        }
        
        private void OnEnable()
        {
            inputActions?.Enable();
            if (jumpAction != null)
            {
                jumpAction.started += OnJumpPressed;
                jumpAction.canceled += OnJumpReleased;
            }
            if (debugInput) { Debug.Log("[Age7InputHandler] Input actions enabled"); }
        }
        
        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.started -= OnJumpPressed;
                jumpAction.canceled -= OnJumpReleased;
            }
            inputActions?.Disable();
            if (debugInput) { Debug.Log("[Age7InputHandler] Input actions disabled"); }
        }
        
        private void Update()
        {
            if (corgiInputManager == null) return;

            // Let Corgi's Input system handle movement (keyboard/gamepad). We only map run here.
            if (runAction != null)
            {
                bool isRunning = runAction.IsPressed();
                if (isRunning) { corgiInputManager.RunButtonPressed(); }
                if (debugInput && isRunning) { Debug.Log("[Age7InputHandler] Run pressed"); }
            }
        }
        
        private void OnJumpPressed(InputAction.CallbackContext context)
        {
            corgiInputManager?.JumpButtonDown();
            if (debugInput) { Debug.Log("[Age7InputHandler] Jump pressed"); }
        }

        private void OnJumpReleased(InputAction.CallbackContext context)
        {
            corgiInputManager?.JumpButtonUp();
            if (debugInput) { Debug.Log("[Age7InputHandler] Jump released"); }
        }
    }
}
