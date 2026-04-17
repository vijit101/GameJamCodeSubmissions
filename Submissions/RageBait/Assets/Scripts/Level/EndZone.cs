using UnityEngine;
using System.Collections;

public class EndZone : MonoBehaviour
{
    public ParticleSystem celebrationParticles;
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(LevelCompleteSequence(other.gameObject));
    }

    IEnumerator LevelCompleteSequence(GameObject player)
    {
        // Disable player control during transition
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.DisableControl();

        if (celebrationParticles != null)
            celebrationParticles.Play();

        if (LevelManager.CurrentLevel < LevelManager.MaxLevels)
        {
            // Show level complete message
            if (RageBaitMessages.Instance != null)
            {
                string[] completeMsgs = {
                    "LEVEL " + LevelManager.CurrentLevel + " COMPLETE!\nif (relief) { nextLevel(); }",
                    "LEVEL " + LevelManager.CurrentLevel + " CLEARED!\n// don't get comfortable",
                    "LEVEL " + LevelManager.CurrentLevel + " DONE!\nreturn nextLevel(); // it gets worse"
                };
                RageBaitMessages.Instance.ShowMessage(
                    completeMsgs[Random.Range(0, completeMsgs.Length)],
                    new Color(0f, 1f, 0.5f), 2.5f);
            }

            // Let the player see the message before transitioning
            yield return new WaitForSeconds(2.5f);

            if (LevelManager.Instance != null)
                LevelManager.Instance.CompleteLevel();
        }
        else
        {
            // Final level — show win
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerWin();
        }
    }
}
