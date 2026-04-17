using UnityEngine;
using System.Collections;

public class SpeedTrap : MonoBehaviour
{
    public float speedMultiplier = 3f;
    public float duration = 2f;
    public bool isSlowTrap = false;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
            StartCoroutine(ApplySpeedTrap(pc));
    }

    IEnumerator ApplySpeedTrap(PlayerController pc)
    {
        float originalSpeed = pc.moveSpeed;

        if (isSlowTrap)
        {
            pc.moveSpeed = originalSpeed * 0.3f;
            if (RageBaitMessages.Instance != null)
                RageBaitMessages.Instance.ShowMessage(
                    "SPEED = 0.3f;\n// good luck",
                    new Color(0.5f, 0.5f, 1f), 1.5f
                );
        }
        else
        {
            pc.moveSpeed = originalSpeed * speedMultiplier;
            if (RageBaitMessages.Instance != null)
                RageBaitMessages.Instance.ShowMessage(
                    "SPEED *= 3;\n// try not to overshoot",
                    new Color(1f, 0.6f, 0f), 1.5f
                );
        }

        yield return new WaitForSeconds(duration);

        pc.moveSpeed = originalSpeed;
        triggered = false;
    }
}
