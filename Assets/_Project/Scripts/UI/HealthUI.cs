using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Advanced Health UI")]
public class AdvancedHealthUI : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Animation")]
    [SerializeField] private float damageAnimationDuration = 0.5f;
    [SerializeField] private float healAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve damageShakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private List<Image> _heartImages = new List<Image>();
    private PlayerHealth _playerHealth;
    private int _lastKnownHealth;

    private void Start()
    {
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (_playerHealth == null)
        {
            Debug.LogWarning("AdvancedHealthUI: No PlayerHealth found in scene!");
            return;
        }

        _playerHealth.OnHealthChanged += OnHealthChanged;
        InitializeHearts();
        UpdateHealthDisplay();
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int newHealth, int maxHealth)
    {
        bool tookDamage = newHealth < _lastKnownHealth;
        UpdateHealthDisplay();
        
        if (tookDamage)
        {
            PlayDamageAnimation();
        }
        else if (newHealth > _lastKnownHealth)
        {
            PlayHealAnimation();
        }
        
        _lastKnownHealth = newHealth;
    }

    private void InitializeHearts()
    {
        if (_playerHealth == null) return;

        // Clear existing hearts
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        _heartImages.Clear();

        // Create heart images for max health
        for (int i = 0; i < _playerHealth.MaxHP; i++)
        {
            GameObject heartObj;
            
            if (heartPrefab != null)
            {
                heartObj = Instantiate(heartPrefab, heartsContainer);
            }
            else
            {
                // Create simple heart if no prefab
                heartObj = new GameObject($"Heart_{i}");
                heartObj.transform.SetParent(heartsContainer);
                heartObj.AddComponent<Image>();
            }

            var heartImage = heartObj.GetComponent<Image>();
            if (heartImage != null)
            {
                heartImage.sprite = fullHeartSprite;
                _heartImages.Add(heartImage);
            }
        }

        _lastKnownHealth = _playerHealth.MaxHP;
    }

    private void UpdateHealthDisplay()
    {
        if (_playerHealth == null) return;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (i < _playerHealth.CurrentHP)
            {
                _heartImages[i].sprite = fullHeartSprite;
                _heartImages[i].color = Color.white;
            }
            else
            {
                _heartImages[i].sprite = emptyHeartSprite;
                _heartImages[i].color = Color.gray;
            }
        }
    }

    private void PlayDamageAnimation()
    {
        StartCoroutine(DamageAnimationCoroutine());
    }

    private void PlayHealAnimation()
    {
        StartCoroutine(HealAnimationCoroutine());
    }

    private System.Collections.IEnumerator DamageAnimationCoroutine()
    {
        float elapsed = 0f;
        Vector3 originalPosition = heartsContainer.localPosition;

        while (elapsed < damageAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageAnimationDuration;
            
            // Shake effect
            float shakeIntensity = damageShakeCurve.Evaluate(t) * 10f;
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0
            );
            
            heartsContainer.localPosition = originalPosition + randomOffset;
            
            // Red flash
            Color flashColor = Color.Lerp(Color.white, Color.red, damageShakeCurve.Evaluate(t));
            foreach (var heart in _heartImages)
            {
                if (heart.sprite == fullHeartSprite)
                {
                    heart.color = flashColor;
                }
            }
            
            yield return null;
        }

        heartsContainer.localPosition = originalPosition;
        
        // Reset colors
        foreach (var heart in _heartImages)
        {
            heart.color = Color.white;
        }
    }

    private System.Collections.IEnumerator HealAnimationCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < healAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / healAnimationDuration;
            
            // Green flash
            Color healColor = Color.Lerp(Color.white, Color.green, Mathf.Sin(t * Mathf.PI));
            foreach (var heart in _heartImages)
            {
                if (heart.sprite == fullHeartSprite)
                {
                    heart.color = healColor;
                }
            }
            
            yield return null;
        }

        // Reset colors
        foreach (var heart in _heartImages)
        {
            heart.color = Color.white;
        }
    }

    public void RefreshDisplay()
    {
        if (_playerHealth != null)
        {
            InitializeHearts();
            UpdateHealthDisplay();
        }
    }
}