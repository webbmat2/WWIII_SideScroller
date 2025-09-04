using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BayatGames
{
    public class RobinHood : MonoBehaviour
    {
        [SerializeField]
        protected CharacterController2D CC2D;
        [SerializeField]
        protected Animator animator;


        [Header("Character Parameters")]
        [SerializeField]
        private float Speed = 5f;
        [SerializeField]
        private float walkSpeed = 0.2f;
        [SerializeField]
        [Range(0, 1000)]
        protected float jumpForce = 200;

        [Header("Bow Parameters")]
        [SerializeField]
        protected GameObject ArrowPrefab;
        [SerializeField]
        protected Transform arrowSpawnPoint;
        [SerializeField]
        protected float fireForce = 400f;
        [SerializeField]
        protected float maxFireForce = 1f;
        [SerializeField]
        protected float currentFireForce = 0f;

        protected bool prepareForShot = false;
        protected bool canShot = false;
        protected bool isDead = false;
        public float CurrentFireForce
        {
            get
            {
                return this.currentFireForce;
            }
        }


        void Start()
        {

        }


        void Update()
        {
            if (isDead)
            {
                this.CC2D.m_rigidbody2D.simulated = false;
                return;
            }
            CC2D.Move(new Vector2(Input.GetAxis("Horizontal") * this.Speed, 0f));

            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;
            diff.Normalize();

            float rot_z = Mathf.Atan2(diff.y, Mathf.Abs(diff.x)) * Mathf.Rad2Deg;
            //this.transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

            if (Input.GetKeyDown(KeyCode.R))
            {
                prepareForShot = true;
                currentFireForce = 0f;
            }

            if (Input.GetButton("Attack") && canShot)
            {
                if (currentFireForce < maxFireForce)
                {
                    currentFireForce += 0.04f;
                    if (currentFireForce > maxFireForce)
                    {
                        currentFireForce = maxFireForce;
                    }
                }

            }
            Vector2 fire = (fireForce * currentFireForce) * diff;
            if (Input.GetButtonUp("Attack") && canShot)
            {
                prepareForShot = false;
                FireArrow(fire);

                //currentFireForce = 0f;
                animator.SetTrigger("Shot");
            }


            if (Input.GetButton("Walk"))
            {
                CC2D.Move(new Vector2(Input.GetAxis("Horizontal") * this.walkSpeed, 0f));
            }
            if (Input.GetButtonDown("Jump") && this.CC2D.IsGrounded)
            {
                CC2D.Jump(jumpForce);
                animator.SetTrigger("Jump");
            }
            if (prepareForShot)
            {
                if (diff.x < 0 && CC2D.FacingRight)
                {
                    CC2D.Flip();
                }
                else if (diff.x > 0 && !CC2D.FacingRight)
                {
                    CC2D.Flip();
                }
            }
            else
            {
                if (CC2D.m_rigidbody2D.linearVelocity.x < 0 && CC2D.FacingRight)
                {
                    CC2D.Flip();
                }
                else if (CC2D.m_rigidbody2D.linearVelocity.x > 0 && !CC2D.FacingRight)
                {
                    CC2D.Flip();
                }
            }
            animator.SetFloat("BodyRotation", rot_z);
            animator.SetFloat("CurrentFireForce", currentFireForce);
            animator.SetBool("PrepareForShot", prepareForShot);
            animator.SetBool("CanShot", canShot);
            animator.SetFloat("SpeedX", Mathf.Abs(CC2D.m_rigidbody2D.linearVelocity.x));
            animator.SetFloat("VelocityY", CC2D.m_rigidbody2D.linearVelocity.y);
            animator.SetBool("Grounded", CC2D.IsGrounded);
        }
        public virtual void FireArrow(Vector2 force)
        {
            GameObject Arrow = Instantiate<GameObject>(ArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rigidbody = Arrow.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(force);
        }

        public virtual void Death()
        {
            isDead = true;
            animator.SetTrigger("Death");
        }
        public virtual void CanShotTrigger()
        {
            canShot = true;
        }
        public virtual void CanShotTriggerFalse()
        {
            canShot = false;

        }
    }
}
