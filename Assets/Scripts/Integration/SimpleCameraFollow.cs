using UnityEngine;

namespace WWIII.SideScroller.Integration
{
    public class SimpleCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 1.5f, -10f);
        public float smoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = target.position + offset;
            desired.z = offset.z; // keep fixed z
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}

