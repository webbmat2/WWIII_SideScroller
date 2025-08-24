using UnityEngine;

[AddComponentMenu("Audio/Audio FX Manager")]
public class AudioFXManager : MonoBehaviour
{
    [Header("Player Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip footstepSound;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip levelCompleteSound;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuSelectSound;

    [Header("Audio Settings")]
    [SerializeField] private float sfxVolume = 0.7f;
    [SerializeField] private AudioSource audioSource;

    private static AudioFXManager _instance;
    public static AudioFXManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.volume = sfxVolume;
        audioSource.playOnAwake = false;
    }

    public static void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (Instance == null || clip == null) return;

        Instance.audioSource.PlayOneShot(clip, volume * Instance.sfxVolume);
    }

    public static void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (Instance == null || clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * Instance.sfxVolume);
    }

    // Specific sound methods for easy access
    public static void PlayJumpSound() => PlaySound(Instance?.jumpSound);
    public static void PlayLandSound() => PlaySound(Instance?.landSound);
    public static void PlayDamageSound() => PlaySound(Instance?.damageSound);
    public static void PlayFootstepSound() => PlaySound(Instance?.footstepSound, 0.3f);
    public static void PlayCoinSound() => PlaySound(Instance?.coinCollectSound);
    public static void PlayCheckpointSound() => PlaySound(Instance?.checkpointSound);
    public static void PlayLevelCompleteSound() => PlaySound(Instance?.levelCompleteSound);
    public static void PlayButtonClickSound() => PlaySound(Instance?.buttonClickSound);
    public static void PlayMenuSelectSound() => PlaySound(Instance?.menuSelectSound);

    public static void SetSFXVolume(float volume)
    {
        if (Instance != null)
        {
            Instance.sfxVolume = Mathf.Clamp01(volume);
            Instance.audioSource.volume = Instance.sfxVolume;
        }
    }
}