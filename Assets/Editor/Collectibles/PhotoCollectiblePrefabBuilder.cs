using System.IO;
using UnityEditor;
using UnityEngine;
using WWIII.SideScroller.Collectibles;

namespace WWIII.SideScroller.Editor.Collectibles
{
    public static class PhotoCollectiblePrefabBuilder
    {
        [MenuItem("WWIII/Collectibles/Create PhotoCollectible Prefab")] 
        public static void CreatePhotoPrefab()
        {
            var folder = "Assets/WWIII/Prefabs/Collectibles";
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "PhotoCollectible.prefab").Replace('\\','/');

            var go = new GameObject("PhotoCollectible");
            var sr = go.AddComponent<SpriteRenderer>();
            var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 0.4f;
            var pc = go.AddComponent<PhotoCollectible>();
            pc.photoId = "photo_" + System.Guid.NewGuid().ToString("N");

            int colLayer = LayerMask.NameToLayer("Collectibles");
            go.layer = colLayer >= 0 ? colLayer : 18;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            EditorGUIUtility.PingObject(prefab);
            EditorUtility.DisplayDialog("WWIII", $"Created prefab at {path}.", "OK");
        }
    }
}

