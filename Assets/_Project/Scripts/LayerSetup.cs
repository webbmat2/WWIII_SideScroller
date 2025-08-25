using UnityEngine;

[AddComponentMenu("WWIII/Layer Setup")]
public class LayerSetup : MonoBehaviour
{
    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayers = -1;
    [SerializeField] private LayerMask hazardLayers = -1;
    [SerializeField] private LayerMask collectibleLayers = -1;
    [SerializeField] private LayerMask playerLayer = -1;
    [SerializeField] private LayerMask npcLayers = -1;
    [SerializeField] private LayerMask cameraBoundsLayer = -1;
    
    public static LayerMask GroundLayers { get; private set; }
    public static LayerMask HazardLayers { get; private set; }
    public static LayerMask CollectibleLayers { get; private set; }
    public static LayerMask PlayerLayer { get; private set; }
    public static LayerMask NPCLayers { get; private set; }
    public static LayerMask CameraBoundsLayer { get; private set; }

    private void Awake()
    {
        SetupLayerMasks();
        ValidateLayerSetup();
    }

    private void SetupLayerMasks()
    {
        // Setup default layer masks based on existing layers
        GroundLayers = CreateLayerMask("Ground");
        HazardLayers = CreateLayerMask("Water", "Hazard"); // Water as fallback
        CollectibleLayers = CreateLayerMask("Default");
        PlayerLayer = CreateLayerMask("Player", "Default"); // Default as fallback
        NPCLayers = CreateLayerMask("Default");
        CameraBoundsLayer = CreateLayerMask("UI", "Default"); // UI as fallback
        
        // Apply inspector overrides if set
        if (groundLayers != -1) GroundLayers = groundLayers;
        if (hazardLayers != -1) HazardLayers = hazardLayers;
        if (collectibleLayers != -1) CollectibleLayers = collectibleLayers;
        if (playerLayer != -1) PlayerLayer = playerLayer;
        if (npcLayers != -1) NPCLayers = npcLayers;
        if (cameraBoundsLayer != -1) CameraBoundsLayer = cameraBoundsLayer;
        
        Debug.Log("Layer masks configured");
    }

    private LayerMask CreateLayerMask(params string[] layerNames)
    {
        LayerMask mask = 0;
        
        foreach (string layerName in layerNames)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                mask |= (1 << layerIndex);
                return mask; // Return first valid layer found
            }
        }
        
        return 1; // Default layer if none found
    }

    private void ValidateLayerSetup()
    {
        Debug.Log("=== LAYER VALIDATION ===");
        Debug.Log($"Ground Layers: {LayerMaskToString(GroundLayers)}");
        Debug.Log($"Hazard Layers: {LayerMaskToString(HazardLayers)}");
        Debug.Log($"Collectible Layers: {LayerMaskToString(CollectibleLayers)}");
        Debug.Log($"Player Layer: {LayerMaskToString(PlayerLayer)}");
        Debug.Log($"NPC Layers: {LayerMaskToString(NPCLayers)}");
        Debug.Log($"Camera Bounds Layer: {LayerMaskToString(CameraBoundsLayer)}");
        
        // Check for required layers
        CheckLayer("Ground");
        CheckLayer("Player");
        CheckLayer("Hazard", "Water");
        CheckLayer("Collectible", "Default");
    }

    private void CheckLayer(string primaryLayer, string fallbackLayer = null)
    {
        int layerIndex = LayerMask.NameToLayer(primaryLayer);
        if (layerIndex != -1)
        {
            Debug.Log($"✅ {primaryLayer} layer exists");
        }
        else if (fallbackLayer != null)
        {
            int fallbackIndex = LayerMask.NameToLayer(fallbackLayer);
            if (fallbackIndex != -1)
            {
                Debug.Log($"⚠️ {primaryLayer} layer missing, using {fallbackLayer}");
            }
            else
            {
                Debug.LogError($"❌ Neither {primaryLayer} nor {fallbackLayer} layers exist!");
            }
        }
        else
        {
            Debug.LogError($"❌ {primaryLayer} layer missing!");
        }
    }

    private string LayerMaskToString(LayerMask mask)
    {
        string result = "";
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    if (result.Length > 0) result += ", ";
                    result += layerName;
                }
            }
        }
        return string.IsNullOrEmpty(result) ? "None" : result;
    }

    [ContextMenu("Auto-Assign Layers")]
    public void AutoAssignLayers()
    {
        AssignLayersToGameObjects();
    }

    private void AssignLayersToGameObjects()
    {
        // Find and assign layers to existing GameObjects
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex == -1) playerLayerIndex = 0; // Default layer
            player.gameObject.layer = playerLayerIndex;
            Debug.Log($"Assigned Player to layer: {LayerMask.LayerToName(playerLayerIndex)}");
        }
        
        var hazards = FindObjectsByType<DamageOnTouch>(FindObjectsSortMode.None);
        foreach (var hazard in hazards)
        {
            int hazardLayerIndex = LayerMask.NameToLayer("Water"); // Use Water as hazard layer
            if (hazardLayerIndex != -1)
            {
                hazard.gameObject.layer = hazardLayerIndex;
                Debug.Log($"Assigned {hazard.name} to hazard layer");
            }
        }
        
        var collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        foreach (var collectible in collectibles)
        {
            collectible.gameObject.layer = 0; // Default layer
        }
        
        Debug.Log("Auto-assigned layers to existing GameObjects");
    }

    // Static utility methods for other scripts
    public static bool IsGroundLayer(int layer)
    {
        return (GroundLayers & (1 << layer)) != 0;
    }

    public static bool IsHazardLayer(int layer)
    {
        return (HazardLayers & (1 << layer)) != 0;
    }

    public static bool IsPlayerLayer(int layer)
    {
        return (PlayerLayer & (1 << layer)) != 0;
    }
}