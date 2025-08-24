using UnityEngine;

[AddComponentMenu("Gameplay/Checkpoint Trigger")]
[RequireComponent(typeof(Collider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool alignYOnly = false;
    private bool consumed;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed && oneShot) return;
        
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;
        
        Vector2 point = transform.position;
        if (alignYOnly)
            point = new Vector2(player.transform.position.x, point.y);
        
        player.SetRespawnPoint(point);
        consumed = true;
    }
}
