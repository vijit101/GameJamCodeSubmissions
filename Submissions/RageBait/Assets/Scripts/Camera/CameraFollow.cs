using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public float smoothTime = 0.15f;
    public float lookAheadAmount = 3f;
    public float minY = 1f;

    private float currentLookAhead = 0f;
    private Rigidbody targetRb;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        FindTarget();
        transform.rotation = Quaternion.identity;
        SnapToTarget();
    }

    void FindTarget()
    {
        // Always auto-find the player if target is missing
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        // Auto-find if lost
        if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        // Cache rigidbody if needed
        if (targetRb == null && target != null)
            targetRb = target.GetComponent<Rigidbody>();

        // Lock rotation every frame
        transform.rotation = Quaternion.identity;

        // Look-ahead based on player velocity
        float targetLookAhead = 0f;
        if (target.gameObject.activeInHierarchy && targetRb != null)
        {
            if (Mathf.Abs(targetRb.velocity.x) > 0.5f)
                targetLookAhead = Mathf.Sign(targetRb.velocity.x) * lookAheadAmount;
        }
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, Time.deltaTime * 5f);

        // Desired position
        Vector3 desiredPos = target.position + offset;
        desiredPos.x += currentLookAhead;
        desiredPos.z = offset.z;
        if (desiredPos.y < minY) desiredPos.y = minY;

        // SmoothDamp — responsive camera
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);

        // Add screen shake offset
        ScreenShake shake = ScreenShake.Instance;
        if (shake != null)
            smoothed += shake.ShakeOffset;

        transform.position = smoothed;
    }

    public void SnapToTarget()
    {
        if (target == null) FindTarget();
        if (target == null) return;
        transform.rotation = Quaternion.identity;
        Vector3 pos = target.position + offset;
        pos.z = offset.z;
        if (pos.y < minY) pos.y = minY;
        transform.position = pos;
        currentLookAhead = 0f;
        velocity = Vector3.zero;
    }
}
