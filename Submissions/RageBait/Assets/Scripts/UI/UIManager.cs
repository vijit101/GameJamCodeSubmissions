using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References (auto-created)")]
    public Image healthBarFill;
    public TextMeshProUGUI deathCountText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI fireRuleText;
    public TextMeshProUGUI spikeRuleText;
    public TextMeshProUGUI enemyRuleText;
    public GameObject startScreen;
    public GameObject winScreen;
    public TextMeshProUGUI winDeathCountText;
    public GameObject deathFlash;

    private List<Image> heartIcons = new List<Image>();
    private Canvas mainCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitUI());
    }

    IEnumerator InitUI()
    {
        yield return null;

        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null) yield break;

        CleanupOldUI();
        BuildHUD();
        BuildDeathFlash();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            GameManager.Instance.OnDeathCountChanged += UpdateDeathCount;
        }
        if (RuleEngine.Instance != null)
        {
            RuleEngine.Instance.OnRulesChanged += UpdateRuleHints;
            UpdateRuleHints();
        }
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
            UpdatePhase(0);
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    void CleanupOldUI()
    {
        string[] oldNames = { "HUD_Panel", "HealthPanel", "DeathFlashPanel", "OldHUD" };
        foreach (string n in oldNames)
        {
            Transform old = mainCanvas.transform.Find(n);
            if (old != null) Destroy(old.gameObject);
        }
    }

    void BuildHUD()
    {
        GameObject hud = new GameObject("HUD_Panel");
        hud.transform.SetParent(mainCanvas.transform, false);
        RectTransform hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.sizeDelta = Vector2.zero;

        BuildHealthBar(hud.transform);
        BuildDeathCounter(hud.transform);
        BuildRuleDisplay(hud.transform);
        BuildPhaseDisplay(hud.transform);
    }

    void BuildHealthBar(Transform parent)
    {
        GameObject container = new GameObject("HealthContainer");
        container.transform.SetParent(parent, false);
        RectTransform cRect = container.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(0, 1);
        cRect.pivot = new Vector2(0, 1);
        cRect.sizeDelta = new Vector2(200, 32);
        cRect.anchoredPosition = new Vector2(12, -12);

        Image containerBg = container.AddComponent<Image>();
        containerBg.color = new Color(0, 0, 0, 0.4f);

        GameObject label = new GameObject("HPLabel");
        label.transform.SetParent(container.transform, false);
        RectTransform lRect = label.AddComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0, 0);
        lRect.anchorMax = new Vector2(0, 1);
        lRect.pivot = new Vector2(0, 0.5f);
        lRect.sizeDelta = new Vector2(36, 0);
        lRect.anchoredPosition = new Vector2(4, 0);
        TextMeshProUGUI lText = label.AddComponent<TextMeshProUGUI>();
        lText.text = "HP";
        lText.fontSize = 14;
        lText.fontStyle = FontStyles.Bold;
        lText.color = new Color(1f, 0.3f, 0.3f);
        lText.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject barBg = new GameObject("BarBG");
        barBg.transform.SetParent(container.transform, false);
        RectTransform bgRect = barBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = new Vector2(38, 6);
        bgRect.offsetMax = new Vector2(-6, -6);
        Image bgImg = barBg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.02f, 0.02f, 0.8f);

        GameObject fill = new GameObject("BarFill");
        fill.transform.SetParent(barBg.transform, false);
        RectTransform fRect = fill.AddComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.offsetMin = new Vector2(2, 2);
        fRect.offsetMax = new Vector2(-2, -2);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(1f, 0.2f, 0.15f);
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillAmount = 1f;
    }

    void BuildDeathCounter(Transform parent)
    {
        GameObject obj = new GameObject("DeathCounter");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(160, 28);
        rect.anchoredPosition = new Vector2(-12, -12);

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.4f);

        deathCountText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        deathCountText.transform.SetParent(obj.transform, false);
        RectTransform tRect = deathCountText.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = new Vector2(8, 0);
        tRect.offsetMax = new Vector2(-8, 0);
        deathCountText.text = "DEATHS: 0";
        deathCountText.fontSize = 16;
        deathCountText.fontStyle = FontStyles.Bold;
        deathCountText.color = new Color(1f, 0.4f, 0.2f);
        deathCountText.alignment = TextAlignmentOptions.MidlineRight;
    }

    void BuildRuleDisplay(Transform parent)
    {
        GameObject container = new GameObject("RuleDisplay");
        container.transform.SetParent(parent, false);
        RectTransform cRect = container.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(0, 1);
        cRect.pivot = new Vector2(0, 1);
        cRect.sizeDelta = new Vector2(180, 80);
        cRect.anchoredPosition = new Vector2(12, -50);

        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.35f);

        GameObject header = new GameObject("Header");
        header.transform.SetParent(container.transform, false);
        RectTransform hRect = header.AddComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0, 1);
        hRect.anchorMax = new Vector2(1, 1);
        hRect.pivot = new Vector2(0.5f, 1);
        hRect.sizeDelta = new Vector2(0, 18);
        hRect.anchoredPosition = new Vector2(0, -2);
        TextMeshProUGUI hText = header.AddComponent<TextMeshProUGUI>();
        hText.text = "// CURRENT RULES";
        hText.fontSize = 10;
        hText.color = new Color(0.4f, 0.7f, 0.4f, 0.6f);
        hText.alignment = TextAlignmentOptions.Top;
        hText.fontStyle = FontStyles.Italic;

        fireRuleText = MakeRuleText(container.transform, "FireRule", 22);
        spikeRuleText = MakeRuleText(container.transform, "SpikeRule", 40);
        enemyRuleText = MakeRuleText(container.transform, "EnemyRule", 58);
    }

    TextMeshProUGUI MakeRuleText(Transform parent, string name, float yOff)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(0, 16);
        rect.anchoredPosition = new Vector2(8, -yOff);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 12;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        return tmp;
    }

    void BuildPhaseDisplay(Transform parent)
    {
        GameObject container = new GameObject("PhaseDisplay");
        container.transform.SetParent(parent, false);
        RectTransform cRect = container.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 1);
        cRect.anchorMax = new Vector2(0.5f, 1);
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.sizeDelta = new Vector2(300, 24);
        cRect.anchoredPosition = new Vector2(0, -12);

        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.3f);

        GameObject textObj = new GameObject("PhaseText");
        textObj.transform.SetParent(container.transform, false);
        RectTransform tRect = textObj.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        phaseText = textObj.AddComponent<TextMeshProUGUI>();
        phaseText.fontSize = 13;
        phaseText.alignment = TextAlignmentOptions.Center;
        phaseText.color = new Color(1f, 1f, 1f, 0.9f);
        phaseText.fontStyle = FontStyles.Bold;
        phaseText.text = LevelManager.GetLevelName();
    }

    void BuildDeathFlash()
    {
        deathFlash = new GameObject("DeathFlashPanel");
        deathFlash.transform.SetParent(mainCanvas.transform, false);
        RectTransform rect = deathFlash.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        Image img = deathFlash.AddComponent<Image>();
        img.color = new Color(1f, 0f, 0f, 0f);
        img.raycastTarget = false;
        deathFlash.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnDeathCountChanged -= UpdateDeathCount;
        }
        if (RuleEngine.Instance != null)
            RuleEngine.Instance.OnRulesChanged -= UpdateRuleHints;
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhase;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBarFill != null)
        {
            float target = (float)current / max;
            healthBarFill.fillAmount = target;
            healthBarFill.color = Color.Lerp(
                new Color(1f, 0.1f, 0.05f),
                new Color(0.1f, 1f, 0.3f),
                target
            );
        }
    }

    public void UpdateDeathCount(int count)
    {
        if (deathCountText != null)
            deathCountText.text = "DEATHS: " + count;
    }

    public void UpdatePhase(int phase)
    {
        if (phaseText != null)
            phaseText.text = PhaseManager.GetPhaseName(phase);
    }

    public void UpdateRuleHints()
    {
        if (RuleEngine.Instance == null) return;
        var rules = RuleEngine.Instance.GetAllRules();

        if (fireRuleText != null)
        {
            HazardBehavior b = rules[HazardType.Fire];
            fireRuleText.text = "FIRE → " + b.ToString().ToUpper();
            fireRuleText.color = GetBehaviorColor(b);
        }
        if (spikeRuleText != null)
        {
            HazardBehavior b = rules[HazardType.Spike];
            spikeRuleText.text = "SPIKE → " + b.ToString().ToUpper();
            spikeRuleText.color = GetBehaviorColor(b);
        }
        if (enemyRuleText != null)
        {
            HazardBehavior b = rules[HazardType.Enemy];
            enemyRuleText.text = "ENEMY → " + b.ToString().ToUpper();
            enemyRuleText.color = GetBehaviorColor(b);
        }
    }

    private Color GetBehaviorColor(HazardBehavior behavior)
    {
        switch (behavior)
        {
            case HazardBehavior.Kill: return new Color(1f, 0.3f, 0.2f);
            case HazardBehavior.Heal: return new Color(0.2f, 1f, 0.4f);
            case HazardBehavior.Bounce: return new Color(1f, 1f, 0.2f);
            default: return Color.white;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                if (startScreen != null) startScreen.SetActive(true);
                if (winScreen != null) winScreen.SetActive(false);
                break;
            case GameState.Playing:
                if (startScreen != null) startScreen.SetActive(false);
                if (winScreen != null) winScreen.SetActive(false);
                break;
            case GameState.Dead:
                StartCoroutine(FlashDeath());
                break;
            case GameState.Won:
                ShowWinScreen();
                break;
        }
    }

    private IEnumerator FlashDeath()
    {
        if (deathFlash == null) yield break;
        deathFlash.SetActive(true);
        Image img = deathFlash.GetComponent<Image>();
        if (img != null)
        {
            Color c = new Color(1f, 0f, 0f, 0.5f);
            img.color = c;
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                c.a = Mathf.Lerp(0.5f, 0f, elapsed / 0.4f);
                img.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            c.a = 0f;
            img.color = c;
        }
        deathFlash.SetActive(false);
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            if (winDeathCountText != null)
                winDeathCountText.text = "Deaths: " + GameManager.Instance.DeathCount;
        }
    }

    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }
}
