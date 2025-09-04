using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Level;
using WWIII.SideScroller.Editor.Collectibles;
using WWIII.SideScroller.Editor.ArtPipeline;
using System.Reflection;

namespace WWIII.SideScroller.Editor.Level
{
    public static class AILevelDesigner
    {
        private class DesignSpec
        {
            public int Age;
            public string DesignId; // e.g., age7
            public string SceneName; // e.g., BioLevel_Age7
        }

        [MenuItem("WWIII/Level/AI Design/Design Target Biographical Levels (7,18,45,50)")]
        public static void DesignTargets()
        {
            var targets = new[] { 7, 18, 45, 50 };
            foreach (var age in targets)
            {
                try { DesignLevel(age); }
                catch (Exception ex) { Debug.LogError($"AI design failed for Age {age}: {ex.Message}\n{ex}"); }
            }
            EditorUtility.DisplayDialog("WWIII", "AI design pass complete for target levels.", "OK");
        }

        [MenuItem("WWIII/Level/AI Design/Design Age 7 (Playground)")]
        public static void DesignAge7() => DesignLevel(7);

        [MenuItem("WWIII/Level/AI Design/Design Current Age Level From CSV Selection")] 
        public static void DesignCurrentFromCsv()
        {
            // Use CSV level definitions for sceneName/designId inference
            var csvPath = "Assets/WWIII/Data/DesignData/Levels.csv";
            if (!File.Exists(csvPath)) { EditorUtility.DisplayDialog("WWIII","Levels.csv not found.","OK"); return; }
            var rows = WWIII.SideScroller.Editor.Data.CSVUtil.Read(csvPath).ToList();
            if (rows.Count == 0) { EditorUtility.DisplayDialog("WWIII","No rows in Levels.csv","OK"); return; }
            foreach (var row in rows)
            {
                var age = ParseI(Get(row,"ageYears"), -1);
                if (age <= 0) continue;
                try { DesignLevel(age); } catch (Exception ex) { Debug.LogError(ex); }
            }
        }

        public static void DesignLevel(int ageYears)
        {
            var spec = ResolveDesignSpec(ageYears);
            if (spec == null) { Debug.LogWarning($"No LevelDefinition found for age {ageYears}; skipping"); return; }

            var theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            var set = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            if (theme == null || set == null) { Debug.LogWarning("Missing LevelThemeProfile or AgeSet"); return; }
            var ageTheme = theme.GetForYears(ageYears);

            // If tiles not configured, attempt to assign Bayat visuals for age 7
            if ((ageTheme.groundTiles == null || ageTheme.groundTiles.Count == 0) && ageYears == 7)
            {
                try { BayatVisualAssigner.AssignAge7Visuals(); } catch { }
                // reload theme
                theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
                ageTheme = theme != null ? theme.GetForYears(ageYears) : ageTheme;
            }

            // Ensure photo collectible prefab exists for fallback placement
            var photoPrefabPath = "Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab";
            if (!File.Exists(photoPrefabPath))
            {
                try { PhotoCollectiblePrefabBuilder.CreatePhotoPrefab(); }
                catch (Exception ex) { Debug.LogWarning($"Couldn't auto-create PhotoCollectible prefab: {ex.Message}"); }
            }

            // Create or open the scene
            var scenePath = $"Assets/WWIII/Scenes/{spec.SceneName}.unity";
            var scene = File.Exists(scenePath) ? EditorSceneManager.OpenScene(scenePath) : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            // Ensure tilemaps
            var maps = EnsureTilemaps($"Tilemaps_{spec.DesignId}");

            // Ground and platform layout
            var seed = AILevelLayoutSampler.SeedFromPrompt(spec.SceneName + ageYears);
            PopulateLayeredGround(maps.ground, ageTheme.groundTiles.ToArray(), ageYears, seed);
            FillBackground(maps.back, ageTheme.backgroundTiles.ToArray(), maps.ground);
            PlaceFloatingPlatforms(maps.ground, ageTheme.platformPrefabs, ageYears, seed + 13);

            // Themed props for environmental storytelling
            PlaceStoryProps(ageTheme.propPrefabs, seed + 23);

            // Age system & HUD anchors
            EnsureAgeSystem(set, ageYears);
            EnsureHudAnchor();

            // Spawns from spec (rebuild under a single root)
            RebuildSpawns(spec.DesignId);

            // Ensure player exists and enemies target it
            EnsurePlayerAndTargets(ageYears);

            // Camera bounds and basic confiner
            EnsureCameraBounds(maps.ground);

            // Save scene
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
        }

