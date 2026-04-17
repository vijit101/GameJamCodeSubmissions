using UnityEngine;
using System.Collections;

public class TeleportTrap : MonoBehaviour
{
    public Vector3 teleportOffset = new Vector3(-20f, 0f, 0f);
    public bool teleportToStart = false;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(DoTeleport(other.transform));
    }

    IEnumerator DoTeleport(Transform player)
    {
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.3f, 0.3f);

        if (RageBaitMessages.Instance != null)
        {
            string[] msgs = {
                "TELEPORTED!\nplayer.position = startPosition;",
                "goto START;",
                "while (progress > 0) { progress--; }",
                "rm -rf /your/progress",
                "git reset --hard HEAD~10"
            };
            RageBaitMessages.Instance.ShowMessage(
                msgs[Random.Range(0, msgs.Length)],
                new Color(0.6f, 0f, 1f), 2f
            );
        }

        yield return new WaitForSeconds(0.1f);

        if (teleportToStart)
        {
            Vector3 spawnPos = Vector3.zero;
            if (GameManager.Instance != null && GameManager.Instance.playerSpawnPoint != null)
                spawnPos = GameManager.Instance.playerSpawnPoint.position;
            else
                spawnPos = new Vector3(0, 2, 0);
            player.position = spawnPos;
        }
        else
        {
            player.position += teleportOffset;
        }

        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null) cam.SnapToTarget();

        yield return new WaitForSeconds(3f);
        triggered = false;
    }
}
