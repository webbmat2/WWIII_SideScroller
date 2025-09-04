using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Level
{
    public static class LevelSpawnSpecLinker
    {
        private const string ReportsDir = "Assets/WWIII/Reports";

        [MenuItem("WWIII/Level/Link LevelDefinitions → SpawnSpecs (Auto)")]
        public static void LinkAll()
        {
            int linked = 0, skipped = 0, missing = 0;
            foreach (var def in FindAllLevelDefinitions())
            {
                var expected = ExpectedSpawnSpecPath(def);
                var spec = AssetDatabase.LoadAssetAtPath<LevelSpawnSpec>(expected);
                if (spec == null)
                {
                    missing++;
                    continue;
                }
                if (def.spawnSpec == spec)
                {
                    skipped++;
                    continue;
                }
                def.spawnSpec = spec;
                EditorUtility.SetDirty(def);
                linked++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("WWIII", $"SpawnSpec linking complete.\nLinked: {linked}\nAlready OK: {skipped}\nMissing specs: {missing}", "OK");
        }

        [MenuItem("WWIII/Level/Validate Spawn Specs and References (Report)")]
        public static void ValidateSpawnSpecs()
        {
            Directory.CreateDirectory(ReportsDir);
            var path = Path.Combine(ReportsDir, $"SpawnValidationReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            int ok = 0, warn = 0, err = 0;
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("Spawn Spec Validation Report");
                sw.WriteLine($"Generated: {DateTime.Now}");
                sw.WriteLine();

                foreach (var def in FindAllLevelDefinitions())
                {
                    if (def == null) continue;
                    if (def.spawnSpec == null)
                    {
                        sw.WriteLine($"[WARN] {def.name} has no spawnSpec linked (expected {ExpectedSpawnSpecPath(def)})");
                        warn++;
                        continue;
                    }

                    var spec = def.spawnSpec;
                    if (spec.entries == null || spec.entries.Count == 0)
                    {
                        sw.WriteLine($"[WARN] {def.name}: spawnSpec {spec.name} has no entries");
                        warn++;
                        continue;
                    }

                    sw.WriteLine($"[OK] {def.name}: spawnSpec {spec.name} entries={spec.entries.Count}"); ok++;

                    int i = 0;
                    foreach (var e in spec.entries)
                    {
                        i++;
                        if (e == null)
                        {
                            sw.WriteLine($"  [ERR] Entry {i}: null"); err++; continue;
                        }
                        var t = (e.type ?? string.Empty).Trim();
                        var id = (e.designId ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(t)) { sw.WriteLine($"  [ERR] Entry {i}: missing type"); err++; continue; }
                        if (!float.IsFinite(e.position.x) || !float.IsFinite(e.position.y))
                        { sw.WriteLine($"  [ERR] Entry {i}: invalid position {e.position}"); err++; }
                        if (e.count <= 0) { sw.WriteLine($"  [WARN] Entry {i}: non-positive count {e.count}"); warn++; }

                        if (t.Equals("Collectible", StringComparison.OrdinalIgnoreCase))
                        {
                            var cdef = LoadByDesignId<WWIII.SideScroller.Design.CollectibleDefinition>("Assets/WWIII/Data/Definitions/Collectibles", id);
                            if (cdef == null)
                            { sw.WriteLine($"  [ERR] Entry {i}: Collectible def not found: {id}"); err++; }
                            else if (!ResolvePrefab(cdef.prefabKey))
                            { sw.WriteLine($"  [ERR] Entry {i}: Collectible prefab missing: {cdef.prefabKey}"); err++; }
                            else { /* ok */ }
                        }
                        else if (t.Equals("PowerUp", StringComparison.OrdinalIgnoreCase))
                        {
                            var ppath = $"Assets/WWIII/Prefabs/Powerups/{id}.prefab";
                            if (!File.Exists(ppath))
                            { sw.WriteLine($"  [ERR] Entry {i}: PowerUp prefab not found: {ppath}"); err++; }
                        }
                        else if (t.Equals("Enemy", StringComparison.OrdinalIgnoreCase))
                        {
                            var edef = LoadByDesignId<WWIII.SideScroller.Design.EnemyDefinition>("Assets/WWIII/Data/Definitions/Enemies", id);
                            if (edef == null)
                            { sw.WriteLine($"  [ERR] Entry {i}: Enemy def not found: {id}"); err++; }
                            else if (!ResolvePrefab(edef.prefabKey))
                            { sw.WriteLine($"  [ERR] Entry {i}: Enemy prefab missing: {edef.prefabKey}"); err++; }
                        }
                        else
                        {
                            sw.WriteLine($"  [WARN] Entry {i}: Unknown type '{t}' for {id}"); warn++;
                        }
                    }
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(path);
            EditorUtility.DisplayDialog("WWIII", $"Spawn validation report done. OK:{ok} WARN:{warn} ERR:{err}", "OK");
        }

        [MenuItem("WWIII/Level/Link + Validate Spawn Specs")]
        public static void LinkAndValidate()
        {
            LinkAll();
            ValidateSpawnSpecs();
        }

        private static IEnumerable<LevelDefinition> FindAllLevelDefinitions()
        {
            var folders = new[] { "Assets/WWIII/Data/LevelDefs", "Assets/WWIII/Data/Definitions/Levels" };
            var guids = AssetDatabase.FindAssets("t:WWIII.SideScroller.Level.LevelDefinition", folders);
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(p);
                if (def != null) yield return def;
            }
        }

        private static string ExpectedSpawnSpecPath(LevelDefinition def)
        {
            // Prefer designId if present, else derive from age years
            var id = !string.IsNullOrEmpty(def.designId) ? def.designId : $"age{def.ageYears}";
            return $"Assets/WWIII/Data/Definitions/Spawns/{id}.asset";
        }

        private static T LoadByDesignId<T>(string folder, string id) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(id)) return null;
            var p = System.IO.Path.Combine(folder, id + ".asset").Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<T>(p);
        }

        private static bool ResolvePrefab(string keyOrPath)
        {
            if (string.IsNullOrEmpty(keyOrPath)) return false;
            // We only validate direct asset paths under Assets/, not addressable keys
            if (keyOrPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(keyOrPath);
                return obj != null;
            }
            return true; // treat non-asset keys as ok (addressables)
        }
    }
}

