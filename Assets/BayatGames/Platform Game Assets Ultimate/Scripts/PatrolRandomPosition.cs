using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class PatrolRandomPosition : MonoBehaviour
    {

        [SerializeField]
        protected LayerMask groundLayer;
        [SerializeField]
        protected LayerMask barrierLayer;
        [SerializeField]
        protected Rigidbody2D rigidbody2d;
        [SerializeField]
        protected Collider2D collider2d;
        [SerializeField]
        protected Transform groundCheck;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected Transform target;

        [Header("Parameters")]
        [SerializeField]
        protected bool isPatrolling = true;
        [SerializeField]
        protected float speed = 1.4f;
        [SerializeField]
        protected float jumpStrength = 5f;
        [SerializeField]
        protected float followRange = 10f;
        [SerializeField]
        protected float rayRange = 1f;
        [SerializeField]
        protected Transform startPosition;
        [SerializeField]
        protected Transform endPosition;

        [Header("Wait Time Random Range")]
        [SerializeField]
        protected float min = 0f;
        [SerializeField]
        protected float max = 6f;

        bool isDead = false;
        protected bool isBarrier;
        protected int direction = 0;
        protected float lastTime = 0f;
        private bool facingLeft = true;
        private bool isGrounded;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3.left * transform.localScale.x * rayRange));
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isDead)
            {
                this.rigidbody2d.simulated = false;
                return;
            }

            RaycastHit2D hitInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.3f, groundLayer);
            isGrounded = hitInfo.collider != null;
            RaycastHit2D barrierHit = Physics2D.Raycast(this.transform.position, Vector2.left * this.transform.localScale.x, rayRange, barrierLayer);
            isBarrier = barrierHit.collider != null;


            if (this.isPatrolling)
            {
                if (Time.time > lastTime)
                {
                    float waitTime = Random.Range(min, max);
                    lastTime = Time.time + waitTime;
                    direction = Random.Range(-1, 2);
                }
            }

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



            Move(new Vector2(speed * direction, 0));


            if (facingLeft && rigidbody2d.linearVelocity.x > 0)
            {
                Flip();
            }
            else if (!facingLeft && rigidbody2d.linearVelocity.x < 0)
            {
                Flip();
            }
            animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
            animator.SetFloat("VelocityY", rigidbody2d.linearVelocity.y);
            animator.SetBool("Grounded", isGrounded);

            //Debug.Log(direction);
            //Debug.Log(waitTime);
        }

        IEnumerator Patrol()
        {
            while (this.isPatrolling)
            {
                int direction = Random.Range(-1, 2);
                yield return new WaitForSeconds(Random.Range(1, 6));
            }
        }
        public virtual void DeathAnimationTrigger()
        {
            isDead = true;
            animator.SetTrigger("Death");
        }

        public virtual void Move(Vector2 velocity)
        {
            Vector2 newVelocity = rigidbody2d.linearVelocity;
            newVelocity.x = velocity.x;
            rigidbody2d.linearVelocity = newVelocity;
        }

        public virtual void Flip()
        {
            facingLeft = !facingLeft;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        public virtual void Jump()
        {
            Vector2 velocity = rigidbody2d.linearVelocity;
            velocity.y = jumpStrength;
            rigidbody2d.linearVelocity = velocity;
        }

    }
}
