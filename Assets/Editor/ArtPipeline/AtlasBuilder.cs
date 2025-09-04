using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class AtlasBuilder
    {
        public static SpriteAtlas CreateOrUpdateAtlas(string atlasPath, string sourceFolder)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(atlasPath) ?? "Assets");
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            bool created = false;
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, atlasPath);
                created = true;
            }

            var pack = atlas.GetPackingSettings();
            pack.enableRotation = false;
            pack.enableTightPacking = false;
            pack.padding = 4;
            atlas.SetPackingSettings(pack);

            var tex = atlas.GetTextureSettings();
            tex.readable = false;
            tex.generateMipMaps = false;
            tex.sRGB = true;
            atlas.SetTextureSettings(tex);

            var ios = new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 50,
                maxTextureSize = 2048
            };
            var android = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 50,
                maxTextureSize = 2048
            };
            SpriteAtlasExtensions.SetPlatformSettings(atlas, ios);
            SpriteAtlasExtensions.SetPlatformSettings(atlas, android);

            // Include the folder
            var folderObj = AssetDatabase.LoadAssetAtPath<Object>(sourceFolder);
            if (folderObj != null)
            {
                atlas.Remove(atlas.GetPackables());
                atlas.Add(new Object[] { folderObj });
            }

            if (created)
                AssetDatabase.SaveAssets();

            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
            return atlas;
        }
    }
}

