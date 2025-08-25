using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Comprehensive input manager for iPhone 16 Pro with Backbone controller, AirPlay/tvOS, and keyboard support
/// Optimized for mobile gameplay and TV casting experience
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("📱 Mobile & TV Input Settings")]
    [SerializeField] private bool enableTouchControls = true;
    [SerializeField] private bool enableBackboneController = true;
    [SerializeField] private bool enableKeyboardFallback = true;
    [SerializeField] private bool optimizeForTVOS = true;
    
    [Header("🎮 Controller Settings")]
    [SerializeField] private float deadZone = 0.2f;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private bool vibrationEnabled = true;
    
    [Header("📺 TV/AirPlay Optimization")]
    [SerializeField] private bool hideUIOnTV = false;
    [SerializeField] private float tvScaleMultiplier = 1.2f;
    
    // Input Actions (New Input System)
    private PlayerInputActions inputActions;
    
    // Input State
    public Vector2 MovementInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool AbilityPressed { get; private set; }
    public bool PausePressed { get; private set; }
    
    // Device Detection
    public bool IsBackboneConnected { get; private set; }
    public bool IsOnTV { get; private set; }
    public bool IsKeyboardActive { get; private set; }
    
    // Touch Controls
    private TouchControls touchControls;
    
    // Events
    public System.Action OnBackboneConnected;
    public System.Action OnBackboneDisconnected;
    public System.Action OnTVModeChanged;
    
    private void Awake()
    {
        // Initialize Input System
        inputActions = new PlayerInputActions();
        
        // Setup touch controls if mobile
        if (Application.isMobilePlatform)
        {
            SetupTouchControls();
        }
        
        // Detect TV mode (AirPlay/tvOS)
        DetectTVMode();
        
        // Setup controller detection
        SetupControllerDetection();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        
        // Bind input events
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Jump.canceled += OnJumpCanceled;
        inputActions.Player.Ability.performed += OnAbility;
        inputActions.Player.Pause.performed += OnPause;
        
        // Device change detection
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    private void Update()
    {
        // Update device states
        UpdateDeviceStates();
        
        // Handle TV mode optimizations
        if (IsOnTV)
        {
            HandleTVOptimizations();
        }
        
        // Provide haptic feedback for Backbone controller
        if (IsBackboneConnected && vibrationEnabled)
        {
            HandleControllerFeedback();
        }
    }
    
    private void SetupTouchControls()
    {
        if (!enableTouchControls) return;
        
        // Create touch controls GameObject
        var touchGO = new GameObject("TouchControls");
        touchGO.transform.SetParent(transform);
        touchControls = touchGO.AddComponent<TouchControls>();
        
        // Configure for iPhone 16 Pro dimensions
        touchControls.ConfigureForDevice("iPhone16Pro");
        
        // Subscribe to touch events
        touchControls.OnMovementInput += (input) => MovementInput = input;
        touchControls.OnJumpPressed += () => JumpPressed = true;
        touchControls.OnAbilityPressed += () => AbilityPressed = true;
    }
    
    private void DetectTVMode()
    {
        // Detect if running on tvOS or AirPlay
        #if UNITY_TVOS
        IsOnTV = true;
        #else
        // For iOS, detect AirPlay/external display
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Check screen resolution for external display indication
            IsOnTV = Screen.width > 2000 || Screen.height > 2000;
        }
        #endif
        
        if (IsOnTV && optimizeForTVOS)
        {
            OptimizeForTV();
        }
    }
    
    private void SetupControllerDetection()
    {
        // Detect Backbone and other MFi controllers
        foreach (var device in InputSystem.devices)
        {
            if (IsBackboneController(device))
            {
                IsBackboneConnected = true;
                OnBackboneConnected?.Invoke();
                break;
            }
        }
    }
    
    private void UpdateDeviceStates()
    {
        // Check for keyboard input activity
        if (Keyboard.current != null)
        {
            IsKeyboardActive = Keyboard.current.anyKey.wasPressedThisFrame;
        }
    }
    
    private void HandleTVOptimizations()
    {
        // Optimize UI for TV viewing distance
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            canvas.scaleFactor = tvScaleMultiplier;
            
            if (hideUIOnTV)
            {
                // Hide touch controls on TV
                if (touchControls != null)
                {
                    touchControls.SetVisibility(false);
                }
                
                // Note: Canvas scaling functionality disabled due to Unity 6 compatibility
                // TODO: Re-implement UI scaling for TV mode
                Debug.Log("📺 TV Mode detected - UI scaling not available in this version");
            }
        }
    }
    
    private void HandleControllerFeedback()
    {
        // Provide haptic feedback for game events
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null && playerHealth.CurrentHP < playerHealth.MaxHP)
        {
            // Check if health changed recently (simplified feedback)
            TriggerHapticFeedback(0.5f, 0.2f);
        }
    }
    
    private bool IsBackboneController(InputDevice device)
    {
        // Detect Backbone controller by name or characteristics
        string deviceName = device.name.ToLower();
        return deviceName.Contains("backbone") || 
               deviceName.Contains("controller") && Application.platform == RuntimePlatform.IPhonePlayer;
    }
    
    private void OptimizeForTV()
    {
        // TV-specific optimizations
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1); // Highest quality for TV
        
        // Adjust camera for TV aspect ratio
        var camera = Camera.main;
        if (camera != null)
        {
            camera.orthographicSize = 8f; // Wider view for TV
        }
        
        // Hide touch controls on TV
        if (touchControls != null)
        {
            touchControls.SetVisibility(false);
        }
    }
    
    private void TriggerHapticFeedback(float intensity, float duration)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        // Use iOS haptic feedback
        Handheld.Vibrate();
        #endif
    }
    
    // Input Event Handlers
    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        // Apply deadzone
        if (input.magnitude < deadZone)
            input = Vector2.zero;
        
        MovementInput = input * sensitivity;
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed = true;
        JumpHeld = true;
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpHeld = false;
    }
    
    private void OnAbility(InputAction.CallbackContext context)
    {
        AbilityPressed = true;
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        PausePressed = true;
    }
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added && IsBackboneController(device))
        {
            IsBackboneConnected = true;
            OnBackboneConnected?.Invoke();
            Debug.Log("🎮 Backbone Controller Connected!");
        }
        else if (change == InputDeviceChange.Removed && IsBackboneController(device))
        {
            IsBackboneConnected = false;
            OnBackboneDisconnected?.Invoke();
            Debug.Log("🎮 Backbone Controller Disconnected!");
        }
    }
    
    // Public methods for other scripts
    public void ConsumeJumpInput()
    {
        JumpPressed = false;
    }
    
    public void ConsumeAbilityInput()
    {
        AbilityPressed = false;
    }
    
    public void ConsumePauseInput()
    {
        PausePressed = false;
    }
    
    public void SetVibration(bool enabled)
    {
        vibrationEnabled = enabled;
    }
    
    public string GetActiveInputMethod()
    {
        if (IsBackboneConnected) return "Backbone Controller";
        if (IsKeyboardActive) return "Keyboard";
        if (enableTouchControls) return "Touch";
        return "Unknown";
    }
}