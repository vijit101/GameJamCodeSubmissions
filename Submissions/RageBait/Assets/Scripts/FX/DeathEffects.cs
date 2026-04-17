using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DeathEffects : MonoBehaviour
{
    public static DeathEffects Instance { get; private set; }

    private Image glitchOverlay;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateOverlay();
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    void CreateOverlay()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject obj = new GameObject("GlitchOverlay");
        obj.transform.SetParent(canvas.transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        glitchOverlay = obj.AddComponent<Image>();
        glitchOverlay.color = new Color(1, 0, 0, 0);
        glitchOverlay.raycastTarget = false;
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Dead)
        {
            StartCoroutine(GlitchEffect());
            StartCoroutine(SlowMoEffect());
        }
        else if (state == GameState.Playing)
        {
            // ALWAYS restore timeScale on respawn
            Time.timeScale = 1f;
        }
    }

    IEnumerator GlitchEffect()
    {
        if (glitchOverlay == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float r = Random.Range(0.5f, 1f);
            float g = Random.Range(0f, 0.2f);
            float b = Random.Range(0f, 0.3f);
            float a = Random.Range(0.15f, 0.4f);
            glitchOverlay.color = new Color(r, g, b, a);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out
        elapsed = 0;
        Color c = glitchOverlay.color;
        while (elapsed < 0.2f)
        {
            c.a = Mathf.Lerp(0.3f, 0f, elapsed / 0.2f);
            glitchOverlay.color = c;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        glitchOverlay.color = new Color(1, 0, 0, 0);
    }

    IEnumerator SlowMoEffect()
    {
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.4f);
        Time.timeScale = 1f; // Always restore
    }
}
