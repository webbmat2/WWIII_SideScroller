using UnityEngine;

namespace WWIII.SideScroller.Audio
{
    internal static class AudioBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAudioSystem()
        {
            if (BiographicalAudioManager.Instance != null) return;

            // Prefer prefab if present (allows designer configuration)
            var prefab = Resources.Load<GameObject>("BiographicalAudioSystem");
            GameObject go;
            if (prefab != null)
            {
                go = Object.Instantiate(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("BiographicalAudioSystem");
                var music = go.AddComponent<AudioSource>();
                var sfx = go.AddComponent<AudioSource>();
                var mgr = go.AddComponent<BiographicalAudioManager>();
                mgr.musicSource = music;
                mgr.effectsSource = sfx;
                AudioOptimizationSettings.ConfigureManager(mgr);
                go.AddComponent<AgeMusicDriver>();
            }
        }
    }
}
