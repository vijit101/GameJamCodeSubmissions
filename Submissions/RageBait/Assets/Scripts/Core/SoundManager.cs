using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Player Sounds")]
    public AudioClip deathClip;
    public AudioClip healClip;
    public AudioClip bounceClip;
    public AudioClip hitClip;

    [Header("Game Sounds")]
    public AudioClip ruleChangeClip;
    public AudioClip fakePlatformVanishClip;

    [Header("UI Sounds")]
    public AudioClip uiSelectClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlayDeath()       => Play(deathClip);
    public void PlayHeal()        => Play(healClip);
    public void PlayBounce()      => Play(bounceClip);
    public void PlayHit()         => Play(hitClip);
    public void PlayRuleChange()  => Play(ruleChangeClip);
    public void PlayFakePlatformVanish() => Play(fakePlatformVanishClip);
    public void PlayUISelect()    => Play(uiSelectClip);

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}
