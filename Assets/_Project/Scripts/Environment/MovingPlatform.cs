using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public sealed class MovingPlatform : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;              // meters per second
    [SerializeField] private bool pingPong = true;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;

    private int _index;
    private int _dir = 1;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Vector2 current = rb.position;
        Vector2 target = waypoints[_index].position;
        float step = speed * Time.fixedDeltaTime;

        Vector2 next = Vector2.MoveTowards(current, target, step);
        rb.MovePosition(next);

        if ((target - next).sqrMagnitude <= 0.0001f)
        {
            if (pingPong)
            {
                if (_index == waypoints.Length - 1) _dir = -1;
                else if (_index == 0) _dir = 1;
                _index += _dir;
            }
            else
            {
                _index = (_index + 1) % waypoints.Length;
            }
        }
    }

    // Top-edge trigger should be on the same GameObject as this script.
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;
        player.AttachToPlatform(transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;
        player.DetachFromPlatform(transform);
    }

    private void OnDisable()
    {
        // Safety: detach any player left parented if platform is disabled/destroyed
        var players = GetComponentsInChildren<PlayerController2D>(true);
        foreach (var p in players) p.DetachFromPlatform(transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
#endif
}