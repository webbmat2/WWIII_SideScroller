using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace WWIII.SideScroller.Data
{
    [Serializable]
    public class CharacterAppearanceData
    {
        public string levelID;
        public int age;
        public string lifeStage;
        public string hairStyle;
        public string hairColor;
        public string outfitType;
        public string outfitColors;
        public string accessories;
        public string skinTone;
        public float bodyScale = 1f;
        public string photoReference;
        public string spineSkeletonFamily; // Child/Teen/Adult
        public string notes;

        public static List<CharacterAppearanceData> LoadFromCSV(string csvPath)
        {
            var list = new List<CharacterAppearanceData>();
            try
            {
                if (!File.Exists(csvPath))
                {
                    Debug.LogWarning($"[CharacterAppearanceData] CSV not found: {csvPath}");
                    return list;
                }
                var lines = File.ReadAllLines(csvPath);
                if (lines.Length <= 1) return list;
                var headers = Split(lines[0]);
                for (int i = 1; i < lines.Length; i++)
                {
                    var row = Split(lines[i]);
                    if (row.Length == 0 || string.IsNullOrWhiteSpace(row[0])) continue;
                    var data = new CharacterAppearanceData();
                    string Get(string name)
                    {
                        for (int c = 0; c < headers.Length && c < row.Length; c++)
                            if (string.Equals(headers[c], name, StringComparison.OrdinalIgnoreCase)) return row[c];
                        return string.Empty;
                    }

                    data.levelID = Get("Level_ID");
                    int.TryParse(Get("Age"), out data.age);
                    data.lifeStage = Get("Life_Stage");
                    data.hairStyle = Get("Hair_Style");
                    data.hairColor = Get("Hair_Color");
                    data.outfitType = Get("Outfit_Type");
                    data.outfitColors = Get("Outfit_Colors");
                    data.accessories = Get("Accessories");
                    data.skinTone = Get("Skin_Tone");
                    float.TryParse(Get("Body_Scale"), NumberStyles.Float, CultureInfo.InvariantCulture, out data.bodyScale);
                    data.photoReference = Get("Photo_Reference");
                    data.spineSkeletonFamily = Get("Spine_Skeleton");
                    data.notes = Get("Notes");
                    list.Add(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CharacterAppearanceData] CSV load failed: {e.Message}");
            }
            return list;
        }

        private static string[] Split(string line)
        {
            var res = new List<string>();
            bool q = false; var cur = "";
            foreach (var ch in line)
            {
                if (ch == '"') { q = !q; continue; }
                if (ch == ',' && !q) { res.Add(cur); cur = ""; continue; }
                cur += ch;
            }
            res.Add(cur);
            for (int i = 0; i < res.Count; i++) res[i] = res[i].Trim();
            return res.ToArray();
        }

        public static bool TryParseHexColor(string hex, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(hex)) return false;
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 6) hex += "FF"; // add alpha
            if (hex.Length != 8) return false;
            if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var val))
            {
                color = new Color(
                    ((val >> 24) & 0xFF) / 255f,
                    ((val >> 16) & 0xFF) / 255f,
                    ((val >> 8) & 0xFF) / 255f,
                    (val & 0xFF) / 255f);
                return true;
            }
            return false;
        }
    }
}

