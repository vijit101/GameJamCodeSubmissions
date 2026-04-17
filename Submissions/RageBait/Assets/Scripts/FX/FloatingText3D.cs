using UnityEngine;
using TMPro;

public class FloatingText3D : MonoBehaviour
{
    public string text = "if (true)";
    public Color color = new Color(0f, 1f, 0.5f, 0.3f);
    public float bobSpeed = 0.5f;
    public float bobAmount = 0.3f;
    public float rotateSpeed = 15f;
    public float fontSize = 3f;

    private Vector3 startPos;
    private TextMeshPro tmp;

    void Start()
    {
        startPos = transform.position;

        tmp = gameObject.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(10, 3);
    }

    void Update()
    {
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = pos;

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
