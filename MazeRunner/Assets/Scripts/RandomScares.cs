using UnityEngine;

public class RandomScares : MonoBehaviour
{
    public AudioClip[] scareClips;
    public float minInterval = 25f;
    public float maxInterval = 60f;
    public float volume = 0.3f;

    private AudioSource audioSource;
    private float nextScareTime;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        ScheduleNext();
    }

    void Update()
    {
        if (Time.time >= nextScareTime)
        {
            PlayRandomScare();
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        nextScareTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void PlayRandomScare()
    {
        if (scareClips == null || scareClips.Length == 0) return;

        AudioClip clip = scareClips[Random.Range(0, scareClips.Length)];
        audioSource.panStereo = Random.Range(-1f, 1f);
        audioSource.PlayOneShot(clip, volume);
    }
}
