using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        protected CharacterController2D CC2D;
        [SerializeField]
        public Animator animator;
        [SerializeField]
        protected Sword m_Sword;
        [SerializeField]
        protected LayerMask enemyLayer;
        [SerializeField]
        protected GameObject[] m_gameObject;
        [Header("Parameters")]
        [SerializeField]
        protected float hitForce = 200f;
        [SerializeField]
        protected float Speed = 5f;
        [SerializeField]
        protected float walkSpeed = 0.2f;
        [SerializeField]
        [Range(0, 1000)]
        protected float jumpForce = 200;


        protected bool sword = false;
        protected bool idleSword = false;
        protected bool canAttack = false;
        protected bool isDead = false;




        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isDead)
            {
                this.CC2D.m_rigidbody2D.simulated = false;
                return;
            }
            CC2D.Move(new Vector2(Input.GetAxis("Horizontal") * this.Speed, 0f));
            if (Input.GetButton("Walk"))
            {
                CC2D.Move(new Vector2(Input.GetAxis("Horizontal") * this.walkSpeed, 0f));
            }
            if (Input.GetButtonDown("Jump") && this.CC2D.IsGrounded)
            {
                CC2D.Jump(jumpForce);
                animator.SetTrigger("Jump");
            }

            if (Input.GetButtonDown("Attack"))
            {
                sword = true;
                animator.SetBool("Sword", sword);
                if (Input.GetButtonDown("Attack") && sword && canAttack)
                {
                    animator.SetTrigger("Attack");
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                idleSword = false;
                canAttack = false;
                sword = false;
                animator.SetBool("Sword", sword);
                animator.SetBool("IdleSword", idleSword);
            }
            if (CC2D.m_rigidbody2D.linearVelocity.x < 0 && CC2D.FacingRight)
            {
                CC2D.Flip();
            }
            else if (CC2D.m_rigidbody2D.linearVelocity.x > 0 && !CC2D.FacingRight)
            {
                CC2D.Flip();
            }


            animator.SetFloat("SpeedX", Mathf.Abs(CC2D.m_rigidbody2D.linearVelocity.x));
            animator.SetFloat("VelocityY", CC2D.m_rigidbody2D.linearVelocity.y);
            animator.SetBool("Grounded", CC2D.IsGrounded);
        }
        public virtual void Death()
        {
            isDead = true;
            animator.SetTrigger("Death");
        }
        public virtual void IdleSwordTrigger()
        {
            idleSword = true;
            animator.SetBool("IdleSword", idleSword);
        }
        public virtual void CanAttack()
        {
            canAttack = true;
        }
        public virtual void ObjectSetActiveTrue(int index)
        {
            if (m_gameObject != null)
            {

                m_gameObject[index].SetActive(true);
            }

        }
        public virtual void ObjectSetActiveFalse(int index)
        {

            if (m_gameObject != null)
            {

                m_gameObject[index].SetActive(false);
            }

        }
        public virtual void ApplyDemageTrigger()
        {
            ApplyDemage();
        }
        public virtual void ApplyDemage()
        {
            if (m_Sword != null)
            {
                RaycastHit2D[] hitInfos = Physics2D.RaycastAll(this.transform.position, Vector2.right * this.transform.localScale, m_Sword.HitRange, enemyLayer);
                for (int i = 0; i < hitInfos.Length; i++)
                {
                    Health health = hitInfos[i].collider.GetComponent<Health>();
                    if (health != null)
                    {
                        hitInfos[i].rigidbody.AddForce(new Vector2(this.transform.localScale.x * hitForce, 0));
                        health.TakeDamage(m_Sword.Demage);
                        health.ApplyEffects(hitInfos[i].point);
                    }
                }
            }

        }
    }
}

