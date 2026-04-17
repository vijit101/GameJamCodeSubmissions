using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WinScreenEnhancer : MonoBehaviour
{
    public static WinScreenEnhancer Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Won)
            StartCoroutine(ShowEpicWinScreen());
    }

    IEnumerator ShowEpicWinScreen()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        int deaths = GameManager.Instance != null ? GameManager.Instance.DeathCount : 0;

        GameObject overlay = new GameObject("WinOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform oRect = overlay.AddComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero;
        oRect.anchorMax = Vector2.one;
        oRect.sizeDelta = Vector2.zero;
        Image bg = overlay.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            bg.color = new Color(0f, 0f, 0f, Mathf.Lerp(0, 0.9f, elapsed));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        CreateWinText(overlay.transform, "YOU ADAPTED.", 60, new Vector2(0, 150),
            new Color(0f, 1f, 0.5f), FontStyles.Bold);

        yield return new WaitForSecondsRealtime(0.5f);

        CreateWinText(overlay.transform, "if (player.adapted) {\n  return \"respect\";\n}",
            24, new Vector2(0, 60), new Color(0.4f, 0.8f, 0.4f), FontStyles.Italic);

        yield return new WaitForSecondsRealtime(0.5f);

        string rating;
        Color ratingColor;
        if (deaths == 0)
        {
            rating = "RATING: IMPOSSIBLE\n// are you cheating?";
            ratingColor = new Color(1f, 0.84f, 0f);
        }
        else if (deaths <= 3)
        {
            rating = "RATING: ELITE\n// skill_level = MAX_INT";
            ratingColor = new Color(0f, 1f, 1f);
        }
        else if (deaths <= 10)
        {
            rating = "RATING: DECENT\n// not bad... not great";
            ratingColor = new Color(0.5f, 1f, 0.5f);
        }
        else if (deaths <= 20)
        {
            rating = "RATING: STUBBORN\n// persistence != skill";
            ratingColor = new Color(1f, 0.6f, 0f);
        }
        else
        {
            rating = $"RATING: LEGENDARY BAD\n// {deaths} deaths. new record?";
            ratingColor = new Color(1f, 0.2f, 0.2f);
        }

        CreateWinText(overlay.transform, rating, 28, new Vector2(0, -30), ratingColor, FontStyles.Normal);

        yield return new WaitForSecondsRealtime(0.5f);

        CreateWinText(overlay.transform, $"Total Deaths: {deaths}", 22, new Vector2(0, -120),
            new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal);

        yield return new WaitForSecondsRealtime(1f);

        CreateWinText(overlay.transform, "[ PRESS SPACE TO SUFFER AGAIN ]", 24, new Vector2(0, -200),
            new Color(1f, 1f, 1f, 0.8f), FontStyles.Normal);

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        GameManager.Instance.RestartGame();
    }

    void CreateWinText(Transform parent, string text, float size, Vector2 pos, Color color, FontStyles style)
    {
        GameObject obj = new GameObject("WinText");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(800, 100);
        rect.anchoredPosition = pos;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.fontStyle = style;
    }
}
