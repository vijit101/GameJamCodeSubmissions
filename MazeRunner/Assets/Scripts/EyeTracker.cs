using UnityEngine;

// Pupil that follows the player. Used for giant wall-eyes that make
// corridors feel watched.
public class EyeTracker : MonoBehaviour
{
    public Transform pupil;
    public float maxOffset = 0.18f;

    Transform player;
    Vector3 pupilBaseLocal;

    void Start()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
        if (pupil != null) pupilBaseLocal = pupil.localPosition;
    }

    void LateUpdate()
    {
        if (player == null || pupil == null) return;

        // Project player position into the eye's local space.
        Vector3 local = transform.InverseTransformPoint(player.position);
        Vector2 flat = new Vector2(local.x, local.y);
        if (flat.sqrMagnitude > 0.0001f) flat.Normalize();
        flat *= maxOffset;

        pupil.localPosition = pupilBaseLocal + new Vector3(flat.x, flat.y, -0.01f);
    }
}
