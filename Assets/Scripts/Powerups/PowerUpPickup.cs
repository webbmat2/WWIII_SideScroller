using System.Collections;
using UnityEngine;
using WWIII.SideScroller.Powerups;

namespace WWIII.SideScroller.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        public PowerUpDefinition definition;
        public AudioSource pickupAudio;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;
            var target = other.GetComponentInParent<MonoBehaviour>();
            if (target == null) return;
            StartCoroutine(ApplyCo(target.gameObject));
        }

        private IEnumerator ApplyCo(GameObject target)
        {
            if (pickupAudio != null) pickupAudio.Play();
            GetComponent<Collider2D>().enabled = false;
            var sr = GetComponentInChildren<SpriteRenderer>(); if (sr != null) sr.enabled = false;

            float duration = definition != null ? definition.duration : 8f;
            var endTime = Time.time + duration;

            // Invulnerability via Corgi Health
            MoreMountains.CorgiEngine.Health health = target.GetComponentInParent<MoreMountains.CorgiEngine.Health>();
            if (definition != null && definition.healAmount > 0f && health != null) health.GetHealth(definition.healAmount, gameObject);

            if (definition != null && definition.invulnerable && health != null) health.DamageDisabled();

            // Speed multiplier via CharacterHorizontalMovement if present
            MoreMountains.CorgiEngine.CharacterHorizontalMovement move = target.GetComponentInParent<MoreMountains.CorgiEngine.CharacterHorizontalMovement>();
            float original = 0f; bool hadMove = false;
            if (move != null && definition != null && definition.speedMultiplier > 0f && definition.speedMultiplier != 1f)
            {
                original = move.MovementSpeed;
                move.MovementSpeed = original * definition.speedMultiplier;
                hadMove = true;
            }

            while (Time.time < endTime) yield return null;

            if (definition != null && definition.invulnerable && health != null) health.DamageEnabled();
            if (hadMove && move != null) move.MovementSpeed = original;

            Destroy(gameObject);
        }
    }
}
