using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Audio.Editor
{
    public static class BiographicalAudioPrefabCreator
    {
        [MenuItem("WWIII/Create/Biographical Audio System Prefab")]
        public static void CreatePrefab()
        {
            var go = new GameObject("BiographicalAudioSystem");

            var music = go.AddComponent<AudioSource>();
            var sfx = go.AddComponent<AudioSource>();
            var mgr = go.AddComponent<BiographicalAudioManager>();
            var driver = go.AddComponent<AgeMusicDriver>();

            mgr.musicSource = music;
            mgr.effectsSource = sfx;
            AudioOptimizationSettings.ConfigureManager(mgr);

            var savePath = "Assets/WWIII/Prefabs/BiographicalAudioSystem.prefab";
            var folder = System.IO.Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(folder) && !System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            GameObject.DestroyImmediate(go);

            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Saved BiographicalAudioSystem prefab to {savePath}");
        }
    }
}
