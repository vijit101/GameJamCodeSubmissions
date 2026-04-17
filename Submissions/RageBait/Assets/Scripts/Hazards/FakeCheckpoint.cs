using UnityEngine;

public class FakeCheckpoint : MonoBehaviour
{
    public bool isReallyAKillZone = true;
    private bool triggered = false;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(0f, 1f, 0.5f);
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", new Color(0f, 1f, 0.5f) * 1.5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (isReallyAKillZone)
        {
            if (RageBaitMessages.Instance != null)
                RageBaitMessages.Instance.ShowMessage(
                    "CHECKPOINT?\nif (checkpoint) { return SIKE; }",
                    new Color(1f, 0f, 0f), 2f
                );

            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.InstantKill();
        }
        else
        {
            if (RageBaitMessages.Instance != null)
                RageBaitMessages.Instance.ShowMessage(
                    "this one was real. enjoy the paranoia.",
                    new Color(0f, 1f, 0.5f), 2f
                );
        }

        triggered = false;
    }
}
