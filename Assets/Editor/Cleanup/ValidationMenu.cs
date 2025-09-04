using System.Linq;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Cleanup
{
    public static class ValidationMenu
    {
        [MenuItem("WWIII/Validation/Quick Validate Core Systems")] 
        public static void QuickValidate()
        {
            int ok = 0, warn = 0, err = 0;

            void OK(string m) { Debug.Log("[OK] " + m); ok++; }
            void WARN(string m) { Debug.LogWarning("[WARN] " + m); warn++; }
            void ERR(string m) { Debug.LogError("[ERR] " + m); err++; }

            // Scene presence
            var age7 = AssetDatabase.FindAssets("BioLevel_Age7 t:Scene").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            if (!string.IsNullOrEmpty(age7)) OK($"Found scene: {age7}"); else ERR("BioLevel_Age7 scene not found under Assets/WWIII/Scenes");

            // CSV design data
            string[] csvs = new[]
            {
                "Assets/WWIII/Data/DesignData/Levels.csv",
                "Assets/WWIII/Data/DesignData/Spawns.csv",
                "Assets/WWIII/Data/DesignData/PowerUps.csv",
                "Assets/WWIII/Data/DesignData/Collectibles.csv",
                "Assets/WWIII/Data/DesignData/Enemies.csv",
            };
            foreach (var p in csvs)
            {
                var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
                if (ta != null && !string.IsNullOrWhiteSpace(ta.text)) OK($"CSV present: {p}"); else WARN($"CSV missing or empty: {p}");
            }

            // Level definitions linked to scenes/spawn specs
            var levelDefGuids = AssetDatabase.FindAssets("t:WWIII.SideScroller.Level.LevelDefinition", new[] { "Assets/WWIII/Data/LevelDefs" });
            foreach (var g in levelDefGuids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(p);
                if (def == null) continue;
                if (!string.IsNullOrEmpty(def.sceneName)) OK($"LevelDef {def.name} has scene name {def.sceneName}"); else WARN($"LevelDef {def.name} missing scene name");
                if (def.spawnSpec != null) OK($"LevelDef {def.name} has spawn spec {def.spawnSpec.name}"); else WARN($"LevelDef {def.name} missing spawn spec reference");
            }

            // Controller evolution system presence
            var controller = AssetDatabase.FindAssets("t:MonoScript JimAgeController").FirstOrDefault();
            if (!string.IsNullOrEmpty(controller)) OK("JimAgeController script present"); else WARN("JimAgeController script not found");

            // Age-based haptic feedback progression presence (if any)
            var haptic = AssetDatabase.FindAssets("Haptic t:MonoScript").FirstOrDefault();
            if (!string.IsNullOrEmpty(haptic)) OK("Haptic-related scripts detected"); else WARN("No obvious haptic scripts found (ok if not implemented yet)");

            EditorUtility.DisplayDialog("WWIII Validation", $"OK: {ok}\nWarnings: {warn}\nErrors: {err}", "OK");
        }
    }
}

