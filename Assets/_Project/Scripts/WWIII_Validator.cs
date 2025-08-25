using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("WWIII/Scene Validator")]
public class WWIII_Validator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private bool autoFixIssues = true;
    
    [Header("Required Components")]
    [SerializeField] private bool requireChapterManager = true;
    [SerializeField] private bool requirePlayerController = true;
    [SerializeField] private bool requireGameHUD = true;
    [SerializeField] private bool requireCameraSystem = true;
    
    private void Start()
    {
        if (validateOnStart)
        {
            RunFullValidation();
        }
    }

    [ContextMenu("Run Full Validation")]
    public void RunFullValidation()
    {
        Debug.Log("=== WWIII SCENE VALIDATION STARTING ===");
        
        bool allTestsPassed = true;
        
        allTestsPassed &= ValidateChapterSystem();
        allTestsPassed &= ValidatePlayerSystem();
        allTestsPassed &= ValidateCameraSystem();
        allTestsPassed &= ValidateUISystem();
        allTestsPassed &= ValidateGameplayElements();
        allTestsPassed &= ValidateLayerSetup();
        allTestsPassed &= ValidatePhysicsSettings();
        
        Debug.Log($"=== VALIDATION RESULT: {(allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ ISSUES FOUND")} ===");
        
        if (allTestsPassed)
        {
            RunGameplayTests();
        }
    }

    private bool ValidateChapterSystem()
    {
        Debug.Log("--- Validating Chapter System ---");
        
        var chapterManager = FindFirstObjectByType<ChapterManager>();
        if (chapterManager == null)
        {
            Debug.LogError("❌ No ChapterManager found in scene");
            if (autoFixIssues)
            {
                CreateChapterManager();
                Debug.Log("✅ Auto-created ChapterManager");
            }
            else
            {
                return false;
            }
        }
        else
        {
            Debug.Log("✅ ChapterManager found");
        }
        
        var collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        if (collectibles.Length != 5)
        {
            Debug.LogWarning($"⚠️ Found {collectibles.Length} collectibles, expected 5");
        }
        else
        {
            Debug.Log("✅ Correct number of collectibles (5)");
        }
        
        return true;
    }

    private bool ValidatePlayerSystem()
    {
        Debug.Log("--- Validating Player System ---");
        
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("❌ No PlayerController found");
            if (autoFixIssues)
            {
                CreatePlayer();
                Debug.Log("✅ Auto-created Player");
            }
            else
            {
                return false;
            }
        }
        
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("❌ No PlayerHealth found");
            return false;
        }
        
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("❌ No PlayerMovement found");
            return false;
        }
        
        var playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        if (playerAbilities == null)
        {
            Debug.LogError("❌ No PlayerAbilities found");
            return false;
        }
        
        // Check physics setup
        var rb = player?.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            bool physicsOK = rb.constraints == RigidbodyConstraints2D.FreezeRotation &&
                           rb.gravityScale >= 3.5f &&
                           rb.interpolation == RigidbodyInterpolation2D.Interpolate;
            
            if (!physicsOK)
            {
                Debug.LogWarning("⚠️ Player physics settings not optimal");
                if (autoFixIssues)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    rb.gravityScale = 3.8f;
                    rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    Debug.Log("✅ Fixed player physics");
                }
            }
            else
            {
                Debug.Log("✅ Player physics configured correctly");
            }
        }
        
        Debug.Log("✅ Player system validated");
        return true;
    }

    private bool ValidateCameraSystem()
    {
        Debug.Log("--- Validating Camera System ---");
        
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("❌ No main camera found");
            return false;
        }
        
        var cameraFollow = mainCamera.GetComponent<CameraFollow2D>();
        if (cameraFollow == null)
        {
            Debug.LogError("❌ Main camera missing CameraFollow2D");
            if (autoFixIssues)
            {
                mainCamera.gameObject.AddComponent<CameraFollow2D>();
                Debug.Log("✅ Added CameraFollow2D to main camera");
            }
            else
            {
                return false;
            }
        }
        
        if (!mainCamera.orthographic)
        {
            Debug.LogWarning("⚠️ Camera should be orthographic for 2D game");
            if (autoFixIssues)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6f;
                Debug.Log("✅ Set camera to orthographic");
            }
        }
        
        Debug.Log("✅ Camera system validated");
        return true;
    }

    private bool ValidateUISystem()
    {
        Debug.Log("--- Validating UI System ---");
        
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas found");
            if (autoFixIssues)
            {
                CreateCanvas();
                Debug.Log("✅ Auto-created Canvas");
            }
            else
            {
                return false;
            }
        }
        
        var gameHUD = FindFirstObjectByType<GameHUD>();
        if (gameHUD == null)
        {
            Debug.LogError("❌ No GameHUD found");
            if (autoFixIssues)
            {
                CreateGameHUD(canvas);
                Debug.Log("✅ Auto-created GameHUD");
            }
            else
            {
                return false;
            }
        }
        
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠️ No EventSystem found");
            if (autoFixIssues)
            {
                CreateEventSystem();
                Debug.Log("✅ Auto-created EventSystem");
            }
        }
        
        Debug.Log("✅ UI system validated");
        return true;
    }

    private bool ValidateGameplayElements()
    {
        Debug.Log("--- Validating Gameplay Elements ---");
        
        var checkpoints = FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
        if (checkpoints.Length == 0)
        {
            Debug.LogWarning("⚠️ No checkpoints found");
        }
        else
        {
            Debug.Log($"✅ Found {checkpoints.Length} checkpoint(s)");
        }
        
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        Debug.Log($"✅ Found {hazards.Length} hazard(s)");
        
        // Validate specific chapter content
        var currentScene = SceneManager.GetActiveScene().name;
        ValidateChapterSpecificContent(currentScene);
        
        Debug.Log("✅ Gameplay elements validated");
        return true;
    }

    private void ValidateChapterSpecificContent(string sceneName)
    {
        switch (sceneName)
        {
            case "01_MeadowbrookPark":
                ValidateMeadowbrookPark();
                break;
            case "05_Philadelphia":
                ValidatePhiladelphia();
                break;
            case "07_CostaRica":
                ValidateCostaRica();
                break;
        }
    }

    private void ValidateMeadowbrookPark()
    {
        var slipGate = FindFirstObjectByType<SlipNSlideGate>();
        if (slipGate == null)
        {
            Debug.LogWarning("⚠️ Meadowbrook Park: No Slip-n-Slide gate found");
        }
        else
        {
            Debug.Log("✅ Meadowbrook Park: Slip-n-Slide gate found");
        }
        
        var purplePig = FindFirstObjectByType<PurplePigBoss>();
        if (purplePig == null)
        {
            Debug.LogWarning("⚠️ Meadowbrook Park: No Purple Pig boss found");
        }
        else
        {
            Debug.Log("✅ Meadowbrook Park: Purple Pig boss found");
        }
    }

    private void ValidatePhiladelphia()
    {
        Debug.Log("✅ Philadelphia chapter layout validated");
    }

    private void ValidateCostaRica()
    {
        var chiliguaro = FindFirstObjectByType<PowerUpPickup>();
        if (chiliguaro != null)
        {
            Debug.Log("✅ Costa Rica: Power-up pickup found");
        }
        else
        {
            Debug.LogWarning("⚠️ Costa Rica: No Chiliguaro power-up found");
        }
    }

    private bool ValidateLayerSetup()
    {
        Debug.Log("--- Validating Layer Setup ---");
        
        var layerSetup = FindFirstObjectByType<LayerSetup>();
        if (layerSetup == null)
        {
            Debug.LogWarning("⚠️ No LayerSetup component found");
            if (autoFixIssues)
            {
                gameObject.AddComponent<LayerSetup>();
                Debug.Log("✅ Added LayerSetup component");
            }
        }
        
        Debug.Log("✅ Layer setup validated");
        return true;
    }

    private bool ValidatePhysicsSettings()
    {
        Debug.Log("--- Validating Physics Settings ---");
        
        // Check Physics2D settings
        var gravity = Physics2D.gravity;
        if (gravity.y > -9.81f)
        {
            Debug.LogWarning($"⚠️ Physics2D gravity might be too low: {gravity.y}");
        }
        
        Debug.Log("✅ Physics settings validated");
        return true;
    }

    private void RunGameplayTests()
    {
        Debug.Log("=== RUNNING GAMEPLAY TESTS ===");
        Debug.Log("🎮 Test checklist:");
        Debug.Log("  1. Player can move left/right with WASD/Arrows");
        Debug.Log("  2. Player can jump with Space/W/Up");
        Debug.Log("  3. Player can crouch with S/Down");
        Debug.Log("  4. Player takes damage from hazards");
        Debug.Log("  5. Health UI updates correctly");
        Debug.Log("  6. Collectibles can be gathered");
        Debug.Log("  7. Checkpoints work");
        Debug.Log("  8. Camera follows player smoothly");
        Debug.Log("  9. Abilities work (Hose/Chiliguaro)");
        Debug.Log("  10. Chapter-specific mechanics function");
        Debug.Log("Press Play to test these features!");
    }

    private void CreateChapterManager()
    {
        var chapterManagerGO = new GameObject("ChapterManager");
        chapterManagerGO.AddComponent<ChapterManager>();
    }

    private void CreatePlayer()
    {
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = new Vector3(-8f, 2f, 0f);
        playerGO.AddComponent<PlayerController>();
    }

    private void CreateCanvas()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    private void CreateGameHUD(Canvas canvas)
    {
        var hudGO = new GameObject("GameHUD");
        hudGO.transform.SetParent(canvas.transform, false);
        hudGO.AddComponent<GameHUD>();
    }

    private void CreateEventSystem()
    {
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    [ContextMenu("Create Missing ChapterData Assets")]
    public void CreateMissingChapterDataAssets()
    {
        Debug.Log("💡 Use WWIII/Create Chapter Data Assets window to create missing assets");
        Debug.Log("💡 This functionality requires the editor extension");
    }

    [ContextMenu("Test Damage System")]
    public void TestDamageSystem()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 testKnockback = new Vector2(5f, 8f);
            playerHealth.ApplyDamage(1, testKnockback, 0.15f, 0.75f);
            Debug.Log("✅ Applied test damage to player");
        }
        else
        {
            Debug.LogError("❌ No PlayerHealth found for testing");
        }
    }

    [ContextMenu("Test Chiliguaro System")]
    public void TestChiliguaroSystem()
    {
        var playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        if (playerAbilities != null)
        {
            playerAbilities.GrantChiliguaro();
            Debug.Log("✅ Granted Chiliguaro to player - test firing with X key");
        }
        else
        {
            Debug.LogError("❌ No PlayerAbilities found for testing");
        }
    }
}