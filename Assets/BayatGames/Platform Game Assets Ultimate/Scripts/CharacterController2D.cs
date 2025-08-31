using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BayatGames
{
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField]
        protected new Rigidbody2D rigidbody2D;
        [SerializeField]
        protected Transform groundCheck;
        [SerializeField]
        protected LayerMask Groundlayer;

        bool facingRight = true;
        protected bool isGrounded = false;


        public virtual bool IsGrounded
        {
            get
            {
                return this.isGrounded;
            }
        }
        public virtual bool FacingRight
        {
            get
            {
                return this.facingRight;
            }
        }
        public virtual Rigidbody2D m_rigidbody2D
        {
            get
            {
                return this.rigidbody2D;
            }
        }
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3.down * 0.3f));

        }
        void Start()
        {

        }

        void Update()
        {
            RaycastHit2D hitinfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.3f, Groundlayer);
            isGrounded = hitinfo.collider != null;

        }
        public virtual void Move(Vector2 newVelocity)
        {
            Vector2 velocity = rigidbody2D.linearVelocity;
            velocity.x = newVelocity.x;
            rigidbody2D.linearVelocity = velocity;
        }
        public virtual void Jump(float force)
        {
            rigidbody2D.AddForce(Vector2.up * force);
        }
        public void Flip()
        {

            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;


        }
    }
}
