using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Breakable Wall")]
public class BreakableWall : MonoBehaviour
{
    [Header("Breaking Settings")]
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private string requiredKeyName = "Keycard";
    [SerializeField] private bool breakOnTouch = true;
    [SerializeField] private bool breakOnJump = false;

    [Header("Rewards")]
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private int coinReward = 5;

    [Header("Effects")]
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private GameObject breakEffect;
    [SerializeField] private float effectDuration = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer wallRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private bool _isBroken = false;
    private Color _originalColor;

    private void Start()
    {
        if (wallRenderer != null)
        {
            _originalColor = wallRenderer.color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isBroken) return;

        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        if (breakOnTouch)
        {
            if (CanBreak(player))
            {
                BreakWall();
            }
            else
            {
                ShowCannotBreakFeedback();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_isBroken || !breakOnJump) return;

        var player = other.gameObject.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        // Check if player hit from above (jumping down onto wall)
        if (other.contacts.Length > 0)
        {
            Vector2 hitDirection = other.contacts[0].normal;
            if (hitDirection.y < -0.5f) // Hit from above
            {
                if (CanBreak(player))
                {
                    BreakWall();
                }
            }
        }
    }

    private bool CanBreak(PlayerController2D player)
    {
        if (!requiresKey) return true;

        // Check if player has required key (this would integrate with an inventory system)
        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            return inventory.HasItem(requiredKeyName);
        }

        // Fallback: check for keycard component
        var keycard = player.GetComponentInChildren<Keycard>();
        return keycard != null;
    }

    private void BreakWall()
    {
        _isBroken = true;

        // Visual effects
        if (breakEffect != null)
        {
            var effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Audio
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // Spawn rewards
        SpawnRewards();

        // Disable the wall
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (wallRenderer != null)
        {
            wallRenderer.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log($"Breakable wall destroyed: {gameObject.name}");
    }

    private void SpawnRewards()
    {
        // Spawn reward prefab
        if (rewardPrefab != null)
        {
            Instantiate(rewardPrefab, transform.position, Quaternion.identity);
        }

        // Add coins directly
        if (coinReward > 0)
        {
            CoinManager.Add(coinReward);
        }
    }

    private void ShowCannotBreakFeedback()
    {
        if (wallRenderer != null)
        {
            StartCoroutine(FlashWall());
        }
        
        Debug.Log($"Need {requiredKeyName} to break this wall!");
    }

    private System.Collections.IEnumerator FlashWall()
    {
        wallRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        wallRenderer.color = _originalColor;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = requiresKey ? Color.red : Color.orange;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        if (requiresKey)
        {
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "Lock", true);
        }
    }
#endif
}