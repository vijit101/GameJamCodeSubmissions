using UnityEngine;

// Heartbeat-style scale pulse. Use it on a fleshy sphere or blob.
public class PulseScale : MonoBehaviour
{
    public float speed = 1.2f;
    public float amount = 0.15f;

    Vector3 baseScale;
    float phase;

    void Start()
    {
        baseScale = transform.localScale;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float s = Mathf.Sin(Time.time * Mathf.PI * 2f * speed + phase);
        float b1 = Mathf.Pow(Mathf.Max(0f, s), 2f);
        transform.localScale = baseScale * (1f + amount * b1);
    }
}
