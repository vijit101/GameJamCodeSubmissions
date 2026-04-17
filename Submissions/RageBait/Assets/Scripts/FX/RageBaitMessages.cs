using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class RageBaitMessages : MonoBehaviour
{
    public static RageBaitMessages Instance { get; private set; }

    private TextMeshProUGUI messageText;
    private TextMeshProUGUI subtitleText;
    private CanvasGroup messageGroup;

    private static readonly string[] DeathTaunts = new string[]
    {
        "WRONG ASSUMPTION",
        "YOU TRUSTED TOO MUCH",
        "IF FAILED",
        "TRY AGAIN",
        "skill issue",
        "the rules changed btw ;)",
        "you trusted the wrong hazard",
        "IF (you != good) { die(); }",
        "ELSE { still die lmao }",
        "try { survive(); } catch { nope(); }",
        "return FAILURE;",
        "// TODO: get better",
        "Exception: PlayerSkillNotFound",
        "Error 404: Skill not found",
        "while(true) { die(); }",
        "nice death streak!",
        "the hazards are laughing at you",
        "have you considered... not dying?",
        "rage quit yet?",
        "your keyboard ok?",
        "that spike says hi",
        "fire goes brrr",
        "you know the rules... and so do I",
        "plot twist: everything kills you",
        "trust nothing.",
        "the floor is... actually fine this time",
        "ELSE IF (patience > 0) { keepTrying(); }",
        "segfault: your pride",
        "git commit -m 'another death'",
        "npm install skill --save",
        "sudo rm -rf /your/confidence"
    };

    private static readonly string[] RuleChangeMessages = new string[]
    {
        "RULES UPDATED. Good luck.",
        "Surprise! New rules.",
        "IF conditions changed...",
        "The rules just flipped.",
        "Nothing is what it seems now.",
        "ELSE branch activated.",
        "Hazards reclassified.",
        "Trust recalibrated to zero.",
        "Your assumptions are wrong now.",
        "switch(rules) { case CHAOS: break; }"
    };

    private static readonly string[] PhaseMessages = new string[]
    {
        "Phase 1: Learn the rules.\nThey won't last.",
        "Phase 2: One rule changed.\nPay attention.",
        "Phase 3: Multiple rules shifting.\nAdapt or die.",
        "Phase 4: CHAOS MODE.\nEverything you know is wrong."
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateUI();
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
        if (RuleEngine.Instance != null)
            RuleEngine.Instance.OnRulesChanged += OnRulesChanged;
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
        if (RuleEngine.Instance != null)
            RuleEngine.Instance.OnRulesChanged -= OnRulesChanged;
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    void CreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject msgObj = new GameObject("RageMessage");
        msgObj.transform.SetParent(canvas.transform, false);
        RectTransform rect = msgObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(800, 100);
        rect.anchoredPosition = new Vector2(0, -50);
        messageGroup = msgObj.AddComponent<CanvasGroup>();
        messageGroup.alpha = 0;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(msgObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 36;
        messageText.color = Color.white;

        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(msgObj.transform, false);
        RectTransform subRect = subObj.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0, 0);
        subRect.anchorMax = new Vector2(1, 0);
        subRect.sizeDelta = new Vector2(0, 30);
        subRect.anchoredPosition = new Vector2(0, -40);
        subtitleText = subObj.AddComponent<TextMeshProUGUI>();
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontSize = 18;
        subtitleText.color = new Color(0.7f, 0.7f, 0.7f);
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Dead)
        {
            string taunt = DeathTaunts[Random.Range(0, DeathTaunts.Length)];
            ShowMessage(taunt, new Color(1f, 0.3f, 0.3f), 2f);
        }
    }

    void OnRulesChanged()
    {
        if (GameManager.Instance != null && GameManager.Instance.DeathCount > 0)
        {
            string msg = RuleChangeMessages[Random.Range(0, RuleChangeMessages.Length)];
            if (subtitleText != null)
                subtitleText.text = msg;
        }
    }

    void OnPhaseChanged(int phase)
    {
        if (phase >= 0 && phase < PhaseMessages.Length)
        {
            ShowMessage(PhaseMessages[phase], new Color(1f, 0.84f, 0f), 3f);
        }
    }

    public void ShowMessage(string text, Color color, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine(text, color, duration));
    }

    private IEnumerator ShowMessageRoutine(string text, Color color, float duration)
    {
        if (messageText == null || messageGroup == null) yield break;

        messageText.text = text;
        messageText.color = color;

        // Fade in
        float t = 0;
        while (t < 0.3f)
        {
            messageGroup.alpha = t / 0.3f;
            t += Time.deltaTime;
            yield return null;
        }
        messageGroup.alpha = 1;

        yield return new WaitForSeconds(duration);

        // Fade out
        t = 0;
        while (t < 0.5f)
        {
            messageGroup.alpha = 1f - (t / 0.5f);
            t += Time.deltaTime;
            yield return null;
        }
        messageGroup.alpha = 0;
        if (subtitleText != null) subtitleText.text = "";
    }
}
