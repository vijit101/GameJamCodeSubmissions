using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.33f;
    public float crouchStepInterval = 0.85f;
    public float volume = 0.4f;
    public float crouchVolumeMul = 0.35f;

    private AudioSource audioSource;
    private PlayerController playerController;
    private float stepTimer;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null || !playerController.IsMoving()) return;

        float interval =
            playerController.IsCrouching() ? crouchStepInterval :
            playerController.IsSprinting() ? sprintStepInterval :
            walkStepInterval;

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            stepTimer = interval;
            PlayFootstep();
            EmitNoise();
        }
    }

    void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(0.92f, 1.08f);
        float vol = volume * (playerController.IsCrouching() ? crouchVolumeMul : 1f);
        audioSource.PlayOneShot(clip, vol);
    }

    void EmitNoise()
    {
        float r = playerController.NoiseRadius();
        if (r > 0f) NoiseSystem.Emit(transform.position, r);
    }
}
