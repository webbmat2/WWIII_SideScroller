using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class Sword : MonoBehaviour
    {
        [SerializeField]
        protected float demage = 20;
        [SerializeField]
        protected float hitRange = 1f;
        [SerializeField]
        protected Transform hitRayPosition;


        public float HitRange
        {
            get
            {
                return this.hitRange;
            }
        }
        public float Demage
        {
            get
            {
                return this.demage;
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hitRayPosition.position, hitRayPosition.position + (Vector3.right * hitRange));
        }

    }
}
