using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] AudioClip pickupSfx;
    bool collected;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;                            // stop any second fire

        // stop further contacts this frame
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        CoinManager.Add(1);                          // +1 exactly once
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        Destroy(gameObject, 0.02f);
    }
}