using UnityEngine;

[AddComponentMenu("Gameplay/Checkpoint Trigger")]
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<CheckpointTrigger>();
        if (player != null)
        {
            player.SetRespawnPoint(respawnPoint.position);
        }
    }
}