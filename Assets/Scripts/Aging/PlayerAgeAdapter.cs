using UnityEngine;

namespace WWIII.SideScroller.Aging
{
    [DisallowMultipleComponent]
    public class PlayerAgeAdapter : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Runtime Movement (read by your controller)")]
        public float maxRunSpeed = 5f;
        public float acceleration = 50f;
        public float deceleration = 60f;
        public float jumpForce = 8f;
        public float gravityScale = 3f;

        private Rigidbody2D _rb;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            maxRunSpeed = config.maxRunSpeed;
            acceleration = config.acceleration;
            deceleration = config.deceleration;
            jumpForce = config.jumpForce;
            gravityScale = config.gravityScale;
            if (_rb != null) _rb.gravityScale = gravityScale;
        }

        public void OnAgeChanged(AgeProfile profile)
        {
            // Hook: play a VFX/SFX, reset state, etc.
        }
    }
}

