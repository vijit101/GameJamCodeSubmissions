using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the full Start scene UI at runtime so we don't need to author it in
// the scene file. Just place an empty GameObject with this component (which
// ProjectBootstrap does for you) — it builds the title, difficulty buttons,
// daily challenge, stats and quit.
public class StartMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    Canvas canvas;
    TextMeshProUGUI statsText;
    TextMeshProUGUI achText;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildUI();
        RefreshStats();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) AppExit.Quit();
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayWith(RunConfig.Difficulty.Easy, false);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayWith(RunConfig.Difficulty.Medium, false);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayWith(RunConfig.Difficulty.Hard, false);
        if (Input.GetKeyDown(KeyCode.D)) PlayWith(RunConfig.difficulty, true);
    }

    void BuildUI()
    {
        EnsureEventSystem();
        canvas = EnsureCanvas();

        // Clear any pre-existing children so a stale StartScene UI doesn't
        // double up with the runtime-built UI.
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Destroy(canvas.transform.GetChild(i).gameObject);

        var root = canvas.transform;

        // Solid black background.
        var bg = AddRect(root, "Background", Vector2.zero, Vector2.one,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        bg.offsetMin = Vector2.zero; bg.offsetMax = Vector2.zero;
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.sprite = Hud.WhitePixel();
        bgImg.color = Color.black;

        AddText(root, "Title", "MAZE RUNNER", 96, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(1400, 120));

        AddText(root, "Subtitle", "Find the exit. Before the dark finds you.",
            26, new Color(0.62f, 0.62f, 0.66f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -240), new Vector2(1400, 60));

        // Difficulty buttons.
        AddButton(root, "Easy",   new Vector2(-260, -70), () => PlayWith(RunConfig.Difficulty.Easy, false));
        AddButton(root, "Medium", new Vector2(   0, -70), () => PlayWith(RunConfig.Difficulty.Medium, false));
        AddButton(root, "Hard",   new Vector2( 260, -70), () => PlayWith(RunConfig.Difficulty.Hard, false));

        AddButton(root, "Daily Challenge", new Vector2(0, -160), () => PlayWith(RunConfig.difficulty, true), 380);

        AddButton(root, "Quit", new Vector2(0, -310), AppExit.Quit, 200);

        // Stats panel (left side).
        var statsRt = AddRect(root, "Stats", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0.5f), new Vector2(60, 0), new Vector2(420, 360));
        statsText = statsRt.gameObject.AddComponent<TextMeshProUGUI>();
        statsText.fontSize = 20;
        statsText.color = new Color(0.78f, 0.78f, 0.82f);
        statsText.alignment = TextAlignmentOptions.TopLeft;
        statsText.richText = true;

        // Achievements panel (right side).
        var achRt = AddRect(root, "Achievements", new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(1, 0.5f), new Vector2(-60, 0), new Vector2(460, 360));
        achText = achRt.gameObject.AddComponent<TextMeshProUGUI>();
        achText.fontSize = 18;
        achText.color = new Color(0.8f, 0.78f, 0.7f);
        achText.alignment = TextAlignmentOptions.TopRight;
        achText.richText = true;

        AddText(root, "Hint",
            "[1/2/3] difficulty   [D] daily challenge   [Esc] quit",
            18, new Color(0.45f, 0.45f, 0.5f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1200, 30));
    }

    void RefreshStats()
    {
        if (statsText != null)
        {
            string s = "<b>STATS</b>\n";
            s += $"\nRuns        {StatsTracker.Runs}";
            s += $"\nEscapes     {StatsTracker.Escapes}";
            s += $"\nDeaths      {StatsTracker.Deaths}";
            s += $"\nAll-pages   {StatsTracker.AllPagesRuns}";
            s += $"\n\nBest score  <b>{StatsTracker.BestScoreOverall:N0}</b>\n";
            foreach (RunConfig.Difficulty d in System.Enum.GetValues(typeof(RunConfig.Difficulty)))
            {
                float bt = StatsTracker.BestTime(d);
                int bs = StatsTracker.BestScore(d);
                string time = bt < float.MaxValue
                    ? $"{Mathf.FloorToInt(bt / 60f)}:{Mathf.FloorToInt(bt % 60f):00}"
                    : "—";
                s += $"\n{d,-7}  {time,-6}  {bs:N0}";
            }
            statsText.text = s;
        }

        if (achText != null)
        {
            string s = $"<b>ACHIEVEMENTS  {AchievementSystem.UnlockedCount()}/{AchievementSystem.TotalCount()}</b>\n";
            foreach (AchievementSystem.Id id in System.Enum.GetValues(typeof(AchievementSystem.Id)))
            {
                bool got = AchievementSystem.IsUnlocked(id);
                var info = AchievementSystem.Catalog[id];
                string mark = got ? "<color=#FFD86E>★</color>" : "<color=#404048>☆</color>";
                string title = got ? info.title : $"<color=#666>{info.title}</color>";
                s += $"\n{mark}  {title}";
            }
            achText.text = s;
        }
    }

    void PlayWith(RunConfig.Difficulty diff, bool daily)
    {
        RunConfig.difficulty = diff;
        RunConfig.dailyMode = daily;
        RunConfig.endlessLevel = 0;
        RunConfig.seed = daily ? RunConfig.TodaysSeed() : RunConfig.RandomSeed();
        LoadGameScene();
    }

    void LoadGameScene()
    {
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        { SceneManager.LoadScene(gameSceneName); return; }

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        { SceneManager.LoadScene(next); return; }

        foreach (var name in new[] { "GameScene", "SampleScene", "Main" })
        {
            if (Application.CanStreamedLevelBeLoaded(name))
            { SceneManager.LoadScene(name); return; }
        }

        Debug.LogError("[StartMenu] No game scene found in Build Settings.");
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    static Canvas EnsureCanvas()
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null) return existing;

        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.matchWidthOrHeight = 0.5f;
        return c;
    }

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
        UnityEngine.Events.UnityAction onClick, float width = 220f)
    {
        var rt = AddRect(parent, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), anchoredPos, new Vector2(width, 56));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Hud.WhitePixel();
        img.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors;
        cb.normalColor = new Color(0.16f, 0.16f, 0.18f, 1f);
        cb.highlightedColor = new Color(0.30f, 0.30f, 0.35f, 1f);
        cb.pressedColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        b.colors = cb;
        b.onClick.AddListener(onClick);

        var t = AddText(rt, "Label", label, 22, Color.white,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
    }
}
