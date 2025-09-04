using UnityEngine;

namespace WWIII.SideScroller.Audio
{
    public static class AudioOptimizationSettings
    {
        public static void ApplyMobileDefaults(AudioSource source, bool isMusic)
        {
            if (source == null) return;

            source.playOnAwake = false;
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
            source.bypassReverbZones = true;
            source.priority = isMusic ? 128 : 64;
            source.spatialize = false;
            source.spatialBlend = 0f; // 2D for side scroller
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 5f;
            source.maxDistance = 50f;
            source.loop = isMusic;
            source.reverbZoneMix = 0f;
            source.ignoreListenerPause = isMusic;
        }

        public static void ConfigureManager(BiographicalAudioManager mgr)
        {
            if (mgr == null) return;
            if (mgr.musicSource != null) ApplyMobileDefaults(mgr.musicSource, true);
            if (mgr.effectsSource != null) ApplyMobileDefaults(mgr.effectsSource, false);
        }
    }
}

