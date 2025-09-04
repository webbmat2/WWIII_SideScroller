using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Level
{
    public static class PhotoCollectiblePlacer
    {
        [MenuItem("WWIII/Collectibles/Place Photos In 9 Bio Levels")] 
        public static void PlaceInLevels()
        {
            var prefabPath = "Assets/WWIII/Prefabs/Collectibles/PhotoCollectible.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("WWIII", "PhotoCollectible prefab not found. Create it first.", "OK");
                return;
            }

            int placedTotal = 0;
            foreach (var age in new[] {7,11,14,17,21,28,42,56,80})
            {
                var scenePath = $"Assets/WWIII/Scenes/BioLevel_Age{age}.unity";
                if (!File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var root = new GameObject("PhotosRoot");
                for (int i = 0; i < 6; i++)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    go.transform.SetParent(root.transform);
                    go.transform.position = new Vector3(6 + i * 12, 12 + Random.Range(-2f, 2f), 0);
                    var pc = go.GetComponent<WWIII.SideScroller.Collectibles.PhotoCollectible>();
                    pc.photoId = $"age{age}_p{i+1}";
                }
                EditorSceneManager.SaveScene(scene);
                placedTotal += 6;
            }

            EditorUtility.DisplayDialog("WWIII", $"Placed {placedTotal} photo collectibles across biographical levels.", "OK");
        }
    }
}

