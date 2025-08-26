using UnityEngine;
using WWIII.Player;

namespace WWIII.Gameplay
{
    public class BasicEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private EnemyType enemyType = EnemyType.Walker;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int health = 1;
        [SerializeField] private int damage = 1;
        
        [Header("Patrol Settings")]
        [SerializeField] private float patrolDistance = 5f;
        [SerializeField] private bool usePatrolPoints = false;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waitTimeAtPoint = 1f;
        
        [Header("Detection Settings")]
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private bool returnToPatrolAfterLoss = true;
        [SerializeField] private float loseTargetTime = 3f;
        
        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private bool canBeStompedOn = true;
        [SerializeField] private float stompBounceForce = 10f;
        
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color aggroColor = Color.red;
        [SerializeField] private float colorTransitionSpeed = 2f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem deathEffect;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip detectionSound;
        
        // State variables
        private EnemyState currentState = EnemyState.Patrol;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private int currentPatrolIndex = 0;
        private float patrolDirection = 1f;
        private float waitTimer = 0f;
        private float attackTimer = 0f;
        private float loseTargetTimer = 0f;
        
        // Component references
        private Rigidbody2D rb;
        private Collider2D col;
        private AudioSource audioSource;
        private Transform playerTransform;
        private PlayerController playerController;
        
        // Properties
        public EnemyState CurrentState => currentState;
        public bool IsAlive => health > 0;
        public EnemyType Type => enemyType;
        
