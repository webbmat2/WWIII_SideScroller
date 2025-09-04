#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.EditorTools
{
    /// <summary>
    /// Temporarily simplifies the level for physics debugging by disabling Foreground/Background
    /// and replacing the existing Ground tilemap with a simple static BoxCollider2D surface.
    /// </summary>
    public static class SimplifyLevelForPhysicsTest
    {
        private const string PlainGroundName = "PlainGround";
        private const string TilemapsRootName = "Tilemaps_age7";

        [MenuItem("WWIII/Level/Simplify To Plain Ground")]
        public static void Simplify()
        {
            // Disable likely visual layers
            SetActiveByName("Foreground", false);
            SetActiveByName("Background", false);

            // Disable tilemap Ground if present (we won't delete it)
            SetActiveByName("Ground", false);

            // Create PlainGround if not present
            var existing = GameObject.Find(PlainGroundName);
            if (existing == null)
            {
                var go = new GameObject(PlainGroundName);
                Undo.RegisterCreatedObjectUndo(go, "Create PlainGround");

                // Determine Y position near player
                float y = 0f;
                var player = FindPlayer();
                if (player != null)
                {
                    y = Mathf.Floor(player.position.y - 1f);
                }
                go.transform.position = new Vector3(0f, y, 0f);

                // Physics: static rigidbody + wide box collider
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(500f, 2f);
                col.offset = new Vector2(0f, -1f);

                // Layer to Ground if exists, else index 3
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer < 0) groundLayer = 3;
                go.layer = groundLayer;

                Debug.Log("[SimplifyLevel] Added PlainGround with static BoxCollider2D for physics testing.");
            }
            else
            {
                existing.SetActive(true);
            }
        }

        [MenuItem("WWIII/Level/Strip To Simple Tile Ground")]
        public static void StripToSimpleTileGround()
        {
            // Remove Foreground/Background completely
            DestroyByName("Foreground");
            DestroyByName("Background");

            // Remove PlainGround if previously added
            var pg = GameObject.Find(PlainGroundName);
            if (pg) Undo.DestroyObjectImmediate(pg);

            // Find or create Grid/Tilemap Ground
            var ground = EnsureGroundTilemap(out var grid);
            if (ground == null)
            {
                Debug.LogError("[StripToSimpleTileGround] Failed to create/find Ground tilemap.");
                return;
            }

            // Prefer our generated SimpleGround tile, else try to find any
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/WWIII/Tiles/SimpleGround.asset");
            if (tile == null)
            {
                tile = FindAnyTile(ground);
            }
            if (tile == null)
            {
                Debug.LogWarning("[StripToSimpleTileGround] No TileBase asset found. Creating physics-only straight ground (no visuals)." );
                // Create a long BoxCollider under Ground if no tiles available
                var go = ground.gameObject;
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb == null) rb = go.AddComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Static;
                else Debug.LogWarning("[StripToSimpleTileGround] Failed to add Rigidbody2D to Ground.");
                var box = go.GetComponent<BoxCollider2D>() ?? go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(500f, 1f);
                box.offset = new Vector2(0f, -0.5f);
                return;
            }

            // Clear and draw a straight line of tiles at Y near player
            var player = FindPlayer();
            int y = 0;
            if (player != null) y = Mathf.FloorToInt(player.position.y - 1f);

            ground.ClearAllTiles();
            for (int x = -100; x <= 100; x++)
            {
                ground.SetTile(new Vector3Int(x, y, 0), tile);
            }
            ground.RefreshAllTiles();
            ground.CompressBounds();

            // Ensure colliders set for composite
            var tmc = ground.GetComponent<TilemapCollider2D>();
            if (tmc == null) tmc = ground.gameObject.AddComponent<TilemapCollider2D>();
            tmc.usedByComposite = true;
            var rb2d = ground.GetComponent<Rigidbody2D>();
            if (rb2d == null) rb2d = ground.gameObject.AddComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.bodyType = RigidbodyType2D.Static;
            else Debug.LogWarning("[StripToSimpleTileGround] Failed to add Rigidbody2D to Ground tilemap.");
            var cc = ground.GetComponent<CompositeCollider2D>() ?? ground.gameObject.AddComponent<CompositeCollider2D>();
            cc.isTrigger = false;
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            cc.generationType = CompositeCollider2D.GenerationType.Synchronous;

            // Force collider refresh
            tmc.enabled = false; tmc.enabled = true;
            cc.enabled = false; cc.enabled = true;

            // Set layer to Ground
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 3;
            ground.gameObject.layer = groundLayer;

            // Delete any other Tilemaps under the root that could interfere
            DeleteSiblingsExcept(ground.transform);

            Debug.Log($"[StripToSimpleTileGround] Drew straight tile line at y={y} using tile '{tile.name}'.");
        }

        private static Tilemap EnsureGroundTilemap(out Grid grid)
        {
            grid = null;
            // Find existing root
            Transform root = null;
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == TilemapsRootName) { root = go.transform; break; }
            }
            if (root == null)
            {
                var gobj = new GameObject(TilemapsRootName);
                Undo.RegisterCreatedObjectUndo(gobj, "Create Tilemaps Root");
                grid = gobj.AddComponent<Grid>();
                root = gobj.transform;
            }
            else
            {
                grid = root.GetComponent<Grid>();
                if (grid == null) grid = root.gameObject.AddComponent<Grid>();
            }

            // Find or create Ground child with Tilemap
            var child = FindChildRecursive(root, "Ground");
            if (child == null)
            {
                var groundGo = new GameObject("Ground");
                Undo.RegisterCreatedObjectUndo(groundGo, "Create Ground Tilemap");
                groundGo.transform.SetParent(root);
                var tm = groundGo.AddComponent<Tilemap>();
                groundGo.AddComponent<TilemapRenderer>();
                return tm;
            }
            return child.GetComponent<Tilemap>() ?? child.gameObject.AddComponent<Tilemap>();
        }

        private static TileBase FindAnyTile(Tilemap tm)
        {
            // Try to pick any tile already in this tilemap
            var bounds = tm.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                var t = tm.GetTile<TileBase>(pos);
                if (t) return t;
            }
