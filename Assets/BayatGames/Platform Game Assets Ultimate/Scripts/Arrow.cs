using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField]
        protected float damage = 20;
        [SerializeField]
        protected float hitForce = 200f;
        [SerializeField]
        protected Rigidbody2D m_rigidbody2D;
        [SerializeField]
        protected LayerMask hitLayer;
        [SerializeField]
        protected GameObject m_gameObject;
        [SerializeField]
        protected Sprite sprite;


        private SpriteRenderer spriteRenderer;
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        void Update()
        {

            Vector2 v = m_rigidbody2D.linearVelocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            Vector2 arrowDirection = this.m_rigidbody2D.linearVelocity;
            arrowDirection.Normalize();
            var collisionPos = collision.gameObject.GetComponent<Collider2D>().ClosestPoint(transform.position);
            bool isHit = hitLayer == (hitLayer | (1 << collision.gameObject.layer));
            Rigidbody2D m_rigidbody2d = collision.gameObject.GetComponent<Rigidbody2D>();
            //Debug.Log(ishit);
            if (isHit)
            {

                m_rigidbody2D.simulated = false;
                m_gameObject.SetActive(false);
                this.spriteRenderer.sprite = sprite;
                if (m_rigidbody2d != null)
                {
                    m_rigidbody2d.AddForce(new Vector2(arrowDirection.x * hitForce, 0));
                }
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                this.gameObject.transform.SetParent(collision.gameObject.transform, true);
            }

            Health health = collision.gameObject.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
                health.ApplyEffects(collisionPos);
            }


        }

    }
}
