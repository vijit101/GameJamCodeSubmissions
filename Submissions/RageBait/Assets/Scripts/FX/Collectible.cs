using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int scoreValue = 10;

    private static int totalCollected = 0;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        totalCollected++;

        if (RageBaitMessages.Instance != null)
        {
            if (totalCollected % 5 == 0)
            {
                string[] msgs = {
                    $"coins_collected = {totalCollected};\n// they do nothing btw",
                    $"score += {scoreValue};\n// what score? lol",
                    "collectible.value = 0;\n// decorative only",
                    "// these coins are meaningless\n// like your progress",
                    $"cout << {totalCollected} << \" coins\" << endl;\n// waste of time"
                };
                RageBaitMessages.Instance.ShowMessage(
                    msgs[Random.Range(0, msgs.Length)],
                    new Color(1f, 0.84f, 0f, 0.8f), 1.5f
                );
            }
        }

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.05f, 0.05f);

        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.transform.SetParent(null);
            ps.Play();
            Destroy(ps.gameObject, 2f);
        }

        Destroy(gameObject);
    }
}
