using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace BayatGames
{
    public class HealthBar : MonoBehaviour
    {

        [SerializeField]
        protected Health health;
        [SerializeField]
        protected Text text;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected RectTransform objectTransform;
        [SerializeField]
        protected ParticleSystem[] damageTakenParticle;

        private RectTransform healthbar;
        private RectTransform m_uiCanvas;

        public RectTransform UiCanvas
        {
            get
            {
                return m_uiCanvas;
            }
            set
            {
                m_uiCanvas = value;
            }
        }
        public Health Health
        {
            get
            {
                return this.health;
            }
            set
            {
                this.health = value;
            }
        }
        void Start()
        {
            health.m_HealthBarDamageTakenEvent.AddListener(AnimationTrigger);
            healthbar = GetComponent<RectTransform>();
        }


        void Update()
        {
            if (health.m_CurrentHealth <= 0)
            {
                Destroy(this.gameObject);
            }
            float healthPercent = health.m_CurrentHealth / health.m_MaxHealth;
            Vector2 healthbar = objectTransform.localScale;
            healthbar.x = healthPercent;
            objectTransform.localScale = healthbar;
            text.text = health.m_CurrentHealth.ToString();

            animator.SetFloat("HealthPercent", healthPercent);
        }
        public virtual void AnimationTrigger()
        {
            animator.SetTrigger("DamageTaken");

        }
        public void EmitDamageTakenParticle()
        {
            for (int i = 0; i < damageTakenParticle.Length; i++)
            {
                damageTakenParticle[i].Play();
            }
        }

    }
}

