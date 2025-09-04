using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Editor.Level
{
    public static class ScenePlayabilityFixer
    {
        [MenuItem("WWIII/Level/Fix Current Scene (Age7 Playability)")]
        public static void FixCurrentScene()
        {
            // Try to detect Age
            int age = 7;
            var active = EditorSceneManager.GetActiveScene();
            if (active.IsValid() && !string.IsNullOrEmpty(active.path))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(active.path);
                if (name != null && name.Contains("Age"))
                {
                    var suffix = name.Substring(name.IndexOf("Age") + 3);
                    if (int.TryParse(suffix, out var n)) age = n;
                }
            }

            // Ensure AgeSystem
            var mgr = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (mgr == null)
            {
                var go = new GameObject("AgeSystem");
                mgr = go.AddComponent<AgeManager>();
            }

            // Ensure Audio bridge exists
            EnsureAudioBridges(mgr);

            // Ensure tilemaps and paint ground/background if empty
            var grid = UnityEngine.Object
                .FindObjectsByType<Grid>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(g => g.name.StartsWith("Tilemaps_"));
            Tilemap ground = null, back = null;
            if (grid != null)
            {
                foreach (Transform c in grid.transform)
                {
                    var tm = c.GetComponent<Tilemap>();
                    if (tm == null) continue;
                    if (tm.name.Contains("Ground")) ground = tm; else if (tm.name.Contains("Background")) back = tm;
                }
            }
            if (ground == null)
            {
                var maps = BiographicalTilemapBuilder.CreateGridWithTilemaps("Tilemaps_age" + age);
                ground = maps.ground; back = maps.back;
            }

            // Ensure some tiles
            var theme = AssetDatabase.LoadAssetAtPath<WWIII.SideScroller.Level.LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            var ageTheme = theme != null ? theme.GetForYears(age) : null;
            if (ageTheme == null || ageTheme.groundTiles == null || ageTheme.groundTiles.Count == 0)
            {
                try { WWIII.SideScroller.Editor.ArtPipeline.BayatVisualAssigner.AssignAge7Visuals(); }
                catch { }
                theme = AssetDatabase.LoadAssetAtPath<WWIII.SideScroller.Level.LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
                ageTheme = theme != null ? theme.GetForYears(age) : ageTheme;
            }

            if (ageTheme != null)
            {
                AILevelDesigner.PopulateLayeredGround(ground, ageTheme.groundTiles.ToArray(), age, AILevelLayoutSampler.SeedFromPrompt(active.name + age));
                AILevelDesigner.FillBackground(back, ageTheme.backgroundTiles.ToArray(), ground);
            }

            EnsureTilemapColliders(ground);

            // Ensure a player exists (prefer Corgi, fallback to Bayat Player)
            var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.tag = "Player";
                var sr = player.AddComponent<SpriteRenderer>(); sr.sortingOrder = 5;
                var box = player.AddComponent<BoxCollider2D>(); box.size = new Vector2(0.7f, 1.5f);

                // Try to add Corgi components via reflection (no direct assembly reference inside Editor asmdef)
                TryAddCorgiComponents(player);

                // Fallback: simple Rigidbody2D to allow movement via arrow keys (very basic)
                if (player.GetComponent<Rigidbody2D>() == null)
                {
                    var rb = player.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 3f; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                }

                // Set layer to Player (11 default) if available
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer < 0) playerLayer = 11;
                player.layer = playerLayer;

                // Place on ground roughly near x=4
                int y = ground != null ? AILevelDesigner.SampleGroundHeight(ground, 4) + 4 : 12;
                player.transform.position = new Vector3(4, y, 0);
            }

            // Wire enemies to target the player
            WireEnemyTargets(player.transform);

            // Ensure camera
            AILevelDesigner.EnsureCamera(player.transform);

            EditorUtility.DisplayDialog("WWIII", "Scene playability fixes applied (player, ground, enemies, camera).", "OK");
        }

        private static void EnsureAudioBridges(AgeManager mgr)
        {
            // BiographicalAudioManager singleton
            var bam = WWIII.SideScroller.Audio.BiographicalAudioManager.Instance;
            if (bam == null)
            {
                var go = new GameObject("BiographicalAudioManager");
                go.AddComponent<WWIII.SideScroller.Audio.BiographicalAudioManager>();
            }
            // Age → music driver on AgeSystem
            if (mgr != null && mgr.GetComponent<WWIII.SideScroller.Audio.AgeMusicDriver>() == null)
            {
                mgr.gameObject.AddComponent<WWIII.SideScroller.Audio.AgeMusicDriver>();
            }
        }

        private static void EnsureTilemapColliders(Tilemap ground)
        {
            if (ground == null || ground.gameObject == null)
            {
                Debug.LogWarning("EnsureTilemapColliders: ground Tilemap is null");
                return;
            }
            try
            {
                // Ensure Static Rigidbody2D first
                var rb = ground.GetComponent<Rigidbody2D>();
                if (rb == null) { rb = ground.gameObject.AddComponent<Rigidbody2D>(); }
                rb.bodyType = RigidbodyType2D.Static;

                // Ensure TilemapCollider2D exists
                var tmCollider = ground.GetComponent<TilemapCollider2D>();
                if (tmCollider == null)
                {
                    tmCollider = ground.gameObject.AddComponent<TilemapCollider2D>();
                }

                // Ensure CompositeCollider2D exists
                var comp = ground.GetComponent<CompositeCollider2D>();
                if (comp == null)
                {
                    comp = ground.gameObject.AddComponent<CompositeCollider2D>();
                }
                comp.geometryType = CompositeCollider2D.GeometryType.Polygons;

                // Now that Composite exists, set TilemapCollider to merge into it
                tmCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            }
            catch (MissingComponentException mce)
            {
                Debug.LogError($"EnsureTilemapColliders: missing component while setting up colliders: {mce.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"EnsureTilemapColliders: exception {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void TryAddCorgiComponents(GameObject player)
        {
            // Add CorgiController
            AddIfFound(player, "MoreMountains.CorgiEngine.CorgiController");
            // Add Character core class
            var character = AddIfFound(player, "MoreMountains.CorgiEngine.Character");
            // Add a few common abilities
            AddIfFound(player, "MoreMountains.CorgiEngine.CharacterHorizontalMovement");
            AddIfFound(player, "MoreMountains.CorgiEngine.CharacterJump");
            AddIfFound(player, "MoreMountains.CorgiEngine.CharacterCrouch");
            AddIfFound(player, "MoreMountains.CorgiEngine.CharacterDash");
        }

        private static Component AddIfFound(GameObject go, string typeName)
        {
            var type = Type.GetType(typeName + ", MoreMountains.CorgiEngine");
            if (type == null) type = Type.GetType(typeName);
            if (type == null) return null;
            return go.GetComponent(type) ?? go.AddComponent(type);
        }

        private static void WireEnemyTargets(Transform player)
        {
            var enemies = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => c != null && c.GetType().Name == "EnemyAI");
            int wired = 0, patrolDisabled = 0, groundCheckAssigned = 0, rbAssigned = 0, animAssigned = 0;

            // Find ground tilemap to sample heights for helper placements
            var ground = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(tm => tm.name.Contains("Ground"));
            foreach (var ai in enemies)
            {
                if (ai == null) continue;
                var so = new SerializedObject(ai);
                var tprop = so.FindProperty("target");
                if (tprop != null)
                {
                    tprop.objectReferenceValue = player;
                    wired++;
                }

                // Ensure groundCheck
                var gcProp = so.FindProperty("groundCheck");
                if (gcProp != null && gcProp.objectReferenceValue == null)
                {
                    var go = (ai as Component).gameObject;
                    var gc = go.transform.Find("GroundCheck")?.transform;
                    if (gc == null)
                    {
                        var gcGO = new GameObject("GroundCheck");
                        gc = gcGO.transform;
                        gc.SetParent(go.transform, worldPositionStays: true);
                        // place slightly below the enemy
                        var pos = go.transform.position;
                        int y = ground != null ? AILevelDesigner.SampleGroundHeight(ground, Mathf.RoundToInt(pos.x)) : Mathf.RoundToInt(pos.y - 1);
                        gc.position = new Vector3(pos.x, y - 0.25f, pos.z);
                    }
                    gcProp.objectReferenceValue = gc;
                    groundCheckAssigned++;
                }

                // Ensure Rigidbody2D reference
                var rbProp = so.FindProperty("rigidbody2d");
                if (rbProp != null && rbProp.objectReferenceValue == null)
                {
                    var go = (ai as Component).gameObject;
                    var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
                    rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rbProp.objectReferenceValue = rb;
                    rbAssigned++;
                }

                // Ensure Animator reference
                var animProp = so.FindProperty("animator");
                if (animProp != null && animProp.objectReferenceValue == null)
                {
                    var go = (ai as Component).gameObject;
                    var anim = go.GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        animProp.objectReferenceValue = anim;
                        animAssigned++;
                    }
                }

                // Disable patrol if start/end are not assigned to avoid runtime exceptions
                var patrolProp = so.FindProperty("patrol");
                var startProp = so.FindProperty("startPosition");
                var endProp = so.FindProperty("endPosition");
                if (patrolProp != null && patrolProp.boolValue)
                {
                    if (startProp == null || endProp == null || startProp.objectReferenceValue == null || endProp.objectReferenceValue == null)
                    {
                        patrolProp.boolValue = false;
                        patrolDisabled++;
                    }
                }

                so.ApplyModifiedProperties();
            }
            Debug.Log($"Wired EnemyAI targets: {wired}; groundCheck assigned: {groundCheckAssigned}; rb2d: {rbAssigned}; animator: {animAssigned}; patrol disabled: {patrolDisabled}");
        }
    }
}
