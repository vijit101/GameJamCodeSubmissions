using UnityEngine;
using System.Collections;

public class GravityZone : MonoBehaviour
{
    public float duration = 3f;

    private bool triggered = false;
    private Rigidbody targetRb;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        targetRb = other.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.useGravity = false;
            targetRb.AddForce(Vector3.up * 15f, ForceMode.Impulse);
        }

        if (RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage("GRAVITY FLIPPED\nif (gravity) { gravity = -gravity; }", new Color(0.8f, 0.2f, 1f), 2f);

        StartCoroutine(RestoreGravity());
    }

    IEnumerator RestoreGravity()
    {
        yield return new WaitForSeconds(duration);
        // Always restore gravity — even if player died
        if (targetRb != null) targetRb.useGravity = true;
        targetRb = null;
        triggered = false;
    }

    void OnDisable()
    {
        // Safety: if this zone is disabled/destroyed, restore gravity
        if (targetRb != null) targetRb.useGravity = true;
    }
}
