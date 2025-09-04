using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Level
{
    public static class URP2DAssetOptimizer
    {
        [MenuItem("WWIII/Level/Optimize Platform Assets for URP 2D (ASTC)")]
        public static void Optimize()
        {
            string root = "Assets/ThirdParty/BayatGames/Platform Game Assets Ultimate";
            if (!AssetDatabase.IsValidFolder(root))
            {
                EditorUtility.DisplayDialog("WWIII", "Platform Game Assets Ultimate folder not found.", "OK");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root });
            int changed = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
                if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                var ios = importer.GetPlatformTextureSettings("iPhone");
                if (!ios.overridden || ios.format != TextureImporterFormat.ASTC_6x6)
                {
                    ios.overridden = true; ios.format = TextureImporterFormat.ASTC_6x6; ios.compressionQuality = 50; ios.maxTextureSize = 1024;
                    importer.SetPlatformTextureSettings(ios); dirty = true;
                }
                var android = importer.GetPlatformTextureSettings("Android");
                if (!android.overridden || android.format != TextureImporterFormat.ASTC_6x6)
                {
                    android.overridden = true; android.format = TextureImporterFormat.ASTC_6x6; android.compressionQuality = 50; android.maxTextureSize = 1024;
                    importer.SetPlatformTextureSettings(android); dirty = true;
                }
                if (dirty) { importer.SaveAndReimport(); changed++; }
            }
            EditorUtility.DisplayDialog("WWIII", $"Optimized {changed} textures for URP 2D.", "OK");
        }
    }
}

