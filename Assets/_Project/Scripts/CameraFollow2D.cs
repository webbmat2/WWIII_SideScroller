using UnityEngine;

[AddComponentMenu("Cameras/Camera Follow 2D")]
[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")] [SerializeField] Transform target;
    [Header("Camera Offset")] [SerializeField] Vector3 offset = new Vector3(0f, 1f, -10f);
    [Header("Follow Smoothing")] [SerializeField, Min(0.01f)] float smoothTime = 0.18f;
    [Header("Bounds (optional)")] [SerializeField] BoxCollider2D worldBounds;

    Vector3 vel; Camera cam;

    void Awake() { cam = GetComponent<Camera>(); if (cam) cam.orthographic = true; }

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + offset; desired.z = offset.z;
        Vector3 next = Vector3.SmoothDamp(transform.position, desired, ref vel, smoothTime);

        if (cam && worldBounds)
        {
            var b = worldBounds.bounds;
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            next.x = Mathf.Clamp(next.x, b.min.x + halfW, b.max.x - halfW);
            next.y = Mathf.Clamp(next.y, b.min.y + halfH, b.max.y - halfH);
        }
        transform.position = next;
    }
}