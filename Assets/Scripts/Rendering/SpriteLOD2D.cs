using UnityEngine;

namespace WWIII.SideScroller.Rendering
{
    public class SpriteLOD2D : MonoBehaviour
    {
        [Tooltip("Camera to measure distance from; defaults to main camera.")]
        public Camera targetCamera;

        [Tooltip("High detail renderer (close distance).")]
        public Renderer highDetail;

        [Tooltip("Low detail renderer (far distance).")]
        public Renderer lowDetail;

        [Tooltip("Distance threshold to switch to low detail.")]
        public float lodDistance = 12f;

        private void Reset()
        {
            targetCamera = Camera.main;
            var rends = GetComponentsInChildren<Renderer>(true);
            if (rends.Length >= 2)
            {
                highDetail = rends[0];
                lowDetail = rends[1];
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null || highDetail == null || lowDetail == null) return;

            var d = Vector3.Distance(targetCamera.transform.position, transform.position);
            bool useLow = d > lodDistance;
            if (lowDetail.gameObject.activeSelf != useLow)
                lowDetail.gameObject.SetActive(useLow);
            if (highDetail.gameObject.activeSelf == useLow)
                highDetail.gameObject.SetActive(!useLow);
        }
    }
}

