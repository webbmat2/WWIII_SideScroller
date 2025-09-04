using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Level
{
    public static class LevelThemeProfileBuilder
    {
        [MenuItem("WWIII/Level/Create Default LevelThemeProfile from Platform Assets")]
        public static void CreateDefault()
        {
            var assetPath = "Assets/WWIII/Data/LevelThemeProfile.asset";
            Directory.CreateDirectory("Assets/WWIII/Data");
            var profile = ScriptableObject.CreateInstance<LevelThemeProfile>();

            // Find a few generic props in Platform Game Assets Ultimate
            string root = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate/Prefabs";
            var props = new List<GameObject>();
            if (AssetDatabase.IsValidFolder(root))
            {
                void AddIf(string filter)
                {
                    var guids = AssetDatabase.FindAssets(filter, new[] { root });
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (go != null && !props.Contains(go)) props.Add(go);
                    }
                }
                AddIf("t:Prefab Coin");
                AddIf("t:Prefab Key");
                AddIf("t:Prefab HealthBar");
                AddIf("t:Prefab Enemy");
                AddIf("t:Prefab Player");
                AddIf("t:Prefab Particle");
            }

            // Create age themes (tiles empty for now; platform/prop prefabs present)
            LevelThemeProfile.AgeTheme Make(int years, Color tint)
            {
                var at = new LevelThemeProfile.AgeTheme
                {
                    ageYears = years,
                    ambientTint = tint,
                    platformPrefabs = new List<GameObject>(),
                    propPrefabs = new List<GameObject>(props)
                };
                return at;
            }

            profile.themes = new List<LevelThemeProfile.AgeTheme>
            {
                Make(7, new Color(1f,1f,1f)),
                Make(11, new Color(0.95f,1f,1f)),
                Make(14, new Color(0.92f,0.98f,1f)),
                Make(17, new Color(0.9f,0.96f,1f)),
                Make(21, new Color(0.88f,0.95f,1f)),
                Make(28, new Color(0.86f,0.94f,1f)),
                Make(42, new Color(0.85f,0.93f,1f)),
                Make(56, new Color(0.84f,0.92f,1f)),
                Make(80, new Color(0.83f,0.91f,1f)),
            };

            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(profile);
            EditorUtility.DisplayDialog("WWIII", "Created default LevelThemeProfile at Assets/WWIII/Data.", "OK");
        }
    }
}

