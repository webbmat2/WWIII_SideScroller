using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null) return;
        pc.Respawn();
    }
}