using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace WWIII.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParameter = "MasterVolume";
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SFXVolume";
        
        [Header("Music Settings")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip[] backgroundMusic;
        [SerializeField] private bool loopMusic = true;
        [SerializeField] private float musicFadeTime = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        
        [Header("SFX Pool")]
        [SerializeField] private AudioSource sfxSourcePrefab;
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private float sfxVolume = 1f;
        
        [Header("UI Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip menuOpenSound;
        [SerializeField] private AudioClip menuCloseSound;
        [SerializeField] private AudioClip notificationSound;
        
        [Header("Gameplay Audio")]
        [SerializeField] private AudioClip playerJumpSound;
        [SerializeField] private AudioClip playerLandSound;
        [SerializeField] private AudioClip coinCollectSound;
        [SerializeField] private AudioClip checkpointSound;
        [SerializeField] private AudioClip hazardHitSound;
        [SerializeField] private AudioClip enemyDeathSound;
        
        public static AudioManager Instance { get; private set; }
        
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentSFXIndex = 0;
        private AudioClip currentMusicClip;
        private bool isMusicPlaying = false;
        private Coroutine musicFadeCoroutine;
        
        // Audio events
        public System.Action<string, float> OnVolumeChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            LoadAudioSettings();
        }
        
        private void InitializeAudioManager()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
            }
            
            // Configure music source
            musicSource.loop = loopMusic;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;
            
            // Set audio mixer group if available
            if (audioMixer != null)
            {
                var musicGroup = audioMixer.FindMatchingGroups("Music");
                if (musicGroup != null && musicGroup.Length > 0)
                {
                    musicSource.outputAudioMixerGroup = musicGroup[0];
                }
            }
            
            // Create SFX pool
            CreateSFXPool();
            
            Debug.Log("AudioManager initialized");
        }
        
        private void CreateSFXPool()
        {
            GameObject sfxParent = new GameObject("SFXPool");
            sfxParent.transform.SetParent(transform);
            
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource sfxSource;
                
                if (sfxSourcePrefab != null)
                {
                    sfxSource = Instantiate(sfxSourcePrefab, sfxParent.transform);
                }
                else
                {
                    GameObject sfxObj = new GameObject($"SFX_{i}");
                    sfxObj.transform.SetParent(sfxParent.transform);
                    sfxSource = sfxObj.AddComponent<AudioSource>();
                }
                
                // Configure SFX source
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.volume = sfxVolume;
                
                // Set audio mixer group if available
                if (audioMixer != null)
                {
                    var sfxGroup = audioMixer.FindMatchingGroups("SFX");
                    if (sfxGroup != null && sfxGroup.Length > 0)
                    {
                        sfxSource.outputAudioMixerGroup = sfxGroup[0];
                    }
                }
                
                sfxPool.Add(sfxSource);
            }
        }
        
        // Music Management
        public void PlayMusic(AudioClip musicClip, bool fadeIn = true)
        {
            if (musicClip == null) return;
            
            if (currentMusicClip == musicClip && isMusicPlaying) return;
            
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            
            if (fadeIn && isMusicPlaying)
            {
                musicFadeCoroutine = StartCoroutine(CrossfadeMusic(musicClip));
            }
            else
            {
                PlayMusicDirect(musicClip);
            }
        }
        
        public void PlayMusic(int musicIndex, bool fadeIn = true)
        {
            if (backgroundMusic == null || musicIndex < 0 || musicIndex >= backgroundMusic.Length)
            {
                Debug.LogWarning($"Invalid music index: {musicIndex}");
                return;
            }
            
            PlayMusic(backgroundMusic[musicIndex], fadeIn);
        }
        
        public void PlayMusic(string musicName, bool fadeIn = true)
        {
            if (backgroundMusic == null) return;
            
            AudioClip clip = System.Array.Find(backgroundMusic, m => m.name == musicName);
            if (clip != null)
            {
                PlayMusic(clip, fadeIn);
            }
            else
            {
                Debug.LogWarning($"Music clip not found: {musicName}");
            }
        }
        
        private void PlayMusicDirect(AudioClip musicClip)
        {
            musicSource.Stop();
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
            
            currentMusicClip = musicClip;
            isMusicPlaying = true;
            
            Debug.Log($"Playing music: {musicClip.name}");
        }
        
        private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            float startVolume = musicSource.volume;
            
            // Fade out current music
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            // Switch to new music
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.Play();
            currentMusicClip = newClip;
            
            // Fade in new music
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += musicVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            musicSource.volume = musicVolume;
            musicFadeCoroutine = null;
            
            Debug.Log($"Crossfaded to music: {newClip.name}");
        }
        
        public void StopMusic(bool fadeOut = true)
        {
            if (!isMusicPlaying) return;
            
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            
            if (fadeOut)
            {
                musicFadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
                isMusicPlaying = false;
                currentMusicClip = null;
            }
        }
        
        private System.Collections.IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / musicFadeTime;
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = musicVolume; // Reset for next track
            isMusicPlaying = false;
            currentMusicClip = null;
            musicFadeCoroutine = null;
        }
        
        public void PauseMusic()
        {
            if (isMusicPlaying && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
        
        public void ResumeMusic()
        {
            if (isMusicPlaying && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }
        
        // SFX Management
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.clip = clip;
                source.volume = volume * sfxVolume;
                source.pitch = pitch;
                source.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.transform.position = position;
                source.clip = clip;
                source.volume = volume * sfxVolume;
                source.pitch = pitch;
                source.spatialBlend = 1f; // 3D sound
                source.Play();
            }
        }
        
        private AudioSource GetAvailableSFXSource()
        {
            // Find an available source
            for (int i = 0; i < sfxPool.Count; i++)
            {
                int index = (currentSFXIndex + i) % sfxPool.Count;
                if (!sfxPool[index].isPlaying)
                {
                    currentSFXIndex = (index + 1) % sfxPool.Count;
                    return sfxPool[index];
                }
            }
            
            // If all sources are busy, use the next one anyway (oldest sound gets cut off)
            AudioSource source = sfxPool[currentSFXIndex];
            currentSFXIndex = (currentSFXIndex + 1) % sfxPool.Count;
            return source;
        }
        
        // Convenience methods for common sounds
        public void PlayButtonClick() => PlaySFX(buttonClickSound);
        public void PlayButtonHover() => PlaySFX(buttonHoverSound);
        public void PlayMenuOpen() => PlaySFX(menuOpenSound);
        public void PlayMenuClose() => PlaySFX(menuCloseSound);
        public void PlayNotification() => PlaySFX(notificationSound);
        
        public void PlayPlayerJump() => PlaySFX(playerJumpSound);
        public void PlayPlayerLand() => PlaySFX(playerLandSound);
        public void PlayCoinCollect() => PlaySFX(coinCollectSound);
        public void PlayCheckpoint() => PlaySFX(checkpointSound);
        public void PlayHazardHit() => PlaySFX(hazardHitSound);
        public void PlayEnemyDeath() => PlaySFX(enemyDeathSound);
        
        // Volume Control
        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(masterVolumeParameter, dbValue);
            }
            else
            {
                AudioListener.volume = volume;
            }
            
            PlayerPrefs.SetFloat("MasterVolume", volume);
            OnVolumeChanged?.Invoke("Master", volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            musicVolume = volume;
            
            if (audioMixer != null)
            {
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(musicVolumeParameter, dbValue);
            }
            else
            {
                musicSource.volume = volume;
            }
            
            PlayerPrefs.SetFloat("MusicVolume", volume);
            OnVolumeChanged?.Invoke("Music", volume);
        }
        
        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            sfxVolume = volume;
            
            if (audioMixer != null)
            {
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(sfxVolumeParameter, dbValue);
            }
            
            PlayerPrefs.SetFloat("SFXVolume", volume);
            OnVolumeChanged?.Invoke("SFX", volume);
        }
        
        // Settings Management
        private void LoadAudioSettings()
        {
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            SetMasterVolume(masterVol);
            SetMusicVolume(musicVol);
            SetSFXVolume(sfxVol);
        }
        
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", GetMasterVolume());
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }
        
        // Getters
        public float GetMasterVolume()
        {
            if (audioMixer != null && audioMixer.GetFloat(masterVolumeParameter, out float value))
            {
                return Mathf.Pow(10, value / 20);
            }
            return AudioListener.volume;
        }
        
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        
        public bool IsMusicPlaying() => isMusicPlaying && musicSource.isPlaying;
        public AudioClip GetCurrentMusic() => currentMusicClip;
        
        // Mute functionality
        public void MuteAll(bool mute)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParameter, mute ? -80f : Mathf.Log10(GetMasterVolume()) * 20);
            }
            else
            {
                AudioListener.volume = mute ? 0f : PlayerPrefs.GetFloat("MasterVolume", 1f);
            }
        }
        
        public void MuteMusic(bool mute)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParameter, mute ? -80f : Mathf.Log10(musicVolume) * 20);
            }
            else
            {
                musicSource.volume = mute ? 0f : musicVolume;
            }
        }
        
        public void MuteSFX(bool mute)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(sfxVolumeParameter, mute ? -80f : Mathf.Log10(sfxVolume) * 20);
            }
            
            // For pooled sources, we'll handle this in PlaySFX
        }
        
        // Cleanup
        private void OnDestroy()
        {
            SaveAudioSettings();
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseMusic();
            }
            else
            {
                ResumeMusic();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                PauseMusic();
            }
            else
            {
                ResumeMusic();
            }
        }
    }
}