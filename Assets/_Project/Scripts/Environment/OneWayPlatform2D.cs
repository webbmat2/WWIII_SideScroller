*** Begin Patch
*** Add File: Assets/_Project/Scripts/Environment/OneWayPlatform2D.cs
+using UnityEngine;
+
+[RequireComponent(typeof(PlatformEffector2D))]
+[RequireComponent(typeof(Collider2D))]
+[DisallowMultipleComponent]
+public sealed class OneWayPlatform2D : MonoBehaviour
+{
+    [SerializeField] float dropDuration = 0.3f;
+    PlatformEffector2D eff;
+    float dropUntil = -1f;
+    bool playerOnTop;
+
+    void Reset()
+    {
+        var col = GetComponent<Collider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
+        col.usedByEffector = true;
+        eff = GetComponent<PlatformEffector2D>();
+        eff.useOneWay = true;
+        eff.surfaceArc = 160f;
+    }
+
+    void Awake() { eff = GetComponent<PlatformEffector2D>(); }
+
+    void Update()
+    {
+        if (!playerOnTop) return;
+        bool down = Input.GetAxisRaw("Vertical") < -0.5f || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
+        bool jump = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
+        if (down && jump) dropUntil = Time.time + dropDuration;
+        eff.rotationalOffset = Time.time < dropUntil ? 180f : 0f;
+    }
+
+    void OnCollisionStay2D(Collision2D c)
+    {
+        if (!c.collider.CompareTag("Player")) return;
+        // Rough check the player is above the platform
+        foreach (var cn in c.contacts)
+        {
+            if (cn.normal.y > 0.5f) { playerOnTop = true; return; }
+        }
+    }
+    void OnCollisionExit2D(Collision2D c)
+    {
+        if (c.collider.CompareTag("Player")) playerOnTop = false;
+    }
+}
+
*** End Patch
