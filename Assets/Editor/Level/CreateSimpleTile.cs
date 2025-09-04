#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.EditorTools
{
    public static class CreateSimpleTile
    {
        [MenuItem("WWIII/Level/Create Simple Ground Tile")] 
        public static void Create()
        {
            const string folder = "Assets/WWIII/Tiles";
            if (!AssetDatabase.IsValidFolder("Assets/WWIII")) AssetDatabase.CreateFolder("Assets", "WWIII");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/WWIII", "Tiles");

            var texPath = Path.Combine(folder, "SimpleGround.png");
            var tilePath = Path.Combine(folder, "SimpleGround.asset");

            // Create a 64x64 texture
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var col = new Color32(90, 140, 90, 255); // muted green
            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels32(pixels);
            tex.Apply();

            File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);

            // Configure importer as Sprite
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32; // 32 px = 1 unit
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
            if (sprite == null)
            {
                Debug.LogWarning("[CreateSimpleTile] Failed to load Sprite.");
                return;
            }

            // Create or update Tile asset
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.Grid;
                AssetDatabase.CreateAsset(tile, tilePath);
            }
            else
            {
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.Grid;
                EditorUtility.SetDirty(tile);
            }
            AssetDatabase.SaveAssets();

            Debug.Log($"[CreateSimpleTile] Created/updated SimpleGround tile at {tilePath}. Now run 'WWIII/Level/Strip To Simple Tile Ground'.");
        }
    }
}
#endif
