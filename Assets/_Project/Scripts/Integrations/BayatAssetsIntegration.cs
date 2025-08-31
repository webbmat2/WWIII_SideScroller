using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.Integrations
{
    /// <summary>
    /// Integration for Bayat Platform Game Assets Ultimate
    /// Follows project rule: "Bayat for tiles/props/backgrounds/GUI"
    /// </summary>
    public class BayatAssetsIntegration : MonoBehaviour
    {
        [Header("Bayat Asset References")]
        [Tooltip("Tilemap for ground and platform tiles")]
        public Tilemap groundTilemap;
        
        [Tooltip("Tilemap for background decoration")]
        public Tilemap backgroundTilemap;
        
        [Tooltip("Tilemap for foreground details")]
        public Tilemap foregroundTilemap;

        [Header("Tile Collections")]
        [Tooltip("Ground and platform tiles from Bayat")]
        public TileBase[] groundTiles;
        
        [Tooltip("Background environment tiles")]
        public TileBase[] backgroundTiles;
        
        [Tooltip("Decorative prop tiles")]
        public TileBase[] propTiles;

        [Header("Prefab References")]
        [Tooltip("Bayat character prefabs")]
        public GameObject[] characterPrefabs;
        
        [Tooltip("Bayat item prefabs")]
        public GameObject[] itemPrefabs;
        
        [Tooltip("Bayat enemy prefabs")]
        public GameObject[] enemyPrefabs;

        private void Start()
        {
            InitializeTilemaps();
            ValidateAssetReferences();
        }

        /// <summary>
        /// Initialize tilemaps with proper sorting and collision
        /// </summary>
        private void InitializeTilemaps()
        {
            if (groundTilemap != null)
            {
                var renderer = groundTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = "Platforms";
                    renderer.sortingOrder = 0;
                }

                var collider = groundTilemap.GetComponent<TilemapCollider2D>();
                if (collider != null)
                {
                    collider.usedByComposite = true;
                }

                Debug.Log("✅ Ground tilemap configured for platforms");
            }

            if (backgroundTilemap != null)
            {
                var renderer = backgroundTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = "Background";
                    renderer.sortingOrder = 0;
                }

                Debug.Log("✅ Background tilemap configured");
            }

            if (foregroundTilemap != null)
            {
                var renderer = foregroundTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerName = "Foreground";
                    renderer.sortingOrder = 0;
                }

                Debug.Log("✅ Foreground tilemap configured");
            }
        }

        /// <summary>
        /// Validate that all Bayat asset references are properly set
        /// </summary>
        private void ValidateAssetReferences()
        {
            int missingAssets = 0;

            if (groundTiles == null || groundTiles.Length == 0)
            {
                Debug.LogWarning("⚠️ No ground tiles assigned from Bayat assets");
                missingAssets++;
            }

            if (characterPrefabs == null || characterPrefabs.Length == 0)
            {
                Debug.LogWarning("⚠️ No character prefabs assigned from Bayat assets");
                missingAssets++;
            }

            if (missingAssets == 0)
            {
                Debug.Log("✅ All Bayat asset references validated");
            }
            else
            {
                Debug.LogWarning($"⚠️ {missingAssets} Bayat asset references need to be configured");
            }
        }

        /// <summary>
        /// Place a tile at the specified position using Bayat tiles
        /// </summary>
        public void PlaceTile(Vector3Int position, TileType tileType, int tileIndex = 0)
        {
            TileBase[] tiles = GetTileArray(tileType);
            Tilemap targetTilemap = GetTargetTilemap(tileType);

            if (tiles != null && tileIndex < tiles.Length && targetTilemap != null)
            {
                targetTilemap.SetTile(position, tiles[tileIndex]);
            }
            else
            {
                Debug.LogWarning($"⚠️ Cannot place tile: {tileType} at index {tileIndex}");
            }
        }

        /// <summary>
        /// Remove tile at the specified position
        /// </summary>
        public void RemoveTile(Vector3Int position, TileType tileType)
        {
            Tilemap targetTilemap = GetTargetTilemap(tileType);
            if (targetTilemap != null)
            {
                targetTilemap.SetTile(position, null);
            }
        }

        /// <summary>
        /// Spawn a Bayat prefab at the specified position
        /// </summary>
        public GameObject SpawnPrefab(Vector3 position, PrefabType prefabType, int prefabIndex = 0)
        {
            GameObject[] prefabs = GetPrefabArray(prefabType);
            
            if (prefabs != null && prefabIndex < prefabs.Length)
            {
                var spawned = Instantiate(prefabs[prefabIndex], position, Quaternion.identity, transform);
                Debug.Log($"✅ Spawned {prefabType} prefab: {spawned.name}");
                return spawned;
            }
            else
            {
                Debug.LogWarning($"⚠️ Cannot spawn prefab: {prefabType} at index {prefabIndex}");
                return null;
            }
        }

        private TileBase[] GetTileArray(TileType tileType)
        {
            return tileType switch
            {
                TileType.Ground => groundTiles,
                TileType.Background => backgroundTiles,
                TileType.Props => propTiles,
                _ => null
            };
        }

        private Tilemap GetTargetTilemap(TileType tileType)
        {
            return tileType switch
            {
                TileType.Ground => groundTilemap,
                TileType.Background => backgroundTilemap,
                TileType.Props => foregroundTilemap,
                _ => null
            };
        }

        private GameObject[] GetPrefabArray(PrefabType prefabType)
        {
            return prefabType switch
            {
                PrefabType.Character => characterPrefabs,
                PrefabType.Item => itemPrefabs,
                PrefabType.Enemy => enemyPrefabs,
                _ => null
            };
        }

        /// <summary>
        /// Get available tile count for a specific type
        /// </summary>
        public int GetTileCount(TileType tileType)
        {
            var tiles = GetTileArray(tileType);
            return tiles?.Length ?? 0;
        }

        /// <summary>
        /// Get available prefab count for a specific type
        /// </summary>
        public int GetPrefabCount(PrefabType prefabType)
        {
            var prefabs = GetPrefabArray(prefabType);
            return prefabs?.Length ?? 0;
        }
    }

    public enum TileType
    {
        Ground,
        Background,
        Props
    }

    public enum PrefabType
    {
        Character,
        Item,
        Enemy
    }
}