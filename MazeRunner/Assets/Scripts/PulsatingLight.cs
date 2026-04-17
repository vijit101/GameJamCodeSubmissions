using UnityEngine;

// Breathing red glow. Used for ritual circles and anything else that
// should pulse like a heartbeat under the floor.
public class PulsatingLight : MonoBehaviour
{
    public float speed = 2.0f;
    public float amplitude = 0.55f;
    public float phase;

    Light l;
    float baseIntensity;

    void Start()
    {
        l = GetComponent<Light>();
        if (l != null) baseIntensity = l.intensity;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (l == null) return;
        float s = Mathf.Sin(Time.time * speed + phase);
        l.intensity = baseIntensity * (1f - amplitude + amplitude * (0.5f + 0.5f * s));
    }
}
