using UnityEngine;
using System.Collections;

namespace WWIII.Core
{
    public class BootstrapManager : MonoBehaviour
    {
        [Header("Bootstrap Settings")]
        [SerializeField] private float splashDuration = 2f;
        [SerializeField] private bool skipSplashInEditor = true;
        [SerializeField] private bool autoLoadMainMenu = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool enableFPSCounter = true;
        
        private void Start()
        {
            StartCoroutine(InitializeGame());
        }
        
        private IEnumerator InitializeGame()
        {
            Debug.Log("WWIII SideScroller - Starting Bootstrap...");
            
            // Initialize core systems
            yield return StartCoroutine(InitializeCoreSystems());
            
            // Apply platform-specific settings
            yield return StartCoroutine(ApplyPlatformSettings());
            
            // Load essential assets
            yield return StartCoroutine(LoadEssentialAssets());
            
            // Show splash screen (if not skipped)
            if (!Application.isEditor || !skipSplashInEditor)
            {
                yield return StartCoroutine(ShowSplashScreen());
            }
            
            // Complete initialization
            CompleteBootstrap();
        }
        
        private IEnumerator InitializeCoreSystems()
        {
            Debug.Log("Initializing core systems...");
            
            // Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                GameObject gameManager = new GameObject("GameManager");
                gameManager.AddComponent<GameManager>();
                gameManager.AddComponent<InputManager>();
                gameManager.AddComponent<MobileOptimizer>();
                gameManager.AddComponent<SceneTransitionManager>();
            }
            
            // Wait for systems to initialize
            yield return new WaitForEndOfFrame();
            
            // Verify critical systems
            VerifyCriticalSystems();
            
            Debug.Log("Core systems initialized successfully.");
        }
        
        private IEnumerator ApplyPlatformSettings()
        {
            Debug.Log("Applying platform-specific settings...");
            
            #if UNITY_IOS
            SetupIOSSettings();
            #elif UNITY_ANDROID
            SetupAndroidSettings();
            #elif UNITY_STANDALONE_OSX
            SetupMacOSSettings();
            #elif UNITY_STANDALONE_WIN
            SetupWindowsSettings();
            #endif
            
            yield return null;
            
            Debug.Log("Platform settings applied.");
        }
        
        private IEnumerator LoadEssentialAssets()
        {
            Debug.Log("Loading essential assets...");
            
            // Load essential UI prefabs, sounds, etc.
            // This would load critical assets that need to be available immediately
            
            yield return null;
            
            Debug.Log("Essential assets loaded.");
        }
        
        private IEnumerator ShowSplashScreen()
        {
            Debug.Log("Showing splash screen...");
            
            // Show splash/logo screen
            // This would display your game logo, Unity logo, etc.
            
            yield return new WaitForSeconds(splashDuration);
            
            Debug.Log("Splash screen completed.");
        }
        
        private void CompleteBootstrap()
        {
            Debug.Log("Bootstrap completed successfully!");
            
            if (showDebugInfo)
            {
                LogSystemInfo();
            }
            
            if (enableFPSCounter)
            {
                EnableFPSDisplay();
            }
            
            // Proceed to main menu
            if (autoLoadMainMenu)
            {
                SceneTransitionManager.Instance?.LoadMainMenu();
            }
        }
        
        private void VerifyCriticalSystems()
        {
            bool allSystemsReady = true;
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found!");
                allSystemsReady = false;
            }
            
            if (InputManager.Instance == null)
            {
                Debug.LogError("InputManager not found!");
                allSystemsReady = false;
            }
            
            if (MobileOptimizer.Instance == null)
            {
                Debug.LogError("MobileOptimizer not found!");
                allSystemsReady = false;
            }
            
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("SceneTransitionManager not found!");
                allSystemsReady = false;
            }
            
            if (!allSystemsReady)
            {
                Debug.LogError("Critical systems missing! Game may not function properly.");
            }
            else
            {
                Debug.Log("All critical systems verified and ready.");
            }
        }
        
        #if UNITY_IOS
        private void SetupIOSSettings()
        {
            Debug.Log("Applying iOS-specific settings...");
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
        }
        #endif
        
        #if UNITY_ANDROID
        private void SetupAndroidSettings()
        {
            Debug.Log("Applying Android-specific settings...");
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
        }
        #endif
        
        #if UNITY_STANDALONE_OSX
        private void SetupMacOSSettings()
        {
            Debug.Log("Applying macOS-specific settings...");
            Application.targetFrameRate = 60;
        }
        #endif
        
        #if UNITY_STANDALONE_WIN
        private void SetupWindowsSettings()
        {
            Debug.Log("Applying Windows-specific settings...");
            Application.targetFrameRate = 60;
        }
        #endif
        
        private void LogSystemInfo()
        {
            Debug.Log($"Device Model: {SystemInfo.deviceModel}");
            Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
            Debug.Log($"Processor: {SystemInfo.processorType}");
            Debug.Log($"Memory: {SystemInfo.systemMemorySize} MB");
            Debug.Log($"Graphics: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
            Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
            Debug.Log($"Screen DPI: {Screen.dpi}");
        }
        
        private void EnableFPSDisplay()
        {
            // Create simple FPS counter
            GameObject fpsCounter = new GameObject("FPS Counter");
            fpsCounter.AddComponent<FPSCounter>();
            DontDestroyOnLoad(fpsCounter);
        }
    }
    
    public class FPSCounter : MonoBehaviour
    {
        private float deltaTime = 0.0f;
        private GUIStyle style = new GUIStyle();
        
        private void Start()
        {
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 24;
            style.normal.textColor = Color.white;
        }
        
        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
        
        private void OnGUI()
        {
            int fps = Mathf.CeilToInt(1.0f / deltaTime); // Convert FPS to integer
            string text = $"FPS: {fps}";
            
            // Change color based on performance
            if (fps >= 50)
                style.normal.textColor = Color.green;
            else if (fps >= 30)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.red;
            
            GUI.Label(new Rect(10, 10, 200, 30), text, style);
        }
    }
}