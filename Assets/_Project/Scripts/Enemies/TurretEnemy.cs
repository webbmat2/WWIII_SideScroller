using UnityEngine;

[AddComponentMenu("Enemies/Turret Enemy")]
public class TurretEnemy : MonoBehaviour
{
    [Header("Turret Settings")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;

    private float _nextFireTime;
    private PlayerController2D _targetPlayer;

    private void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Update()
    {
        DetectPlayer();
        
        if (_targetPlayer != null && Time.time >= _nextFireTime)
        {
            FireProjectile();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private void DetectPlayer()
    {
        var player = FindFirstObjectByType<PlayerController2D>();
        if (player == null) 
        {
            _targetPlayer = null;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= detectionRange)
        {
            // Check line of sight
            Vector3 direction = (player.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRange, ~0);
            
            if (hit.collider != null && hit.collider.GetComponentInParent<PlayerController2D>() != null)
            {
                _targetPlayer = player;
                return;
            }
        }
        
        _targetPlayer = null;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || _targetPlayer == null) return;

        // Calculate direction to player
        Vector3 direction = (_targetPlayer.transform.position - firePoint.position).normalized;
        
        // Create projectile
        var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<Projectile>();
        
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, projectileSpeed);
        }
        else
        {
            // Fallback: just add velocity to rigidbody
            var rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
        }

        // Play sound
        if (fireSound != null)
        {
            AudioSource.PlayClipAtPoint(fireSound, transform.position);
        }

        Debug.Log($"Turret fired at player!");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }

        // Draw line to target if detected
        if (Application.isPlaying && _targetPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetPlayer.transform.position);
        }
    }
#endif
}