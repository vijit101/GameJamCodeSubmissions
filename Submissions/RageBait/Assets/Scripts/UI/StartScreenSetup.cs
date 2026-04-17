using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartScreenSetup : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(SetupAfterFrame());
    }

    IEnumerator SetupAfterFrame()
    {
        yield return new WaitForSeconds(0.1f);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        Transform existingStart = canvas.transform.Find("StartScreen");
        if (existingStart != null) Destroy(existingStart.gameObject);

        GameObject startScreen = new GameObject("StartScreen");
        startScreen.transform.SetParent(canvas.transform, false);
        RectTransform startRect = startScreen.AddComponent<RectTransform>();
        startRect.anchorMin = Vector2.zero;
        startRect.anchorMax = Vector2.one;
        startRect.sizeDelta = Vector2.zero;

        // Much darker, more menacing background
        Image startBG = startScreen.AddComponent<Image>();
        startBG.color = new Color(0.02f, 0.02f, 0.05f, 0.98f);

        // Scanline overlay - thin horizontal dark bars across the full screen
        AddScanlineOverlay(startScreen.transform);

        TextMeshProUGUI titleTMP = MakeText(startScreen.transform, "Title", "ADAPT OR DIE", 56,
            new Vector2(700, 80), new Vector2(0, 140),
            TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.2f));
        titleTMP.fontStyle = FontStyles.Bold;

        MakeText(startScreen.transform, "Subtitle", "IF / ELSE SURVIVAL", 30,
            new Vector2(500, 40), new Vector2(0, 80),
            TextAlignmentOptions.Center, new Color(0f, 1f, 0.8f));

        string levelName = LevelManager.GetLevelName();
        MakeText(startScreen.transform, "LevelName", levelName, 24,
            new Vector2(500, 35), new Vector2(0, 40),
            TextAlignmentOptions.Center, new Color(1f, 0.84f, 0f));

        MakeText(startScreen.transform, "Tagline", "THE RULES WILL CHANGE.", 22,
            new Vector2(500, 30), new Vector2(0, 20),
            TextAlignmentOptions.Center, new Color(0.9f, 0.9f, 0.9f, 0.8f));

        MakeText(startScreen.transform, "CodeLine1",
            "if (rules == learned) { rules = changed; }", 14,
            new Vector2(450, 22), new Vector2(0, -10),
            TextAlignmentOptions.Center, new Color(0.3f, 0.8f, 0.3f, 0.5f));

        // Flashing WARNING lie above the tip box
        TextMeshProUGUI warningTMP = MakeText(startScreen.transform, "WarningFlash",
            "WARNING: THIS GAME IS FAIR", 16,
            new Vector2(440, 26), new Vector2(0, -30),
            TextAlignmentOptions.Center, new Color(1f, 0.9f, 0f));
        warningTMP.fontStyle = FontStyles.Bold;
        StartCoroutine(FlashText(warningTMP));

        // THE FAKE TIP - THIS IS THE LIE that sets players up to fail
        GameObject tipBox = new GameObject("FakeTipBox");
        tipBox.transform.SetParent(startScreen.transform, false);
        RectTransform tipRect = tipBox.AddComponent<RectTransform>();
        tipRect.sizeDelta = new Vector2(440, 48);
        tipRect.anchoredPosition = new Vector2(0, -55);
        Image tipBG = tipBox.AddComponent<Image>();
        tipBG.color = new Color(0.2f, 0.02f, 0.02f, 0.85f);

        // Red outline glow on the tip box
        Outline tipOutline = tipBox.AddComponent<Outline>();
        tipOutline.effectColor = new Color(1f, 0.05f, 0.05f, 0.9f);
        tipOutline.effectDistance = new Vector2(3f, 3f);

        MakeText(tipBox.transform, "FakeTip",
            "TIP: AVOID ALL HAZARDS. FIRE IS DEADLY.", 20,
            new Vector2(420, 38), Vector2.zero,
            TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.2f));
        // ^ THIS IS A LIE. Fire will become HEAL after first death. MAXIMUM RAGEBAIT.

        GameObject controlsBox = new GameObject("ControlsBox");
        controlsBox.transform.SetParent(startScreen.transform, false);
        RectTransform cbRect = controlsBox.AddComponent<RectTransform>();
        cbRect.sizeDelta = new Vector2(340, 110);
        cbRect.anchoredPosition = new Vector2(0, -115);
        Image cbBG = controlsBox.AddComponent<Image>();
        cbBG.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

        MakeText(controlsBox.transform, "Ctrl1", "A/D or Arrows = Move", 16,
            new Vector2(320, 22), new Vector2(0, 30),
            TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.9f));
        MakeText(controlsBox.transform, "Ctrl2", "SPACE = Jump", 16,
            new Vector2(320, 22), new Vector2(0, 5),
            TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.9f));
        MakeText(controlsBox.transform, "Ctrl3", "Survive. Reach the gold zone.", 14,
            new Vector2(320, 22), new Vector2(0, -20),
            TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.7f));
        MakeText(controlsBox.transform, "Warning", "Rules change when you die!", 14,
            new Vector2(320, 22), new Vector2(0, -42),
            TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));

        TextMeshProUGUI promptTMP = MakeText(startScreen.transform, "StartPrompt", "[ PRESS SPACE TO BEGIN ]", 26,
            new Vector2(500, 40), new Vector2(0, -220),
            TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.9f));

        UIManager uiMgr = FindObjectOfType<UIManager>();
        if (uiMgr != null)
            uiMgr.startScreen = startScreen;

        startScreen.SetActive(true);
        StartCoroutine(PulseText(promptTMP));
        StartCoroutine(GlitchTitle(titleTMP));
    }

    void AddScanlineOverlay(Transform parent)
    {
        GameObject scanlines = new GameObject("ScanlineOverlay");
        scanlines.transform.SetParent(parent, false);
        RectTransform scanRect = scanlines.AddComponent<RectTransform>();
        scanRect.anchorMin = Vector2.zero;
        scanRect.anchorMax = Vector2.one;
        scanRect.sizeDelta = Vector2.zero;

        // Use a vertical layout group of thin dark bars to simulate scanlines
        VerticalLayoutGroup vlg = scanlines.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Add enough bars to cover a typical screen height (720+ px worth)
        for (int i = 0; i < 200; i++)
        {
            GameObject bar = new GameObject("Bar" + i);
            bar.transform.SetParent(scanlines.transform, false);
            RectTransform barRect = bar.AddComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(0, 1f);
            Image barImg = bar.AddComponent<Image>();
            barImg.color = new Color(0f, 0f, 0f, 0.18f);
        }

        // Scanlines sit on top but don't block raycasts
        Image scanImg = scanlines.AddComponent<Image>();
        scanImg.color = new Color(0f, 0f, 0f, 0f);
        scanImg.raycastTarget = false;
    }

    // Glitch effect: random small X offset, occasional cyan color flash
    IEnumerator GlitchTitle(TextMeshProUGUI title)
    {
        Vector2 basePos = title.rectTransform.anchoredPosition;
        Color baseColor = title.color;
        Color glitchColor = new Color(0f, 1f, 1f); // cyan glitch

        while (title != null)
        {
            float offsetX = Random.Range(-3f, 3f);
            title.rectTransform.anchoredPosition = new Vector2(basePos.x + offsetX, basePos.y);

            // Occasionally flash to glitch cyan color
            if (Random.value < 0.08f)
            {
                title.color = glitchColor;
                yield return new WaitForSeconds(0.05f);
                title.color = baseColor;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    // Flash WARNING text between visible and invisible
    IEnumerator FlashText(TextMeshProUGUI text)
    {
        while (text != null)
        {
            text.enabled = true;
            yield return new WaitForSeconds(0.6f);
            text.enabled = false;
            yield return new WaitForSeconds(0.4f);
        }
    }

    // Faster, more urgent pulse speed
    IEnumerator PulseText(TextMeshProUGUI text)
    {
        while (text != null)
        {
            float a = 0.5f + Mathf.Sin(Time.time * 5f) * 0.45f;
            text.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
    }

    TextMeshProUGUI MakeText(Transform parent, string name, string text, float size,
        Vector2 dims, Vector2 pos, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = dims;
        rect.anchoredPosition = pos;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        return tmp;
    }
}
