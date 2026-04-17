using UnityEngine;
using System.Collections;

public class FakePlatform : MonoBehaviour
{
    public float fallDelay = 0.5f;
    public float respawnDelay = 3f;
    public float shakeAmount = 0.05f;

    private Vector3 originalPos;
    private bool isFalling = false;
    private Collider col;
    private Renderer rend;

    void Start()
    {
        originalPos = transform.position;
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isFalling) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallSequence());
        }
    }

    IEnumerator FallSequence()
    {
        isFalling = true;

        // Shake warning
        float elapsed = 0;
        while (elapsed < fallDelay)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fall!
        if (col != null) col.enabled = false;
        if (rend != null)
        {
            Color c = rend.material.color;
            float fade = 0;
            Vector3 fallPos = originalPos;
            while (fade < 1f)
            {
                fade += Time.deltaTime * 2f;
                fallPos.y -= Time.deltaTime * 8f;
                transform.position = fallPos;
                c.a = 1f - fade;
                rend.material.color = c;
                yield return null;
            }
        }

        // Hide
        gameObject.SetActive(false);

        // Respawn after delay
        yield return new WaitForSeconds(respawnDelay);

        transform.position = originalPos;
        if (col != null) col.enabled = true;
        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = 1f;
            rend.material.color = c;
        }
        gameObject.SetActive(true);
        isFalling = false;
    }
}
