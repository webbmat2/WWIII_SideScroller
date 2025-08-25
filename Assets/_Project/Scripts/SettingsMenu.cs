using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings menu optimized for iPhone 16 Pro, Backbone controller, and TV/AirPlay
/// Provides controls for input sensitivity, graphics quality, and mobile optimizations
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("📱 Mobile Settings UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;
    
    [Header("🎮 Input Settings")]
    [SerializeField] private Slider touchSensitivitySlider;
    [SerializeField] private Slider controllerSensitivitySlider;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle autoRotationToggle;
    
    [Header("🎨 Graphics Settings")]
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Slider brightnessSlider;
    
    [Header("📺 TV/AirPlay Settings")]
    [SerializeField] private Toggle tvModeToggle;
    [SerializeField] private Slider tvUIScaleSlider;
    [SerializeField] private Toggle hideTouchControlsToggle;
    
    // References
    private MobileInputManager inputManager;
    private bool isOpen = false;
    
    // Settings
    private SettingsData settings = new SettingsData();
    
    [System.Serializable]
    public class SettingsData
    {
        public float touchSensitivity = 1.0f;
        public float controllerSensitivity = 1.0f;
        public bool vibrationEnabled = true;
        public bool autoRotationEnabled = true;
        public int qualityLevel = 2;
        public bool vsyncEnabled = true;
        public float brightness = 1.0f;
        public bool tvModeEnabled = false;
        public float tvUIScale = 1.3f;
        public bool hideTouchControlsInTV = true;
    }
    
    private void Awake()
    {
        inputManager = FindFirstObjectByType<MobileInputManager>();
        
        LoadSettings();
        SetupUI();
    }
    
    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        ApplySettings();
    }
    
    private void SetupUI()
    {
        // Close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        // Input settings
        if (touchSensitivitySlider != null)
        {
            touchSensitivitySlider.value = settings.touchSensitivity;
            touchSensitivitySlider.onValueChanged.AddListener(OnTouchSensitivityChanged);
        }
        
        if (controllerSensitivitySlider != null)
        {
            controllerSensitivitySlider.value = settings.controllerSensitivity;
            controllerSensitivitySlider.onValueChanged.AddListener(OnControllerSensitivityChanged);
        }
        
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = settings.vibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
        }
        
        if (autoRotationToggle != null)
        {
            autoRotationToggle.isOn = settings.autoRotationEnabled;
            autoRotationToggle.onValueChanged.AddListener(OnAutoRotationToggled);
        }
        
        // Graphics settings
        if (qualityDropdown != null)
        {
            qualityDropdown.value = settings.qualityLevel;
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
            // Populate quality options
            qualityDropdown.options.Clear();
            string[] qualityNames = QualitySettings.names;
            for (int i = 0; i < qualityNames.Length; i++)
            {
                qualityDropdown.options.Add(new Dropdown.OptionData(qualityNames[i]));
            }
        }
        
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = settings.vsyncEnabled;
            vsyncToggle.onValueChanged.AddListener(OnVSyncToggled);
        }
        
        if (brightnessSlider != null)
        {
            brightnessSlider.value = settings.brightness;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
        
        // TV/AirPlay settings
        if (tvModeToggle != null)
        {
            tvModeToggle.isOn = settings.tvModeEnabled;
            tvModeToggle.onValueChanged.AddListener(OnTVModeToggled);
        }
        
        if (tvUIScaleSlider != null)
        {
            tvUIScaleSlider.value = settings.tvUIScale;
            tvUIScaleSlider.onValueChanged.AddListener(OnTVUIScaleChanged);
        }
        
        if (hideTouchControlsToggle != null)
        {
            hideTouchControlsToggle.isOn = settings.hideTouchControlsInTV;
            hideTouchControlsToggle.onValueChanged.AddListener(OnHideTouchControlsToggled);
        }
    }
    
    private void Update()
    {
        // Handle input to open/close settings
        if (inputManager != null)
        {
            if (inputManager.PausePressed)
            {
                ToggleSettings();
                inputManager.ConsumePauseInput();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            ToggleSettings();
        }
    }
    
    public void ToggleSettings()
    {
        if (isOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }
    
    public void OpenSettings()
    {
        isOpen = true;
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        
        // Pause game when settings are open
        Time.timeScale = 0f;
        
        // Hide touch controls temporarily
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null)
        {
            touchControls.SetVisibility(false);
        }
    }
    
    public void CloseSettings()
    {
        isOpen = false;
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Restore touch controls
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null && !settings.tvModeEnabled)
        {
            touchControls.SetVisibility(true);
        }
        
        SaveSettings();
    }
    
    // Input Settings Callbacks
    private void OnTouchSensitivityChanged(float value)
    {
        settings.touchSensitivity = value;
        // Apply to touch controls
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null)
        {
            // Apply sensitivity settings
        }
    }
    
    private void OnControllerSensitivityChanged(float value)
    {
        settings.controllerSensitivity = value;
        if (inputManager != null)
        {
            // Apply controller sensitivity
        }
    }
    
    private void OnVibrationToggled(bool enabled)
    {
        settings.vibrationEnabled = enabled;
        if (inputManager != null)
        {
            inputManager.SetVibration(enabled);
        }
    }
    
    private void OnAutoRotationToggled(bool enabled)
    {
        settings.autoRotationEnabled = enabled;
        if (enabled)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
    
    // Graphics Settings Callbacks
    private void OnQualityChanged(int qualityIndex)
    {
        settings.qualityLevel = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    private void OnVSyncToggled(bool enabled)
    {
        settings.vsyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
    
    private void OnBrightnessChanged(float value)
    {
        settings.brightness = value;
        Screen.brightness = value;
    }
    
    // TV/AirPlay Settings Callbacks
    private void OnTVModeToggled(bool enabled)
    {
        settings.tvModeEnabled = enabled;
        // Can be extended to work with GameHUD if needed
    }
    
    private void OnTVUIScaleChanged(float value)
    {
        settings.tvUIScale = value;
        // Note: Canvas scaling functionality disabled due to Unity 6 compatibility
        // TODO: Re-implement UI scaling functionality
        Debug.Log($"📺 TV UI Scale setting: {value} (not applied due to compatibility)");
    }
    
    private void OnHideTouchControlsToggled(bool enabled)
    {
        settings.hideTouchControlsInTV = enabled;
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null && settings.tvModeEnabled)
        {
            touchControls.SetVisibility(!enabled);
        }
    }
    
    private void ApplySettings()
    {
        // Apply all current settings
        QualitySettings.SetQualityLevel(settings.qualityLevel);
        QualitySettings.vSyncCount = settings.vsyncEnabled ? 1 : 0;
        Screen.brightness = settings.brightness;
        
        if (inputManager != null)
        {
            inputManager.SetVibration(settings.vibrationEnabled);
        }
        
        OnAutoRotationToggled(settings.autoRotationEnabled);
    }
    
    private void LoadSettings()
    {
        // Load from PlayerPrefs
        settings.touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1.0f);
        settings.controllerSensitivity = PlayerPrefs.GetFloat("ControllerSensitivity", 1.0f);
        settings.vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        settings.autoRotationEnabled = PlayerPrefs.GetInt("AutoRotationEnabled", 1) == 1;
        settings.qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        settings.vsyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 1) == 1;
        settings.brightness = PlayerPrefs.GetFloat("Brightness", 1.0f);
        settings.tvModeEnabled = PlayerPrefs.GetInt("TVModeEnabled", 0) == 1;
        settings.tvUIScale = PlayerPrefs.GetFloat("TVUIScale", 1.3f);
        settings.hideTouchControlsInTV = PlayerPrefs.GetInt("HideTouchControlsInTV", 1) == 1;
    }
    
    private void SaveSettings()
    {
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("TouchSensitivity", settings.touchSensitivity);
        PlayerPrefs.SetFloat("ControllerSensitivity", settings.controllerSensitivity);
        PlayerPrefs.SetInt("VibrationEnabled", settings.vibrationEnabled ? 1 : 0);
        PlayerPrefs.SetInt("AutoRotationEnabled", settings.autoRotationEnabled ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", settings.qualityLevel);
        PlayerPrefs.SetInt("VSyncEnabled", settings.vsyncEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("Brightness", settings.brightness);
        PlayerPrefs.SetInt("TVModeEnabled", settings.tvModeEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("TVUIScale", settings.tvUIScale);
        PlayerPrefs.SetInt("HideTouchControlsInTV", settings.hideTouchControlsInTV ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // Public API
    public bool IsOpen() => isOpen;
    public SettingsData GetSettings() => settings;
}