using TMPro;
using UnityEngine;
using UnityEngine.UI;

// In-game pause overlay. Esc or P toggles. Built at runtime by SceneBootstrap.
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    Canvas canvas;
    GameObject panel;
    CanvasGroup panelGroup;
    bool isPaused;

    CursorLockMode prevLock;
    bool prevVisible;

    public bool IsPaused => isPaused;

    public static PauseMenu Build(Canvas canvas)
    {
        if (Instance != null) return Instance;
        var go = new GameObject("PauseMenu");
        go.transform.SetParent(canvas.transform, false);
        var pm = go.AddComponent<PauseMenu>();
        pm.canvas = canvas;
        pm.BuildUI();
        Instance = pm;
        return pm;
    }

    void BuildUI()
    {
        // Full-screen dim panel.
        var rt = AddRect(transform, "PauseRoot",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        panel = rt.gameObject;

        var bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = Hud.WhitePixel();
        bg.color = new Color(0f, 0f, 0f, 0.72f);
        bg.raycastTarget = true; // swallow clicks behind it

        panelGroup = rt.gameObject.AddComponent<CanvasGroup>();
        panelGroup.alpha = 1f;

        // Title
        AddText(rt, "Title", "PAUSED", 80, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -220), new Vector2(1200, 120));

        AddText(rt, "Sub", "Take a breath.", 22, new Color(0.6f, 0.6f, 0.65f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -300), new Vector2(1200, 40));

        AddButton(rt, "Resume",    new Vector2(0,  20), Resume);
        AddButton(rt, "Restart",   new Vector2(0, -60), DoRestart);
        AddButton(rt, "Main Menu", new Vector2(0,-140), DoToMenu);
        AddButton(rt, "Quit",      new Vector2(0,-220), AppExit.Quit);

        AddText(rt, "Hint",
            "[Esc] or [P] to resume",
            18, new Color(0.45f, 0.45f, 0.5f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(800, 30));

        panel.SetActive(false);
    }

    void Update()
    {
        // Don't intercept input when the game is over; GameManager owns those keys.
        if (GameManager.Instance != null && GameManager.Instance.gameIsOver)
        {
            if (isPaused) Resume();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            Toggle();
    }

    public void Toggle() { if (isPaused) Resume(); else Pause(); }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        prevLock = Cursor.lockState;
        prevVisible = Cursor.visible;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        panel.SetActive(false);

        Cursor.lockState = prevLock;
        Cursor.visible = prevVisible;
    }

    void DoRestart()
    {
        Resume();
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    void DoToMenu()
    {
        Resume();
        if (GameManager.Instance != null) GameManager.Instance.ToStartMenu();
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    static RectTransform AddRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, float size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = AddRect(parent, name, anchorMin, anchorMax,
            new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static void AddButton(Transform parent, string label, Vector2 anchoredPos,
        UnityEngine.Events.UnityAction onClick, float width = 260f)
    {
        var rt = AddRect(parent, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), anchoredPos, new Vector2(width, 56));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Hud.WhitePixel();
        img.color = new Color(0.14f, 0.14f, 0.16f, 0.95f);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = new Color(0.18f, 0.18f, 0.20f, 1f);
        cb.highlightedColor = new Color(0.32f, 0.32f, 0.38f, 1f);
        cb.pressedColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        b.colors = cb;
        b.onClick.AddListener(onClick);

        var t = AddText(rt, "Label", label, 24, Color.white,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
    }
}
