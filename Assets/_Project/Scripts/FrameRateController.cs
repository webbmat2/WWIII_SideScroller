using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Ensure VSync is disabled for manual frame rate control.
    }
}
