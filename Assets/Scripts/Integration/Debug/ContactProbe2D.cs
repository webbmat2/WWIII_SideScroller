using UnityEngine;

namespace WWIII.SideScroller.Integration.Debugging
{
    /// <summary>
    /// Attach to Player to log actual 2D contacts and a downward ray probe from feet.
    /// Helps verify whether the physics world reports the ground.
    /// </summary>
    public class ContactProbe2D : MonoBehaviour
    {
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float probeDistance = 1.5f;
        [SerializeField] private bool logEveryFixedUpdate = false;

        private Rigidbody2D rb;
        private Collider2D col;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (groundMask == 0)
            {
                int ground = LayerMask.NameToLayer("Ground");
                int def = LayerMask.NameToLayer("Default");
                groundMask = 0;
                if (ground >= 0) groundMask |= 1 << ground;
                if (def >= 0) groundMask  |= 1 << def;
            }
        }

        private void FixedUpdate()
        {
            if (!col) return;
            var b = col.bounds;
            var origin = new Vector2(b.center.x, b.min.y + 0.05f);
            var hit = Physics2D.Raycast(origin, Vector2.down, probeDistance, groundMask);
            if (logEveryFixedUpdate)
            {
                Debug.Log($"[ContactProbe2D] Ray from {origin} dist={probeDistance} hit={(hit.collider ? hit.collider.name : "<none>")}");
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log($"[ContactProbe2D] OnCollisionEnter2D with {collision.collider.name} (layer {collision.collider.gameObject.layer})");
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            Debug.Log($"[ContactProbe2D] OnCollisionStay2D with {collision.collider.name} (contacts {collision.contactCount})");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[ContactProbe2D] OnTriggerEnter2D with {other.name} (isTrigger={other.isTrigger})");
        }
    }
}

