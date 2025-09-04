using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Emergency constraint enforcer for Unity 6000.2 projects where serialized constraints get corrupted.
    /// Keeps RB2D at FreezeRotation only and performs a one-time deep repair pass.
    /// </summary>
    [DefaultExecutionOrder(int.MaxValue)]
    public sealed class ConstraintCorruptionFixer : MonoBehaviour
    {
        private Rigidbody2D rb;
        private bool repaired;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            FixConstraints();
        }

        private void Start()
        {
            FixConstraints();
        }

        private void FixedUpdate()
        {
            if (!rb) return;
            if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
            {
                Debug.LogWarning($"[ConstraintCorruptionFixer] Constraint corruption detected! Was: {rb.constraints}, fixing to FreezeRotation.");
                FixConstraints();
            }
        }

        private void FixConstraints()
        {
            if (!rb) return;
            // Deep repair once: toggle simulation and body type to force engine to rebuild state
            if (!repaired)
            {
                var originalSim = rb.simulated;
                var originalType = rb.bodyType;
                rb.simulated = false;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.bodyType = originalType;
                rb.simulated = originalSim;
                repaired = true;
            }
            rb.freezeRotation = false; // avoid legacy bool interference
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}

