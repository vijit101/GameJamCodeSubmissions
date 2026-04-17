using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JumpScareZone : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(DoJumpScare());
    }

    IEnumerator DoJumpScare()
    {
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.4f, 0.5f);

        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject flash = null;
        if (canvas != null)
        {
            flash = new GameObject("JumpScareFlash");
            flash.transform.SetParent(canvas.transform, false);
            RectTransform rect = flash.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            Image img = flash.AddComponent<Image>();
            img.color = new Color(1f, 0f, 0f, 0.9f);
            img.raycastTarget = false;
        }

        if (RageBaitMessages.Instance != null)
        {
            string[] scares = {
                "BOO!",
                "BEHIND YOU!",
                "throw new JumpScareException();",
                "catch (Fear e) { /* too late */ }",
                "SYSTEM.PANIC()"
            };
            RageBaitMessages.Instance.ShowMessage(
                scares[Random.Range(0, scares.Length)],
                Color.white, 1.5f
            );
        }

        yield return new WaitForSeconds(0.15f);

        if (flash != null)
        {
            Image img = flash.GetComponent<Image>();
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(0.9f, 0f, elapsed / 0.3f);
                img.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(flash);
        }

        yield return new WaitForSeconds(5f);
        triggered = false;
    }
}