        private static DesignSpec ResolveDesignSpec(int age)
        {
            // Prefer LevelDefs folder, fallback to Definitions/Levels CSV-based
            var def = LoadLevelDef($"Assets/WWIII/Data/LevelDefs/BioLevel_Age{age}.asset");
            if (def == null)
            {
                var gCsv = AssetDatabase.FindAssets("t:WWIII.SideScroller.Level.LevelDefinition", new[]{"Assets/WWIII/Data/Definitions/Levels"});
                foreach (var g in gCsv)
                {
                    var p = AssetDatabase.GUIDToAssetPath(g);
                    var d = LoadLevelDef(p);
                    if (d != null && d.ageYears == age) { def = d; break; }
                }
            }
            if (def == null) return null;
            return new DesignSpec
            {
                Age = def.ageYears,
                DesignId = string.IsNullOrEmpty(def.designId) ? $"age{def.ageYears}" : def.designId,
                SceneName = string.IsNullOrEmpty(def.sceneName) ? $"BioLevel_Age{def.ageYears}" : def.sceneName,
            };
        }

        private static LevelDefinition LoadLevelDef(string path)
            => AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);

        private static BiographicalTilemapBuilder.Tilemaps EnsureTilemaps(string rootName)
        {
            var existing = GameObject.Find(rootName);
            if (existing != null)
            {
                var grid = existing.GetComponent<Grid>();
                var tms = new BiographicalTilemapBuilder.Tilemaps { grid = grid };
                foreach (Transform c in existing.transform)
                {
                    var tm = c.GetComponent<Tilemap>(); if (tm == null) continue;
                    if (tm.name.Contains("Ground")) tms.ground = tm; else if (tm.name.Contains("Foreground")) tms.front = tm; else if (tm.name.Contains("Background")) tms.back = tm;
                }
                if (tms.ground == null) tms.ground = existing.AddComponent<Tilemap>();
                return tms;
            }
            return BiographicalTilemapBuilder.CreateGridWithTilemaps(rootName);
        }

        public static void PopulateLayeredGround(Tilemap ground, TileBase[] tiles, int age, int seed)
        {
            if (ground == null || tiles == null || tiles.Length == 0) return;
            int width = Mathf.Clamp(96 + (age - 7) * 2, 96, 160);
            int height = 32;
            int baseLine = Mathf.Clamp(8 + (age/10), 8, 14);
            BiographicalTilemapBuilder.PopulateGround(ground, tiles, width, height, baseLine, seed);
        }

        private static void PlaceFloatingPlatforms(Tilemap ground, List<GameObject> platforms, int age, int seed)
        {
            if (ground == null || platforms == null || platforms.Count == 0) return;
            var rng = new System.Random(seed);
            int count = Mathf.Clamp(3 + age/10, 3, 10);
            var root = GameObject.Find("Platforms") ?? new GameObject("Platforms");
            for (int i = 0; i < count; i++)
            {
                var prefab = platforms[rng.Next(platforms.Count)];
                if (prefab == null) continue;
                int x = rng.Next(12, Mathf.Max(24, ground.size.x - 12));
                int y = SampleGroundHeight(ground, x) + rng.Next(4, 10);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                go.transform.SetParent(root.transform);
            }
        }

        public static int SampleGroundHeight(Tilemap ground, int x)
        {
            // find the highest filled tile at column x
            var b = ground.cellBounds;
            int maxY = b.yMin;
            for (int y = b.yMin; y <= b.yMax; y++)
            {
                if (ground.HasTile(new Vector3Int(x, y, 0))) maxY = y;
            }
            return maxY;
        }

        public static void FillBackground(Tilemap background, TileBase[] tiles, Tilemap ground)
        {
            if (background == null || tiles == null || tiles.Length == 0) return;
            var rng = new System.Random(12345);
            var b = ground != null ? ground.cellBounds : new BoundsInt(0,0,0,120,40,1);
            int y = b.yMin + (int)(b.size.y * 0.6f);
            for (int x = b.xMin; x < b.xMin + b.size.x; x+=3)
            {
                var t = tiles[rng.Next(tiles.Length)];
                background.SetTile(new Vector3Int(x, y, 0), t);
            }
            background.CompressBounds();
        }

