using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace BayatGames
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        protected float maxHealth = 100f;
        [SerializeField]
        protected float currentHealth = 5f;
        [SerializeField]
        protected float armor = 5f;
        [SerializeField]
        protected ParticleSystem m_bloodParticle;
        [SerializeField]
        protected HealthBarDamageTakenEvent healthBarDamageTakenEvent;
        [SerializeField]
        protected DeathEvent deathEvent;
        [SerializeField]
        protected DamgaeTakenEvent damgaeTakenEvent;




        public HealthBarDamageTakenEvent m_HealthBarDamageTakenEvent
        {
            get
            {
                return healthBarDamageTakenEvent;
            }
        }
        public float m_CurrentHealth
        {
            get
            {
                return this.currentHealth;
            }
        }
        public float m_MaxHealth
        {
            get
            {
                return this.maxHealth;
            }
        }
        void Start()
        {
            currentHealth = maxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentHealth == 0)
            {
                this.deathEvent?.Invoke();
            }
        }

        public virtual void TakeDamage(float demage)
        {
            float demageReduction = demage - armor;
            this.healthBarDamageTakenEvent?.Invoke();
            this.damgaeTakenEvent?.Invoke();
            currentHealth -= demageReduction;
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

        }

        public virtual void ApplyEffects(Vector2 position)
        {
            ParticleSystem bloodParticle = Instantiate<ParticleSystem>(m_bloodParticle, position, Quaternion.identity);
            Destroy(bloodParticle.gameObject, bloodParticle.main.duration);
        }

        public virtual void Die()
        {
            Destroy(this.gameObject);
        }
        [System.Serializable]
        public class HealthBarDamageTakenEvent : UnityEvent { }
        [System.Serializable]
        public class DamgaeTakenEvent : UnityEvent { }
        [System.Serializable]
        public class DeathEvent : UnityEvent { }
    }
}
