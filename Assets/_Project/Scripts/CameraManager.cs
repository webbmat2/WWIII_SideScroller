using UnityEngine;
using Unity.Cinemachine;

[AddComponentMenu("Camera/Camera Manager")]
public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera followCamera;
    [SerializeField] private float screenShakeIntensity = 0.5f;
    [SerializeField] private float screenShakeDuration = 0.2f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.3f;

    private CinemachineBasicMultiChannelPerlin _noiseComponent;
    private Transform _playerTransform;
    private Vector3 _currentLookAhead;
    private Vector3 _lookAheadVelocity;

    private void Start()
    {
        // Find player
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        // Setup camera if not assigned
        if (followCamera == null)
        {
            followCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        // Get noise component for screen shake
        if (followCamera != null)
        {
            _noiseComponent = followCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        SetupCameraFollow();
    }

    private void SetupCameraFollow()
    {
        if (followCamera == null || _playerTransform == null) return;

        // Set player as follow target
        followCamera.Follow = _playerTransform;

        // Configure framing transposer
        var framingTransposer = followCamera.GetComponent<CinemachinePositionComposer>();
        if (framingTransposer != null)
        {
            framingTransposer.TargetOffset = Vector3.zero;
            framingTransposer.LookAhead = 2f;
            framingTransposer.LookAheadSmoothing = 10f;
            framingTransposer.LookAheadIgnoreY = true;
        }
    }

    private void Update()
    {
        UpdateLookAhead();
    }

    private void UpdateLookAhead()
    {
        if (_playerTransform == null) return;

        // Get player input direction
        float inputX = 0f;
        
        // Try to get input from player controller
        var playerController = _playerTransform.GetComponent<PlayerController2D>();
        if (playerController != null)
        {
            // We'd need to expose the input direction from PlayerController2D
            // For now, use Input directly
            inputX = Input.GetAxisRaw("Horizontal");
        }

        // Calculate target look ahead
        Vector3 targetLookAhead = Vector3.right * (inputX * lookAheadDistance);
        
        // Smooth the look ahead
        _currentLookAhead = Vector3.SmoothDamp(_currentLookAhead, targetLookAhead, 
            ref _lookAheadVelocity, lookAheadSmoothTime);

        // Apply to camera (this would need to be done through Cinemachine's composer)
        if (followCamera != null)
        {
            var composer = followCamera.GetComponent<CinemachinePositionComposer>();
            if (composer != null)
            {
                composer.TargetOffset = _currentLookAhead;
            }
        }
    }

    public void TriggerScreenShake(float intensity = -1f, float duration = -1f)
    {
        if (_noiseComponent == null) return;

        float shakeIntensity = intensity > 0 ? intensity : screenShakeIntensity;
        float shakeDuration = duration > 0 ? duration : screenShakeDuration;

        StartCoroutine(DoScreenShake(shakeIntensity, shakeDuration));
    }

    private System.Collections.IEnumerator DoScreenShake(float intensity, float duration)
    {
        _noiseComponent.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        
        // Fade out shake
        float elapsed = 0f;
        float fadeTime = 0.1f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            _noiseComponent.AmplitudeGain = Mathf.Lerp(intensity, 0f, t);
            yield return null;
        }
        
        _noiseComponent.AmplitudeGain = 0f;
    }

    public void SetCameraTarget(Transform newTarget)
    {
        if (followCamera != null)
        {
            followCamera.Follow = newTarget;
            _playerTransform = newTarget;
        }
    }
}