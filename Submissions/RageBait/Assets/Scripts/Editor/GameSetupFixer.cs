using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class GameSetupFixer : EditorWindow
{
    [MenuItem("Tools/Fix Game Setup and Polish Visuals")]
    public static void FixEverything()
    {
        FixPlatformColliders();
        FixCamera();
        FixLighting();
        FixMaterialEmissions();
        FixRenderSettings();
        AttachFXComponents();
        SetupRageManagers();

        if (!EditorApplication.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("[GameSetupFixer] All fixes applied and scene saved! Press Play.");
    }

    static void SetupRageManagers()
    {
        EnsureManager<RageBaitMessages>("RageBaitMessages");
        EnsureManager<DeathEffects>("DeathEffects");
        EnsureManager<ControlReverser>("ControlReverser");
        EnsureManager<TrollingTimer>("TrollingTimer");
        EnsureManager<RageUI>("RageUI");
        EnsureManager<WinScreenEnhancer>("WinScreenEnhancer");

        GameObject neon = EnsureManager<NeonEnvironment>("NeonEnvironment");
        if (neon.GetComponent<ParticleBackground>() == null)
            neon.AddComponent<ParticleBackground>();

        EnsureManager<PostProcessSetup>("PostProcessSetup");
        EnsureManager<LevelManager>("LevelManager");

        Debug.Log("[Fix] Rage managers set up: Messages, Effects, Timer, RageUI, LevelManager, WinEnhancer");
    }

    static GameObject EnsureManager<T>(string name) where T : Component
    {
        T existing = Object.FindObjectOfType<T>();
        if (existing != null) return existing.gameObject;

        GameObject obj = new GameObject(name);
        obj.AddComponent<T>();
        EditorUtility.SetDirty(obj);
        return obj;
    }

    static void AttachFXComponents()
    {
        // Add PlayerTrail to Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerTrail>() == null)
        {
            player.AddComponent<PlayerTrail>();
            EditorUtility.SetDirty(player);
        }

        // Add HazardPulse + HazardGlow to all hazards
        string[] hazardNames = {
            "Hazard_Fire_1", "Hazard_Fire_2", "Hazard_Fire_3",
            "Hazard_Spike_1", "Hazard_Spike_2", "Hazard_Spike_3",
            "Hazard_Enemy_1", "Hazard_Enemy_2"
        };
        foreach (string name in hazardNames)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) continue;
            if (go.GetComponent<HazardPulse>() == null) go.AddComponent<HazardPulse>();
            if (go.GetComponent<HazardGlow>() == null) go.AddComponent<HazardGlow>();
            EditorUtility.SetDirty(go);
        }

        // Add EndZoneBeacon to EndZone
        GameObject endZone = GameObject.Find("EndZone");
        if (endZone != null && endZone.GetComponent<EndZoneBeacon>() == null)
        {
            endZone.AddComponent<EndZoneBeacon>();
            EditorUtility.SetDirty(endZone);
        }

        // Add point light to player for glow
        if (player != null && player.GetComponentInChildren<Light>() == null)
        {
            GameObject playerGlow = new GameObject("PlayerGlow");
            playerGlow.transform.SetParent(player.transform, false);
            Light pl = playerGlow.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1f, 0.5f, 0.1f);
            pl.intensity = 2f;
            pl.range = 5f;
            EditorUtility.SetDirty(player);
        }

        Debug.Log("[Fix] FX components attached: PlayerTrail, HazardPulse, HazardGlow, EndZoneBeacon, PlayerGlow");
    }

    static void FixPlatformColliders()
    {
        string[] platformNames = {
            "Platform_Start", "Platform_2", "Platform_3", "Platform_4",
            "Platform_5", "Platform_6", "Platform_7", "Platform_8", "Platform_End"
        };

        foreach (string name in platformNames)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) continue;

            // Ensure BoxCollider exists and is NOT a trigger
            BoxCollider bc = go.GetComponent<BoxCollider>();
            if (bc == null) bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = false;

            EditorUtility.SetDirty(go);
        }
        Debug.Log("[Fix] Platform colliders fixed - all solid BoxColliders");
    }

    static void FixCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.65f, 0.85f, 1f);
        cam.fieldOfView = 60f;
        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[Fix] Camera: dark background, FOV 65");
    }

    static void FixLighting()
    {
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.type != LightType.Directional) continue;
            l.color = new Color(1f, 0.95f, 0.85f);
            l.intensity = 1.8f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.8f;
            l.transform.rotation = Quaternion.Euler(45, -30, 0);
            EditorUtility.SetDirty(l.gameObject);
        }
        Debug.Log("[Fix] Directional light: warm, dramatic shadows");
    }

    static void FixMaterialEmissions()
    {
        // Player - bright cyan glow
        SetEmission("Assets/Materials/MAT_Player.mat", new Color(1f, 0.5f, 0.1f) * 2f);

        // Hazards - bright red glow
        SetEmission("Assets/Materials/MAT_Fire.mat", new Color(1f, 0.15f, 0.05f) * 2.5f);
        SetEmission("Assets/Materials/MAT_Spike.mat", new Color(1f, 0.1f, 0.1f) * 2.5f);
        SetEmission("Assets/Materials/MAT_Enemy.mat", new Color(0.9f, 0.05f, 0.05f) * 2.5f);

        // End zone - super bright gold
        SetEmission("Assets/Materials/MAT_EndZone.mat", new Color(1f, 0.84f, 0f) * 4f);

        // State materials
        SetEmission("Assets/Materials/MAT_Kill.mat", new Color(1f, 0f, 0f) * 3f);
        SetEmission("Assets/Materials/MAT_Heal.mat", new Color(0f, 1f, 0.3f) * 3f);
        SetEmission("Assets/Materials/MAT_Bounce.mat", new Color(1f, 1f, 0f) * 3f);

        // Make platforms slightly reflective
        Material platMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MAT_Platform.mat");
        if (platMat != null)
        {
            platMat.SetFloat("_Metallic", 0.4f);
            platMat.SetFloat("_Glossiness", 0.7f);
            platMat.SetColor("_Color", new Color(0.15f, 0.15f, 0.22f));
            EditorUtility.SetDirty(platMat);
        }
        Material platDarkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MAT_Platform_Dark.mat");
        if (platDarkMat != null)
        {
            platDarkMat.SetFloat("_Metallic", 0.45f);
            platDarkMat.SetFloat("_Glossiness", 0.75f);
            platDarkMat.SetColor("_Color", new Color(0.08f, 0.08f, 0.12f));
            EditorUtility.SetDirty(platDarkMat);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Fix] All material emissions enabled + bright glows");
    }

    static void SetEmission(string path, Color emissionColor)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) return;
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mat.SetColor("_EmissionColor", emissionColor);
        EditorUtility.SetDirty(mat);
    }

    static void FixRenderSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.7f, 0.8f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.5f, 0.55f, 0.6f);
        RenderSettings.ambientGroundColor = new Color(0.3f, 0.25f, 0.2f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.6f, 0.75f, 0.9f);
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 200f;
        RenderSettings.skybox = null;
        Debug.Log("[Fix] Render settings: bright ambient, linear fog");
    }
}
