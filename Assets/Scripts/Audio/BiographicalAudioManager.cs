using UnityEngine;
using DG.Tweening;

namespace WWIII.SideScroller.Audio
{
    [System.Serializable]
    public class LifeStageAudio
    {
        [Header("Life Stage Configuration")]
        public string stageName;
        public int minAge = 7;
        public int maxAge = 12;
        
        [Header("Audio")]
        public AudioClip musicTrack;
        [Range(0f, 1f)] public float volume = 0.7f;
        
        [Header("Mobile Optimization")]
        public bool preloadOnStart = true;
    }

    public class BiographicalAudioManager : MonoBehaviour
    {
        [Header("Life Stage Music")]
        public LifeStageAudio[] lifeStages = new LifeStageAudio[]
        {
            new LifeStageAudio { stageName = "Childhood", minAge = 7, maxAge = 12 },
            new LifeStageAudio { stageName = "Adolescence", minAge = 13, maxAge = 17 },
            new LifeStageAudio { stageName = "Adult", minAge = 18, maxAge = 45 },
            new LifeStageAudio { stageName = "Reflection", minAge = 46, maxAge = 99 }
        };
        
        [Header("Memory Stingers")]
        public AudioClip memoryRevealStinger;
        public AudioClip[] additionalStingers;
        
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource effectsSource;
        
        [Header("Crossfade Settings")]
        [Range(0.5f, 5f)] public float crossfadeDuration = 2f;
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        
        private static BiographicalAudioManager _instance;
        public static BiographicalAudioManager Instance => _instance;
        
        private LifeStageAudio currentStage;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0f;
            }
            
            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
                effectsSource.loop = false;
                effectsSource.playOnAwake = false;
            }
        }
        
        public void SwitchMusicForAge(int age)
        {
            var targetStage = GetLifeStageForAge(age);
            if (targetStage?.musicTrack != null && targetStage != currentStage)
            {
                CrossfadeToTrack(targetStage);
                currentStage = targetStage;
            }
        }
        
        public void PlayMemoryStinger()
        {
            if (memoryRevealStinger != null)
            {
                effectsSource.PlayOneShot(memoryRevealStinger, masterVolume);
            }
        }
        
        private LifeStageAudio GetLifeStageForAge(int age)
        {
            foreach (var stage in lifeStages)
            {
                if (age >= stage.minAge && age <= stage.maxAge)
                    return stage;
            }
            return lifeStages.Length > 0 ? lifeStages[0] : null; // Default to first
        }
        
        private void CrossfadeToTrack(LifeStageAudio newStage)
        {
            float half = Mathf.Max(0.01f, crossfadeDuration * 0.5f);
            // Cancel any existing tweens on this source
            DOTween.Kill(musicSource);

            if (musicSource.isPlaying)
            {
                // Fade out current, then fade in new
                DOTween.To(() => musicSource.volume, v => musicSource.volume = v, 0f, half)
                    .SetEase(Ease.Linear)
                    .SetTarget(musicSource)
                    .OnComplete(() => { PlayNewTrack(newStage); });
            }
            else
            {
                // No current track, just start new one
                PlayNewTrack(newStage);
            }
        }
        
        private void PlayNewTrack(LifeStageAudio stage)
        {
            float half = Mathf.Max(0.01f, crossfadeDuration * 0.5f);
            musicSource.clip = stage.musicTrack;
            musicSource.Play();
            DOTween.Kill(musicSource);
            DOTween.To(() => musicSource.volume, v => musicSource.volume = v, stage.volume * masterVolume, half)
                .SetEase(Ease.Linear)
                .SetTarget(musicSource);
        }
        
        // Public methods for external control
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (musicSource.isPlaying && currentStage != null)
            {
                musicSource.volume = currentStage.volume * masterVolume;
            }
        }
        
        public void StopMusic()
        {
            if (musicSource.isPlaying)
            {
                float half = Mathf.Max(0.01f, crossfadeDuration * 0.5f);
                DOTween.Kill(musicSource);
                DOTween.To(() => musicSource.volume, v => musicSource.volume = v, 0f, half)
                    .SetEase(Ease.Linear)
                    .SetTarget(musicSource)
                    .OnComplete(() => { musicSource.Stop(); });
            }
        }
    }
}
