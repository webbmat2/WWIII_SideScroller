using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BayatGames
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField]
        protected GroundedEvent onGrounded;
        [SerializeField]
        protected Transform target;
        [SerializeField]
        protected Transform groundCheck;
        [SerializeField]
        protected LayerMask barrierLayer;
        [SerializeField]
        protected Rigidbody2D rigidbody2d;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected Health health;
        [SerializeField]
        protected LayerMask attackLayer;
        [SerializeField]
        protected Sword m_Sword;
        [SerializeField]
        protected bool attack = false;
        [SerializeField]
        protected GameObject[] m_gameObject;

        [Header("Parameters")]
        [SerializeField]
        protected float walkSpeed = 1f;
        [SerializeField]
        protected float runSpeed = 1.8f;
        [SerializeField]
        protected float barrierHitRange = 3f;
        [SerializeField]
        protected float minRange = 3f;
        [SerializeField]
        protected float followRange = 10f;
        [SerializeField]
        protected float jumpStrength = 5f;

        public bool chaseCharacter = false;

        [Header("Patrol")]
        public bool runWhenTargetInRange = true;
        public bool patrol = false;
        [SerializeField]
        protected Transform startPosition;
        [SerializeField]
        protected Transform endPosition;



        public bool inRange = false;
        bool inCenter;
        bool isDead = false;
        bool facingLeft = true;
        bool isBarrier = false;
        bool isGrounded = false;
        public bool canAttack = false;
        private bool chase;
        private int direction = 1;


        public virtual float Speed
        {
            get
            {
                return this.walkSpeed;
            }
        }
        public virtual Animator m_Animator
        {
            get
            {
                return this.animator;
            }
        }
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minRange);
            Gizmos.DrawWireSphere(transform.position, followRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3.left * barrierHitRange));
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3.down * 0.3f));
        }
        void Start()
        {

        }


        void Update()
        {
            if (isDead)
            {
                this.rigidbody2d.simulated = false;
                return;
            }
            RaycastHit2D hitinfo = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, barrierHitRange, barrierLayer);
            isBarrier = hitinfo.collider != null;
            RaycastHit2D groundhitinfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.3f, barrierLayer);
            if (!isGrounded && groundhitinfo.collider != null)
            {
                onGrounded?.Invoke();
            }
            isGrounded = groundhitinfo.collider != null;


            if (!patrol)
            {

                FollowCharacter();

            }
            else if (patrol)
            {

                if (checkFollowRadius(target.position, this.transform.position))
                {
                    FollowCharacter();
                }
                else
                {
                    Patrol();
                }
            }

            if (rigidbody2d.linearVelocity.x > 0 && facingLeft)
            {
                Flip();
            }
            else if (rigidbody2d.linearVelocity.x < 0 && !facingLeft)
            {
                Flip();
            }

            if (!chaseCharacter)
            {
                inRange = checkFollowRadius(target.position, this.transform.position);
            }
            animator.SetBool("inRange", inRange);
            animator.SetBool("Grounded", isGrounded);
            animator.SetFloat("VelocityY", rigidbody2d.linearVelocity.y);



        }




        public bool checkMinRange(float playerPoition, float enemyPostion)
        {
            if (Mathf.Abs(playerPoition - enemyPostion) < minRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool checkFollowRadius(Vector2 playerPosition, Vector2 enemyPosition)
        {
            if (Vector2.Distance(playerPosition, enemyPosition) < followRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual void Move(Vector2 velocity)
        {
            Vector2 newVelocity = rigidbody2d.linearVelocity;
            newVelocity.x = velocity.x;
            rigidbody2d.linearVelocity = newVelocity;
        }
        public virtual void Jump()
        {
            Vector2 velocity = rigidbody2d.linearVelocity;
            velocity.y = jumpStrength;
            rigidbody2d.linearVelocity = velocity;
        }
        public void Flip()
        {
            facingLeft = !facingLeft;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        public void FollowCharacter()
        {
            if (checkFollowRadius(target.position, this.transform.position))
            {
                if (checkMinRange(target.position.x, this.transform.position.x))
                {
                    canAttack = true;
                    if (attack)
                    {
                        animator.SetTrigger("Attack");
                    }
                    Move(new Vector2(0, 0));
                    animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
                }
                else
                {
                    canAttack = false;
                    if (isBarrier && isGrounded)
                    {
                        Jump();
                        animator.SetTrigger("Jump");
                    }
                    if (runWhenTargetInRange)
                    {
                        if (target.position.x > this.transform.position.x)
                        {

                            Move(new Vector2(runSpeed, 0));

                        }

                        if (target.position.x < this.transform.position.x)
                        {

                            Move(new Vector2(-runSpeed, 0));

                        }
                    }
                    else
                    {
                        if (target.position.x > this.transform.position.x)
                        {

                            Move(new Vector2(walkSpeed, 0));

                        }

                        if (target.position.x < this.transform.position.x)
                        {

                            Move(new Vector2(-walkSpeed, 0));

                        }
                    }
                    animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
                }
                animator.SetBool("CanAttack", canAttack);
                if (chaseCharacter)
                {
                    chase = true;
                }
            }
            else
            {
                Move(new Vector2(0, 0));
                animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
            }
            if (chase)
            {
                inRange = true;
                Chase();
            }
        }
        public void Patrol()
        {
            if (this.transform.position.x > endPosition.position.x)
            {
                direction = -1;
            }
            else if (this.transform.position.x < startPosition.position.x)
            {
                direction = 1;
            }
            if (isBarrier && isGrounded)
            {
                Jump();
                animator.SetTrigger("Jump");
            }
            Move(new Vector2(walkSpeed * direction, 0));
            animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
        }
        public void Chase()
        {

            if (runWhenTargetInRange)
            {

                if (target.position.x > this.transform.position.x)
                {
                    Move(new Vector2(runSpeed, 0));
                }

                if (target.position.x < this.transform.position.x)
                {
                    Move(new Vector2(-runSpeed, 0));
                }

            }
            else
            {

                if (target.position.x > this.transform.position.x)
                {
                    Move(new Vector2(walkSpeed, 0));
                }

                if (target.position.x < this.transform.position.x)
                {
                    Move(new Vector2(-walkSpeed, 0));
                }

            }

            animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));

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
        public virtual void DeathAnimationTrigger()
        {
            isDead = true;
            animator.SetTrigger("Death");
        }

        public virtual void ApplyDamageAnimationTrigger()
        {
            ApplyDamage();
        }

        public void ApplyDamage()
        {
            if (m_Sword != null)
            {

                RaycastHit2D hitInfo = Physics2D.Raycast(this.transform.position, Vector2.left * transform.localScale.x, m_Sword.HitRange, attackLayer);
                if (hitInfo.collider != null)
                {
                    Health health = hitInfo.collider.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(m_Sword.Demage);
                        health.ApplyEffects(hitInfo.point);
                    }
                }

            }
        }
    }
}
[System.Serializable]
public class GroundedEvent : UnityEvent { }
