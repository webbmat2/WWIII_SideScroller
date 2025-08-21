using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int value = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CoinManager.Add(value);
        Destroy(gameObject);
    }
}