using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Editor.Data;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.DesignImporters
{
    public static class SpawnsImporter
    {
        private const string Source = "Assets/WWIII/Data/DesignData/Spawns.csv";
        private const string OutFolder = "Assets/WWIII/Data/Definitions/Spawns";

        [MenuItem("WWIII/Data/Rebuild Spawns from CSV")] 
        public static void Rebuild()
        {
            Directory.CreateDirectory(OutFolder);
            // Aggregate entries per levelDesignId
            LevelSpawnSpec current = null;
            string currentId = null;
            void Flush()
            {
                if (current == null) return;
                EditorUtility.SetDirty(current);
                current = null; currentId = null;
            }

            foreach (var row in CSVUtil.Read(Source))
            {
                var levelId = Get(row, "levelDesignId"); if (string.IsNullOrEmpty(levelId)) continue;
                if (currentId != levelId)
                {
                    Flush();
                    var path = Path.Combine(OutFolder, levelId + ".asset").Replace("\\", "/");
                    current = AssetDatabase.LoadAssetAtPath<LevelSpawnSpec>(path);
                    if (current == null)
                    {
                        current = ScriptableObject.CreateInstance<LevelSpawnSpec>();
                        current.levelDesignId = levelId;
                        AssetDatabase.CreateAsset(current, path);
                    }
                    else
                    {
                        current.entries.Clear();
                        current.levelDesignId = levelId;
                    }
                    currentId = levelId;
                }

                var entry = new LevelSpawnSpec.Entry
                {
                    type = Get(row, "type"),
                    designId = Get(row, "designId"),
                    position = new Vector2(ParseF(Get(row, "x"), 0f), ParseF(Get(row, "y"), 0f)),
                    count = ParseI(Get(row, "count"), 1)
                };
                current.entries.Add(entry);
            }
            Flush();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Spawns imported.", "OK");
        }

        private static string Get(System.Collections.Generic.Dictionary<string,string> d, string k, string def = "")
            => d.TryGetValue(k, out var v) ? v : def;
        private static float ParseF(string s, float def)
            => float.TryParse(s, out var f) ? f : def;
        private static int ParseI(string s, int def)
            => int.TryParse(s, out var i) ? i : def;
    }
}
