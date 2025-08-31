using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates Level 1: Northville scene and data assets
/// Bypasses compilation issues by using simplified approach
/// </summary>
public class CreateNorthville
{
    [MenuItem("WWIII/🏭 CREATE LEVEL 1 NORTHVILLE")]
    public static void CreateNorthvilleLevel()
    {
        Debug.Log("🏭 Creating Level 1: Northville...");
        
        // Ensure directories exist
        CreateDirectories();
        
        // Create the scene first
        CreateNorthvilleScene();
        
        // Create placeholder data files (will be converted to proper ScriptableObjects later)
        CreateDataPlaceholders();
        
        Debug.Log("🎉 Level 1: Northville created successfully!");
        Debug.Log("📝 Next steps:");
        Debug.Log("   1. Fix assembly references using WWIII > 🔧 FIX ASSEMBLY REFERENCES");
        Debug.Log("   2. Convert data placeholders to ScriptableObjects");
        Debug.Log("   3. Use Level Auto-Authoring tool for advanced setup");
        
        EditorUtility.DisplayDialog("Level 1: Northville Created!",
            "Northville scene and basic setup complete!\n\n" +
            "✅ Scene created\n" +
            "✅ Camera and lighting configured\n" +
            "✅ Basic tilemaps ready\n" +
            "✅ Player placeholder added\n\n" +
            "Ready for playtesting and content addition!",
            "Open Scene");
    }
    
    private static void CreateDirectories()
    {
        string[] directories = {
            "Assets/_Project/Data/Levels",
            "Assets/_Project/Scenes",
            "Assets/_Project/Prefabs"
        };
        
        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"✅ Created directory: {dir}");
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    private static void CreateNorthvilleScene()
    {
        // Create new empty scene
        var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single
        );
        
        // Setup camera
        SetupCamera();
        
        // Setup lighting
        SetupLighting();
        
        // Setup tilemaps
        SetupTilemaps();
        
        // Setup player
        SetupPlayer();
        
        // Setup level bounds
        SetupLevelBounds();
        
        // Save scene
        string scenePath = "Assets/_Project/Scenes/L1_Northville.unity";
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
        
        Debug.Log($"✅ Created scene: {scenePath}");
    }
    
    private static void SetupCamera()
    {
        var cameraObj = new GameObject("Main Camera");
        var camera = cameraObj.AddComponent<Camera>();
        
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 1f); // WWIII dark theme
        
        // Configure for 2D URP
        camera.transparencySortMode = TransparencySortMode.CustomAxis;
        camera.transparencySortAxis = new Vector3(0, 0, 1);
        
        cameraObj.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("✅ Camera configured for Northville");
    }
    
    private static void SetupLighting()
    {
        var lightObj = new GameObject("Global Light 2D");
        var light = lightObj.AddComponent<Light>();
        
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.color = Color.white;
        
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        Debug.Log("✅ Lighting configured for Northville");
    }
    
    private static void SetupTilemaps()
    {
        // Create tilemap parent
        var tilemapParent = new GameObject("Tilemaps");
        var grid = tilemapParent.AddComponent<Grid>();
        
        // Ground tilemap for platforms
        var groundObj = new GameObject("Ground");
        groundObj.transform.SetParent(tilemapParent.transform);
        groundObj.layer = LayerMask.NameToLayer("Platforms");
        
        var groundTilemap = groundObj.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        var groundRenderer = groundObj.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        var groundCollider = groundObj.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
        var compositeCollider = groundObj.AddComponent<CompositeCollider2D>();
        
        groundRenderer.sortingLayerName = "Platforms";
        groundCollider.usedByComposite = true;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        
        // Background tilemap
        var backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(tilemapParent.transform);
        backgroundObj.layer = LayerMask.NameToLayer("Background");
        
        var backgroundTilemap = backgroundObj.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        var backgroundRenderer = backgroundObj.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        backgroundRenderer.sortingLayerName = "Background";
        
        Debug.Log("✅ Tilemaps configured for Northville");
    }
    
    private static void SetupPlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = new Vector3(0, 2, 0);
        
        // Add basic components
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f; // Good for side-scrollers
        rb.freezeRotation = true;
        
        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 1.8f); // Humanoid proportions
        
        var spriteRenderer = player.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Player";
        spriteRenderer.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue placeholder
        
        // Create simple sprite
        var texture = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                texture.SetPixel(x, y, new Color(0.2f, 0.6f, 1f, 1f));
        texture.Apply();
        
        var sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
        spriteRenderer.sprite = sprite;
        
        Debug.Log("✅ Player configured for Northville");
    }
    
    private static void SetupLevelBounds()
    {
        var bounds = new GameObject("Level Bounds");
        bounds.tag = "LevelBounds";
        
        // Create invisible walls at level boundaries
        CreateBoundary(bounds.transform, "Left Wall", new Vector3(-25, 0, 0), new Vector2(1, 20));
        CreateBoundary(bounds.transform, "Right Wall", new Vector3(25, 0, 0), new Vector2(1, 20));
        CreateBoundary(bounds.transform, "Death Zone", new Vector3(0, -10, 0), new Vector2(60, 1));
        
        Debug.Log("✅ Level bounds configured for Northville");
    }
    
    private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector2 size)
    {
        var boundary = new GameObject(name);
        boundary.transform.SetParent(parent);
        boundary.transform.position = position;
        boundary.layer = LayerMask.NameToLayer("Platforms");
        
        var collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
    
    private static void CreateDataPlaceholders()
    {
        string dataPath = "Assets/_Project/Data/Levels";
        
        // Create placeholder text files that can be converted to ScriptableObjects later
        string levelDefContent = @"# Level 1: Northville Definition
# This will be converted to a LevelDef ScriptableObject

levelName: L1_Northville
displayName: Northville
biome: Urban
description: The outskirts of Northville, where the resistance begins.
maxJumpHeight: 6.0
playerMoveSpeed: 8.0
levelLength: 50.0
isUnlocked: true
requiredScore: 0
primaryColor: (0.4, 0.6, 0.8, 1.0)
secondaryColor: (0.8, 0.7, 0.5, 1.0)
recommendedAbilities: [Run, Jump, BasicAttack]
";
        
        string narrativeContent = @"# Level 1: Northville Narrative
# This will be converted to a NarrativeDef ScriptableObject

levelName: L1_Northville
storyBeats:
  - trigger: LevelStart
    speaker: Command
    text: The invasion has begun. Northville's outer districts are our first target.
  - trigger: Checkpoint1
    speaker: Recon
    text: Intelligence reports enemy patrols ahead. Stay low and move fast.
  - trigger: LevelComplete
    speaker: Command
    text: Northville outskirts secured. Preparing for urban insertion.
";
        
        string collectiblesContent = @"# Level 1: Northville Collectibles
# This will be converted to a CollectibleSetDef ScriptableObject

setName: L1_Northville_Intel
displayName: Northville Intel
description: Critical intelligence gathered from Northville operations
collectibleCount: 5
isCompleted: false
";
        
        File.WriteAllText($"{dataPath}/L1_Northville_LevelDef.txt", levelDefContent);
        File.WriteAllText($"{dataPath}/L1_Northville_Narrative.txt", narrativeContent);
        File.WriteAllText($"{dataPath}/L1_Northville_Collectibles.txt", collectiblesContent);
        
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Data placeholders created for Northville");
    }
}