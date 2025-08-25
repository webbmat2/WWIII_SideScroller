using UnityEngine;

[AddComponentMenu("WWIII/Create Hazard Prefabs")]
public class CreateHazardPrefabs : MonoBehaviour
{
    [Header("Prefab Creation")]
    [SerializeField] private bool createInScene = true;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    [ContextMenu("Create Spike Hazard")]
    public void CreateSpikeHazard()
    {
        var spikeGO = new GameObject("Hazard_Spikes");
        
        if (createInScene)
        {
            spikeGO.transform.position = spawnPosition + Vector3.right * 2f;
        }
        
        // Add visual (simple colored square)
        var spriteRenderer = spikeGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSimpleSprite();
        spriteRenderer.color = Color.red;
        
        // Add trigger collider
        var collider = spikeGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;
        
        // Add damage component
        var damageScript = spikeGO.AddComponent<DamageOnTouch>();
        
        // Set layer (use Water if Hazard doesn't exist)
        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer == -1) hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1) 
        {
            spikeGO.layer = hazardLayer;
            Debug.Log($"Set spike to layer: {LayerMask.LayerToName(hazardLayer)}");
        }
        
        Debug.Log($"✅ Created spike hazard at {spikeGO.transform.position}");
    }

    [ContextMenu("Create Pit Hazard")]
    public void CreatePitHazard()
    {
        var pitGO = new GameObject("Hazard_Pit");
        
        if (createInScene)
        {
            pitGO.transform.position = spawnPosition + Vector3.right * 5f;
        }
        
        // Add edge collider for pit shape
        var edgeCollider = pitGO.AddComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
        
        // Create simple pit shape
        Vector2[] points = new Vector2[]
        {
            new Vector2(-1f, 0f),   // Top left
            new Vector2(-1f, -2f),  // Bottom left
            new Vector2(1f, -2f),   // Bottom right
            new Vector2(1f, 0f)     // Top right
        };
        edgeCollider.points = points;
        
        // Add damage component
        var damageScript = pitGO.AddComponent<DamageOnTouch>();
        
        // Set layer
        int hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer == -1) hazardLayer = LayerMask.NameToLayer("Water");
        if (hazardLayer != -1) 
        {
            pitGO.layer = hazardLayer;
            Debug.Log($"Set pit to layer: {LayerMask.LayerToName(hazardLayer)}");
        }
        
        Debug.Log($"✅ Created pit hazard at {pitGO.transform.position}");
    }

    private Sprite CreateSimpleSprite()
    {
        // Create a simple white 1x1 texture for basic sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    [ContextMenu("Test Hazard on Player")]
    public void TestHazardOnPlayer()
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
            Debug.LogError("❌ No PlayerHealth2D found for testing");
        }
    }
}