        public System.Action<BasicEnemy> OnEnemyDeath;
        public System.Action<BasicEnemy, GameObject> OnPlayerDetected;
        public System.Action<BasicEnemy> OnPlayerLost;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupEnemy();
        }
        
        private void Update()
        {
            if (!IsAlive) return;
            
            UpdateState();
            UpdateVisuals();
            UpdateTimers();
        }
        
        private void FixedUpdate()
        {
            if (!IsAlive) return;
            
            HandleMovement();
        }
        
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
                
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        private void SetupEnemy()
        {
            startPosition = transform.position;
            
            // Setup patrol
            if (!usePatrolPoints)
            {
                // Simple back-and-forth patrol
                targetPosition = startPosition + Vector3.right * patrolDistance;
            }
            else if (patrolPoints != null && patrolPoints.Length > 0)
            {
                targetPosition = patrolPoints[0].position;
            }
            
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
            
            Debug.Log($"Enemy {name} initialized as {enemyType} in {currentState} state");
        }
        
        private void UpdateState()
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    HandlePatrolState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
                case EnemyState.Wait:
                    HandleWaitState();
                    break;
                case EnemyState.Return:
                    HandleReturnState();
                    break;
            }
            
            // Check for player detection
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Wait)
            {
                CheckPlayerDetection();
            }
        }
        
        private void HandlePatrolState()
        {
            if (usePatrolPoints && patrolPoints != null && patrolPoints.Length > 0)
            {
                // Patrol using points
                if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                {
                    ChangeState(EnemyState.Wait);
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    targetPosition = patrolPoints[currentPatrolIndex].position;
                }
            }
            else
            {
                // Simple back-and-forth patrol
                if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                {
                    patrolDirection *= -1f;
                    targetPosition = startPosition + Vector3.right * (patrolDistance * patrolDirection);
                    ChangeState(EnemyState.Wait);
                }
            }
        }
        
        private void HandleChaseState()
        {
            if (playerTransform == null)
            {
                ChangeState(EnemyState.Return);
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Check if in attack range
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyState.Attack);
                return;
            }
            
            // Check if player is still in detection range
            if (distanceToPlayer > detectionRange)
            {
                loseTargetTimer += Time.deltaTime;
                if (loseTargetTimer >= loseTargetTime)
                {
                    OnPlayerLost?.Invoke(this);
                    if (returnToPatrolAfterLoss)
                    {
                        ChangeState(EnemyState.Return);
                    }
                    else
                    {
                        ChangeState(EnemyState.Patrol);
                    }
                }
            }
            else
            {
                loseTargetTimer = 0f;
                targetPosition = playerTransform.position;
            }
        }
        
        private void HandleAttackState()
        {
            if (playerTransform == null)
            {
                ChangeState(EnemyState.Return);
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Check if player moved out of attack range
            if (distanceToPlayer > attackRange)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
            
            // Perform attack
            if (attackTimer <= 0f)
            {
                PerformAttack();
                attackTimer = attackCooldown;
            }
        }
        
        private void HandleWaitState()
        {
            if (waitTimer <= 0f)
            {
                ChangeState(EnemyState.Patrol);
            }
        }
        
        private void HandleReturnState()
        {
            // Return to start position or patrol
            if (Vector3.Distance(transform.position, startPosition) < 0.5f)
            {
                ChangeState(EnemyState.Patrol);
            }
            else
            {
                targetPosition = startPosition;
            }
        }
        
        private void CheckPlayerDetection()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                // Check line of sight
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRange, ~playerLayerMask);
                
                if (hit.collider == null || hit.collider.CompareTag("Player"))
                {
                    // Player detected
                    OnPlayerDetected?.Invoke(this, playerTransform.gameObject);
                    PlayDetectionSound();
                    ChangeState(EnemyState.Chase);
                }
            }
        }
        
        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;
            
            // Exit current state
            switch (currentState)
            {
                case EnemyState.Wait:
                    waitTimer = 0f;
                    break;
            }
            
            // Enter new state
            currentState = newState;
            switch (newState)
            {
                case EnemyState.Wait:
                    waitTimer = waitTimeAtPoint;
                    break;
                case EnemyState.Chase:
                    loseTargetTimer = 0f;
                    break;
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetInteger("State", (int)currentState);
            }
        }
        
        private void HandleMovement()
        {
            Vector3 currentPosition = transform.position;
            Vector3 direction = Vector3.zero;
            float currentMoveSpeed = 0f;
            
            switch (currentState)
            {
                case EnemyState.Patrol:
                case EnemyState.Return:
                    direction = (targetPosition - currentPosition).normalized;
                    currentMoveSpeed = moveSpeed;
                    break;
                case EnemyState.Chase:
                    direction = (targetPosition - currentPosition).normalized;
                    currentMoveSpeed = chaseSpeed;
                    break;
                case EnemyState.Attack:
                case EnemyState.Wait:
                    // No movement
                    break;
            }
            
            // Apply movement
            if (direction != Vector3.zero)
            {
                rb.linearVelocity = new Vector2(direction.x * currentMoveSpeed, rb.linearVelocity.y);
                
                // Flip sprite based on movement direction
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            }
        }
        
        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;
            
            // Color transition based on state
            Color targetColor = (currentState == EnemyState.Chase || currentState == EnemyState.Attack) ? aggroColor : normalColor;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, colorTransitionSpeed * Time.deltaTime);
        }
        
        private void UpdateTimers()
        {
            if (waitTimer > 0f)
                waitTimer -= Time.deltaTime;
                
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }
        
        private void PerformAttack()
        {
            Debug.Log($"Enemy {name} (type: {enemyType}) attacks!");
            
            // Play attack sound
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // Deal damage to player (if in range)
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                // Different damage based on enemy type
                int actualDamage = GetDamageByType();
                Debug.Log($"Player takes {actualDamage} damage from {enemyType}!");
                
                // For now, just trigger respawn
                var checkpointManager = CheckpointManager.Instance;
                if (checkpointManager != null)
                {
                    checkpointManager.RespawnPlayer();
                }
            }
        }
        
        private int GetDamageByType()
        {
            return enemyType switch
            {
                EnemyType.Walker => damage,
                EnemyType.Jumper => damage,
                EnemyType.Flyer => damage,
                EnemyType.Turret => damage * 2, // Turrets deal more damage
                EnemyType.Chaser => damage,
                _ => damage
            };
        }
        
        private void PlayDetectionSound()
        {
            if (audioSource != null && detectionSound != null)
            {
                audioSource.PlayOneShot(detectionSound);
            }
        }
        
        public void TakeDamage(int damageAmount)
        {
            health -= damageAmount;
            
            if (health <= 0)
            {
                Die();
            }
            else
            {
                // Flash effect or damage animation
                StartCoroutine(DamageFlash());
            }
        }
        
        private System.Collections.IEnumerator DamageFlash()
        {
            if (spriteRenderer == null) yield break;
            
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        
        public void Die()
        {
            Debug.Log($"Enemy {name} died!");
            
            // Play death effects
            if (deathEffect != null)
            {
                ParticleSystem effect = Instantiate(deathEffect, transform.position, transform.rotation);
                Destroy(effect.gameObject, 3f);
            }
            
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            // Notify listeners
            OnEnemyDeath?.Invoke(this);
            
            // Destroy enemy
            Destroy(gameObject, 0.1f);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && canBeStompedOn)
            {
                // Check if player is falling onto enemy
                Vector2 relativeVelocity = collision.relativeVelocity;
                if (relativeVelocity.y < -2f) // Player is falling fast enough
                {
                    // Player stomped on enemy
                    TakeDamage(1);
                    
                    // Bounce player
                    if (playerController != null)
                    {
                        // Add upward velocity to player
                        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, stompBounceForce);
                        }
                    }
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw patrol area
            if (!usePatrolPoints)
            {
                Gizmos.color = Color.blue;
                Vector3 start = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawLine(start - Vector3.right * patrolDistance, start + Vector3.right * patrolDistance);
            }
            else if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                        
                        int nextIndex = (i + 1) % patrolPoints.Length;
                        if (patrolPoints[nextIndex] != null)
                        {
                            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                        }
                    }
                }
            }
        }
    }
    
    public enum EnemyType
    {
        Walker,
        Jumper,
        Flyer,
        Turret,
        Chaser
    }
    
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Wait,
        Return,
        Dead
    }
}