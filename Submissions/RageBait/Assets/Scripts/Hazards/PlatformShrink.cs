using UnityEngine;
using System.Collections;

public class PlatformShrink : MonoBehaviour
{
    public float shrinkDelay = 1f;
    public float shrinkDuration = 2f;
    public float minScale = 0.3f;
    public float regrowDelay = 4f;

    private Vector3 originalScale;
    private bool isShrinking = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnCollisionStay(Collision collision)
    {
        if (isShrinking) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        StartCoroutine(ShrinkSequence());
    }

    IEnumerator ShrinkSequence()
    {
        isShrinking = true;

        yield return new WaitForSeconds(shrinkDelay);

        if (RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage(
                "platform.scale *= 0.3f;",
                new Color(1f, 0.5f, 0f, 0.8f), 1f
            );

        float elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            float s = Mathf.Lerp(1f, minScale, t);
            transform.localScale = new Vector3(
                originalScale.x * s,
                originalScale.y,
                originalScale.z * s
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(regrowDelay);

        elapsed = 0f;
        while (elapsed < 1f)
        {
            float t = elapsed / 1f;
            float s = Mathf.Lerp(minScale, 1f, t);
            transform.localScale = new Vector3(
                originalScale.x * s,
                originalScale.y,
                originalScale.z * s
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        isShrinking = false;
    }
}
