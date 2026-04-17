using UnityEngine;

public class PlayerTrail : MonoBehaviour
{
    private TrailRenderer trail;

    void Start()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.0f;
        trail.startColor = new Color(1f, 0.4f, 0.1f, 0.6f);
        trail.endColor = new Color(1f, 0.2f, 0.0f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.minVertexDistance = 0.1f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
    }
}
