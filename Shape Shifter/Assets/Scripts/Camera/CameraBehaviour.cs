using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowVertical : MonoBehaviour
{
    [SerializeField] Transform target;                        // the player
    [SerializeField, Range(0f, 1f)] float topEdge = 0.8f;     // move up when player is above 80% of the screen
    [SerializeField, Range(0f, 1f)] float bottomEdge = 0.2f;  // move down when below 20%
    [SerializeField, Range(0f, 1f)] float recenterAt = 0.35f; // where the player ends up on screen
    [SerializeField] float smoothTime = 0.3f;                 // how smooth the camera moves (s)

    Camera cam;
    float targetY;
    float velY;
    float minY;   // the camera never goes below where it started

    void Start()
    {
        cam = GetComponent<Camera>();
        targetY = transform.position.y;
        minY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Player's height on screen: 0 = bottom, 1 = top
        float screenY = cam.WorldToViewportPoint(target.position).y;

        // Near an edge: work out a new camera height
        if (screenY > topEdge || screenY < bottomEdge)
            targetY = transform.position.y + (screenY - recenterAt) * VisibleHeight();

        targetY = Mathf.Max(targetY, minY);

        Vector3 p = transform.position;
        p.y = Mathf.SmoothDamp(p.y, targetY, ref velY, smoothTime);
        transform.position = p;
    }

    // How many metres the screen shows from top to bottom
    float VisibleHeight()
    {
        if (cam.orthographic) return 2f * cam.orthographicSize;
        float depth = Vector3.Dot(target.position - transform.position, transform.forward);
        return 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }
}