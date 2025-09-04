using UnityEngine;
using UnityEngine.Tilemaps;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Ensures critical Player and Ground physics settings are valid at runtime.
    /// Attach to the Player root. Provides ContextMenu for editor fix too.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class PlayerPhysicsAutoFixer : MonoBehaviour
    {
        [Header("Player Physics")]
        [SerializeField] private float desiredGravityScale = 3f;
        [SerializeField] private bool enforceFreezeRotation = true;
        [SerializeField] private float desiredMass = 1f;
        [SerializeField] private float desiredDrag = 0f;
        [SerializeField] private float desiredAngularDrag = 0.05f;
        [SerializeField] private bool preferDiscreteCollision = false; // try Discrete if Continuous misbehaves with Composite
        [SerializeField] private bool forceEnableSpriteRenderers = true; // safeguard against invisible character

        [Header("Ground Path (optional)")]
        [SerializeField] private string groundRootName = "Tilemaps_age7";
        [SerializeField] private string groundChildName = "Ground";

        private void Awake()
        {
            AutoFixPhysics();
            // Also run a robust repair across a couple of frames for Unity 6000.2
            StartCoroutine(CoRepairConstraints());
        }

        [ContextMenu("Auto-Fix Physics Setup")]
        private void AutoFixPhysics()
        {
            EnsurePlayerLayer();
            FixPlayerRigidbody();
            ValidatePlayerCollider();
            EnsureLayerCollisions();
            TryFixGroundTilemap();
            TryAlignCorgiMasks();
            VerifyLayerCollisions();
            EnsureFrictionlessMaterials();
            if (forceEnableSpriteRenderers) EnableAllSpriteRenderers();
        }

        [ContextMenu("Fix Player Constraints Now")]
        private void FixConstraintsNow()
        {
            FixPlayerRigidbody();
        }

        private void FixPlayerRigidbody()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("[PlayerPhysicsAutoFixer] No Rigidbody2D found on Player.");
                return;
            }

            // Body type & core physical params
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = desiredGravityScale;
            rb.mass = desiredMass;
            rb.drag = desiredDrag;
            rb.angularDrag = desiredAngularDrag;
            rb.freezeRotation = false; // ensure the legacy flag doesn't conflict
            rb.collisionDetectionMode = preferDiscreteCollision ? CollisionDetectionMode2D.Discrete : CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Remove FreezeAll if present, keep only FreezeRotation
            if (enforceFreezeRotation)
            {
                var hadFreezeAll = (rb.constraints & RigidbodyConstraints2D.FreezeAll) == RigidbodyConstraints2D.FreezeAll;
                var desired = RigidbodyConstraints2D.FreezeRotation;
                if (rb.constraints != desired)
                {
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.constraints = desired;
                    if (hadFreezeAll)
                    {
                        Debug.Log("[PlayerPhysicsAutoFixer] Fixed player physics constraints - removed FreezeAll");
                    }
                    else
                    {
                        Debug.Log("[PlayerPhysicsAutoFixer] Set Rigidbody2D constraints to FreezeRotation only");
                    }
                }
            }
        }

        private System.Collections.IEnumerator CoRepairConstraints()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) yield break;

            // Only attempt the heavy repair if anything other than FreezeRotation is present
            var desired = RigidbodyConstraints2D.FreezeRotation;
            if ((rb.constraints & RigidbodyConstraints2D.FreezeAll) == RigidbodyConstraints2D.FreezeAll || rb.constraints != desired)
            {
                var originalSim = rb.simulated;
                var originalType = rb.bodyType;

                // Step 1: fully clear constraints while paused
                rb.simulated = false;
                rb.constraints = RigidbodyConstraints2D.None;

                // Step 2: force engine to rebuild internal state by toggling body type (Unity 6000.2 workaround)
                rb.bodyType = RigidbodyType2D.Kinematic;
                yield return null; // 1 frame
                yield return new WaitForFixedUpdate();

                // Step 3: restore body type and set final constraints
                rb.bodyType = originalType;
                rb.constraints = desired;
                rb.simulated = originalSim;

                Debug.Log("[PlayerPhysicsAutoFixer] Constraint repair pass completed (Unity 6000.2 workaround)");
            }

            // Optional: verify serialized constraints in Editor
            VerifySerializedConstraintsEditor();
        }

        private void ValidatePlayerCollider()
        {
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning("[PlayerPhysicsAutoFixer] No Collider2D found on Player. Adding BoxCollider2D for stable collisions.");
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.size = new Vector2(1f, 2f);
                box.edgeRadius = 0.02f;
                col = box;
            }
            if (col.isTrigger)
            {
                Debug.LogWarning("[PlayerPhysicsAutoFixer] Player's Collider2D is set as Trigger. Disable 'Is Trigger' for ground collisions.");
            }
        }

        private static PhysicsMaterial2D _frictionless;
        private void EnsureFrictionlessMaterials()
        {
            if (_frictionless == null)
            {
                _frictionless = new PhysicsMaterial2D("Frictionless2D") { friction = 0f, bounciness = 0f };
            }

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                if (col.sharedMaterial == null || col.sharedMaterial.friction > 0f || col.sharedMaterial.bounciness > 0f)
                {
                    col.sharedMaterial = _frictionless;
                    Debug.Log("[PlayerPhysicsAutoFixer] Applied frictionless material to Player collider.");
                }
            }

            // Ground composite
            var root = FindRootByName(groundRootName);
            var ground = root ? FindChildRecursive(root, groundChildName) : null;
            if (ground != null)
            {
                var cc = ground.GetComponent<CompositeCollider2D>();
                if (cc != null)
                {
                    if (cc.sharedMaterial == null || cc.sharedMaterial.friction > 0f || cc.sharedMaterial.bounciness > 0f)
                    {
                        cc.sharedMaterial = _frictionless;
                        Debug.Log("[PlayerPhysicsAutoFixer] Applied frictionless material to Ground composite.");
                    }
                }
            }
        }

        private void EnableAllSpriteRenderers()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                if (!r.enabled)
                {
                    r.enabled = true;
                }
                var c = r.color;
                if (c.a < 0.99f)
                {
                    c.a = 1f;
                    r.color = c;
                }
            }
        }

        private void EnsurePlayerLayer()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0 && gameObject.layer != playerLayer)
            {
                gameObject.layer = playerLayer;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void VerifySerializedConstraintsEditor()
        {
#if UNITY_EDITOR
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;
            var so = new UnityEditor.SerializedObject(rb);
            var prop = so.FindProperty("m_Constraints");
            if (prop != null)
            {
                Debug.Log($"[PlayerPhysicsAutoFixer] Serialized m_Constraints={prop.intValue}");
            }
#endif
        }

        private void EnsureLayerCollisions()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int groundLayer = LayerMask.NameToLayer("Ground");
            int defaultLayer = LayerMask.NameToLayer("Default");

            if (playerLayer < 0) playerLayer = 11; // Fallback to expected index
            if (groundLayer < 0) groundLayer = 3;
            if (defaultLayer < 0) defaultLayer = 0;

            Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
            Physics2D.IgnoreLayerCollision(playerLayer, defaultLayer, false);
            // Also ensure Default-Ground in case child colliders differ
            Physics2D.IgnoreLayerCollision(defaultLayer, groundLayer, false);
        }

        private void VerifyLayerCollisions()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int groundLayer = LayerMask.NameToLayer("Ground");
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (playerLayer < 0) playerLayer = 11;
            if (groundLayer < 0) groundLayer = 3;
            if (defaultLayer < 0) defaultLayer = 0;

            bool pg = !Physics2D.GetIgnoreLayerCollision(playerLayer, groundLayer);
            bool pd = !Physics2D.GetIgnoreLayerCollision(playerLayer, defaultLayer);
            bool gd = !Physics2D.GetIgnoreLayerCollision(groundLayer, defaultLayer);
            Debug.Log($"[CollisionCheck] Player-Ground: {pg}; Player-Default: {pd}; Ground-Default: {gd}");
        }

        private void TryFixGroundTilemap()
        {
            // Best-effort: locate /Tilemaps_age7/Ground
            var root = FindRootByName(groundRootName);
            if (root == null)
            {
                Debug.Log("[PlayerPhysicsAutoFixer] Ground root '" + groundRootName + "' not found (optional)");
                return;
            }
            var ground = FindChildRecursive(root, groundChildName);
            if (ground == null)
            {
                Debug.Log("[PlayerPhysicsAutoFixer] Ground child '" + groundChildName + "' not found under '" + groundRootName + "' (optional)");
                return;
            }

            // Layer assignment
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 3;
            ground.gameObject.layer = groundLayer;

            // Ensure components
            var tm = ground.GetComponent<Tilemap>();
            var tmc = ground.GetComponent<TilemapCollider2D>();
            if (tmc == null)
            {
                tmc = ground.gameObject.AddComponent<TilemapCollider2D>();
            }
            tmc.usedByComposite = true;

            var rb = ground.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = ground.gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;

            var cc = ground.GetComponent<CompositeCollider2D>();
            if (cc == null)
            {
                cc = ground.gameObject.AddComponent<CompositeCollider2D>();
            }
            cc.isTrigger = false;
            // Favor Polygons for stable ground surfaces
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            cc.generationType = CompositeCollider2D.GenerationType.Synchronous;

            // Diagnostics: path count and bounds
            int paths = cc.pathCount;
            var bounds = cc.bounds;
            Debug.Log($"[GroundValidation] Composite collider paths: {paths}; bounds: {bounds}");
            if (paths == 0)
            {
                Debug.LogWarning("[GroundValidation] No composite paths found. Is the tilemap empty or collider generation disabled?");
            }

            Debug.Log("[PlayerPhysicsAutoFixer] Verified Ground tilemap colliders and layer");
        }

        private static Transform FindRootByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == name) return go.transform;
            }
            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName)) return null;
            foreach (Transform child in parent)
            {
                if (child.name == childName) return child;
                var found = FindChildRecursive(child, childName);
                if (found != null) return found;
            }
            return null;
        }

        private void TryAlignCorgiMasks()
        {
            // Align Corgi masks to include Ground layer
            var ctrl = GetComponentInParent<MonoBehaviour>();
            // Find by exact type name to avoid hard dep
            var ccType = System.Type.GetType("MoreMountains.CorgiEngine.CorgiController, MoreMountains.CorgiEngine");
            if (ccType == null)
            {
                // try by scanning components
                foreach (var mb in GetComponentsInParent<MonoBehaviour>(true))
                {
                    if (mb != null && mb.GetType().Name == "CorgiController") { ctrl = mb; break; }
                }
            }
            else
            {
                ctrl = GetComponentInParent(ccType) as MonoBehaviour;
            }

            if (ctrl == null) return;

            int ground = LayerMask.NameToLayer("Ground");
            int def = LayerMask.NameToLayer("Default");
            int maskBits(int layer) => (layer >= 0) ? (1 << layer) : 0;
            int add = maskBits(ground) | maskBits(def);

            var t = ctrl.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            foreach (var name in new[] { "PlatformMask", "MovingPlatformMask", "OneWayPlatformMask", "MovingOneWayPlatformMask", "MidHeightOneWayPlatformMask" })
            {
                var f = t.GetField(name, flags);
                if (f == null) continue;
                try
                {
                    int v = (int)f.GetValue(ctrl);
                    int nv = v | add;
                    if (nv != v)
                    {
                        f.SetValue(ctrl, nv);
                        Debug.Log($"[CorgiMask] Added Ground/Default to {name} (was {v}, now {nv})");
                    }
                }
                catch { }
            }
        }
    }
}
