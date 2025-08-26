using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WWIII.Core;
using WWIII.Player;

namespace WWIII.UI
{
    public class TouchControls : MonoBehaviour
    {
        [Header("Control References")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button jumpButton;
        [SerializeField] private GameObject touchControlsPanel;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image leftButtonImage;
        [SerializeField] private Image rightButtonImage;
        [SerializeField] private Image jumpButtonImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.gray;
        
        [Header("Settings")]
        [SerializeField] private bool showOnlyOnMobile = true;
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private float buttonScale = 1f;
        
        private PlayerController playerController;
        private InputManager inputManager;
        
        // Touch state
        private bool leftPressed = false;
        private bool rightPressed = false;
        private bool jumpPressed = false;
        
        // Input values
        private float horizontalInput = 0f;
        
        private void Start()
        {
            InitializeTouchControls();
            SetupButtonListeners();
            ConfigureVisibility();
        }
        
        private void Update()
        {
            HandleTouchInput();
            UpdateVisualFeedback();
        }
        
        private void InitializeTouchControls()
        {
            // Find player controller
            playerController = FindFirstObjectByType<PlayerController>();
            inputManager = InputManager.Instance;
            
            // Scale buttons for different screen sizes
            ScaleButtonsForDevice();
            
            Debug.Log("Touch controls initialized successfully");
        }
        
        private void SetupButtonListeners()
        {
            // Left button
            if (leftButton != null)
            {
                EventTrigger leftTrigger = leftButton.gameObject.GetComponent<EventTrigger>() ?? leftButton.gameObject.AddComponent<EventTrigger>();
                
                EventTrigger.Entry leftPointerDown = new EventTrigger.Entry();
                leftPointerDown.eventID = EventTriggerType.PointerDown;
                leftPointerDown.callback.AddListener((data) => OnLeftButtonDown());
                leftTrigger.triggers.Add(leftPointerDown);
                
                EventTrigger.Entry leftPointerUp = new EventTrigger.Entry();
                leftPointerUp.eventID = EventTriggerType.PointerUp;
                leftPointerUp.callback.AddListener((data) => OnLeftButtonUp());
                leftTrigger.triggers.Add(leftPointerUp);
            }
            
            // Right button
            if (rightButton != null)
            {
                EventTrigger rightTrigger = rightButton.gameObject.GetComponent<EventTrigger>() ?? rightButton.gameObject.AddComponent<EventTrigger>();
                
                EventTrigger.Entry rightPointerDown = new EventTrigger.Entry();
                rightPointerDown.eventID = EventTriggerType.PointerDown;
                rightPointerDown.callback.AddListener((data) => OnRightButtonDown());
                rightTrigger.triggers.Add(rightPointerDown);
                
                EventTrigger.Entry rightPointerUp = new EventTrigger.Entry();
                rightPointerUp.eventID = EventTriggerType.PointerUp;
                rightPointerUp.callback.AddListener((data) => OnRightButtonUp());
                rightTrigger.triggers.Add(rightPointerUp);
            }
            
            // Jump button
            if (jumpButton != null)
            {
                EventTrigger jumpTrigger = jumpButton.gameObject.GetComponent<EventTrigger>() ?? jumpButton.gameObject.AddComponent<EventTrigger>();
                
                EventTrigger.Entry jumpPointerDown = new EventTrigger.Entry();
                jumpPointerDown.eventID = EventTriggerType.PointerDown;
                jumpPointerDown.callback.AddListener((data) => OnJumpButtonDown());
                jumpTrigger.triggers.Add(jumpPointerDown);
                
                EventTrigger.Entry jumpPointerUp = new EventTrigger.Entry();
                jumpPointerUp.eventID = EventTriggerType.PointerUp;
                jumpPointerUp.callback.AddListener((data) => OnJumpButtonUp());
                jumpTrigger.triggers.Add(jumpPointerUp);
            }
        }
        
        private void ConfigureVisibility()
        {
            bool shouldShow = true;
            
            if (showOnlyOnMobile)
            {
                #if UNITY_EDITOR
                shouldShow = false;
                #elif UNITY_STANDALONE
                shouldShow = false;
                #elif UNITY_WEBGL
                shouldShow = false;
                #endif
            }
            
            if (touchControlsPanel != null)
            {
                touchControlsPanel.SetActive(shouldShow);
            }
            
            Debug.Log($"Touch controls visibility: {shouldShow}");
        }
        
        private void ScaleButtonsForDevice()
        {
            float screenScale = Mathf.Min(Screen.width, Screen.height) / 1080f;
            float finalScale = buttonScale * screenScale;
            
            // Clamp scale to reasonable bounds
            finalScale = Mathf.Clamp(finalScale, 0.5f, 2f);
            
            if (touchControlsPanel != null)
            {
                touchControlsPanel.transform.localScale = Vector3.one * finalScale;
            }
            
            Debug.Log($"Touch controls scaled to: {finalScale}");
        }
        
        private void HandleTouchInput()
        {
            // Calculate horizontal input
            horizontalInput = 0f;
            
            if (leftPressed)
                horizontalInput -= 1f;
                
            if (rightPressed)
                horizontalInput += 1f;
            
            // Send input to player controller
            if (playerController != null)
            {
                playerController.SetHorizontalInput(horizontalInput);
                
                if (jumpPressed)
                {
                    playerController.Jump();
                    jumpPressed = false; // Reset after one frame
                }
            }
        }
        
        private void UpdateVisualFeedback()
        {
            // Update button colors
            if (leftButtonImage != null)
            {
                leftButtonImage.color = leftPressed ? pressedColor : normalColor;
            }
            
            if (rightButtonImage != null)
            {
                rightButtonImage.color = rightPressed ? pressedColor : normalColor;
            }
            
            if (jumpButtonImage != null)
            {
                jumpButtonImage.color = jumpPressed ? pressedColor : normalColor;
            }
        }
        
        // Button event handlers
        private void OnLeftButtonDown()
        {
            leftPressed = true;
            TriggerHapticFeedback();
        }
        
        private void OnLeftButtonUp()
        {
            leftPressed = false;
        }
        
        private void OnRightButtonDown()
        {
            rightPressed = true;
            TriggerHapticFeedback();
        }
        
        private void OnRightButtonUp()
        {
            rightPressed = false;
        }
        
        private void OnJumpButtonDown()
        {
            jumpPressed = true;
            TriggerHapticFeedback();
            
            if (playerController != null)
            {
                playerController.Jump();
            }
        }
        
        private void OnJumpButtonUp()
        {
            if (playerController != null)
            {
                playerController.ReleaseJump();
            }
        }
        
        private void TriggerHapticFeedback()
        {
            if (enableHapticFeedback && MobileOptimizer.Instance != null)
            {
                MobileOptimizer.Instance.TriggerHapticFeedback(HapticFeedbackType.LightImpact);
            }
        }
        
        // Public methods for external control
        public void SetControlsVisible(bool visible)
        {
            if (touchControlsPanel != null)
            {
                touchControlsPanel.SetActive(visible);
            }
        }
        
        public void SetButtonScale(float scale)
        {
            buttonScale = scale;
            ScaleButtonsForDevice();
        }
        
        public void SetHapticFeedback(bool enabled)
        {
            enableHapticFeedback = enabled;
        }
        
        public bool AreControlsVisible()
        {
            return touchControlsPanel != null && touchControlsPanel.activeInHierarchy;
        }
        
        public float GetHorizontalInput()
        {
            return horizontalInput;
        }
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (leftButton != null)
            {
                EventTrigger leftTrigger = leftButton.GetComponent<EventTrigger>();
                if (leftTrigger != null)
                    leftTrigger.triggers.Clear();
            }
            
            if (rightButton != null)
            {
                EventTrigger rightTrigger = rightButton.GetComponent<EventTrigger>();
                if (rightTrigger != null)
                    rightTrigger.triggers.Clear();
            }
            
            if (jumpButton != null)
            {
                EventTrigger jumpTrigger = jumpButton.GetComponent<EventTrigger>();
                if (jumpTrigger != null)
                    jumpTrigger.triggers.Clear();
            }
        }
    }
}