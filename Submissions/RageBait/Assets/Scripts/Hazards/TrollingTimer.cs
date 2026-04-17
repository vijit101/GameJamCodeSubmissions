using UnityEngine;
using System.Collections;
using TMPro;

public class TrollingTimer : MonoBehaviour
{
    public static TrollingTimer Instance { get; private set; }

    private TextMeshProUGUI timerText;
    private float fakeTimer = 30f;
    private bool timerActive = false;
    private bool hasExploded = false;
    private bool uiCreated = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.5f);
        CreateTimerUI();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
            GameManager.Instance.OnDeathCountChanged += OnDeathCountChanged;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
            GameManager.Instance.OnDeathCountChanged -= OnDeathCountChanged;
        }
    }

    void CreateTimerUI()
    {
        if (uiCreated) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Transform existing = canvas.transform.Find("TrollingTimerPanel");
        if (existing != null) Destroy(existing.gameObject);

        GameObject panel = new GameObject("TrollingTimerPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(300, 40);
        rect.anchoredPosition = new Vector2(0, 80);

        timerText = panel.AddComponent<TextMeshProUGUI>();
        timerText.text = "";
        timerText.fontSize = 20;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        timerText.fontStyle = FontStyles.Bold;

        uiCreated = true;
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing && !timerActive)
        {
            timerActive = false;
            hasExploded = false;
        }
    }

    void OnDeathCountChanged(int count)
    {
        if (count >= 5 && !timerActive && !hasExploded)
        {
            fakeTimer = 30f;
            timerActive = true;
        }
    }

    void Update()
    {
        if (!timerActive) return;

        fakeTimer -= Time.deltaTime;

        if (timerText != null)
        {
            if (fakeTimer > 0)
            {
                timerText.text = "TIME: " + Mathf.CeilToInt(fakeTimer).ToString("00");
                if (fakeTimer < 10f)
                    timerText.color = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(Time.time * 3f, 1f));
                else
                    timerText.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            }
            else
            {
                hasExploded = true;
                timerActive = false;
                timerText.text = "TIME: 00";
                StartCoroutine(FakeExplosion());
            }
        }
    }

    IEnumerator FakeExplosion()
    {
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.5f, 0.3f);

        if (timerText != null)
            timerText.text = "BOOM!";

        yield return new WaitForSeconds(1f);

        if (timerText != null)
        {
            timerText.text = "jk. no timer lol";
            timerText.color = new Color(0.5f, 1f, 0.5f, 0.7f);
        }

        yield return new WaitForSeconds(3f);

        if (timerText != null) timerText.text = "";
    }
}
