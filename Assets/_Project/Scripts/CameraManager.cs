using UnityEngine;

// Minimal CameraManager for WWIII_SideScroller
// Using CameraFollow2D as primary camera system as per deliverables
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Minimal screen shake for compatibility
    public void TriggerScreenShake(float intensity, float duration)
    {
        // Screen shake implementation would go here
        Debug.Log($"Screen shake: intensity={intensity}, duration={duration}");
    }
}