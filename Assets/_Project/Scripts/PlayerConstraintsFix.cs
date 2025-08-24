using UnityEngine;

[AddComponentMenu("Gameplay/Player Constraints Fix")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerConstraintsFix : MonoBehaviour
{
    [Header("Force Fix Settings")]
    [SerializeField] private bool fixOnAwake = true;
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool continuousCheck = true;
    [SerializeField] private float checkInterval = 0.1f;

    private Rigidbody2D _rb;
    private float _nextCheck;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        if (fixOnAwake)
        {
            ForceFixConstraints();
        }
    }

    private void Start()
    {
        if (fixOnStart)
        {
            ForceFixConstraints();
        }
    }

    private void Update()
    {
        if (continuousCheck && Time.time >= _nextCheck)
        {
            CheckAndFixConstraints();
            _nextCheck = Time.time + checkInterval;
        }
    }

    private void CheckAndFixConstraints()
    {
        if (_rb == null) return;

        // Check if constraints are wrong
        if (_rb.constraints == RigidbodyConstraints2D.FreezeAll || 
            _rb.constraints.HasFlag(RigidbodyConstraints2D.FreezePositionX) ||
            _rb.constraints.HasFlag(RigidbodyConstraints2D.FreezePositionY))
        {
            Debug.LogWarning($"Player has incorrect constraints: {_rb.constraints}. Fixing...");
            ForceFixConstraints();
        }
    }

    [ContextMenu("Force Fix Constraints")]
    public void ForceFixConstraints()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) return;

        var oldConstraints = _rb.constraints;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        Debug.Log($"Player constraints fixed: {oldConstraints} → {_rb.constraints}");
    }
}