using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class Enemy_Patrol : MonoBehaviour
    {
        [SerializeField]
        protected LayerMask barrierLayer;
        [SerializeField]
        protected LayerMask groundLayer;
        [SerializeField]
        protected Transform groundCheck;
        [SerializeField]
        protected Rigidbody2D rigidbody2d;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected float speed = 1f;
        [SerializeField]
        protected float barrierHitRange = 3f;

        public bool withRange = false;
        int direction = 1;
        bool isGrounded = false;
        bool isBarrier;
        bool facingLeft = true;

        // Start is called before the first frame update

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + (Vector3.left * transform.localScale.x * barrierHitRange));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3.down * 0.3f));

        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            RaycastHit2D hitinfo = Physics2D.Raycast(this.transform.position, Vector2.left * transform.localScale.x, barrierHitRange, barrierLayer);
            isBarrier = hitinfo.collider != null;
            RaycastHit2D groundhitinfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.3f, groundLayer);
            isGrounded = groundhitinfo.collider != null;
            if (isBarrier && !withRange)
            {
                direction *= -1;
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

            animator.SetFloat("Speedy", rigidbody2d.linearVelocity.y);
            animator.SetFloat("SpeedX", Mathf.Abs(rigidbody2d.linearVelocity.x));
            animator.SetBool("Grounded", isGrounded);
        }
        public virtual void Move(Vector2 velocity)
        {
            Vector2 newVelocity = rigidbody2d.linearVelocity;
            newVelocity.x = velocity.x;
            rigidbody2d.linearVelocity = newVelocity;
        }
        public void Flip()
        {
            facingLeft = !facingLeft;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (withRange && collision.tag == "FollowRange")
            {
                direction *= -1;
            }
        }
    }
}
