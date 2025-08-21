using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] int value = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CollectibleManager.Add(value);
        Destroy(gameObject);
    }
}