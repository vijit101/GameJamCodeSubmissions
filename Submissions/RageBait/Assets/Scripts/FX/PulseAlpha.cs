using UnityEngine;
using TMPro;

public class PulseAlpha : MonoBehaviour
{
    public float speed = 2f;
    private TextMeshProUGUI text;
    private Color baseColor;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        if (text != null) baseColor = text.color;
    }

    void Update()
    {
        if (text == null) return;
        float alpha = 0.4f + Mathf.Sin(Time.unscaledTime * speed) * 0.5f;
        text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