        // Ensure a controllable player exists and wire enemies to target it
        private static void EnsurePlayerAndTargets(int ageYears)
        {
            // Try AgeManager's current player first
            var mgr = UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            GameObject player = mgr != null ? mgr.currentPlayer : null;

            if (player == null)
            {
                // Create a PlayerRoot under AgeSystem or at scene root
                Transform root = null;
                var ageRootGO = GameObject.Find("AgeSystem");
                root = ageRootGO != null ? ageRootGO.transform : null;
                if (root == null)
                {
                    var pr = GameObject.Find("PlayerRoot");
                    root = pr != null ? pr.transform : new GameObject("PlayerRoot").transform;
                }

                // Try to instantiate project's age-aware player prefab via AgeProfile
                GameObject fallback = null;
                try
                {
                    if (mgr != null && mgr.ageSet != null)
                    {
                        var profile = mgr.ageSet.ages.FirstOrDefault(a => a != null && a.ageYears == ageYears);
                        if (profile != null && profile.playerPrefab != null && profile.playerPrefab.RuntimeKeyIsValid())
                        {
                            // Synchronous instantiate not available for Addressables in editor here; will fallback
                        }
                    }
                }
                catch { }

                if (fallback == null)
                {
                    // Fallback: Bayat Player prefab
                    var bayatPath = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate/Prefabs/Characters/Player/Player.prefab";
                    fallback = AssetDatabase.LoadAssetAtPath<GameObject>(bayatPath);
                }

                if (fallback != null)
                {
                    player = (GameObject)PrefabUtility.InstantiatePrefab(fallback);
                    player.name = "Player";
                    if (root != null) player.transform.SetParent(root);
                    // Place above ground roughly at x=4
                    var ground = UnityEngine.Object.FindFirstObjectByType<Tilemap>();
                    int y = ground != null ? SampleGroundHeight(ground, 4) + 4 : 12;
                    player.transform.position = new Vector3(4, y, 0);

                    // Inform AgeManager
                    if (mgr != null)
                    {
                        mgr.currentPlayer = player;
                    }
                }
            }

            if (player == null) return;

            // Assign all EnemyAI.target to this player
            var enemies = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => c != null && c.GetType().Name == "EnemyAI");
            foreach (var ai in enemies)
            {
                if (ai == null) continue;
                var so = new SerializedObject(ai);
                var tprop = so.FindProperty("target");
                if (tprop != null)
                {
                    tprop.objectReferenceValue = player.transform;
                    so.ApplyModifiedProperties();
                }
            }

