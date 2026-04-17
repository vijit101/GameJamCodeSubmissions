using UnityEngine;

// Slow horror-movie sway. Perfect for anything dangling from a chain.
public class SwayAnimator : MonoBehaviour
{
    public float speed = 0.6f;
    public float pitchDegrees = 3f;
    public float rollDegrees = 2f;

    float phase;

    void Start()
    {
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float a = Mathf.Sin(Time.time * speed + phase) * pitchDegrees;
        float b = Mathf.Cos(Time.time * speed * 0.7f + phase) * rollDegrees;
        transform.localRotation = Quaternion.Euler(a, 0f, b);
    }
}
