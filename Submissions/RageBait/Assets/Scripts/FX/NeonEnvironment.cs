using UnityEngine;

public class NeonEnvironment : MonoBehaviour
{
    public static NeonEnvironment Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetupEnvironment();
    }

    void SetupEnvironment()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.01f, 0.06f);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.08f, 0.05f, 0.15f);
        RenderSettings.ambientEquatorColor = new Color(0.05f, 0.02f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.01f, 0.03f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.06f);
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 100f;
    }

    // Called by LevelManager to apply per-level color themes.
    public void ApplyTheme(Color fogColor, Color ambientColor)
    {
        RenderSettings.fogColor = fogColor;
        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientEquatorColor = ambientColor * 0.6f;
        RenderSettings.ambientGroundColor = ambientColor * 0.25f;

        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = fogColor;
    }
}
