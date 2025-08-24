using UnityEngine;

/// <summary>
/// EMERGENCY FIX: This script will immediately fix the Player's FreezeAll constraint issue
/// Add this component to your Player GameObject RIGHT NOW to fix movement!
/// </summary>
[AddComponentMenu("EMERGENCY/Player Fix")]
public class EMERGENCY_PLAYER_FIX : MonoBehaviour
{
    private void Awake()
    {
        FixPlayerImmediately();
    }

    private void Start()
    {
        FixPlayerImmediately();
    }

    private void Update()
    {
        // Keep checking and fixing every frame until constraints are correct
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.constraints != RigidbodyConstraints2D.FreezeRotation)
        {
            FixPlayerImmediately();
        }
    }

    [ContextMenu("FIX PLAYER NOW")]
    public void FixPlayerImmediately()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("EMERGENCY_PLAYER_FIX: No Rigidbody2D found!");
            return;
        }

        var oldConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        Debug.Log($"🚨 EMERGENCY FIX APPLIED! Player constraints: {oldConstraints} → {rb.constraints}");
        Debug.Log("🎮 Your player should now be able to move left/right and jump!");
        
        // Also ensure other settings are correct
        if (rb.gravityScale <= 0f)
        {
            rb.gravityScale = 3.5f;
            Debug.Log($"🔧 Fixed gravity scale: {rb.gravityScale}");
        }

        if (rb.mass <= 0f)
        {
            rb.mass = 1f;
            Debug.Log($"🔧 Fixed mass: {rb.mass}");
        }

        // Remove this component once fixed (to avoid conflicts)
        Destroy(this, 1f);
    }
}

/*
INSTRUCTIONS:
1. Add this component to your Player GameObject
2. Press Play
3. Your player will immediately start working
4. Use WASD or Arrow Keys to move
5. Space to jump

This is a temporary emergency fix. The proper PlayerConstraintsFix component will handle this going forward.
*/