#if UNITY_EDITOR
            // Search project for any TileBase asset
            var guids = AssetDatabase.FindAssets("t:TileBase");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var t = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (t) return t;
            }
#endif
            return null;
        }

        private static void DeleteSiblingsExcept(Transform keep)
        {
            var parent = keep.parent;
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var c = parent.GetChild(i);
                if (c == keep) continue;
                Undo.DestroyObjectImmediate(c.gameObject);
            }
        }

        private static void DestroyByName(string name)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var t = FindChildRecursive(root.transform, name);
                if (t != null)
                {
                    Undo.DestroyObjectImmediate(t.gameObject);
                }
            }
        }

        [MenuItem("WWIII/Level/Restore Original Level Visuals")]        
        public static void Restore()
        {
            // Re-enable layers if they exist
            SetActiveByName("Foreground", true);
            SetActiveByName("Background", true);
            SetActiveByName("Ground", true);

            var pg = GameObject.Find(PlainGroundName);
            if (pg != null)
            {
                Undo.DestroyObjectImmediate(pg);
                Debug.Log("[SimplifyLevel] Removed PlainGround and restored original objects.") ;
            }
        }

        private static void SetActiveByName(string name, bool active)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var t = FindChildRecursive(root.transform, name);
                if (t != null)
                {
                    Undo.RecordObject(t.gameObject, "Toggle Active");
                    t.gameObject.SetActive(active);
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var f = FindChildRecursive(child, name);
                if (f != null) return f;
            }
            return null;
        }

        private static Transform FindPlayer()
        {
            var byTag = GameObject.FindGameObjectWithTag("Player");
            if (byTag != null) return byTag.transform;

            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var t = FindChildRecursive(root.transform, "Player");
                if (t != null) return t;
            }
            return null;
        }
    }
}
#endif
