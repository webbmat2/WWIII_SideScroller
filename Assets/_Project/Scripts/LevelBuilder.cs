using UnityEngine;

[AddComponentMenu("Level Design/Level Builder")]
public class LevelBuilder : MonoBehaviour
{
    [Header("Building Blocks")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private GameObject movingPlatformPrefab;
    [SerializeField] private GameObject oneWayPlatformPrefab;

    [Header("Encounter Prefabs")]
    [SerializeField] private GameObject patrolEnemyPrefab;
    [SerializeField] private GameObject turretEnemyPrefab;
    [SerializeField] private GameObject breakableWallPrefab;

    [Header("Quick Build Tools")]
    [SerializeField] private bool showBuildGrid = true;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Vector2Int buildArea = new Vector2Int(50, 20);

    private void Reset()
    {
        // Try to auto-find prefabs if not assigned
        if (groundTilePrefab == null)
        {
            var ground = Resources.Load<GameObject>("GroundTile");
            if (ground != null) groundTilePrefab = ground;
        }
    }

    [ContextMenu("Create Hazard Lane")]
    public void CreateHazardLane()
    {
        CreateHazardLane(transform.position, 3, true);
    }

    public void CreateHazardLane(Vector3 startPos, int length = 3, bool addCoinReward = true)
    {
        var parent = new GameObject("Hazard Lane").transform;
        parent.SetParent(transform);
        parent.position = startPos;

        // Create spike tiles
        for (int i = 0; i < length; i++)
        {
            Vector3 spikePos = startPos + Vector3.right * i;
            var spike = CreateSpike(spikePos);
            spike.transform.SetParent(parent);
        }

        // Add coin reward just beyond hazard
        if (addCoinReward && coinPrefab != null)
        {
            Vector3 coinPos = startPos + Vector3.right * (length + 0.5f) + Vector3.up * 0.5f;
            var coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, parent);
        }
    }

    [ContextMenu("Create Patrol Setup")]
    public void CreatePatrolSetup()
    {
        CreatePatrolSetup(transform.position);
    }

    public void CreatePatrolSetup(Vector3 position)
    {
        var parent = new GameObject("Patrol Setup").transform;
        parent.SetParent(transform);
        parent.position = position;

        // Create ground platform
        for (int i = 0; i < 6; i++)
        {
            Vector3 groundPos = position + Vector3.right * i;
            var ground = CreateGround(groundPos);
            ground.transform.SetParent(parent);
        }

        // Add patrol enemy
        if (patrolEnemyPrefab != null)
        {
            Vector3 enemyPos = position + Vector3.right * 3f + Vector3.up * 1f;
            var enemy = Instantiate(patrolEnemyPrefab, enemyPos, Quaternion.identity, parent);
        }

        // Add some coins for risk/reward
        if (coinPrefab != null)
        {
            Vector3 coinPos = position + Vector3.right * 1.5f + Vector3.up * 1.5f;
            var coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, parent);
        }
    }

    [ContextMenu("Create Vertical Ladder")]
    public void CreateVerticalLadder()
    {
        CreateVerticalLadder(transform.position, 5);
    }

    public void CreateVerticalLadder(Vector3 startPos, int height = 5)
    {
        var parent = new GameObject("Vertical Ladder").transform;
        parent.SetParent(transform);
        parent.position = startPos;

        // Create one-way platforms every 2 units
        for (int i = 0; i < height; i += 2)
        {
            Vector3 platformPos = startPos + Vector3.up * i;
            var platform = CreateOneWayPlatform(platformPos);
            platform.transform.SetParent(parent);

            // Add coin every second platform
            if (coinPrefab != null && i > 0)
            {
                Vector3 coinPos = platformPos + Vector3.up * 0.5f;
                var coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, parent);
            }
        }

        // Add spike at top side as mentioned in requirements
        if (spikePrefab != null)
        {
            Vector3 spikePos = startPos + Vector3.up * height + Vector3.right * 2f;
            var spike = CreateSpike(spikePos);
            spike.transform.SetParent(parent);
        }
    }

    [ContextMenu("Create Secret Cache")]
    public void CreateSecretCache()
    {
        CreateSecretCache(transform.position);
    }

    public void CreateSecretCache(Vector3 position)
    {
        var parent = new GameObject("Secret Cache").transform;
        parent.SetParent(transform);
        parent.position = position;

        // Create breakable wall
        if (breakableWallPrefab != null)
        {
            var wall = Instantiate(breakableWallPrefab, position, Quaternion.identity, parent);
        }

        // Create coin cache behind wall
        if (coinPrefab != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 coinPos = position + Vector3.right * (1.5f + i * 0.3f) + 
                                Vector3.up * (0.5f + Mathf.Sin(i) * 0.3f);
                var coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, parent);
            }
        }
    }

    // Helper methods for creating individual elements
    public GameObject CreateGround(Vector3 position)
    {
        if (groundTilePrefab != null)
        {
            return Instantiate(groundTilePrefab, position, Quaternion.identity);
        }
        
        // Fallback: create simple ground tile
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.position = position;
        ground.layer = LayerMask.NameToLayer("Ground");
        ground.name = "Ground Tile";
        return ground;
    }

    public GameObject CreateSpike(Vector3 position)
    {
        if (spikePrefab != null)
        {
            return Instantiate(spikePrefab, position, Quaternion.identity);
        }
        
        // Fallback: create simple spike
        var spike = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        spike.transform.position = position;
        spike.name = "Spike";
        var damageScript = spike.AddComponent<DamageOnTouch>();
        return spike;
    }

    public GameObject CreateOneWayPlatform(Vector3 position)
    {
        if (oneWayPlatformPrefab != null)
        {
            return Instantiate(oneWayPlatformPrefab, position, Quaternion.identity);
        }
        
        // Fallback: create simple one-way platform
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(3f, 0.2f, 1f);
        platform.name = "One-Way Platform";
        platform.AddComponent<OneWayPlatform>();
        return platform;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showBuildGrid) return;

        Gizmos.color = Color.gray;
        Vector3 center = transform.position;
        
        // Draw grid
        for (int x = -buildArea.x/2; x <= buildArea.x/2; x++)
        {
            for (int y = -buildArea.y/2; y <= buildArea.y/2; y++)
            {
                Vector3 gridPos = center + new Vector3(x * gridSize, y * gridSize, 0);
                Gizmos.DrawWireCube(gridPos, Vector3.one * gridSize * 0.1f);
            }
        }
    }
#endif
}