using UnityEngine;
using UnityEngine.InputSystem;

namespace WWIII.Core
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private bool enableTouchControls = true; // Controls touch input
        [SerializeField] private bool enableGamepadSupport = true; // Controls gamepad input  
        [SerializeField] private bool enableKeyboardSupport = true; // Controls keyboard input
        
        public static InputManager Instance { get; private set; }
        
        private PlayerInput playerInput;
        private bool isPaused = false; // Tracks pause state for input filtering
        
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool PausePressed { get; private set; }
        
        public System.Action OnJumpPressed;
        public System.Action OnJumpReleased;
        public System.Action OnPausePressed;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInput();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeInput()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on InputManager!");
            }
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (IsInputAllowed())
            {
                MoveInput = context.ReadValue<Vector2>();
            }
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!IsInputAllowed()) return;
            
            if (context.performed)
            {
                JumpPressed = true;
                JumpHeld = true;
                JumpReleased = false;
                OnJumpPressed?.Invoke();
            }
            else if (context.canceled)
            {
                JumpHeld = false;
                JumpReleased = true;
                OnJumpReleased?.Invoke();
            }
        }
        
        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PausePressed = true;
                OnPausePressed?.Invoke();
            }
        }
        
        private bool IsInputAllowed()
        {
            if (isPaused) return false;
            
            string currentScheme = playerInput?.currentControlScheme;
            
            return currentScheme switch
            {
                "Touch" => enableTouchControls,
                "Gamepad" => enableGamepadSupport,
                "Keyboard&Mouse" => enableKeyboardSupport,
                _ => true
            };
        }
        
        private void LateUpdate()
        {
            // Reset one-frame input flags
            JumpPressed = false;
            JumpReleased = false;
            PausePressed = false;
        }
        
        public void SetInputMode(string mode)
        {
            if (playerInput != null)
            {
                playerInput.SwitchCurrentControlScheme(mode);
            }
        }
        
        public void EnableInput()
        {
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
            isPaused = false;
        }
        
        public void DisableInput()
        {
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
            isPaused = true;
        }
        
        public bool IsUsingGamepad()
        {
            return playerInput != null && playerInput.currentControlScheme == "Gamepad";
        }
        
        public bool IsUsingTouch()
        {
            return playerInput != null && playerInput.currentControlScheme == "Touch";
        }
        
        public bool IsUsingKeyboard()
        {
            return playerInput != null && playerInput.currentControlScheme == "Keyboard&Mouse";
        }
    }
}