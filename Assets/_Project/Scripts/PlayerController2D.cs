using UnityEngine;

[AddComponentMenu("Gameplay/Player Controller 2D")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    private int health;
    private Rigidbody2D rb;
    private Vector2 respawnPoint;
    private Transform currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
        if (respawnPoint == Vector2.zero) respawnPoint = transform.position;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        var v = rb.linearVelocity;
        v.x = h * moveSpeed;
        rb.linearVelocity = v;

        if (Input.GetButtonDown("Jump") && IsGrounded())
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void SetRespawnPoint(Vector2 point) => respawnPoint = point;

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = respawnPoint;
    }

    public void ApplyDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = maxHealth;
            Respawn();
        }
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    public void AttachToPlatform(Transform platform)
    {
        if (platform == null) return;
        transform.SetParent(platform);
        currentPlatform = platform;
    }

    public void DetachFromPlatform()
    {
        transform.SetParent(null);
        currentPlatform = null;
    }

    public void DetachFromPlatform(Transform platform)
    {
        if (currentPlatform == platform) DetachFromPlatform();
    }

    private void OnDisable()
    {
        DetachFromPlatform();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}