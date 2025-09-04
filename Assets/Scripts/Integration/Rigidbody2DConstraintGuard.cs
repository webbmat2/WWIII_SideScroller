using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    // Runs after most scripts to override accidental writes to constraints.
    [DefaultExecutionOrder(int.MaxValue)]
    public sealed class Rigidbody2DConstraintGuard : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;

        private RigidbodyConstraints2D lastApplied = (RigidbodyConstraints2D)(-1);

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Awake() => Enforce();
        private void OnEnable() => Enforce();
        private void FixedUpdate() => Enforce();

        private void Enforce()
        {
            if (!rb) return;
            var desired = RigidbodyConstraints2D.FreezeRotation;
            if (rb.constraints != desired)
            {
                // Clear all stale bits and write only FreezeRotation
                Debug.Log($"[ConstraintGuard] Correcting constraints from {rb.constraints} to FreezeRotation");
                rb.freezeRotation = false; // avoid legacy flag interference
                rb.constraints = RigidbodyConstraints2D.None;
                rb.constraints = desired;

                if (lastApplied != desired)
                {
                    Debug.Log("[Rigidbody2DConstraintGuard] Corrected constraints to FreezeRotation only");
                    lastApplied = desired;
                }
            }
        }
    }
}
