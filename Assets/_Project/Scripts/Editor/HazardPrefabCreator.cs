using UnityEngine;
using UnityEditor;

public class HazardPrefabCreator : EditorWindow
{
    [MenuItem("WWIII/Create Hazard Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<HazardPrefabCreator>("Hazard Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hazard Prefab Creator", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Hazard_Spikes.prefab"))
        {
            CreateSpikesPrefab();
        }
        
        if (GUILayout.Button("Create Hazard_Pit.prefab"))
        {
            CreatePitPrefab();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Layers"))
        {
            SetupLayers();
        }
    }

    private void CreateSpikesPrefab()
    {
        // Create spike GameObject
        var spikeGO = new GameObject("Hazard_Spikes");
        
        // Add SpriteRenderer
        var spriteRenderer = spikeGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        spriteRenderer.color = Color.red;
        
        // Add BoxCollider2D
        var collider = spikeGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;
        
        // Add DamageOnTouch
        var damageScript = spikeGO.AddComponent<DamageOnTouch>();
        
        // Set layer
        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer == -1) hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1) spikeGO.layer = hazardLayer;
        
        // Create prefab
        string prefabPath = "Assets/_Project/Prefabs/Hazard_Spikes.prefab";
        PrefabUtility.SaveAsPrefabAsset(spikeGO, prefabPath);
        
        // Cleanup
        DestroyImmediate(spikeGO);
        
        Debug.Log($"Created {prefabPath}");
        AssetDatabase.Refresh();
    }

    private void CreatePitPrefab()
    {
        // Create pit GameObject
        var pitGO = new GameObject("Hazard_Pit");
        
        // Add EdgeCollider2D for pit shape
        var edgeCollider = pitGO.AddComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
        
        // Define pit shape (rectangular pit)
        Vector2[] points = new Vector2[]
        {
            new Vector2(-2f, 0f),   // Top left
            new Vector2(-2f, -2f),  // Bottom left
            new Vector2(2f, -2f),   // Bottom right
            new Vector2(2f, 0f)     // Top right
        };
        edgeCollider.points = points;
        
        // Add DamageOnTouch
        var damageScript = pitGO.AddComponent<DamageOnTouch>();
        
        // Set layer
        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer == -1) hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1) pitGO.layer = hazardLayer;
        
        // Create prefab
        string prefabPath = "Assets/_Project/Prefabs/Hazard_Pit.prefab";
        PrefabUtility.SaveAsPrefabAsset(pitGO, prefabPath);
        
        // Cleanup
        DestroyImmediate(pitGO);
        
        Debug.Log($"Created {prefabPath}");
        AssetDatabase.Refresh();
    }

    private void SetupLayers()
    {
        Debug.Log("Setting up layers and collision matrix...");
        
        // Note: Layer creation requires manual setup in Project Settings
        // This is just a reminder
        EditorUtility.DisplayDialog("Layer Setup", 
            "Please manually create these layers in Project Settings → Tags and Layers:\n\n" +
            "• Player\n" +
            "• Ground\n" +
            "• Hazard\n" +
            "• Collectible\n" +
            "• CameraBounds\n\n" +
            "Then set up collision matrix in Project Settings → Physics 2D", 
            "OK");
    }
}