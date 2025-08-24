using UnityEngine;

/// <summary>
/// Helper script to fix problematic constraints that may be set in the Inspector but need runtime fixes.
/// Add this to any GameObject in the scene to automatically fix constraints on Start.
/// </summary>
[AddComponentMenu("Utilities/Runtime Constraints Fixer")]
public class RuntimeConstraintsFixer : MonoBehaviour
{
    [Header("Auto-fix these objects on Start")]
    [SerializeField] private bool fixPlayerConstraints = true;
    [SerializeField] private bool fixMovingPlatformConstraints = true;
    
    private void Start()
    {
        if (fixPlayerConstraints)
        {
            var player = FindFirstObjectByType<PlayerController2D>();
            if (player != null)
            {
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null && rb.constraints != RigidbodyConstraints2D.FreezeRotation)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    Debug.Log($"RuntimeConstraintsFixer: Fixed Player constraints to {rb.constraints}");
                }
            }
        }

        if (fixMovingPlatformConstraints)
        {
            var platforms = FindObjectsByType<MovingPlatform>(FindObjectsSortMode.None);
            foreach (var platform in platforms)
            {
                var rb = platform.GetComponent<Rigidbody2D>();
                if (rb != null && rb.constraints != RigidbodyConstraints2D.FreezeRotation)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    Debug.Log($"RuntimeConstraintsFixer: Fixed MovingPlatform {platform.name} constraints to {rb.constraints}");
                }
            }
        }
    }
}