using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class SpriteImportUtility
    {
        public static void ConfigureFolderForSprites(string folder)
        {
            var files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
            foreach (var f in files)
                ConfigureSpriteImporter(f);
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureSpriteImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single; // per file is one frame; frame animation via multiple files
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 64; // adjust to your game scale
            importer.maxTextureSize = 1024;

            // iOS ASTC settings
            var ios = importer.GetPlatformTextureSettings("iPhone");
            ios.overridden = true;
            ios.format = TextureImporterFormat.ASTC_6x6;
            ios.compressionQuality = 50; // balanced
            ios.maxTextureSize = 1024;
            importer.SetPlatformTextureSettings(ios);

            // Android ASTC as well for parity
            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_6x6;
            android.compressionQuality = 50;
            android.maxTextureSize = 1024;
            importer.SetPlatformTextureSettings(android);

            importer.SaveAndReimport();
        }
    }
}

