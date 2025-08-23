// Assets/_Project/Scripts/Environment/MovingPlatform.cs
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class MovingPlatform : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] Transform[] points;
    [SerializeField, Min(0.01f)] float speed = 3f;
    [SerializeField, Min(0f)] float waitAtPoint = 0.1f;
    [SerializeField] bool pingPong = true;

    [Header("Carry via Top Sensor (recommended)")]
    [SerializeField] bool useTopTrigger = true;
    [Tooltip("Assign a thin BoxCollider2D on a child named 'TopSensor' set IsTrigger=ON, hovering ~0.06 above deck.")]
    [SerializeField] Collider2D topTrigger;
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform carryRoot; // optional; defaults to this.transform

    Rigidbody2D rb; Collider2D col;
    int idx; int dir = 1; float waitTimer;
    Vector2 lastPos;

    readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();
    readonly Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

    public Vector2 GetDeltaPosition() => rb ? (Vector2)rb.position - lastPos : Vector2.zero;

    void OnValidate()
    {
        if (topTrigger) topTrigger.isTrigger = true;
    }

    void Reset()
    {
        var r = GetComponent<Rigidbody2D>();
        r.bodyType = RigidbodyType2D.Kinematic;
        r.useFullKinematicContacts = true;               // callbacks for kinematic pairs
        r.interpolation = RigidbodyInterpolation2D.Interpolate;
        r.constraints = RigidbodyConstraints2D.FreezeRotation;

        col = GetComponent<Collider2D>();
        if (!col) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.usedByEffector = false;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (!carryRoot) carryRoot = transform;
        lastPos = rb.position;
    }

    void OnEnable()
    {
        lastPos = rb.position;
        if (points != null && points.Length > 0) idx = NearestIndex();
    }

    int NearestIndex()
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

    void FixedUpdate()
    {
        if (points == null || points.Length < 2) { lastPos = rb.position; return; }

        if (waitTimer > 0f)
        { waitTimer -= Time.fixedDeltaTime; lastPos = rb.position; return; }

        Vector2 target = points[idx].position;
        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if ((rb.position - target).sqrMagnitude <= 0.0001f)
        {
            waitTimer = waitAtPoint;
            if (pingPong) { if (idx == points.Length - 1) dir = -1; else if (idx == 0) dir = 1; idx += dir; }
            else { idx = (idx + 1) % points.Length; }
        }

        lastPos = rb.position;
    }

    // Trigger-based parenting (robust)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTopTrigger) return;
        if (topTrigger == null) return;                       // require explicit sensor
        if (other == topTrigger) return;                      // ignore self
        if (playerTag.Length > 0 && !other.CompareTag(playerTag)) return;

        var rb2d = other.attachedRigidbody; if (!rb2d) return;
        if (riders.Add(rb2d))
        {
            var t = rb2d.transform;
            if (!originalParents.ContainsKey(t)) originalParents[t] = t.parent;
            t.SetParent(carryRoot, true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!useTopTrigger) return;
        if (topTrigger == null) return;
        if (playerTag.Length > 0 && !other.CompareTag(playerTag)) return;

        var rb2d = other.attachedRigidbody; if (rb2d) Unparent(rb2d);
    }

    void OnDisable()
    {
        // Clean-up riders so we never strand a childed player
        foreach (var rb2d in riders)
        {
            if (!rb2d) continue;
            var t = rb2d.transform;
            if (originalParents.TryGetValue(t, out var p)) t.SetParent(p, true);
            else t.SetParent(null, true);
        }
        riders.Clear(); originalParents.Clear();
    }

    void Unparent(Rigidbody2D other)
    {
        if (!riders.Remove(other)) return;
        var t = other.transform;
        if (originalParents.TryGetValue(t, out var p)) t.SetParent(p, true);
        else t.SetParent(null, true);
        originalParents.Remove(t);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
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