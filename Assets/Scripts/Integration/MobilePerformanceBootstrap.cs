using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Mobile 60 FPS optimization and Unity 6000.2 physics defaults for 2D.
    /// Add to AgeManager or any bootstrap object in the first scene.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class MobilePerformanceBootstrap : MonoBehaviour
    {
        [Header("Frame Rate")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync = true;

        [Header("Physics 2D (Unity 6000.2)")]
        [SerializeField] private float fixedDeltaTime = 0.02f; // 50Hz physics for Corgi stability
        [SerializeField] private float defaultContactOffset = 0.01f; // Optimize 2D collision precision

        private void Awake()
        {
            ApplySettings();
        }

        [ContextMenu("Apply Mobile Performance Settings Now")]
        private void ApplySettings()
        {
            Application.targetFrameRate = targetFrameRate;
            if (disableVSync) QualitySettings.vSyncCount = 0; // Disable VSync for consistent frame timing

            Time.fixedDeltaTime = fixedDeltaTime;
            Physics2D.defaultContactOffset = defaultContactOffset;

            Debug.Log("[MobilePerformanceBootstrap] Applied 60 FPS and 2D physics settings");
        }
    }
}

