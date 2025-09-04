using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BayatGames
{
    public class Bow : MonoBehaviour
    {
        [SerializeField]
        protected Transform arrowSpawnPoint;
        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected GameObject ArrowPrefab;
        [SerializeField]
        protected float fireForce = 400f;
        [SerializeField]
        protected float maxFireForce = 1f;
        [SerializeField]
        protected float currentFireForce = 0f;


        protected bool canShot = false;

        void Start()
        {

        }


        void Update()
        {
            Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;
            diff.Normalize();

            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            this.transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

            if (Input.GetButtonDown("Attack"))
            {
                canShot = true;
                currentFireForce = 0f;
            }

            if (Input.GetButton("Attack"))
            {
                if (currentFireForce < maxFireForce)
                {
                    currentFireForce += 0.02f;
                    if (currentFireForce > maxFireForce)
                    {
                        currentFireForce = maxFireForce;
                    }
                }
            }
            Vector2 fire = (fireForce * currentFireForce) * diff;
            if (Input.GetButtonUp("Attack"))
            {
                FireArrow(fire);
                canShot = false;
                currentFireForce = 0f;
            }
            //Debug.Log(fire);
            animator.SetFloat("BowPower", currentFireForce);
        }
        public virtual void FireArrow(Vector2 force)
        {
            GameObject Arrow = Instantiate<GameObject>(ArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Rigidbody2D rigidbody = Arrow.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(force);
        }
    }
}
