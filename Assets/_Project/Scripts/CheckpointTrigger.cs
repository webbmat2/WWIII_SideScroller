using UnityEngine;

[AddComponentMenu("Gameplay/Checkpoint Trigger")]
[RequireComponent(typeof(Collider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool alignYOnly = false;
    [SerializeField] private bool activated = false;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private GameObject activationEffect;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer flagRenderer;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    private bool consumed;
    private static CheckpointTrigger lastCheckpoint;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed && oneShot) return;
        
        // Try PlayerHealth first (preferred), fallback to PlayerController2D
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        var playerController = other.GetComponentInParent<PlayerController2D>();
        
        if (playerHealth == null && playerController == null) return;
        
        Vector2 point = transform.position;
        if (alignYOnly)
        {
            Vector2 playerPos = playerHealth != null ? playerHealth.transform.position : playerController.transform.position;
            point = new Vector2(playerPos.x, point.y);
        }
        
        // Set respawn point on preferred component
        if (playerHealth != null)
        {
            ActivateCheckpoint(playerHealth, point);
        }
        else if (playerController != null)
        {
            playerController.SetRespawnPoint(point);
            ActivateVisualFeedback();
        }
        
        consumed = true;
    }

    private void ActivateCheckpoint(PlayerHealth playerHealth, Vector2 point)
    {
        // Deactivate previous checkpoint
        if (lastCheckpoint != null && lastCheckpoint != this)
        {
            lastCheckpoint.Deactivate();
        }

        // Activate this checkpoint
        activated = true;
        lastCheckpoint = this;

        // Set respawn point
        playerHealth.SetRespawnPoint(point);
        
        ActivateVisualFeedback();
        
        Debug.Log($"Checkpoint activated: {gameObject.name}");
    }

    private void ActivateVisualFeedback()
    {
        // Visual feedback
        UpdateVisual();

        // Audio feedback
        if (checkpointSound != null)
        {
            AudioSource.PlayClipAtPoint(checkpointSound, transform.position);
        }

        // VFX
        if (activationEffect != null)
        {
            var effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }

    private void Deactivate()
    {
        activated = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (flagRenderer != null)
        {
            flagRenderer.color = activated ? activeColor : inactiveColor;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = activated ? activeColor : inactiveColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
#endif
}
