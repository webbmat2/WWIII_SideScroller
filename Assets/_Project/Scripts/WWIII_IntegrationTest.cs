using UnityEngine;
using UnityEditor;

[AddComponentMenu("WWIII/Integration Test")]
public class WWIII_IntegrationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = false;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Test Results")]
    [SerializeField] private bool chaptersReady = false;
    [SerializeField] private bool playerSystemReady = false;
    [SerializeField] private bool uiSystemReady = false;
    [SerializeField] private bool gameplayReady = false;
    
    private void Start()
    {
        if (runOnStart)
        {
            RunIntegrationTests();
        }
    }

    [ContextMenu("Run Complete Integration Test")]
    public void RunIntegrationTests()
    {
        Debug.Log("🚀 WWIII INTEGRATION TEST STARTING");
        Debug.Log("=====================================");
        
        chaptersReady = TestChapterSystem();
        playerSystemReady = TestPlayerSystem();
        uiSystemReady = TestUISystem();
        gameplayReady = TestGameplayMechanics();
        
        bool allSystemsReady = chaptersReady && playerSystemReady && uiSystemReady && gameplayReady;
        
        Debug.Log("=====================================");
        Debug.Log($"🏁 INTEGRATION TEST RESULT: {(allSystemsReady ? "✅ READY FOR DEPLOYMENT" : "❌ ISSUES FOUND")}");
        
        if (allSystemsReady)
        {
            LogDeploymentInstructions();
        }
        else
        {
            LogTroubleshootingSteps();
        }
    }

    private bool TestChapterSystem()
    {
        Debug.Log("🧭 Testing Chapter System...");
        
        bool chapterManagerExists = FindFirstObjectByType<ChapterManager>() != null;
        bool chapterDataExists = CheckChapterDataAssets();
        
        Log($"ChapterManager: {Status(chapterManagerExists)}");
        Log($"Chapter Data Assets: {Status(chapterDataExists)}");
        
        if (chapterManagerExists && chapterDataExists)
        {
            TestChapterLoadingSystem();
        }
        
        bool result = chapterManagerExists && chapterDataExists;
        Log($"Chapter System: {Status(result)}");
        return result;
    }

    private bool CheckChapterDataAssets()
    {
#if UNITY_EDITOR
        string[] expectedChapters = {
            "MeadowbrookPark", "TorchLake", "NotreDame", "HighSchool",
            "Philadelphia", "ParsonsChicken", "CostaRica"
        };
        
        int foundChapters = 0;
        foreach (string chapter in expectedChapters)
        {
            var assets = AssetDatabase.FindAssets($"{chapter} t:ChapterData");
            if (assets.Length > 0) foundChapters++;
        }
        
        Log($"Found {foundChapters}/{expectedChapters.Length} chapter assets");
        return foundChapters >= 3; // At least 3 chapters needed for basic testing
#else
        return true; // Assume assets exist in build
#endif
    }

    private void TestChapterLoadingSystem()
    {
        var chapterManager = FindFirstObjectByType<ChapterManager>();
        if (chapterManager != null)
        {
            // Test loading first chapter
            try
            {
                chapterManager.LoadChapter("meadowbrook-park");
                Log("✅ Chapter loading system functional");
            }
            catch (System.Exception e)
            {
                Log($"❌ Chapter loading failed: {e.Message}");
            }
        }
    }

    private bool TestPlayerSystem()
    {
        Debug.Log("🎮 Testing Player System...");
        
        var playerController = FindFirstObjectByType<PlayerController>();
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        var playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        
        bool playerExists = playerController != null;
        bool componentsExist = playerMovement != null && playerHealth != null && playerAbilities != null;
        bool physicsValid = ValidatePlayerPhysics(playerController?.gameObject);
        
        Log($"Player Controller: {Status(playerExists)}");
        Log($"Required Components: {Status(componentsExist)}");
        Log($"Physics Setup: {Status(physicsValid)}");
        
        if (playerExists && componentsExist)
        {
            TestPlayerFunctionality(playerController);
        }
        
        bool result = playerExists && componentsExist && physicsValid;
        Log($"Player System: {Status(result)}");
        return result;
    }

    private bool ValidatePlayerPhysics(GameObject player)
    {
        if (player == null) return false;
        
        var rb = player.GetComponent<Rigidbody2D>();
        var collider = player.GetComponent<Collider2D>();
        
        bool hasPhysics = rb != null && collider != null;
        bool correctSettings = rb != null && 
                             rb.bodyType == RigidbodyType2D.Dynamic &&
                             rb.constraints == RigidbodyConstraints2D.FreezeRotation;
        
        return hasPhysics && correctSettings;
    }

    private void TestPlayerFunctionality(PlayerController player)
    {
        Log("Testing player functionality...");
        
        // Test movement component
        var movement = player.Movement;
        if (movement != null)
        {
            Log($"  Movement grounding: {Status(movement.IsGrounded)}");
        }
        
        // Test health component  
        var health = player.Health;
        if (health != null)
        {
            Log($"  Health: {health.CurrentHP}/{health.MaxHP}");
        }
        
        // Test abilities
        var abilities = player.Abilities;
        if (abilities != null)
        {
            Log($"  Current ability: {abilities.CurrentAbility}");
        }
    }

    private bool TestUISystem()
    {
        Debug.Log("🖥️ Testing UI System...");
        
        var canvas = FindFirstObjectByType<Canvas>();
        var gameHUD = FindFirstObjectByType<GameHUD>();
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        
        bool canvasExists = canvas != null;
        bool hudExists = gameHUD != null;
        bool eventSystemExists = eventSystem != null;
        
        Log($"Canvas: {Status(canvasExists)}");
        Log($"Game HUD: {Status(hudExists)}");
        Log($"Event System: {Status(eventSystemExists)}");
        
        if (hudExists)
        {
            TestHUDFunctionality(gameHUD);
        }
        
        bool result = canvasExists && hudExists && eventSystemExists;
        Log($"UI System: {Status(result)}");
        return result;
    }

    private void TestHUDFunctionality(GameHUD hud)
    {
        hud.ForceRefreshUI();
        Log("  HUD refresh test completed");
    }

    private bool TestGameplayMechanics()
    {
        Debug.Log("⚙️ Testing Gameplay Mechanics...");
        
        var collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        var checkpoints = FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
        var camera = Camera.main;
        var cameraFollow = camera?.GetComponent<CameraFollow2D>();
        
        bool collectiblesExist = collectibles.Length > 0;
        bool hazardsConfigured = hazards.Length >= 0; // Optional
        bool checkpointsExist = checkpoints.Length > 0;
        bool cameraSetup = camera != null && cameraFollow != null && camera.orthographic;
        
        Log($"Collectibles: {collectibles.Length} found");
        Log($"Hazards: {hazards.Length} found");
        Log($"Checkpoints: {checkpoints.Length} found");
        Log($"Camera System: {Status(cameraSetup)}");
        
        TestSpecialMechanics();
        
        bool result = collectiblesExist && checkpointsExist && cameraSetup;
        Log($"Gameplay Mechanics: {Status(result)}");
        return result;
    }

    private void TestSpecialMechanics()
    {
        var slipGates = FindObjectsByType<SlipNSlideGate>(FindObjectsSortMode.None);
        var bosses = FindObjectsByType<PurplePigBoss>(FindObjectsSortMode.None);
        var powerUps = FindObjectsByType<PowerUpPickup>(FindObjectsSortMode.None);
        
        Log($"  Slip-n-Slide Gates: {slipGates.Length}");
        Log($"  Bosses: {bosses.Length}");
        Log($"  Power-ups: {powerUps.Length}");
    }

    private void LogDeploymentInstructions()
    {
        Debug.Log("");
        Debug.Log("🎯 DEPLOYMENT READY!");
        Debug.Log("===================");
        Debug.Log("📋 Quick Start Guide:");
        Debug.Log("1. Press Play to test current scene");
        Debug.Log("2. Use WASD/Arrows to move player");
        Debug.Log("3. Space/W/Up to jump");
        Debug.Log("4. S/Down to crouch");
        Debug.Log("5. X to use abilities (Hose/Chiliguaro)");
        Debug.Log("6. Collect items and test checkpoints");
        Debug.Log("");
        Debug.Log("🏗️ To create new chapters:");
        Debug.Log("• Use WWIII/Create Chapter Data Assets");
        Debug.Log("• Use WWIII/Create Chapter Scenes");
        Debug.Log("• Run WWIII_Validator for scene validation");
        Debug.Log("");
        Debug.Log("📚 Documentation: ChapterData/WWIII_CHAPTER_SYSTEM_README.md");
    }

    private void LogTroubleshootingSteps()
    {
        Debug.Log("");
        Debug.LogWarning("🔧 TROUBLESHOOTING REQUIRED");
        Debug.Log("============================");
        
        if (!chaptersReady)
        {
            Debug.Log("❌ Chapter System Issues:");
            Debug.Log("  • Run WWIII/Create Chapter Data Assets");
            Debug.Log("  • Ensure ChapterManager is in scene");
        }
        
        if (!playerSystemReady)
        {
            Debug.Log("❌ Player System Issues:");
            Debug.Log("  • Add PlayerController component to player");
            Debug.Log("  • Verify Rigidbody2D and Collider2D setup");
            Debug.Log("  • Check player tag is 'Player'");
        }
        
        if (!uiSystemReady)
        {
            Debug.Log("❌ UI System Issues:");
            Debug.Log("  • Create Canvas with GameHUD component");
            Debug.Log("  • Add EventSystem to scene");
            Debug.Log("  • Verify UI components are assigned");
        }
        
        if (!gameplayReady)
        {
            Debug.Log("❌ Gameplay Issues:");
            Debug.Log("  • Add collectibles to scene");
            Debug.Log("  • Create at least one checkpoint");
            Debug.Log("  • Ensure camera has CameraFollow2D");
            Debug.Log("  • Set camera to orthographic");
        }
        
        Debug.Log("");
        Debug.Log("🛠️ Auto-fix available: Run WWIII_Validator component");
    }

    private string Status(bool condition)
    {
        return condition ? "✅" : "❌";
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"  {message}");
        }
    }

    [ContextMenu("Test Damage System")]
    public void TestDamageSystem()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            int originalHealth = playerHealth.CurrentHP;
            playerHealth.ApplyDamage(1, Vector2.right * 5f, 0.15f, 0.75f);
            Debug.Log($"Damage test: {originalHealth} -> {playerHealth.CurrentHP}");
        }
    }

    [ContextMenu("Test Ability System")]
    public void TestAbilitySystem()
    {
        var playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        if (playerAbilities != null)
        {
            playerAbilities.GrantChiliguaro();
            Debug.Log("Granted Chiliguaro - test with X key");
        }
    }

    [ContextMenu("Load Meadowbrook Park")]
    public void LoadMeadowbrookPark()
    {
        var chapterManager = FindFirstObjectByType<ChapterManager>();
        if (chapterManager != null)
        {
            chapterManager.LoadChapter("meadowbrook-park");
            Debug.Log("Loaded Meadowbrook Park chapter");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Open Chapter System Documentation")]
    public void OpenDocumentation()
    {
        string path = "Assets/_Project/Scripts/ChapterData/WWIII_CHAPTER_SYSTEM_README.md";
        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);
    }
#endif
}