*** Begin Patch
*** Add File: Assets/_Project/Scripts/Environment/MovingPlatform.cs
+using UnityEngine;
+
+[RequireComponent(typeof(Rigidbody2D))]
+[DisallowMultipleComponent]
+public sealed class MovingPlatform : MonoBehaviour
+{
+    [SerializeField] Transform[] points;
+    [SerializeField] float speed = 3f;
+    [SerializeField] float waitAtPoint = 0.15f;
+    [SerializeField] bool pingPong = true;
+
+    Rigidbody2D rb;
+    int idx = 0;
+    int dir = 1;
+    float waitTimer = 0f;
+
+    void Reset()
+    {
+        var rb2d = GetComponent<Rigidbody2D>();
+        rb2d.bodyType = RigidbodyType2D.Kinematic;
+        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
+        var col = GetComponent<Collider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
+        col.usedByEffector = false;
+    }
+
+    void Awake() { rb = GetComponent<Rigidbody2D>(); }
+
+    void FixedUpdate()
+    {
+        if (points == null || points.Length < 2) return;
+        if (waitTimer > 0f) { waitTimer -= Time.fixedDeltaTime; return; }
+
+        var target = points[idx].position;
+        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
+        rb.MovePosition(next);
+
+        if ((Vector2)transform.position == (Vector2)target)
+        {
+            waitTimer = waitAtPoint;
+            if (pingPong)
+            {
+                if (idx == points.Length - 1) dir = -1;
+                else if (idx == 0) dir = 1;
+                idx += dir;
+            }
+            else
+            {
+                idx = (idx + 1) % points.Length;
+            }
+        }
+    }
+
+    void OnCollisionEnter2D(Collision2D c)
+    {
+        if (c.collider.CompareTag("Player")) c.collider.transform.SetParent(transform);
+    }
+    void OnCollisionExit2D(Collision2D c)
+    {
+        if (c.collider.CompareTag("Player")) c.collider.transform.SetParent(null);
+    }
+
+#if UNITY_EDITOR
+    void OnDrawGizmosSelected()
+    {
+        if (points == null || points.Length < 2) return;
+        Gizmos.color = Color.cyan;
+        for (int i = 0; i < points.Length - 1; i++)
+            Gizmos.DrawLine(points[i].position, points[i + 1].position);
+    }
+#endif
+}
+
*** End Patch
