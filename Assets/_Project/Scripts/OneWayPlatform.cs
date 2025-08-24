using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Gameplay/One Way Platform")]
public class OneWayPlatform : MonoBehaviour
{
    [Header("One-Way Settings")]
    [SerializeField] private float passThroughBuffer = 0.1f;
    [SerializeField] private LayerMask playerLayer = -1;

    private Collider2D _platformCollider;
    private ContactFilter2D _contactFilter;

    private void Awake()
    {
        _platformCollider = GetComponent<Collider2D>();
        
        // Setup contact filter for player layer
        _contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = playerLayer,
            useTriggers = false
        };
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
            col.usedByEffector = true;
        }

        // Try to add PlatformEffector2D automatically
        var effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
        {
            effector = gameObject.AddComponent<PlatformEffector2D>();
        }
        
        effector.useOneWay = true;
        effector.surfaceArc = 180f;
        effector.sideArc = 1f;
    }

    private void FixedUpdate()
    {
        CheckPlayerPassThrough();
    }

    private void CheckPlayerPassThrough()
    {
        var results = new Collider2D[10];
        int hitCount = Physics2D.OverlapCollider(_platformCollider, _contactFilter, results);

        for (int i = 0; i < hitCount; i++)
        {
            var playerCollider = results[i];
            var player = playerCollider.GetComponentInParent<PlayerController2D>();
            
            if (player != null)
            {
                // Check if player is moving downward and pressing down
                var rb = player.GetComponent<Rigidbody2D>();
                bool pressingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                bool movingDown = rb != null && rb.linearVelocity.y < -0.1f;

                if (pressingDown && movingDown)
                {
                    // Allow pass-through by temporarily ignoring collision
                    Physics2D.IgnoreCollision(_platformCollider, playerCollider, true);
                    StartCoroutine(ReenableCollisionAfterDelay(playerCollider, 0.5f));
                }
            }
        }
    }

    private System.Collections.IEnumerator ReenableCollisionAfterDelay(Collider2D playerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (playerCollider != null && _platformCollider != null)
        {
            Physics2D.IgnoreCollision(_platformCollider, playerCollider, false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        var bounds = GetComponent<Collider2D>()?.bounds ?? new Bounds(transform.position, Vector3.one);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // Draw one-way indicator
        Vector3 center = bounds.center;
        Vector3 topLeft = center + Vector3.left * bounds.size.x * 0.4f + Vector3.up * bounds.size.y * 0.3f;
        Vector3 topRight = center + Vector3.right * bounds.size.x * 0.4f + Vector3.up * bounds.size.y * 0.3f;
        Vector3 bottomCenter = center + Vector3.down * bounds.size.y * 0.3f;
        
        Gizmos.DrawLine(topLeft, bottomCenter);
        Gizmos.DrawLine(topRight, bottomCenter);
    }
#endif
}