using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Editor.Level;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Campaign
{
    public static class BiographicalCampaignBuilder
    {
        private class Entry
        {
            public int years; public string name; public string prompt; public string idPrefix; public string[] notes;
        }

        [MenuItem("WWIII/Level/Generate Biographical Campaign (9 Levels)")]
        public static void GenerateCampaign()
        {
            var theme = AssetDatabase.LoadAssetAtPath<LevelThemeProfile>("Assets/WWIII/Data/LevelThemeProfile.asset");
            if (theme == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Missing LevelThemeProfile at Assets/WWIII/Data/LevelThemeProfile.asset. Run 'Create Default LevelThemeProfile' first.", "OK");
                return;
            }
            var set = AssetDatabase.LoadAssetAtPath<AgeSet>("Assets/WWIII/Ages/Jim_AgeSet.asset");
            if (set == null)
            {
                EditorUtility.DisplayDialog("WWIII", "Missing AgeSet at Assets/WWIII/Ages/Jim_AgeSet.asset. Create ages first.", "OK");
                return;
            }

            var entries = new List<Entry>
            {
                new Entry{years=7,  name="Northville: Meadowbrook Park → Home", idPrefix="age7", prompt="Playground, slip-and-slide, house facade; critters; boss placeholder KristenPurplePig.", notes=new[]{"Goal: reach house","Boss: Kristen Purple Pig"}},
                new Entry{years=13, name="Torch Lake: Cottage → Party Store", idPrefix="age13", prompt="Lake edge, woods, street sprint; mayfly swarms under streetlights", notes=new[]{"Collect: Moose Tracks"}},
                new Entry{years=16, name="Farmington High School", idPrefix="age16", prompt="Hallways, posters, practice field; jeep cameo", notes=new[]{"Collect: Broccoli Supreme"}},
                new Entry{years=18, name="Notre Dame Campus + Dorm", idPrefix="age18", prompt="Campus quads, dorm stealth; RA patrol cones", notes=new[]{"Collect: Shamrocks"}},
                new Entry{years=21, name="Philadelphia: General Mills", idPrefix="age21", prompt="Office spaces, city blocks; morning commute platforms", notes=new[]{"Adult controls"}},
                new Entry{years=28, name="Chicago: 414 W. Dickens", idPrefix="age28", prompt="Brownstones, rooftops, L-station hints; patio start", notes=new[]{"Pacing up"}},
                new Entry{years=35, name="Ironman Gauntlet", idPrefix="age35", prompt="Training routes, track lanes, water hazards; medal reward", notes=new[]{"Ironman medal"}},
                new Entry{years=45, name="Chicago: Parson's → Airport", idPrefix="age45", prompt="Restaurant patio exit to CTA run; suitcase props", notes=new[]{"Family balance"}},
                new Entry{years=50, name="Costa Rica: Dominical → Casa Lumpusita", idPrefix="age50", prompt="Jungle paths, beach segments, hillside villa entry", notes=new[]{"Reflection"}},
            };

            Directory.CreateDirectory("Assets/WWIII/Scenes");
            foreach (var e in entries)
            {
                GenerateLevel(theme, set, e);
            }
            EditorUtility.DisplayDialog("WWIII", "Biographical campaign generated.", "OK");
        }

        private static void GenerateLevel(LevelThemeProfile theme, AgeSet set, Entry e)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            var maps = BiographicalTilemapBuilder.CreateGridWithTilemaps($"Tilemaps_{e.idPrefix}");
            var ageTheme = theme.GetForYears(e.years);
            BiographicalTilemapBuilder.PopulateGround(maps.ground, ageTheme.groundTiles.ToArray(), width: 120, height: 32, baseline: 8, seed: AILevelLayoutSampler.SeedFromPrompt(e.prompt));

            // Age system
            var ageRoot = new GameObject("AgeSystem");
            var mgr = ageRoot.AddComponent<AgeManager>();
            mgr.ageSet = set;
            int idx = set.ages.FindIndex(a => a != null && a.ageYears == e.years);
            mgr.initialAgeIndex = Mathf.Max(0, idx);

            // HUD anchor
            var hud = new GameObject("HUD");
            hud.AddComponent<WWIII.SideScroller.UI.SafeAreaPadding>();

            // Data-driven placement via LevelSpawnSpec if available
            var spawnSpec = LoadSpawnSpecForDesignId(e.idPrefix);
            if (spawnSpec != null)
            {
                var root = new GameObject("Spawns");
                foreach (var s in spawnSpec.entries)
                {
                    for (int c = 0; c < Mathf.Max(1, s.count); c++)
                    {
                        PlaceBySpec(root.transform, s);
                    }
                }
            }
            else
            {
                // Fallback: Place photo collectibles (5)
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab");
                if (prefab != null)
                {
                    var root = new GameObject("PhotosRoot");
                    for (int i = 0; i < 5; i++)
                    {
                        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        go.transform.position = new Vector3(8 + i * 18, 12 + Random.Range(-2f, 2f), 0);
                        go.transform.SetParent(root.transform);
                        var pc = go.GetComponent<WWIII.SideScroller.Collectibles.PhotoCollectible>();
                        pc.photoId = $"{e.idPrefix}_p{i+1}";
                    }
                }
            }

            // Drop a few simple power-ups placeholders
            PlacePowerup("RunningShoes", new Vector3(12, 16, 0));
            PlacePowerup("Cheeseball", new Vector3(24, 14, 0));

            // Save prompt/notes for provenance
            var notes = string.Join("\n- ", e.notes ?? new string[0]);
            var text = $"Level: {e.name}\nAge: {e.years}\nPrompt: {e.prompt}\nNotes:\n- {notes}";
            var promptPath = $"Assets/WWIII/Scenes/{e.idPrefix}_Notes.txt";
            File.WriteAllText(promptPath, text);
            AssetDatabase.ImportAsset(promptPath);

            // Save scene
            string scenePath = $"Assets/WWIII/Scenes/BioLevel_Age{e.years}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // LevelDefinition
            var def = ScriptableObject.CreateInstance<LevelDefinition>();
            def.displayName = e.name; def.ageYears = e.years; def.sceneName = Path.GetFileNameWithoutExtension(scenePath); def.photoCount = 5; def.summary = e.prompt;
            var defPath = $"Assets/WWIII/Data/LevelDefs/BioLevel_Age{e.years}.asset";
            Directory.CreateDirectory("Assets/WWIII/Data/LevelDefs");
            AssetDatabase.CreateAsset(def, defPath);
        }

        private static void PlacePowerup(string name, Vector3 pos)
        {
            var prefabPath = $"Assets/WWIII/Prefabs/Powerups/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
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
    }
}
