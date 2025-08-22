// Assets/_Project/Scripts/Environment/MovingPlatform.cs
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class MovingPlatform : MonoBehaviour
{
    [Header("Path")] 
    [SerializeField] private Transform[] points; 
    [SerializeField, Min(0.01f)] private float speed = 3f; 
    [SerializeField, Min(0f)] private float waitAtPoint = 0.15f; 
    [SerializeField] private bool pingPong = true;

    [Header("Carry")]
    [Tooltip("Parents the rider while they are on top. Turn off to disable parenting carry.")]
    [SerializeField] private bool useParentingCarry = true;
    [Tooltip("Minimum upward normal to count as 'on top'. Lower if slide persists.")]
    [Range(0f, 1f)] [SerializeField] private float topNormalMinY = 0.02f;
    [Tooltip("Geometric fallback: if rider bottom is above platform top by this tolerance, treat as on top.")]
    [Min(0f)] [SerializeField] private float topYTolerance = 0.05f;

    private Rigidbody2D rb;
    private Collider2D col;
    private int idx = 0;
    private int dir = 1;
    private float waitTimer = 0f;
    private Vector2 lastPos;

    private readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();
    private readonly Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

    /// <summary>Delta world position moved since last FixedUpdate.</summary>
    public Vector2 GetDeltaPosition() => rb ? (Vector2)rb.position - lastPos : Vector2.zero;

    private void Reset()
    {
        var r = GetComponent<Rigidbody2D>();
        r.bodyType = RigidbodyType2D.Kinematic;
        r.useFullKinematicContacts = true;
        r.interpolation = RigidbodyInterpolation2D.Interpolate;
        r.constraints = RigidbodyConstraints2D.FreezeRotation;

        col = GetComponent<Collider2D>();
        if (!col) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.usedByEffector = false;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        lastPos = rb.position;
    }

    private void OnEnable()
    {
        lastPos = rb.position;
        if (points == null || points.Length == 0) return;
        idx = NearestIndex();
    }

    private int NearestIndex()
    {
        if (points == null || points.Length == 0) return 0;
        Vector2 p = rb.position; int best = 0; float bestD = float.PositiveInfinity;
        for (int i = 0; i < points.Length; i++)
        {
            var t = points[i]; if (!t) continue;
            float d = ((Vector2)t.position - p).sqrMagnitude;
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    private void FixedUpdate()
    {
        if (points == null || points.Length < 2) { lastPos = rb.position; return; }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            lastPos = rb.position;
            return;
        }

        Vector2 target = points[idx].position;
        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if ((rb.position - target).sqrMagnitude <= 0.0001f)
        {
            waitTimer = waitAtPoint;
            if (pingPong)
            {
                if (idx == points.Length - 1) dir = -1; else if (idx == 0) dir = 1; idx += dir;
            }
            else idx = (idx + 1) % points.Length;
        }

        lastPos = rb.position;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!useParentingCarry) return;
        if (!IsOnTop(c)) return;
        var otherRb = c.rigidbody; if (!otherRb) return;
        if (riders.Add(otherRb))
        {
            var t = otherRb.transform;
            if (!originalParents.ContainsKey(t)) originalParents[t] = t.parent;
            t.SetParent(transform, true);
        }
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (!useParentingCarry) return;
        var otherRb = c.rigidbody; if (!otherRb) return;
        if (IsOnTop(c))
        {
            if (riders.Add(otherRb))
            {
                var t = otherRb.transform;
                if (!originalParents.ContainsKey(t)) originalParents[t] = t.parent;
                t.SetParent(transform, true);
            }
        }
        else Unparent(otherRb);
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (!useParentingCarry) return;
        if (c.rigidbody) Unparent(c.rigidbody);
    }

    private void Unparent(Rigidbody2D other)
    {
        if (!riders.Remove(other)) return;
        var t = other.transform;
        if (originalParents.TryGetValue(t, out var p)) t.SetParent(p, true); else t.SetParent(null, true);
        originalParents.Remove(t);
    }

    private bool IsOnTop(Collision2D c)
    {
        // Normal-based check first
        for (int i = 0; i < c.contactCount; i++)
        {
            var n = c.GetContact(i).normal; // normal from platform to other
            if (n.y > topNormalMinY) return true;
        }
        // Geometric fallback
        if (col && c.collider)
        {
            float platTop = col.bounds.max.y - topYTolerance;
            if (c.collider.bounds.min.y >= platTop) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Length - 1; i++)
        {
            var a = points[i]; var b = points[i + 1];
            if (!a || !b) continue;
            Gizmos.DrawLine(a.position, b.position);
        }
    }
#endif
}
