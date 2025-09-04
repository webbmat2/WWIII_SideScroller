using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Audio
{
    public static class CreateBiographicalAudioConfigMenu
    {
        [MenuItem("WWIII/Audio/Create Default Biographical Audio System")] 
        public static void CreatePrefab()
        {
            var folderPath = "Assets/WWIII/Prefabs";
            if (!AssetDatabase.IsValidFolder("Assets/WWIII"))
            {
                AssetDatabase.CreateFolder("Assets", "WWIII");
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/WWIII", "Prefabs");
            }

            var prefabPath = folderPath + "/BiographicalAudioSystem.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                EditorUtility.DisplayDialog("WWIII", "BiographicalAudioSystem.prefab already exists in WWIII/Prefabs.", "OK");
                Selection.activeObject = existingPrefab;
                EditorGUIUtility.PingObject(existingPrefab);
                return;
            }

            var go = new GameObject("BiographicalAudioSystem");
            var music = go.AddComponent<AudioSource>();
            var sfx = go.AddComponent<AudioSource>();
            var mgr = go.AddComponent<WWIII.SideScroller.Audio.BiographicalAudioManager>();
            go.AddComponent<WWIII.SideScroller.Audio.AgeMusicDriver>();

            mgr.musicSource = music;
            mgr.effectsSource = sfx;
            WWIII.SideScroller.Audio.AudioOptimizationSettings.ConfigureManager(mgr);

            // Try to auto-wire placeholder clips if found
            var pChild = "Assets/Audio/Music/Childhood_Theme.wav";
            var pTeen = "Assets/Audio/Music/Adolescence_Theme.wav";
            var pAdult = "Assets/Audio/Music/Adult_Theme.wav";
            var pReflect = "Assets/Audio/Music/Reflection_Theme.wav";
            var pStinger = "Assets/Audio/SFX/MemoryReveal_Stinger.wav";

            var cChild = AssetDatabase.LoadAssetAtPath<AudioClip>(pChild);
            var cTeen = AssetDatabase.LoadAssetAtPath<AudioClip>(pTeen);
            var cAdult = AssetDatabase.LoadAssetAtPath<AudioClip>(pAdult);
            var cReflect = AssetDatabase.LoadAssetAtPath<AudioClip>(pReflect);
            var cStinger = AssetDatabase.LoadAssetAtPath<AudioClip>(pStinger);

            if (mgr.lifeStages != null && mgr.lifeStages.Length >= 4)
            {
                if (cChild != null) mgr.lifeStages[0].musicTrack = cChild;
                if (cTeen != null) mgr.lifeStages[1].musicTrack = cTeen;
                if (cAdult != null) mgr.lifeStages[2].musicTrack = cAdult;
                if (cReflect != null) mgr.lifeStages[3].musicTrack = cReflect;
            }
            if (cStinger != null) mgr.memoryRevealStinger = cStinger;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            EditorUtility.DisplayDialog("WWIII", "Created WWIII/Prefabs/BiographicalAudioSystem.prefab. Assign music and stinger clips.", "OK");
        }
    }
}
