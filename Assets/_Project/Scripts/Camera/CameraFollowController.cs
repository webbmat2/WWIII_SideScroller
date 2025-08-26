using UnityEngine;

namespace WWIII.Camera
{
    public class CameraFollowController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        
        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 2f;
        [SerializeField] private float lookAheadDistance = 3f;
        [SerializeField] private float lookAheadSpeed = 1f;
        [SerializeField] private bool useSmoothing = true;
        
        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Transform boundsTransform;
        [SerializeField] private Vector2 minBounds = new Vector2(-10, -5);
        [SerializeField] private Vector2 maxBounds = new Vector2(10, 5);
        
        [Header("Deadzone")]
        [SerializeField] private bool useDeadzone = true;
        [SerializeField] private Vector2 deadzoneSize = new Vector2(2f, 1f);
        
        [Header("Shake Settings")]
        [SerializeField] private float shakeIntensity = 0f;
        [SerializeField] private float shakeDuration = 0f;
        [SerializeField] private float shakeDecay = 2f;
        
        private UnityEngine.Camera cam;
        private Vector3 currentVelocity;
        private Vector3 targetPosition;
        private Vector3 lookAheadTarget;
        private float targetFacingDirection = 1f;
        private float currentFacingDirection = 1f;
        
        // Shake variables
        private float shakeTimer;
        private Vector3 shakeOffset;
        
        // Bounds calculation
        private float cameraHalfWidth;
        private float cameraHalfHeight;
        
        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            CalculateCameraBounds();
        }
        
        private void Start()
        {
            InitializeCamera();
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            UpdateLookAhead();
            UpdateTargetPosition();
            UpdateCameraPosition();
            UpdateShake();
        }
        
        private void InitializeCamera()
        {
            if (target != null)
            {
                // Set initial position
                Vector3 initialPos = target.position + offset;
                transform.position = ApplyBounds(initialPos);
            }
            
            // Find bounds automatically if not set
            if (useBounds && boundsTransform == null)
            {
                GameObject boundsObj = GameObject.FindGameObjectWithTag("CameraBounds");
                if (boundsObj != null)
                {
                    boundsTransform = boundsObj.transform;
                    SetBoundsFromCollider();
                }
            }
            
            Debug.Log("Camera follow controller initialized");
        }
        
        private void CalculateCameraBounds()
        {
            if (cam != null)
            {
                cameraHalfHeight = cam.orthographicSize;
                cameraHalfWidth = cam.orthographicSize * cam.aspect;
            }
        }
        
        private void SetBoundsFromCollider()
        {
            if (boundsTransform == null) return;
            
            Collider2D boundsCollider = boundsTransform.GetComponent<Collider2D>();
            if (boundsCollider != null)
            {
                Bounds bounds = boundsCollider.bounds;
                minBounds = new Vector2(bounds.min.x + cameraHalfWidth, bounds.min.y + cameraHalfHeight);
                maxBounds = new Vector2(bounds.max.x - cameraHalfWidth, bounds.max.y - cameraHalfHeight);
                
                Debug.Log($"Camera bounds set from collider: {minBounds} to {maxBounds}");
            }
        }
        
        private void UpdateLookAhead()
        {
            // Determine target facing direction
            if (target.localScale.x > 0)
                targetFacingDirection = 1f;
            else if (target.localScale.x < 0)
                targetFacingDirection = -1f;
            
            // Smoothly update current facing direction
            currentFacingDirection = Mathf.Lerp(currentFacingDirection, targetFacingDirection, lookAheadSpeed * Time.deltaTime);
            
            // Calculate look ahead offset
            lookAheadTarget = Vector3.right * (currentFacingDirection * lookAheadDistance);
        }
        
        private void UpdateTargetPosition()
        {
            Vector3 baseTargetPos = target.position + offset + lookAheadTarget;
            
            if (useDeadzone)
            {
                Vector3 currentPos = transform.position;
                Vector3 deltaMove = baseTargetPos - currentPos;
                
                // Apply deadzone
                if (Mathf.Abs(deltaMove.x) > deadzoneSize.x * 0.5f)
                {
                    targetPosition.x = baseTargetPos.x;
                }
                else
                {
                    targetPosition.x = currentPos.x;
                }
                
                if (Mathf.Abs(deltaMove.y) > deadzoneSize.y * 0.5f)
                {
                    targetPosition.y = baseTargetPos.y;
                }
                else
                {
                    targetPosition.y = currentPos.y;
                }
                
                targetPosition.z = baseTargetPos.z;
            }
            else
            {
                targetPosition = baseTargetPos;
            }
        }
        
        private void UpdateCameraPosition()
        {
            Vector3 newPosition;
            
            if (useSmoothing)
            {
                newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);
            }
            else
            {
                newPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
            
            // Apply bounds
            newPosition = ApplyBounds(newPosition);
            
            // Apply shake
            newPosition += shakeOffset;
            
            transform.position = newPosition;
        }
        
        private Vector3 ApplyBounds(Vector3 position)
        {
            if (!useBounds) return position;
            
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            
            return position;
        }
        
        private void UpdateShake()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                
                // Generate random shake offset
                shakeOffset = Random.insideUnitSphere * shakeIntensity;
                shakeOffset.z = 0; // Keep shake in 2D plane
                
                // Decay shake intensity
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDecay * Time.deltaTime);
                
                if (shakeTimer <= 0)
                {
                    shakeOffset = Vector3.zero;
                    shakeIntensity = 0f;
                }
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }
        
        // Public methods
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }
        
        public void SetBounds(Collider2D boundsCollider)
        {
            if (boundsCollider != null)
            {
                boundsTransform = boundsCollider.transform;
                SetBoundsFromCollider();
            }
        }
        
        public void EnableBounds(bool enable)
        {
            useBounds = enable;
        }
        
        public void TriggerShake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeTimer = duration;
        }
        
        public void SetFollowSpeed(float speed)
        {
            followSpeed = speed;
        }
        
        public void SetLookAheadDistance(float distance)
        {
            lookAheadDistance = distance;
        }
        
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
        
        public Vector3 GetTargetPosition()
        {
            return targetPosition;
        }
        
        public bool IsShaking()
        {
            return shakeTimer > 0;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw bounds
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 boundsCenter = new Vector3(
                    (minBounds.x + maxBounds.x) * 0.5f,
                    (minBounds.y + maxBounds.y) * 0.5f,
                    transform.position.z
                );
                Vector3 boundsSize = new Vector3(
                    maxBounds.x - minBounds.x,
                    maxBounds.y - minBounds.y,
                    0
                );
                Gizmos.DrawWireCube(boundsCenter, boundsSize);
            }
            
            // Draw deadzone
            if (useDeadzone && target != null)
            {
                Gizmos.color = Color.green;
                Vector3 deadzoneCenter = target.position + offset;
                Gizmos.DrawWireCube(deadzoneCenter, new Vector3(deadzoneSize.x, deadzoneSize.y, 0));
            }
            
            // Draw look ahead
            if (target != null)
            {
                Gizmos.color = Color.blue;
                Vector3 lookAheadPos = target.position + lookAheadTarget;
                Gizmos.DrawWireSphere(lookAheadPos, 0.5f);
                Gizmos.DrawLine(target.position, lookAheadPos);
            }
        }
    }
}