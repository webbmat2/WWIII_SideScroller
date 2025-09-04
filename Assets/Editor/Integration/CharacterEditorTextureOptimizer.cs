using UnityEngine;
using UnityEditor;
using System.IO;

namespace WWIII.SideScroller.Editor.Integration
{
    public static class CharacterEditorTextureOptimizer
    {
        [MenuItem("WWIII/Character Editor/Fix Texture Compression")]
        public static void OptimizeCharacterEditorTextures()
        {
            string[] roots = new[]
            {
                "Assets/HeroEditor/FantasyHeroes/Sprites",
                "Assets/HeroEditor/Common/Sprites"
            };

            int processed = 0;

            // Process fixed roots
            foreach (var root in roots)
            {
                if (AssetDatabase.IsValidFolder(root))
                {
                    processed += OptimizeUnderFolder(root);
                }
            }

            // Process Extensions/*/Sprites
            var extensionsRoot = "Assets/HeroEditor/Extensions";
            if (AssetDatabase.IsValidFolder(extensionsRoot))
            {
                var subdirs = Directory.GetDirectories(extensionsRoot, "*", SearchOption.AllDirectories);
                foreach (var dir in subdirs)
                {
                    if (dir.Replace('\\','/').EndsWith("/Sprites"))
                    {
                        processed += OptimizeUnderFolder(dir);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[CharacterEditorTextureOptimizer] Optimized {processed} textures for mobile performance");
        }

        private static int OptimizeUnderFolder(string folder)
        {
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (OptimizeTextureForMobile(path))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool OptimizeTextureForMobile(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;
            importer.compressionQuality = 50;

            var ios = new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_4x4,
                compressionQuality = 50,
                crunchedCompression = true
            };
            importer.SetPlatformTextureSettings(ios);

            EditorUtility.SetDirty(importer);
            try
            {
                importer.SaveAndReimport();
            }
            catch
            {
                // keep going if one asset fails
            }
            return true;
        }
    }
}

