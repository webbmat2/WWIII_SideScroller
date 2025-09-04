#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.EditorTools
{
    public static class ImportBiographicalCSVs
    {
        private const string DefaultFolder = "Assets/StreamingAssets/Biographical";

        [MenuItem("WWIII/Content/Import CSV/Character_Appearance.csv...")]
        public static void ImportCharacterAppearance()
        {
            EnsureFolder();
            var src = EditorUtility.OpenFilePanel("Select Character_Appearance.csv", Application.dataPath, "csv");
            if (string.IsNullOrEmpty(src)) return;
            var dst = Path.Combine(DefaultFolder, "Character_Appearance.csv");
            File.Copy(src, dst, overwrite: true);
            AssetDatabase.Refresh();
            Debug.Log($"[CSV Import] Imported Character_Appearance.csv to {dst}");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/Biographical"))
            {
                AssetDatabase.CreateFolder("Assets/StreamingAssets", "Biographical");
            }
        }
    }
}
#endif

