using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Design;

namespace WWIII.SideScroller.Editor.DesignImporters
{
    public static class CollectiblesImporter
    {
        private const string Source = "Assets/WWIII/Data/DesignData/Collectibles.csv";
        private const string OutFolder = "Assets/WWIII/Data/Definitions/Collectibles";

        [MenuItem("WWIII/Data/Rebuild Collectibles from CSV")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(OutFolder);
            foreach (var row in CSVUtil.Read(Source))
            {
                var id = Get(row, "designId"); if (string.IsNullOrEmpty(id)) continue;
                var path = Path.Combine(OutFolder, id + ".asset").Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<CollectibleDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<CollectibleDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.name = id; asset.designId = id;
                asset.displayName = Get(row, "displayName", id);
                asset.prefabKey = Get(row, "prefabKey");
                asset.points = ParseI(Get(row, "points"), 1);
                asset.isPhoto = ParseB(Get(row, "isPhoto"));
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Collectibles imported.", "OK");
        }

        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static int ParseI(string s, int def)
            => int.TryParse(s, out var i) ? i : def;
        private static bool ParseB(string s)
            => s.Equals("true", System.StringComparison.OrdinalIgnoreCase) || s=="1";
    }
}
