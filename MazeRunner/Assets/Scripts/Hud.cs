using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Built at runtime by SceneBootstrap. Holds references to live UI elements so
// other systems (Pickup, Flashlight, AchievementSystem, GameManager) can push
// updates without doing UI plumbing themselves.
public class Hud : MonoBehaviour
{
    public static Hud Instance;

    Image batteryFill;
    TextMeshProUGUI pageText;
    TextMeshProUGUI scoreText;
    TextMeshProUGUI cornerText;
    TextMeshProUGUI noticeText;
    RectTransform pickupPing;
    Vector3 pickupPingBase;
    float pickupPingTime;

    Flashlight flashlight;

    struct ToastEntry { public string msg; public float ttl; public float life; }
    Queue<ToastEntry> toasts = new();
    ToastEntry? activeToast;
    float toastTimer;

    public static Hud Build(Canvas canvas)
    {
        if (Instance != null) return Instance;

        var go = new GameObject("Hud");
        go.transform.SetParent(canvas.transform, false);
        var h = go.AddComponent<Hud>();
        h.BuildElements(canvas.transform);
        Instance = h;
        return h;
    }

    static Sprite cachedWhite;
    public static Sprite WhitePixel()
    {
        if (cachedWhite != null) return cachedWhite;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        cachedWhite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        return cachedWhite;
    }

    void BuildElements(Transform parent)
    {
        // Battery bar (top-left)
        var batteryRoot = AddRect(parent, "BatteryBar",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -40), new Vector2(280, 22));
        var bg = batteryRoot.gameObject.AddComponent<Image>();
        bg.sprite = WhitePixel();
        bg.color = new Color(0, 0, 0, 0.55f);

        var fillRoot = AddRect(batteryRoot, "Fill",
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
            Vector2.zero, new Vector2(0, 0));
        fillRoot.anchorMin = new Vector2(0, 0);
        fillRoot.anchorMax = new Vector2(1, 1);
        fillRoot.offsetMin = new Vector2(2, 2);
        fillRoot.offsetMax = new Vector2(-2, -2);
        batteryFill = fillRoot.gameObject.AddComponent<Image>();
        batteryFill.sprite = WhitePixel();
        batteryFill.color = new Color(0.4f, 0.95f, 0.55f);
        batteryFill.type = Image.Type.Filled;
        batteryFill.fillMethod = Image.FillMethod.Horizontal;
        batteryFill.fillAmount = 1f;

        var batLabel = AddText(batteryRoot, "Label", "BATTERY", 14, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(280, 22));
        batLabel.alignment = TextAlignmentOptions.Center;

        // Pages (top-left below battery)
        var pageRt = AddRect(parent, "Pages",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -75), new Vector2(360, 32));
        pageText = pageRt.gameObject.AddComponent<TextMeshProUGUI>();
        pageText.fontSize = 22;
        pageText.color = new Color(0.95f, 0.92f, 0.78f);
        pageText.alignment = TextAlignmentOptions.Left;
        pageText.text = "Pages 0/0";

        // Score
        var scoreRt = AddRect(parent, "Score",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-40, -300), new Vector2(360, 32));
        scoreText = scoreRt.gameObject.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 22;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Right;

        // Corner info (bottom-right): difficulty + seed
        var cornerRt = AddRect(parent, "Corner",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-40, 30), new Vector2(360, 28));
        cornerText = cornerRt.gameObject.AddComponent<TextMeshProUGUI>();
        cornerText.fontSize = 16;
        cornerText.color = new Color(0.65f, 0.65f, 0.7f);
        cornerText.alignment = TextAlignmentOptions.Right;
        cornerText.text =
            $"{RunConfig.DifficultyLabel}    {RunConfig.SeedLabel}\n" +
            "[Esc/P] pause";

        // Notice / toast (centred bottom)
        var noticeRt = AddRect(parent, "Notice",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 140), new Vector2(900, 90));
        noticeText = noticeRt.gameObject.AddComponent<TextMeshProUGUI>();
        noticeText.fontSize = 26;
        noticeText.color = Color.white;
        noticeText.alignment = TextAlignmentOptions.Center;
        noticeText.richText = true;

        // Pickup ping (centre flash on collection)
        var pingRt = AddRect(parent, "PickupPing",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(36, 36));
        var img = pingRt.gameObject.AddComponent<Image>();
        img.sprite = WhitePixel();
        img.color = new Color(1f, 0.95f, 0.6f, 0f);
        img.raycastTarget = false;
        pickupPing = pingRt;
        pickupPingBase = pingRt.localScale;
    }

    static RectTransform AddRect(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
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

    static TextMeshProUGUI AddText(Transform parent, string name, string text,
        float size, Color color, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = AddRect(parent, name, anchorMin, anchorMax,
            new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.color = color;
        tmp.text = text;
        return tmp;
    }

    void Update()
    {
        if (flashlight == null) flashlight = Object.FindFirstObjectByType<Flashlight>();
        if (flashlight != null && batteryFill != null)
        {
            float p = Mathf.Clamp01(flashlight.GetBatteryPercent());
            batteryFill.fillAmount = p;
            batteryFill.color = Color.Lerp(new Color(1f, 0.2f, 0.15f),
                                           new Color(0.4f, 0.95f, 0.55f), p);
        }

        if (pageText != null)
            pageText.text = $"Pages  {ScoreSystem.pagesCollected}/{ScoreSystem.totalPages}";

        if (scoreText != null)
        {
            ScoreSystem.Compute(out _, out _, out _, out _);
            scoreText.text = $"{ScoreSystem.lastFinalScore:N0}";
        }

        // Pickup ping fade.
        if (pickupPing != null)
        {
            pickupPingTime -= Time.deltaTime;
            float a = Mathf.Clamp01(pickupPingTime / 0.5f);
            var img = pickupPing.GetComponent<Image>();
            if (img != null) { var c = img.color; c.a = a * 0.7f; img.color = c; }
            float scale = 1f + (1f - a) * 1.5f;
            pickupPing.localScale = pickupPingBase * scale;
        }

        // Toast queue.
        if (activeToast == null && toasts.Count > 0)
        {
            activeToast = toasts.Dequeue();
            toastTimer = activeToast.Value.ttl;
            noticeText.text = activeToast.Value.msg;
            var c = noticeText.color; c.a = 1f; noticeText.color = c;
        }
        else if (activeToast != null)
        {
            toastTimer -= Time.deltaTime;
            if (toastTimer <= 0f)
            {
                activeToast = null;
                noticeText.text = "";
            }
            else if (toastTimer < 0.6f)
            {
                var c = noticeText.color; c.a = toastTimer / 0.6f; noticeText.color = c;
            }
        }
    }

    public static void Toast(string message, float duration = 2.2f)
    {
        if (Instance == null) return;
        Instance.toasts.Enqueue(new ToastEntry { msg = message, ttl = duration });
    }

    public static void PingPickup()
    {
        if (Instance == null || Instance.pickupPing == null) return;
        Instance.pickupPingTime = 0.5f;
    }
}
