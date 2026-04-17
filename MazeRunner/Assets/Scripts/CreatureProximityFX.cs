using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Player feedback that scales with creature proximity:
//   - heartbeat audio rises in volume + pitch as it gets close
//   - subtle camera roll shake (Z rotation only — doesn't fight head bob)
//   - chromatic aberration + vignette intensify when chased
public class CreatureProximityFX : MonoBehaviour
{
    public CreatureAI creature;
    public Transform cameraTransform;
    public AudioSource heartbeatSource;
    public Volume volume;

    [Header("Tuning")]
    public float maxAudibleDistance = 14f;
    public float maxShakeDistance = 6f;
    public float maxShakeRollDegrees = 1.5f;
    public float chaseFovBoost = 8f;

    Camera cam;
    float baseFov;
    ChromaticAberration chroma;
    Vignette vignette;
    float baseVignetteIntensity;

    void Start()
    {
        if (cameraTransform != null) cam = cameraTransform.GetComponent<Camera>();
        if (cam != null) baseFov = cam.fieldOfView;

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out chroma);
            volume.profile.TryGet(out vignette);
            if (vignette != null) baseVignetteIntensity = vignette.intensity.value;
        }
    }

    void LateUpdate()
    {
        bool gameOver = GameManager.Instance != null && GameManager.Instance.gameIsOver;
        if (creature == null || !creature.IsActive || gameOver)
        {
            DampShake();
            SetHeartbeat(0f);
            SetPostFx(0f);
            if (cam != null) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov, Time.unscaledDeltaTime * 6f);
            return;
        }

        float dist = creature.DistanceToPlayer;
        float closeness = 1f - Mathf.Clamp01(dist / maxAudibleDistance);

        SetHeartbeat(closeness);

        // Camera roll shake — only Z rotation, leaves head bob and look intact.
        float shakeCloseness = 1f - Mathf.Clamp01(dist / maxShakeDistance);
        if (creature.IsChasing) shakeCloseness = Mathf.Min(1f, shakeCloseness + 0.25f);
        if (cameraTransform != null && shakeCloseness > 0.01f)
        {
            float t = Time.time * 22f;
            float roll = (Mathf.PerlinNoise(t, 0.31f) - 0.5f) * 2f * maxShakeRollDegrees * shakeCloseness;
            var e = cameraTransform.localEulerAngles;
            cameraTransform.localEulerAngles = new Vector3(e.x, e.y, roll);
        }
        else DampShake();

        // FOV pulse during a chase — primal "predator close" cue.
        if (cam != null)
        {
            float targetFov = baseFov + (creature.IsChasing ? chaseFovBoost * shakeCloseness : 0f);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 4f);
        }

        SetPostFx(shakeCloseness);
    }

    void SetHeartbeat(float closeness)
    {
        if (heartbeatSource == null) return;
        heartbeatSource.volume = Mathf.Lerp(0f, 0.65f, closeness);
        heartbeatSource.pitch = Mathf.Lerp(0.85f, 1.55f, closeness);
        if (closeness > 0.05f && !heartbeatSource.isPlaying) heartbeatSource.Play();
        if (closeness <= 0.02f && heartbeatSource.isPlaying) heartbeatSource.Stop();
    }

    void SetPostFx(float intensity)
    {
        if (chroma != null) chroma.intensity.value = Mathf.Lerp(chroma.intensity.value, intensity * 0.7f, Time.deltaTime * 5f);
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value,
                baseVignetteIntensity + intensity * 0.25f, Time.deltaTime * 5f);
    }

    void DampShake()
    {
        if (cameraTransform == null) return;
        var e = cameraTransform.localEulerAngles;
        float z = e.z > 180f ? e.z - 360f : e.z;
        z = Mathf.Lerp(z, 0f, Time.deltaTime * 8f);
        cameraTransform.localEulerAngles = new Vector3(e.x, e.y, z);
    }
}
