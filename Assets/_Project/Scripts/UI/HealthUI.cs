using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Health UI")]
public class HealthUI : MonoBehaviour
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
    private PlayerController2D _player;
    private int _lastKnownHealth;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController2D>();
        if (_player == null)
        {
            Debug.LogWarning("HealthUI: No PlayerController2D found in scene!");
            return;
        }

        InitializeHearts();
        UpdateHealthDisplay();
    }

    private void Update()
    {
        if (_player == null) return;

        // Check for health changes
        if (_player.CurrentHealth != _lastKnownHealth)
        {
            bool tookDamage = _player.CurrentHealth < _lastKnownHealth;
            UpdateHealthDisplay();
            
            if (tookDamage)
            {
                PlayDamageAnimation();
            }
            else
            {
                PlayHealAnimation();
            }
            
            _lastKnownHealth = _player.CurrentHealth;
        }
    }

    private void InitializeHearts()
    {
        if (_player == null) return;

        // Clear existing hearts
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        _heartImages.Clear();

        // Create heart images for max health
        for (int i = 0; i < _player.MaxHealth; i++)
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

        _lastKnownHealth = _player.MaxHealth;
    }

    private void UpdateHealthDisplay()
    {
        if (_player == null) return;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (i < _player.CurrentHealth)
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
        if (_player != null)
        {
            InitializeHearts();
            UpdateHealthDisplay();
        }
    }
}