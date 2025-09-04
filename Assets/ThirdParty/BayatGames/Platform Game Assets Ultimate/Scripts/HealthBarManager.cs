using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField]
        protected RectTransform uiCanvas;
        [SerializeField]
        protected GameObject[] characters;
        [SerializeField]
        protected GameObject healthBarPrefab;




        void Start()
        {

            HealthBar();
        }


        void Update()
        {

        }

        public virtual void HealthBar()
        {
            for (int i = 0; i < characters.Length; i++)
            {
                Health ChHealth = characters[i].GetComponent<Health>();
                GameObject go = Instantiate(healthBarPrefab, characters[i].transform.position, Quaternion.identity, uiCanvas);
                go.GetComponent<HealthBar>().Health = ChHealth;
                go.GetComponent<UIAnchor>().objectToFollow = characters[i].transform;


            }
        }

    }
}
