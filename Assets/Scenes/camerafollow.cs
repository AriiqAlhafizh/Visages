using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;       // Drag your Player object here
    public float smoothSpeed = 0.125f; // Higher = faster camera
    public Vector3 offset = new Vector3(0, 2, -10); // Adjust to position camera above player

    [Header("Boundaries (Optional)")]
    public bool limitCamera = false;
    public Vector2 minPosition; // Minimum X and Y
    public Vector2 maxPosition; // Maximum X and Y

    void LateUpdate()
    {
        if (target == null) return;

        // The position we want the camera to be at
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move from current position to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply boundaries if enabled
        if (limitCamera)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minPosition.x, maxPosition.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minPosition.y, maxPosition.y);
        }

        transform.position = smoothedPosition;
    }
}