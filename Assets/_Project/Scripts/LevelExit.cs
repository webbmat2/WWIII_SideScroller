using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Level Exit")]
public class LevelExit : MonoBehaviour
{
    [Header("Exit Settings")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool requireAllCoins = false;
    [SerializeField] private int minimumCoinsRequired = 0;
    [SerializeField] private float exitDelay = 1f;

    [Header("Feedback")]
    [SerializeField] private AudioClip exitSound;
    [SerializeField] private GameObject exitEffect;
    [SerializeField] private string exitMessage = "Level Complete!";

    private bool _exitTriggered = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null || _exitTriggered) return;

        if (CanExit())
        {
            TriggerExit();
        }
        else
        {
            ShowRequirementMessage();
        }
    }

    private bool CanExit()
    {
        if (requireAllCoins)
        {
            // Check if player collected all coins in level
            var totalCoinsInLevel = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            return CoinManager.Count >= totalCoinsInLevel;
        }
        
        return CoinManager.Count >= minimumCoinsRequired;
    }

    private void TriggerExit()
    {
        _exitTriggered = true;

        // Audio feedback
        if (exitSound != null)
        {
            AudioSource.PlayClipAtPoint(exitSound, transform.position);
        }

        // VFX
        if (exitEffect != null)
        {
            var effect = Instantiate(exitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        // Show completion message
        Debug.Log($"{exitMessage} Coins: {CoinManager.Count}");

        // Load next scene after delay
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Invoke(nameof(LoadNextScene), exitDelay);
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    private void ShowRequirementMessage()
    {
        int needed = requireAllCoins ? 
            FindObjectsByType<Coin>(FindObjectsSortMode.None).Length - CoinManager.Count :
            minimumCoinsRequired - CoinManager.Count;
            
        Debug.Log($"Need {needed} more coins to exit!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        Gizmos.DrawIcon(transform.position, "Goal", true);
    }
#endif
}