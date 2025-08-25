using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Touch controls optimized for iPhone 16 Pro (6.1" display, 2556×1179 resolution)
/// Supports both portrait and landscape orientations for AirPlay casting
/// </summary>
public class TouchControls : MonoBehaviour
{
    [Header("📱 iPhone 16 Pro Touch Controls")]
    [SerializeField] private bool adaptToOrientation = true;
    [SerializeField] private float buttonSize = 80f;
    [SerializeField] private float joystickSize = 120f;
    [SerializeField] private float edgeMargin = 40f;
    
    [Header("🎨 Visual Settings")]
    [SerializeField] private Color buttonColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color joystickColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
    [SerializeField] private bool hideWhenControllerConnected = true;
    
    // Touch Input Components
    private Canvas touchCanvas;
    private VirtualJoystick movementJoystick;
    private TouchButton jumpButton;
    private TouchButton abilityButton;
    private TouchButton pauseButton;
    
    // Events
    public System.Action<Vector2> OnMovementInput;
    public System.Action OnJumpPressed;
    public System.Action OnJumpReleased;
    public System.Action OnAbilityPressed;
    public System.Action OnPausePressed;
    
    // Device-specific configurations
    private DeviceConfig currentConfig;
    
    private struct DeviceConfig
    {
        public Vector2 resolution;
        public float dpi;
        public float safeAreaMultiplier;
        public Vector2 joystickPosition;
        public Vector2 jumpButtonPosition;
        public Vector2 abilityButtonPosition;
        public Vector2 pauseButtonPosition;
    }
    
    private void Start()
    {
        CreateTouchCanvas();
        SetupTouchControls();
        ConfigureForCurrentDevice();
        
        // Listen for orientation changes
        if (adaptToOrientation)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
    
    private void Update()
    {
        // Handle orientation changes
        if (adaptToOrientation && HasOrientationChanged())
        {
            UpdateLayoutForOrientation();
        }
        
        // Hide/show based on controller connection
        if (hideWhenControllerConnected)
        {
            var inputManager = FindFirstObjectByType<MobileInputManager>();
            if (inputManager != null)
            {
                SetVisibility(!inputManager.IsBackboneConnected);
            }
        }
    }
    
    private void CreateTouchCanvas()
    {
        var canvasGO = new GameObject("TouchCanvas");
        canvasGO.transform.SetParent(transform);
        
        touchCanvas = canvasGO.AddComponent<Canvas>();
        touchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        touchCanvas.sortingOrder = 1000; // Always on top
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2556, 1179); // iPhone 16 Pro resolution
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
    }
    
    private void SetupTouchControls()
    {
        // Movement Joystick (bottom-left)
        movementJoystick = CreateVirtualJoystick("MovementJoystick", currentConfig.joystickPosition);
        movementJoystick.OnValueChanged += (value) => OnMovementInput?.Invoke(value);
        
        // Jump Button (bottom-right)
        jumpButton = CreateTouchButton("JumpButton", "JUMP", currentConfig.jumpButtonPosition);
        jumpButton.OnPointerDown += () => OnJumpPressed?.Invoke();
        jumpButton.OnPointerUp += () => OnJumpReleased?.Invoke();
        
        // Ability Button (right side, middle)
        abilityButton = CreateTouchButton("AbilityButton", "X", currentConfig.abilityButtonPosition);
        abilityButton.OnPointerDown += () => OnAbilityPressed?.Invoke();
        
        // Pause Button (top-right)
        pauseButton = CreateTouchButton("PauseButton", "⏸", currentConfig.pauseButtonPosition);
        pauseButton.OnPointerDown += () => OnPausePressed?.Invoke();
    }
    
    private VirtualJoystick CreateVirtualJoystick(string name, Vector2 position)
    {
        var joystickGO = new GameObject(name);
        joystickGO.transform.SetParent(touchCanvas.transform, false);
        
        var rectTransform = joystickGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.one * joystickSize;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = position;
        
        // Background
        var bgImage = joystickGO.AddComponent<Image>();
        bgImage.color = joystickColor;
        bgImage.sprite = CreateCircleSprite();
        
        // Handle
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(joystickGO.transform, false);
        
        var handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.sizeDelta = Vector2.one * (joystickSize * 0.6f);
        handleRect.anchoredPosition = Vector2.zero;
        
        var handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;
        handleImage.sprite = CreateCircleSprite();
        
        var joystick = joystickGO.AddComponent<VirtualJoystick>();
        joystick.Setup(bgImage, handleImage, joystickSize * 0.5f);
        
        return joystick;
    }
    
    private TouchButton CreateTouchButton(string name, string text, Vector2 position)
    {
        var buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(touchCanvas.transform, false);
        
        var rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.one * buttonSize;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = position;
        
        var image = buttonGO.AddComponent<Image>();
        image.color = buttonColor;
        image.sprite = CreateCircleSprite();
        
        // Button text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.sizeDelta = Vector2.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        var textComponent = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 24;
        textComponent.color = Color.black;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        
        var touchButton = buttonGO.AddComponent<TouchButton>();
        touchButton.Setup(image);
        
        return touchButton;
    }
    
    private Sprite CreateCircleSprite()
    {
        // Create a simple white circle texture
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = Vector2.one * (size / 2f);
        float radius = size / 2f - 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }
    
    public void ConfigureForDevice(string deviceType)
    {
        switch (deviceType)
        {
            case "iPhone16Pro":
                currentConfig = new DeviceConfig
                {
                    resolution = new Vector2(2556, 1179),
                    dpi = 460f,
                    safeAreaMultiplier = 0.9f,
                    joystickPosition = new Vector2(100, 100),
                    jumpButtonPosition = new Vector2(2456, 100),
                    abilityButtonPosition = new Vector2(2356, 200),
                    pauseButtonPosition = new Vector2(2456, 1079)
                };
                break;
            default:
                ConfigureForCurrentDevice();
                break;
        }
        
        if (movementJoystick != null)
        {
            UpdateLayoutForConfiguration();
        }
    }
    
    private void ConfigureForCurrentDevice()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        currentConfig = new DeviceConfig
        {
            resolution = new Vector2(screenWidth, screenHeight),
            dpi = Screen.dpi,
            safeAreaMultiplier = 0.9f,
            joystickPosition = new Vector2(edgeMargin + joystickSize/2, edgeMargin + joystickSize/2),
            jumpButtonPosition = new Vector2(screenWidth - edgeMargin - buttonSize/2, edgeMargin + buttonSize/2),
            abilityButtonPosition = new Vector2(screenWidth - edgeMargin - buttonSize/2, edgeMargin + buttonSize * 2),
            pauseButtonPosition = new Vector2(screenWidth - edgeMargin - buttonSize/2, screenHeight - edgeMargin - buttonSize/2)
        };
    }
    
    private void UpdateLayoutForConfiguration()
    {
        if (movementJoystick != null)
            movementJoystick.GetComponent<RectTransform>().anchoredPosition = currentConfig.joystickPosition;
        if (jumpButton != null)
            jumpButton.GetComponent<RectTransform>().anchoredPosition = currentConfig.jumpButtonPosition;
        if (abilityButton != null)
            abilityButton.GetComponent<RectTransform>().anchoredPosition = currentConfig.abilityButtonPosition;
        if (pauseButton != null)
            pauseButton.GetComponent<RectTransform>().anchoredPosition = currentConfig.pauseButtonPosition;
    }
    
    private bool HasOrientationChanged()
    {
        return Screen.width != currentConfig.resolution.x || Screen.height != currentConfig.resolution.y;
    }
    
    private void UpdateLayoutForOrientation()
    {
        ConfigureForCurrentDevice();
        UpdateLayoutForConfiguration();
    }
    
    public void SetVisibility(bool visible)
    {
        if (touchCanvas != null)
        {
            touchCanvas.gameObject.SetActive(visible);
        }
    }
    
    public void SetOpacity(float alpha)
    {
        if (movementJoystick != null)
            movementJoystick.GetComponent<Image>().color = new Color(joystickColor.r, joystickColor.g, joystickColor.b, alpha);
        if (jumpButton != null)
            jumpButton.GetComponent<Image>().color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, alpha);
        if (abilityButton != null)
            abilityButton.GetComponent<Image>().color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, alpha);
        if (pauseButton != null)
            pauseButton.GetComponent<Image>().color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, alpha);
    }
}