using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Level;

namespace WWIII.SideScroller.Editor.Level
{
    public static class PlatformAssetsConfigurator
    {
        [MenuItem("WWIII/Level/Configure Platform Game Assets Ultimate")]
        public static void Configure()
        {
            string root = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate";
            if (!AssetDatabase.IsValidFolder(root))
            {
                EditorUtility.DisplayDialog("WWIII", "Platform Game Assets Ultimate folder not found.", "OK");
                return;
            }

            int updated = 0;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { root });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                var hasRenderer = go.GetComponentInChildren<SpriteRenderer>() != null;
                if (!hasRenderer) continue;

                // Ensure sorting layer and layer
                var wasDirty = false;
                foreach (var r in go.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (r.sortingLayerName != "Environment")
                    {
                        r.sortingLayerName = "Environment"; wasDirty = true;
                    }
                }

                // Set collectibles layer if likely collectible
                if (go.name.ToLower().Contains("collect") || go.name.ToLower().Contains("coin") || go.name.ToLower().Contains("gem") || go.name.ToLower().Contains("photo"))
                {
                    int colLayer = LayerMask.NameToLayer("Collectibles");
                    if (colLayer < 0) colLayer = 18; // fallback index
                    foreach (var tr in go.GetComponentsInChildren<Transform>(true))
                    {
                        tr.gameObject.layer = colLayer;
                    }
                    wasDirty = true;
                }

                // Add age gating component optionally
                if (go.GetComponent<AgeConditionalObject>() == null)
                {
                    var comp = go.AddComponent<AgeConditionalObject>();
                    comp.useYears = true; comp.minYears = 7; comp.maxYears = 999;
                    wasDirty = true;
                }

                if (wasDirty)
                {
                    EditorUtility.SetDirty(go);
                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", $"Configured {updated} prefabs.", "OK");
        }
    }
}
