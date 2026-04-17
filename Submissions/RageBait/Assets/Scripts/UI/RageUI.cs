using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RageUI : MonoBehaviour
{
    public static RageUI Instance { get; private set; }

    private TextMeshProUGUI deathStatsText;
    private Image rageMeterFill;
    private TextMeshProUGUI rageMeterLabel;
    private TextMeshProUGUI codeSnippetText;

    private int totalDeaths = 0;
    private float rageMeter = 0f;
    private float timePlayed = 0f;

    private static readonly string[] CodeSnippets = {
        "if (player.alive) {\n  rules.shuffle();\n}",
        "while (hope > 0) {\n  hope--;\n  difficulty++;\n}",
        "try {\n  win();\n} catch {\n  die(); laugh();\n}",
        "switch (sanity) {\n  case 0: ragequit();\n  default: suffer();\n}",
        "for (i=0; i<deaths; i++)\n  taunt(player);",
        "return deaths > 10\n  ? \"legendary\"\n  : \"pathetic\";",
        "if (checkpoint)\n  checkpoint = false;",
        "player.confidence =\n  max(0, conf - 1);",
        "// TODO: make fair\n// WONTFIX",
        "assert(skill > 0);\n// AssertionError"
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(BuildUINextFrame());
    }

    IEnumerator BuildUINextFrame()
    {
        yield return null;
        CreateUI();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDeathCountChanged += OnDeathCountChanged;
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDeathCountChanged -= OnDeathCountChanged;
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
        }
    }

    void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Transform old = canvas.transform.Find("RageUIPanel");
        if (old != null) Destroy(old.gameObject);

        GameObject panel = new GameObject("RageUIPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        CreateCodeSnippet(panel.transform);
        CreateRageMeter(panel.transform);
    }

    void CreateCodeSnippet(Transform parent)
    {
        GameObject obj = new GameObject("CodeSnippet");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(220, 70);
        rect.anchoredPosition = new Vector2(8, 8);

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.08f, 0.5f);

        codeSnippetText = new GameObject("Code").AddComponent<TextMeshProUGUI>();
        codeSnippetText.transform.SetParent(obj.transform, false);
        RectTransform tRect = codeSnippetText.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = new Vector2(-8, -4);
        tRect.anchoredPosition = Vector2.zero;
        codeSnippetText.fontSize = 11;
        codeSnippetText.alignment = TextAlignmentOptions.TopLeft;
        codeSnippetText.color = new Color(0.3f, 0.7f, 0.3f, 0.5f);
        codeSnippetText.fontStyle = FontStyles.Italic;
        codeSnippetText.text = CodeSnippets[0];
    }

    void CreateRageMeter(Transform parent)
    {
        GameObject container = new GameObject("RageMeter");
        container.transform.SetParent(parent, false);
        RectTransform cRect = container.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(1, 0.3f);
        cRect.anchorMax = new Vector2(1, 0.7f);
        cRect.pivot = new Vector2(1, 0.5f);
        cRect.sizeDelta = new Vector2(18, 0);
        cRect.anchoredPosition = new Vector2(-6, 0);

        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.03f, 0.03f, 0.5f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform, false);
        RectTransform fRect = fill.AddComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = new Vector2(1, 0);
        fRect.pivot = new Vector2(0.5f, 0);
        fRect.offsetMin = new Vector2(2, 2);
        fRect.offsetMax = new Vector2(-2, 0);
        rageMeterFill = fill.AddComponent<Image>();
        rageMeterFill.color = new Color(1f, 0.3f, 0f, 0.7f);

        GameObject label = new GameObject("Label");
        label.transform.SetParent(container.transform, false);
        RectTransform lRect = label.AddComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0, 1);
        lRect.anchorMax = new Vector2(1, 1);
        lRect.pivot = new Vector2(0.5f, 0);
        lRect.sizeDelta = new Vector2(40, 16);
        lRect.anchoredPosition = new Vector2(0, 4);
        rageMeterLabel = label.AddComponent<TextMeshProUGUI>();
        rageMeterLabel.fontSize = 9;
        rageMeterLabel.alignment = TextAlignmentOptions.Center;
        rageMeterLabel.color = new Color(1f, 0.4f, 0.2f, 0.7f);
        rageMeterLabel.text = "RAGE";
    }

    void OnDeathCountChanged(int count)
    {
        totalDeaths = count;
        rageMeter = Mathf.Clamp01(totalDeaths / 15f);
        UpdateCodeSnippet();
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing && totalDeaths == 0)
            timePlayed = 0f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            timePlayed += Time.deltaTime;
            UpdateRageMeter();
        }
    }

    void UpdateRageMeter()
    {
        if (rageMeterFill == null) return;
        RectTransform fRect = rageMeterFill.GetComponent<RectTransform>();
        float current = fRect.anchorMax.y;
        float smooth = Mathf.Lerp(current, rageMeter, Time.deltaTime * 2f);
        fRect.anchorMax = new Vector2(1, smooth);
        rageMeterFill.color = Color.Lerp(
            new Color(1f, 0.6f, 0f, 0.7f),
            new Color(1f, 0f, 0f, 0.9f),
            rageMeter
        );
    }

    void UpdateCodeSnippet()
    {
        if (codeSnippetText == null) return;
        int idx = Mathf.Min(totalDeaths, CodeSnippets.Length - 1);
        codeSnippetText.text = CodeSnippets[idx];
    }
}
