using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CreatureDistortion : MonoBehaviour
{
    public Volume postProcessVolume;
    public Transform creature;
    public Transform player;
    public float maxDistance = 10f;

    private ChromaticAberration chromatic;
    private Vignette vignette;

    void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out chromatic);
            postProcessVolume.profile.TryGet(out vignette);
        }
    }

    void Update()
    {
        if (creature == null || player == null || !creature.gameObject.activeInHierarchy) return;

        float dist = Vector3.Distance(player.position, creature.position);
        float intensity = 1f - Mathf.Clamp01(dist / maxDistance);

        if (chromatic != null)
            chromatic.intensity.value = intensity * 0.8f;

        if (vignette != null)
            vignette.intensity.value = 0.35f + intensity * 0.35f;
    }
}
