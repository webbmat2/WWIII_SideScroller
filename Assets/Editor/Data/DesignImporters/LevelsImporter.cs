using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.DesignImporters
{
    public static class LevelsImporter
    {
        private const string Source = "Assets/WWIII/Data/DesignData/Levels.csv";
        private const string OutFolder = "Assets/WWIII/Data/Definitions/Levels";

        [MenuItem("WWIII/Data/Rebuild Levels from CSV")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(OutFolder);
            foreach (var row in CSVUtil.Read(Source))
            {
                var id = Get(row, "designId"); if (string.IsNullOrEmpty(id)) continue;
                var path = Path.Combine(OutFolder, id + ".asset").Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<LevelDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.name = id;
                asset.designId = id;
                asset.displayName = Get(row, "displayName", id);
                asset.ageYears = ParseI(Get(row, "ageYears"), 7);
                asset.sceneName = Get(row, "sceneName", $"BioLevel_Age{asset.ageYears}");
                asset.photoCount = ParseI(Get(row, "photoCount"), 5);
                asset.bossId = Get(row, "bossId");
                asset.musicCue = Get(row, "musicCue");
                asset.ambientCue = Get(row, "ambientCue");
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Levels imported.", "OK");
        }

        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static int ParseI(string s, int def)
            => int.TryParse(s, out var i) ? i : def;
    }
}
