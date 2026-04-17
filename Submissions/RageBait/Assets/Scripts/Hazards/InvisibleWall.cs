using UnityEngine;
using System.Collections;

public class InvisibleWall : MonoBehaviour
{
    public float revealDuration = 1.5f;
    public float tauntDelay = 0.3f;

    private Renderer rend;
    private bool revealed = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = 0f;
            rend.material.color = c;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (revealed) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        revealed = true;
        StartCoroutine(RevealAndTaunt());
    }

    IEnumerator RevealAndTaunt()
    {
        yield return new WaitForSeconds(tauntDelay);

        if (RageBaitMessages.Instance != null)
        {
            string[] taunts = {
                "if (wall.visible == false) { wall.solid = true; }",
                "INVISIBLE WALL lmao",
                "you really thought that path was clear?",
                "Error: PathBlocked. Reason: LOL",
                "the wall was always there. you just couldn't see it."
            };
            RageBaitMessages.Instance.ShowMessage(
                taunts[Random.Range(0, taunts.Length)],
                new Color(0.8f, 0.4f, 1f), 2f
            );
        }

        if (rend != null)
        {
            float elapsed = 0f;
            Color c = rend.material.color;
            while (elapsed < revealDuration)
            {
                float a = Mathf.PingPong(elapsed * 4f, 0.6f) + 0.2f;
                c.a = a;
                rend.material.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            c.a = 0f;
            rend.material.color = c;
            revealed = false;
        }
    }
}