            // Ensure there is a Main Camera (URP-friendly)
            EnsureCamera(player.transform);
        }

        public static void EnsureCamera(Transform follow)
        {
            var camGO = GameObject.FindWithTag("MainCamera") ?? GameObject.Find("Main Camera");
            Camera cam;
            if (camGO == null)
            {
                camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
            }
            else
            {
                cam = camGO.GetComponent<Camera>() ?? camGO.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(follow.position.x, follow.position.y + 2f, -10f);

            // Add URP additional camera data if available
            var urpType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (urpType != null && camGO.GetComponent(urpType) == null)
            {
                camGO.AddComponent(urpType);
            }
        }

        private static void PlaceStoryProps(List<GameObject> props, int seed)
        {
            if (props == null || props.Count == 0) return;
            var rng = new System.Random(seed);
            var root = GameObject.Find("StoryProps") ?? new GameObject("StoryProps");
            int count = Mathf.Min(6, props.Count);
            for (int i = 0; i < count; i++)
            {
                var p = props[rng.Next(props.Count)]; if (p == null) continue;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p);
                go.transform.position = new Vector3(8 + i * 16 + rng.Next(-2,3), 12 + rng.Next(-2,3), 0);
                go.transform.SetParent(root.transform);
            }
        }

        private static void EnsureAgeSystem(AgeSet set, int ageYears)
        {
            var root = GameObject.Find("AgeSystem") ?? new GameObject("AgeSystem");
            var mgr = root.GetComponent<AgeManager>() ?? root.AddComponent<AgeManager>();
            mgr.ageSet = set;
            int idx = set != null ? set.ages.FindIndex(a => a != null && a.ageYears == ageYears) : -1;
            mgr.initialAgeIndex = Mathf.Max(0, idx);
        }

        private static void EnsureHudAnchor()
        {
            if (GameObject.Find("HUD") == null)
            {
                var hud = new GameObject("HUD");
                hud.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();
            }
        }

        private static void RebuildSpawns(string designId)
        {
            var existing = GameObject.Find("Spawns");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
            var specPath = $"Assets/WWIII/Data/Definitions/Spawns/{designId}.asset";
            var spec = AssetDatabase.LoadAssetAtPath<LevelSpawnSpec>(specPath);
            if (spec == null) return;
            var root = new GameObject("Spawns");
            foreach (var s in spec.entries)
            {
                var n = Mathf.Max(1, s.count);
                for (int i = 0; i < n; i++) PlaceBySpec(root.transform, s);
            }

            // Ensure at least 5 photo collectibles exist for narrative reveals
            EnsureMinimumPhotos(5);
        }

        private static void PlaceBySpec(Transform parent, LevelSpawnSpec.Entry s)
        {
            GameObject prefab = null;
            if (s.type.Equals("Collectible", StringComparison.OrdinalIgnoreCase))
            {
                var cdef = LoadByDesignId<WWIII.SideScroller.Design.CollectibleDefinition>("Assets/WWIII/Data/Definitions/Collectibles", s.designId);
                if (cdef != null && !string.IsNullOrEmpty(cdef.prefabKey)) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cdef.prefabKey);
                if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab");
            }
            else if (s.type.Equals("PowerUp", StringComparison.OrdinalIgnoreCase))
            {
                var ppath = $"Assets/WWIII/Prefabs/Powerups/{s.designId}.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ppath);
            }
            else if (s.type.Equals("Enemy", StringComparison.OrdinalIgnoreCase))
            {
                var edef = LoadByDesignId<WWIII.SideScroller.Design.EnemyDefinition>("Assets/WWIII/Data/Definitions/Enemies", s.designId);
                if (edef != null && !string.IsNullOrEmpty(edef.prefabKey)) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(edef.prefabKey);
            }
            if (prefab == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(s.position.x, s.position.y, 0f);
        }

        private static T LoadByDesignId<T>(string folder, string id) where T : ScriptableObject
        {
            var p = System.IO.Path.Combine(folder, id + ".asset").Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<T>(p);
        }

        private static void EnsureMinimumPhotos(int target)
        {
            var photoType = typeof(WWIII.SideScroller.Collectibles.PhotoCollectible);
            var existing = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => c != null && photoType.IsInstanceOfType(c)).ToList();
            int need = target - existing.Count;
            if (need <= 0) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab");
            if (prefab == null) return;

            // Spread additional photos across the level width
            var ground = UnityEngine.Object.FindFirstObjectByType<Tilemap>();
            var bounds = ground != null ? ground.cellBounds : new BoundsInt(0,0,0,120,40,1);
            float step = Mathf.Max(12f, bounds.size.x / Mathf.Max(1, need+1));
            for (int i = 0; i < need; i++)
            {
                float x = bounds.xMin + step * (i + 1);
                int col = Mathf.RoundToInt(x);
                int y = ground != null ? SampleGroundHeight(ground, col) + 6 : 12;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = new Vector3(col + 0.5f, y + 0.5f, 0);
            }
        }

        // CSV helpers
        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d != null && d.TryGetValue(k, out var v) ? v : def;
        private static int ParseI(string s, int def)
            => int.TryParse(s, out var i) ? i : def;

        private static void EnsureCameraBounds(Tilemap ground)
        {
            // Create a polygon around the ground bounds; CinemachineConfiner2D can use it if available
            var go = GameObject.Find("CameraBounds");
            if (go == null) go = new GameObject("CameraBounds");
            var poly = go.GetComponent<PolygonCollider2D>();
            if (poly == null) poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;

            var b = ground != null ? ground.localBounds : new Bounds(Vector3.zero, new Vector3(120, 40, 0));
            var min = b.min; var max = b.max;
            var margin = 2f;
            var pts = new Vector2[]
            {
                new Vector2(min.x - margin, min.y - margin),
                new Vector2(max.x + margin, min.y - margin),
                new Vector2(max.x + margin, max.y + margin),
                new Vector2(min.x - margin, max.y + margin),
            };
            poly.pathCount = 1; poly.SetPath(0, pts);
        }
    }
}
