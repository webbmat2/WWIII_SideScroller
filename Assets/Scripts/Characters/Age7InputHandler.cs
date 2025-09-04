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
        public InputActionAsset inputActions;
        
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction runAction;
        private InputManager corgiInputManager;
        
        private void Awake()
        {
            corgiInputManager = GetComponent<InputManager>();
            
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
        }
        
        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.started -= OnJumpPressed;
                jumpAction.canceled -= OnJumpReleased;
            }
            inputActions?.Disable();
        }
        
        private void Update()
        {
            if (corgiInputManager == null) return;
            
            // Handle movement
            if (moveAction != null)
            {
                Vector2 moveInput = moveAction.ReadValue<Vector2>();
                corgiInputManager.PrimaryMovement = moveInput;
            }
            
            // Handle run button
            if (runAction != null)
            {
                bool isRunning = runAction.IsPressed();
                corgiInputManager.RunButton.State.ChangeState(
                    isRunning ? MMInput.ButtonStates.ButtonPressed : MMInput.ButtonStates.ButtonUp
                );
            }
        }
        
        private void OnJumpPressed(InputAction.CallbackContext context)
        {
            corgiInputManager?.JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonDown);
        }
        
        private void OnJumpReleased(InputAction.CallbackContext context)
        {
            corgiInputManager?.JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
        }
    }
}

