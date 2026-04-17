using UnityEngine;

public class EndZoneBeacon : MonoBehaviour
{
    public float rotateSpeed = 45f;
    public float pulseSpeed = 1f;

    private Vector3 startScale;
    private Light pointLight;

    void Start()
    {
        startScale = transform.localScale;

        // Add a point light for glow effect
        pointLight = gameObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.84f, 0f);
        pointLight.intensity = 3f;
        pointLight.range = 8f;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
        transform.localScale = startScale * pulse;

        if (pointLight != null)
            pointLight.intensity = 2f + Mathf.Sin(Time.time * pulseSpeed * 2f) * 1f;
    }
}
