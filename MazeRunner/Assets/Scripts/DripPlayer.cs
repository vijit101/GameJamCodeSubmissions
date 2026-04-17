using UnityEngine;

// Fires an occasional water-drip AudioSource with a bit of pitch variation.
public class DripPlayer : MonoBehaviour
{
    public float minInterval = 3f;
    public float maxInterval = 11f;

    AudioSource src;
    float nextDripTime;

    void Start()
    {
        src = GetComponent<AudioSource>();
        ScheduleNext(startDelay: Random.Range(0f, 4f));
    }

    void Update()
    {
        if (src == null || src.clip == null) return;
        if (Time.time >= nextDripTime)
        {
            src.pitch = Random.Range(0.85f, 1.15f);
            src.PlayOneShot(src.clip, Random.Range(0.6f, 1f));
            ScheduleNext();
        }
    }

    void ScheduleNext(float startDelay = 0f)
    {
        nextDripTime = Time.time + startDelay + Random.Range(minInterval, maxInterval);
    }
}
