using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Checkpoint")]
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private GameObject activationEffect;
    [SerializeField] private SpriteRenderer flagRenderer;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    private static Checkpoint _currentCheckpoint;

    public static Vector3 CurrentRespawnPoint => _currentCheckpoint != null ? _currentCheckpoint.transform.position : Vector3.zero;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        
        // Auto-setup visual feedback
        flagRenderer = GetComponent<SpriteRenderer>();
        if (flagRenderer != null)
        {
            flagRenderer.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null || isActivated) return;

        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        // Deactivate previous checkpoint
        if (_currentCheckpoint != null && _currentCheckpoint != this)
        {
            _currentCheckpoint.DeactivateVisual();
        }

        _currentCheckpoint = this;
        isActivated = true;

        // Visual feedback
        if (flagRenderer != null)
        {
            flagRenderer.color = activeColor;
        }

        // Audio feedback
        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }

        // VFX
        if (activationEffect != null)
        {
            var effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        Debug.Log($"Checkpoint activated: {gameObject.name}");
    }

    private void DeactivateVisual()
    {
        if (flagRenderer != null)
        {
            flagRenderer.color = inactiveColor;
        }
    }

    public static void RespawnPlayer()
    {
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player != null && _currentCheckpoint != null)
        {
            player.transform.position = _currentCheckpoint.transform.position;
            Debug.Log($"Player respawned at: {_currentCheckpoint.gameObject.name}");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? activeColor : inactiveColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
#endif
}