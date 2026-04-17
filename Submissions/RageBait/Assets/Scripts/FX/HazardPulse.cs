using UnityEngine;

public class HazardPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.03f;    // Very subtle scale pulse
    public float floatSpeed = 1.5f;
    public float floatAmount = 0.04f;    // Barely visible hover
    public float rotateSpeed = 12f;       // Slow gentle rotation

    private Vector3 startPos;
    private Vector3 startScale;
    private float randomOffset;

    void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float t = Time.time + randomOffset;

        // Subtle breathing scale
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * pulse;

        // Very subtle float — objects stay grounded
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(t * floatSpeed) * floatAmount;
        transform.position = pos;

        // Gentle rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}
