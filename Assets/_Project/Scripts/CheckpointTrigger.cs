using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (!pc) return;
        pc.SetRespawnPoint(transform.position);
        Debug.Log("Checkpoint reached at " + transform.position);
    }
}