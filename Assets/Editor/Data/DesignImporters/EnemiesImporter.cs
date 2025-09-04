using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Design;

namespace WWIII.SideScroller.Editor.DesignImporters
{
    public static class EnemiesImporter
    {
        private const string Source = "Assets/WWIII/Data/DesignData/Enemies.csv";
        private const string OutFolder = "Assets/WWIII/Data/Definitions/Enemies";

        [MenuItem("WWIII/Data/Rebuild Enemies from CSV")]
        public static void Rebuild()
        {
            Directory.CreateDirectory(OutFolder);
            foreach (var row in CSVUtil.Read(Source))
            {
                var id = Get(row, "designId"); if (string.IsNullOrEmpty(id)) continue;
                var path = Path.Combine(OutFolder, id + ".asset").Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<EnemyDefinition>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.name = id; asset.designId = id;
                asset.displayName = Get(row, "displayName", id);
                asset.prefabKey = Get(row, "prefabKey");
                asset.hp = ParseF(Get(row, "hp"), 10f);
                asset.damage = ParseF(Get(row, "damage"), 1f);
                asset.speed = ParseF(Get(row, "speed"), 3f);
                asset.aiType = Get(row, "aiType", "patrol");
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Enemies imported.", "OK");
        }

        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static float ParseF(string s, float def)
            => float.TryParse(s, out var f) ? f : def;
    }
}
