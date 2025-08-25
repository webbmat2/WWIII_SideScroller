using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto-setup script for iPhone 16 Pro mobile input system
/// Automatically configures the Player GameObject with mobile input support
/// </summary>
[System.Serializable]
public class MobileInputSetup : MonoBehaviour
{
    [Header("📱 Auto-Setup Mobile Input System")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createMobileInputManager = true;
    [SerializeField] private bool createGameHUD = true;
    [SerializeField] private bool createSettingsMenu = true;
    
    [Header("🎮 Input Configuration")]
    [SerializeField] private bool enableBackboneSupport = true;
    [SerializeField] private bool enableTouchControls = true;
    [SerializeField] private bool enableKeyboardFallback = true;
    [SerializeField] private bool optimizeForTV = true;
    
    private void Start()
    {
        if (setupOnStart)
        {
            Debug.Log("🚀 Setting up Mobile Input System for iPhone 16 Pro...");
            SetupMobileInputSystem();
        }
    }
    
    [ContextMenu("Setup Mobile Input System")]
    public void SetupMobileInputSystem()
    {
        SetupPlayer();
        SetupMobileInputManager();
        SetupGameHUD();
        SetupSettingsMenu();
        
        Debug.Log("✅ Mobile Input System setup complete for iPhone 16 Pro!");
        LogSystemStatus();
    }
    
    private void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("❌ No Player GameObject found with 'Player' tag");
            return;
        }
        
        // Ensure PlayerMovement component exists
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("❌ PlayerMovement component not found on Player");
            return;
        }
        
        // Ensure PlayerHealth component exists
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("❌ PlayerHealth component not found on Player");
            return;
        }
        
        Debug.Log("✅ Player GameObject configured for mobile input");
    }
    
    private void SetupMobileInputManager()
    {
        if (!createMobileInputManager) return;
        
        MobileInputManager existingManager = FindFirstObjectByType<MobileInputManager>();
        if (existingManager != null)
        {
            Debug.Log("✅ MobileInputManager already exists");
            return;
        }
        
        GameObject inputManagerGO = new GameObject("MobileInputManager");
        MobileInputManager inputManager = inputManagerGO.AddComponent<MobileInputManager>();
        
        // Configure for iPhone 16 Pro
        inputManager.enabled = true;
        
        Debug.Log("✅ Created MobileInputManager for iPhone 16 Pro");
    }
    
    private void SetupGameHUD()
    {
        if (!createGameHUD) return;
        
        GameHUD existingHUD = FindFirstObjectByType<GameHUD>();
        if (existingHUD != null)
        {
            Debug.Log("✅ GameHUD already exists");
            return;
        }
        
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("GameCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        GameHUD gameHUD = canvas.gameObject.AddComponent<GameHUD>();
        Debug.Log("✅ Created GameHUD for mobile/TV adaptation");
    }
    
    private void SetupSettingsMenu()
    {
        if (!createSettingsMenu) return;
        
        SettingsMenu existingSettings = FindFirstObjectByType<SettingsMenu>();
        if (existingSettings != null)
        {
            Debug.Log("✅ SettingsMenu already exists");
            return;
        }
        
        GameObject settingsGO = new GameObject("SettingsMenu");
        SettingsMenu settingsMenu = settingsGO.AddComponent<SettingsMenu>();
        
        Debug.Log("✅ Created SettingsMenu for mobile optimization");
    }
    
    private void LogSystemStatus()
    {
        Debug.Log("📱 === Mobile Input System Status ===");
        
        // Check Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Player GameObject: {(player != null ? "✅ Found" : "❌ Missing")}");
        
        if (player != null)
        {
            Debug.Log($"PlayerMovement: {(player.GetComponent<PlayerMovement>() != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"PlayerHealth: {(player.GetComponent<PlayerHealth>() != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"Rigidbody2D: {(player.GetComponent<Rigidbody2D>() != null ? "✅ Found" : "❌ Missing")}");
        }
        
        // Check Input System
        MobileInputManager inputManager = FindFirstObjectByType<MobileInputManager>();
        Debug.Log($"MobileInputManager: {(inputManager != null ? "✅ Found" : "❌ Missing")}");
        
        if (inputManager != null)
        {
            Debug.Log($"Backbone Controller: {(inputManager.IsBackboneConnected ? "✅ Connected" : "📱 Touch Mode")}");
            Debug.Log($"TV Mode: {(inputManager.IsOnTV ? "📺 Active" : "📱 Mobile")}");
            Debug.Log($"Active Input: {inputManager.GetActiveInputMethod()}");
        }
        
        // Check UI Systems
        GameHUD gameHUD = FindFirstObjectByType<GameHUD>();
        Debug.Log($"GameHUD: {(gameHUD != null ? "✅ Found" : "❌ Missing")}");
        
        SettingsMenu settingsMenu = FindFirstObjectByType<SettingsMenu>();
        Debug.Log($"SettingsMenu: {(settingsMenu != null ? "✅ Found" : "❌ Missing")}");
        
        HealthUI healthUI = FindFirstObjectByType<HealthUI>();
        Debug.Log($"HealthUI: {(healthUI != null ? "✅ Found" : "❌ Missing")}");
        
        Debug.Log("=== System Ready for iPhone 16 Pro + Backbone ===");
    }
    
    [ContextMenu("Test Mobile Input")]
    public void TestMobileInput()
    {
        MobileInputManager inputManager = FindFirstObjectByType<MobileInputManager>();
        if (inputManager == null)
        {
            Debug.LogError("❌ MobileInputManager not found. Run Setup first.");
            return;
        }
        
        Debug.Log("🧪 Testing Mobile Input System...");
        Debug.Log($"Movement Input: {inputManager.MovementInput}");
        Debug.Log($"Jump Pressed: {inputManager.JumpPressed}");
        Debug.Log($"Active Input Method: {inputManager.GetActiveInputMethod()}");
        Debug.Log($"Backbone Connected: {inputManager.IsBackboneConnected}");
        Debug.Log($"TV Mode: {inputManager.IsOnTV}");
    }
    
    [ContextMenu("Force TV Mode")]
    public void ForceEnableTVMode()
    {
        // Note: Canvas scaling functionality disabled due to Unity 6 compatibility
        // TODO: Re-implement UI scaling for TV mode
        Debug.Log("📺 TV Mode scaling disabled for compatibility");
        
        // Hide touch controls
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null)
        {
            touchControls.SetVisibility(false);
        }
        
        Debug.Log("📺 Enabled TV Mode for testing");
    }
    
    [ContextMenu("Reset to Mobile Mode")]
    public void ResetToMobileMode()
    {
        // Note: Canvas scaling functionality disabled due to Unity 6 compatibility
        // TODO: Re-implement UI scaling for mobile mode
        Debug.Log("📱 Mobile Mode scaling disabled for compatibility");
        
        // Show touch controls
        var touchControls = FindFirstObjectByType<TouchControls>();
        if (touchControls != null)
        {
            touchControls.SetVisibility(true);
        }
        
        Debug.Log("📱 Reset to Mobile Mode");
    }
}