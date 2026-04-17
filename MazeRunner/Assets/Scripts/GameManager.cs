using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("State")]
    public bool gameIsOver = false;

    [Header("References")]
    public Flashlight flashlight;
    public PlayerController playerController;

    [Header("UI Panels")]
    public CanvasGroup winPanel;
    public CanvasGroup losePanel;
    public TextMeshProUGUI winTimeText;
    public TextMeshProUGUI loseReasonText;

    [Header("Death by Darkness")]
    public float darknessGracePeriod = 5f;
    private float darknessTimer = 0f;

    private float gameStartTime;
    private float flashlightOffTimer = 0f;

    void Awake()
    {
        Instance = this;
        ScoreSystem.BeginRun();
        StatsTracker.RegisterRunStart();

        if (flashlight == null) flashlight = FindFirstObjectByType<Flashlight>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();

        EnsureUI();
    }

    void EnsureUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Sweep stale orphan UI children the scene may have accumulated from
        // past saves (LosePanel clones, floating LoseReason text, etc.).
        ReclaimOrCreate("WinPanel", canvas, ref winPanel);
        ReclaimOrCreate("LosePanel", canvas, ref losePanel);
        NukeStrays(canvas, new[] { "LoseReason", "LoseTitle" });

        if (winPanel != null)
            winTimeText = BuildEndPanel(winPanel, "YOU ESCAPED", showEndless: true);
        if (losePanel != null)
            loseReasonText = BuildEndPanel(losePanel, "YOU DIED", showEndless: false);

        if (winPanel != null) { winPanel.alpha = 0; winPanel.gameObject.SetActive(false); }
        if (losePanel != null) { losePanel.alpha = 0; losePanel.gameObject.SetActive(false); }
    }

    // Find the canvas-child with this name OR build a fresh full-screen panel.
    void ReclaimOrCreate(string name, Canvas canvas, ref CanvasGroup field)
    {
        if (field != null) return;
        var existing = canvas.transform.Find(name);
        if (existing == null)
        {
            // Also try a deep find in case it's nested.
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
                if (t.name == name) { existing = t; break; }
        }
        if (existing != null)
        {
            field = existing.GetComponent<CanvasGroup>();
            if (field == null) field = existing.gameObject.AddComponent<CanvasGroup>();
            // Make sure it fills the screen — earlier saves may have left it weird.
            var rt = existing as RectTransform ?? existing.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            // Ensure it has a dim Image background.
            var img = existing.GetComponent<Image>();
            if (img == null) img = existing.gameObject.AddComponent<Image>();
            if (img.sprite == null) img.sprite = Hud.WhitePixel();
            img.color = new Color(0f, 0f, 0f, 0.82f);
            img.raycastTarget = true;
        }
        else
        {
            field = CreateBlankPanel(canvas.transform, name);
        }
    }

    // Destroy any canvas-child with one of these names that isn't inside our panels.
    void NukeStrays(Canvas canvas, string[] names)
    {
        var doomed = new System.Collections.Generic.List<GameObject>();
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            foreach (var n in names)
            {
                if (t.name != n) continue;
                // Keep if inside our panels — BuildEndPanel will wipe those.
                if (winPanel != null && t.IsChildOf(winPanel.transform)) continue;
                if (losePanel != null && t.IsChildOf(losePanel.transform)) continue;
                doomed.Add(t.gameObject);
                break;
            }
        }
        foreach (var go in doomed) DestroyImmediate(go);
    }

    CanvasGroup CreateBlankPanel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = Hud.WhitePixel();
        img.color = new Color(0f, 0f, 0f, 0.82f);
        img.raycastTarget = true;
        return go.AddComponent<CanvasGroup>();
    }

    TextMeshProUGUI BuildEndPanel(CanvasGroup panel, string title, bool showEndless)
    {
        // Wipe any existing authored children so layout is ours.
        for (int i = panel.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(panel.transform.GetChild(i).gameObject);

        Color accent = title.StartsWith("YOU DIED")
            ? new Color(0.95f, 0.35f, 0.35f)
            : new Color(0.95f, 0.85f, 0.45f);

        AddPanelText(panel.transform, "Title", title, 96, accent,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(1400, 160));

        var body = AddPanelText(panel.transform, "Body", "", 26, new Color(0.88f, 0.88f, 0.92f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1100, 440));
        body.alignment = TextAlignmentOptions.Center;
        body.richText = true;

        // Button row at bottom.
        int count = showEndless ? 4 : 3;
        float width = 220f, spacing = 24f;
        float totalWidth = count * width + (count - 1) * spacing;
        float x = -totalWidth * 0.5f + width * 0.5f;
        float y = 180f;

        AddPanelButton(panel.transform, "Restart", new Vector2(x, y), RestartGame);
        x += width + spacing;
        if (showEndless)
        {
            AddPanelButton(panel.transform, "Next Floor", new Vector2(x, y), ContinueEndless);
            x += width + spacing;
        }
        AddPanelButton(panel.transform, "Main Menu", new Vector2(x, y), ToStartMenu);
        x += width + spacing;
        AddPanelButton(panel.transform, "Quit", new Vector2(x, y), AppExit.Quit);

        return body;
    }

    static TextMeshProUGUI AddPanelText(Transform parent, string name, string text, float size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static void AddPanelButton(Transform parent, string label, Vector2 anchoredPos,
        UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(220f, 60f);

        var img = go.GetComponent<Image>();
        img.sprite = Hud.WhitePixel();
        img.color = new Color(0.14f, 0.14f, 0.17f, 0.96f);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = new Color(0.18f, 0.18f, 0.21f, 1f);
        cb.highlightedColor = new Color(0.34f, 0.34f, 0.40f, 1f);
        cb.pressedColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    void Start()
    {
        gameStartTime = Time.time;
    }

    void Update()
    {
        if (gameIsOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) RestartGame();
            else if (Input.GetKeyDown(KeyCode.E)) ContinueEndless();
            else if (Input.GetKeyDown(KeyCode.Escape)) ToStartMenu();
            return;
        }

        // Track time with flashlight off (Brave Soul achievement).
        if (flashlight != null && !flashlight.IsFlashlightOn() && !flashlight.IsDead())
        {
            flashlightOffTimer += Time.deltaTime;
            if (flashlightOffTimer >= 60f)
                AchievementSystem.Unlock(AchievementSystem.Id.BraveSoul);
        }

        // Survivor: 5 minutes alive.
        if (Time.time - gameStartTime >= 300f)
            AchievementSystem.Unlock(AchievementSystem.Id.Survivor);

        if (flashlight != null && flashlight.IsDead())
        {
            darknessTimer += Time.deltaTime;
            if (darknessTimer >= darknessGracePeriod)
                Lose("The dark took you.");
        }
        else
        {
            darknessTimer = 0f;
        }
    }

    public void Win()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float timeTaken = Time.time - gameStartTime;
        int score = ScoreSystem.Compute(out int tBonus, out int pBonus, out int bBonus, out int comp);
        bool allPages = (ScoreSystem.totalPages > 0 &&
                         ScoreSystem.pagesCollected == ScoreSystem.totalPages);
        bool record = StatsTracker.RegisterWin(timeTaken, score, allPages);

        AchievementSystem.Unlock(AchievementSystem.Id.FirstEscape);
        if (timeTaken < 120f) AchievementSystem.Unlock(AchievementSystem.Id.SubTwoMinute);
        if (allPages) AchievementSystem.Unlock(AchievementSystem.Id.Collector);
        if (RunConfig.difficulty == RunConfig.Difficulty.Hard)
            AchievementSystem.Unlock(AchievementSystem.Id.HardEscape);
        if (RunConfig.dailyMode) AchievementSystem.Unlock(AchievementSystem.Id.DailyChallenge);
        if (RunConfig.endlessLevel >= 3) AchievementSystem.Unlock(AchievementSystem.Id.EndlessFloor3);
        if (StatsTracker.AllPagesRuns >= 5)
            AchievementSystem.Unlock(AchievementSystem.Id.AllPages5Times);

        int minutes = Mathf.FloorToInt(timeTaken / 60f);
        int seconds = Mathf.FloorToInt(timeTaken % 60f);
        float bestTime = StatsTracker.BestTime(RunConfig.difficulty);
        int bestScore = StatsTracker.BestScore(RunConfig.difficulty);

        string body = $"<size=110%>Escaped in {minutes}:{seconds:00}</size>";
        body += $"\n\nScore  <b>{score:N0}</b>";
        body += $"\n  Time bonus  {tBonus}";
        body += $"\n  Pages       {pBonus}  ({ScoreSystem.pagesCollected}/{ScoreSystem.totalPages})";
        body += $"\n  Batteries   {bBonus}";
        if (comp > 0) body += $"\n  Complete    +{comp}";
        body += $"\n\nBest on {RunConfig.DifficultyLabel}  {bestScore:N0}";
        if (bestTime < float.MaxValue)
        {
            int bm = Mathf.FloorToInt(bestTime / 60f);
            int bs = Mathf.FloorToInt(bestTime % 60f);
            body += $"\nBest time   {bm}:{bs:00}";
        }
        if (record) body += "\n\n<color=#FFD86E><b>NEW RECORD</b></color>";

        ShowEndPanel(isWin: true, titleText: "YOU ESCAPED", bodyText: body);

        if (playerController != null) playerController.enabled = false;
    }

    public void Lose(string reason)
    {
        if (gameIsOver) return;
        gameIsOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StatsTracker.RegisterDeath();

        float timeTaken = Time.time - gameStartTime;
        int minutes = Mathf.FloorToInt(timeTaken / 60f);
        int seconds = Mathf.FloorToInt(timeTaken % 60f);

        string body = $"<size=110%>{reason}</size>";
        body += $"\n\nSurvived  {minutes}:{seconds:00}";
        body += $"\nPages     {ScoreSystem.pagesCollected}/{ScoreSystem.totalPages}";

        ShowEndPanel(isWin: false, titleText: "YOU DIED", bodyText: body);

        if (playerController != null) playerController.enabled = false;
    }

    // Nuclear rebuild: at the moment of ending, tear down any stale panel and
    // replace it with a freshly-built one. No reliance on Awake state.
    void ShowEndPanel(bool isWin, string titleText, string bodyText)
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Wipe every Canvas child named like our panels so we end up with
        // exactly one of each.
        var doomed = new System.Collections.Generic.List<GameObject>();
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (n == "WinPanel" || n == "LosePanel" ||
                n == "LoseReason" || n == "LoseTitle" ||
                n == "WinTitle" || n == "WinTime")
            {
                // Only skip if it's a fresh panel we're currently building.
                doomed.Add(t.gameObject);
            }
        }
        foreach (var go in doomed) if (go != null) DestroyImmediate(go);

        // Fresh panel.
        var panel = CreateBlankPanel(canvas.transform, isWin ? "WinPanel" : "LosePanel");
        panel.transform.SetAsLastSibling();
        panel.gameObject.SetActive(true);
        panel.alpha = 1f; // show instantly, no fade
        panel.interactable = true;
        panel.blocksRaycasts = true;

        var body = BuildEndPanel(panel, titleText, showEndless: isWin);
        body.text = bodyText;
        body.richText = true;

        if (isWin) { winPanel = panel; winTimeText = body; }
        else       { losePanel = panel; loseReasonText = body; }
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        // Reroll seed so it's a fresh maze (unless daily — keep same).
        if (!RunConfig.dailyMode) RunConfig.seed = RunConfig.RandomSeed();
        RunConfig.endlessLevel = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueEndless()
    {
        Time.timeScale = 1;
        RunConfig.endlessLevel++;
        RunConfig.seed = RunConfig.RandomSeed();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToStartMenu()
    {
        Time.timeScale = 1;
        if (SceneManager.sceneCountInBuildSettings > 0)
            SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
