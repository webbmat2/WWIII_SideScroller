using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Editor.Cleanup;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Level
{
    public static class BiographicalCampaignCsvRegenerator
    {
        private const string CsvPath = "Assets/WWIII/Data/DesignData/Levels.csv";

        private class Row
        {
            public string id;
            public string displayName;
            public int years;
            public string sceneName;
        }

        [MenuItem("WWIII/Level/Regenerate Biographical Campaign From CSV")] 
        public static void RegenerateFromCsv()
        {
            var theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            var set = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            if (theme == null || set == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Missing LevelThemeProfile or AgeSet. Ensure they exist under Assets/WWIII/Data and Assets/WWIII/Ages.", "OK");
                return;
            }

            var rows = LoadRows();
            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("WWIII", "No rows found in Levels.csv", "OK");
                return;
            }

            Directory.CreateDirectory("Assets/WWIII/Scenes");
            Directory.CreateDirectory("Assets/WWIII/Data/LevelDefs");

            foreach (var r in rows)
            {
                GenerateLevel(theme, set, r);
                UpsertLevelDefinition(r);
            }

            // Archive old scenes with previous ages (42 → 45, 56 → 50) if present
            ArchiveIfExists("Assets/WWIII/Scenes/BioLevel_Age42.unity", "Age42_Scene");
            ArchiveIfExists("Assets/WWIII/Scenes/age42_Notes.txt", "Age42_Scene");
            ArchiveIfExists("Assets/WWIII/Scenes/BioLevel_Age56.unity", "Age56_Scene");
            ArchiveIfExists("Assets/WWIII/Scenes/age56_Notes.txt", "Age56_Scene");

            // Rebuild additional LevelDefinitions from CSV (Definitions/Levels)
            try { WWIII.SideScroller.Editor.DesignImporters.LevelsImporter.Rebuild(); }
            catch { }

            // Link spawn specs to all LevelDefinitions (LevelDefs and Definitions/Levels)
            try { WWIII.SideScroller.Editor.Level.LevelSpawnSpecLinker.LinkAll(); }
            catch { }

            // Update Build Settings to just WWIII scenes
            ProjectCleanupTools.PruneBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", "Biographical campaign regenerated from CSV, build settings pruned, and old scenes archived.", "OK");
        }

        private static List<Row> LoadRows()
        {
            var result = new List<Row>();
            foreach (var row in CSVUtil.Read(CsvPath))
            {
                var id = Get(row, "designId");
                if (string.IsNullOrEmpty(id)) continue;
                var years = ParseI(Get(row, "ageYears"), -1);
                var scene = Get(row, "sceneName", years > 0 ? $"BioLevel_Age{years}" : null);
                var display = Get(row, "displayName", id);
                if (years <= 0 || string.IsNullOrEmpty(scene)) continue;
                result.Add(new Row { id = id, years = years, sceneName = scene, displayName = display });
            }
            return result;
        }

        private static void GenerateLevel(LevelThemeProfile theme, AgeSet set, Row r)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var maps = BiographicalTilemapBuilder.CreateGridWithTilemaps($"Tilemaps_{r.id}");
            var ageTheme = theme.GetForYears(r.years);
            BiographicalTilemapBuilder.PopulateGround(maps.ground, ageTheme.groundTiles.ToArray(), width: 120, height: 32, baseline: 8, seed: AILevelLayoutSampler.SeedFromPrompt(r.displayName ?? r.id));

            // Age system
            var ageRoot = new GameObject("AgeSystem");
            var mgr = ageRoot.AddComponent<AgeManager>();
            mgr.ageSet = set;
            int idx = set.ages.FindIndex(a => a != null && a.ageYears == r.years);
            mgr.initialAgeIndex = Mathf.Max(0, idx);

            // HUD anchor
            var hud = new GameObject("HUD");
            hud.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();

            // Spawns by spec if available
            var spec = LoadSpawnSpecForDesignId(r.id);
            if (spec != null)
            {
                var root = new GameObject("Spawns");
                foreach (var s in spec.entries)
                {
                    for (int c = 0; c < Mathf.Max(1, s.count); c++)
                    {
                        PlaceBySpec(root.transform, s);
                    }
                }
            }

            // Save scene
            var scenePath = $"Assets/WWIII/Scenes/{r.sceneName}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Save simple notes
            var notes = $"Level: {r.displayName}\nAge: {r.years}\nID: {r.id}";
            var promptPath = $"Assets/WWIII/Scenes/{r.id}_Notes.txt";
            File.WriteAllText(promptPath, notes);
            AssetDatabase.ImportAsset(promptPath);
        }

        private static void UpsertLevelDefinition(Row r)
        {
            var newPath = $"Assets/WWIII/Data/LevelDefs/BioLevel_Age{r.years}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<LevelDefinition>(newPath);
            if (asset == null)
            {
                // Handle legacy rename (42→45, 56→50)
                string legacyPath = null;
                if (r.years == 45) legacyPath = "Assets/WWIII/Data/LevelDefs/BioLevel_Age42.asset";
                else if (r.years == 50) legacyPath = "Assets/WWIII/Data/LevelDefs/BioLevel_Age56.asset";
                if (!string.IsNullOrEmpty(legacyPath) && File.Exists(legacyPath))
                {
                    var err = AssetDatabase.MoveAsset(legacyPath, newPath);
                    if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"Couldn't move {legacyPath} → {newPath}: {err}");
                    asset = AssetDatabase.LoadAssetAtPath<LevelDefinition>(newPath);
                }
            }
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(asset, newPath);
            }
            asset.name = Path.GetFileNameWithoutExtension(newPath);
            asset.designId = r.id;
            asset.displayName = r.displayName;
            asset.ageYears = r.years;
            asset.sceneName = r.sceneName;
            asset.photoCount = 5;
            EditorUtility.SetDirty(asset);
        }

        private static void ArchiveIfExists(string assetPath, string group)
        {
            if (!File.Exists(assetPath)) return;
            var trash = ProjectCleanupTools.EnsureTrashRoot(group);
            var dest = trash + "/" + Path.GetFileName(assetPath);
            var err = AssetDatabase.MoveAsset(assetPath, dest);
            if (!string.IsNullOrEmpty(err)) Debug.LogWarning($"Couldn't move {assetPath} → {trash}: {err}");
        }

        private static WWIII.SideScroller.Level.LevelSpawnSpec LoadSpawnSpecForDesignId(string prefix)
        {
            var path = $"Assets/WWIII/Data/Definitions/Spawns/{prefix}.asset";
            return AssetDatabase.LoadAssetAtPath<WWIII.SideScroller.Level.LevelSpawnSpec>(path);
        }

        private static void PlaceBySpec(Transform parent, WWIII.SideScroller.Level.LevelSpawnSpec.Entry s)
        {
            GameObject prefab = null;
            if (s.type.Equals("Collectible", System.StringComparison.OrdinalIgnoreCase))
            {
                var cdef = LoadByDesignId<WWIII.SideScroller.Design.CollectibleDefinition>("Assets/WWIII/Data/Definitions/Collectibles", s.designId);
                if (cdef != null && !string.IsNullOrEmpty(cdef.prefabKey))
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cdef.prefabKey);
                if (prefab == null)
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab");
            }
            else if (s.type.Equals("PowerUp", System.StringComparison.OrdinalIgnoreCase))
            {
                var ppath = $"Assets/WWIII/Prefabs/Powerups/{s.designId}.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ppath);
            }
            else if (s.type.Equals("Enemy", System.StringComparison.OrdinalIgnoreCase))
            {
                var edef = LoadByDesignId<WWIII.SideScroller.Design.EnemyDefinition>("Assets/WWIII/Data/Definitions/Enemies", s.designId);
                if (edef != null && !string.IsNullOrEmpty(edef.prefabKey))
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(edef.prefabKey);
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

        private static string Get(Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static int ParseI(string s, int def)
            => int.TryParse(s, out var i) ? i : def;
    }
}
