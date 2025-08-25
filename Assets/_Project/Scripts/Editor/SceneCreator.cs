using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneCreator : EditorWindow
{
    [MenuItem("WWIII/Create Chapter Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SceneCreator>("Scene Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("WWIII Chapter Scene Creator", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create All 7 Chapter Scenes"))
        {
            CreateAllChapterScenes();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create 01_MeadowbrookPark"))
        {
            CreateMeadowbrookParkScene();
        }
        
        if (GUILayout.Button("Create 02_TorchLake"))
        {
            CreateTorchLakeScene();
        }
        
        if (GUILayout.Button("Create 03_NotreDame"))
        {
            CreateNotreDameScene();
        }
        
        if (GUILayout.Button("Create 04_HighSchool"))
        {
            CreateHighSchoolScene();
        }
        
        if (GUILayout.Button("Create 05_Philadelphia"))
        {
            CreatePhiladelphiaScene();
        }
        
        if (GUILayout.Button("Create 06_ParsonsChicken"))
        {
            CreateParsonsChickenScene();
        }
        
        if (GUILayout.Button("Create 07_CostaRica"))
        {
            CreateCostaRicaScene();
        }
    }

    private void CreateAllChapterScenes()
    {
        CreateMeadowbrookParkScene();
        CreateTorchLakeScene();
        CreateNotreDameScene();
        CreateHighSchoolScene();
        CreatePhiladelphiaScene();
        CreateParsonsChickenScene();
        CreateCostaRicaScene();
        
        Debug.Log("All 7 chapter scenes created!");
    }

    private void CreateMeadowbrookParkScene()
    {
        CreateChapterScene("01_MeadowbrookPark", "meadowbrook-park", true);
    }

    private void CreateTorchLakeScene()
    {
        CreateChapterScene("02_TorchLake", "torch-lake", false);
    }

    private void CreateNotreDameScene()
    {
        CreateChapterScene("03_NotreDame", "notre-dame", false);
    }

    private void CreateHighSchoolScene()
    {
        CreateChapterScene("04_HighSchool", "high-school", false);
    }

    private void CreatePhiladelphiaScene()
    {
        CreateChapterScene("05_Philadelphia", "philadelphia", true);
    }

    private void CreateParsonsChickenScene()
    {
        CreateChapterScene("06_ParsonsChicken", "parsons-chicken", false);
    }

    private void CreateCostaRicaScene()
    {
        CreateChapterScene("07_CostaRica", "costa-rica", true);
    }

    private void CreateChapterScene(string sceneName, string chapterId, bool hasBoss)
    {
        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Create basic scene structure
        CreateBasicSceneStructure(chapterId, hasBoss);
        
        // Save scene
        string scenePath = $"Assets/_Project/Scenes/{sceneName}.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log($"Created scene: {scenePath}");
    }

    private void CreateBasicSceneStructure(string chapterId, bool hasBoss)
    {
        // Main Camera with CameraFollow2D
        var mainCamera = new GameObject("Main Camera");
        var camera = mainCamera.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.backgroundColor = new Color(0.2f, 0.3f, 0.5f);
        mainCamera.tag = "MainCamera";
        mainCamera.AddComponent<AudioListener>();
        
        // Add CameraFollow2D
        var cameraFollow = mainCamera.AddComponent<CameraFollow2D>();
        
        // Create Ground with Tilemap
        var groundGO = new GameObject("Ground");
        var grid = groundGO.AddComponent<Grid>();
        
        var tilemapGO = new GameObject("Tilemap");
        tilemapGO.transform.SetParent(groundGO.transform);
        var tilemap = tilemapGO.AddComponent<Tilemap>();
        var tilemapRenderer = tilemapGO.AddComponent<TilemapRenderer>();
        var tilemapCollider = tilemapGO.AddComponent<TilemapCollider2D>();
        
        tilemapGO.layer = LayerMask.NameToLayer("Ground");
        
        // Create Camera Bounds
        var cameraBounds = new GameObject("CameraBounds");
        var boundsCollider = cameraBounds.AddComponent<BoxCollider2D>();
        boundsCollider.isTrigger = true;
        boundsCollider.size = new Vector2(20f, 12f);
        cameraBounds.layer = LayerMask.NameToLayer("UI"); // Use UI layer as fallback for camera bounds
        
        // Create Player spawn point
        var playerSpawn = new GameObject("PlayerSpawn");
        playerSpawn.transform.position = new Vector3(-8f, 2f, 0f);
        
        // Create Player (will be configured by ChapterManager)
        var player = new GameObject("Player");
        player.transform.position = playerSpawn.transform.position;
        player.tag = "Player";
        
        // Add PlayerController (which will auto-add other components)
        var playerController = player.AddComponent<PlayerController>();
        
        // Create Checkpoint at start
        var checkpoint = new GameObject("Checkpoint");
        checkpoint.transform.position = new Vector3(-7f, 2f, 0f);
        checkpoint.AddComponent<CheckpointTrigger>();
        var checkpointCollider = checkpoint.AddComponent<BoxCollider2D>();
        checkpointCollider.isTrigger = true;
        
        // Create 5 Collectibles
        for (int i = 0; i < 5; i++)
        {
            var collectible = new GameObject($"Collectible_{i+1}");
            collectible.transform.position = new Vector3(-6f + i * 3f, 3f, 0f);
            collectible.AddComponent<Collectible>();
            var collectibleCollider = collectible.AddComponent<BoxCollider2D>();
            collectibleCollider.isTrigger = true;
            collectible.layer = LayerMask.NameToLayer("Default");
        }
        
        // Create Canvas and UI
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Add GameHUD
        var hudGO = new GameObject("GameHUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        hudGO.AddComponent<GameHUD>();
        
        // Create EventSystem
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create ChapterManager
        var chapterManagerGO = new GameObject("ChapterManager");
        var chapterManager = chapterManagerGO.AddComponent<ChapterManager>();
        
        // Chapter-specific additions
        CreateChapterSpecificContent(chapterId, hasBoss);
        
        Debug.Log($"Created basic scene structure for chapter: {chapterId}");
    }

    private void CreateChapterSpecificContent(string chapterId, bool hasBoss)
    {
        switch (chapterId)
        {
            case "meadowbrook-park":
                CreateMeadowbrookParkContent();
                break;
            case "philadelphia":
                CreatePhiladelphiaContent();
                break;
            case "costa-rica":
                CreateCostaRicaContent();
                break;
        }
    }

    private void CreateMeadowbrookParkContent()
    {
        // Create Slip-n-Slide Gate
        var slipGate = new GameObject("SlipNSlideGate");
        slipGate.transform.position = new Vector3(0f, 1f, 0f);
        slipGate.AddComponent<SlipNSlideGate>();
        var gateCollider = slipGate.AddComponent<BoxCollider2D>();
        gateCollider.size = new Vector2(1f, 2f);
        gateCollider.isTrigger = false; // Starts solid
        
        // Create Purple Pig Boss
        var purplePig = new GameObject("PurplePigBoss");
        purplePig.transform.position = new Vector3(8f, 2f, 0f);
        purplePig.AddComponent<PurplePigBoss>();
        var pigCollider = purplePig.AddComponent<BoxCollider2D>();
        pigCollider.size = new Vector2(1f, 1f);
        
        // Create GMC Jimmy vehicle cameo
        var vehicle = new GameObject("GMC_Jimmy_1996");
        vehicle.transform.position = new Vector3(5f, -1f, 0f);
        // Add sprite renderer for visual (placeholder)
        var vehicleRenderer = vehicle.AddComponent<SpriteRenderer>();
        vehicleRenderer.color = new Color(0.5f, 0f, 0f); // Burgundy color
        
        Debug.Log("Created Meadowbrook Park content: Slip-n-Slide gate, Purple Pig boss, GMC Jimmy");
    }

    private void CreatePhiladelphiaContent()
    {
        // Create Hamburger Helper Glove Boss
        var hamburgerBoss = new GameObject("HamburgerHelperGlove");
        hamburgerBoss.transform.position = new Vector3(8f, 2f, 0f);
        // Add placeholder boss component
        var bossCollider = hamburgerBoss.AddComponent<BoxCollider2D>();
        bossCollider.size = new Vector2(2f, 2f);
        
        Debug.Log("Created Philadelphia content: Hamburger Helper Glove boss");
    }

    private void CreateCostaRicaContent()
    {
        // Create Araña Reina (Spider Queen) Boss
        var spiderQueen = new GameObject("AranaReinaBoss");
        spiderQueen.transform.position = new Vector3(12f, 4f, 0f);
        // Add placeholder boss component
        var bossCollider = spiderQueen.AddComponent<BoxCollider2D>();
        bossCollider.size = new Vector2(3f, 3f);
        
        // Create Casa Lumpusita (ending location)
        var casaLumpusita = new GameObject("CasaLumpusita");
        casaLumpusita.transform.position = new Vector3(15f, 2f, 0f);
        
        // Create jungle hazards (jaguars, spiders)
        var jaguar = new GameObject("JaguarHazard");
        jaguar.transform.position = new Vector3(5f, 1f, 0f);
        jaguar.AddComponent<DamageOnTouch>();
        var jaguarCollider = jaguar.AddComponent<BoxCollider2D>();
        jaguarCollider.isTrigger = true;
        
        var spider = new GameObject("SpiderHazard");
        spider.transform.position = new Vector3(7f, 3f, 0f);
        spider.AddComponent<DamageOnTouch>();
        var spiderCollider = spider.AddComponent<BoxCollider2D>();
        spiderCollider.isTrigger = true;
        
        // Create Chiliguaro power-up
        var chiliguaro = new GameObject("ChiliguaroPowerUp");
        chiliguaro.transform.position = new Vector3(3f, 2f, 0f);
        // Add power-up script when created
        var powerUpCollider = chiliguaro.AddComponent<BoxCollider2D>();
        powerUpCollider.isTrigger = true;
        
        Debug.Log("Created Costa Rica content: Spider Queen boss, jungle hazards, Chiliguaro power-up");
    }
}