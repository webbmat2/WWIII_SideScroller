using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/Keycard")]
public class Keycard : MonoBehaviour
{
    [Header("Keycard Settings")]
    [SerializeField] private string keycardID = "Keycard";
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer cardRenderer;
    [SerializeField] private float rotationSpeed = 90f;

    private bool _hasBeenCollected = false;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (cardRenderer == null)
        {
            cardRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (_hasBeenCollected) return;

        // Rotate the keycard for visual appeal
        if (cardRenderer != null)
        {
            cardRenderer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasBeenCollected) return;

        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        CollectKeycard(player);
    }

    private void CollectKeycard(PlayerController2D player)
    {
        _hasBeenCollected = true;

        // Add keycard to player's inventory
        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = player.gameObject.AddComponent<PlayerInventory>();
        }

        inventory.AddItem(keycardID);

        // Audio feedback
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Visual effect
        if (pickupEffect != null)
        {
            var effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        Debug.Log($"Keycard collected: {keycardID}");

        // Hide/destroy the keycard
        if (cardRenderer != null)
        {
            cardRenderer.enabled = false;
        }

        Destroy(gameObject, 0.1f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);
        Gizmos.DrawIcon(transform.position, "Key", true);
    }
#endif
}