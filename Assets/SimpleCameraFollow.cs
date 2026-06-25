using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(1.8f, 1.15f, -10f);
    [SerializeField] private float smoothTime = 0.16f;
    [SerializeField] private Vector2 minBounds = new Vector2(-7.5f, -1.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(17.2f, 2.6f);

    private Transform target;
    private Vector3 velocity;
    private Rigidbody2D targetBody;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        targetBody = followTarget != null ? followTarget.GetComponent<Rigidbody2D>() : null;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float lookAhead = targetBody != null ? Mathf.Clamp(targetBody.linearVelocity.x * 0.18f, -0.65f, 0.9f) : 0f;
        Vector3 desiredPosition = target.position + offset + new Vector3(lookAhead, 0f, 0f);
        desiredPosition.z = offset.z;
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
