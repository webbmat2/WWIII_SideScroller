using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Powerups;

namespace WWIII.SideScroller.Editor.DesignImporters
{
    public static class PowerUpsImporter
    {
        private const string Source = "Assets/WWIII/Data/DesignData/PowerUps.csv";
        private const string OutFolder = "Assets/WWIII/Data/Definitions/PowerUps";

        [MenuItem("WWIII/Data/Rebuild PowerUps from CSV")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(OutFolder);
            foreach (var row in CSVUtil.Read(Source))
            {
                var id = Get(row, "designId"); if (string.IsNullOrEmpty(id)) continue;
                var path = Path.Combine(OutFolder, id + ".asset").Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<PowerUpDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<PowerUpDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.name = id;
                asset.displayName = Get(row, "displayName", id);
                asset.duration = ParseF(Get(row, "duration"), 10f);
                asset.invulnerable = ParseB(Get(row, "invulnerable"));
                asset.speedMultiplier = ParseF(Get(row, "speedMultiplier"), 1f);
                asset.healAmount = ParseF(Get(row, "healAmount"), 0f);
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "PowerUps imported.", "OK");
        }

        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static float ParseF(string s, float def)
            => float.TryParse(s, out var f) ? f : def;
        private static bool ParseB(string s)
            => s.Equals("true", System.StringComparison.OrdinalIgnoreCase) || s=="1";
    }
}
