using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Runs on every scene load.
// Wires up anything the designer forgot in the editor so the game
// works end-to-end with just the default SampleScene.
public static class SceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Run();
    }

    static void Run()
    {
        // Only do the heavy wiring in the game scene (where MazeGenerator lives).
        if (Object.FindFirstObjectByType<MazeGenerator>() == null) return;

        SetupFog();
        DimDirectionalLights();
        SetupPlayerAudio();
        SetupAmbientAudio();
        EnsureCanvasAndHud();
        SetupPostProcessing();
        SpawnCreature();
        ShowIntro();
    }

    static void ShowIntro()
    {
        Hud.Toast(
            "<size=130%>Find the exit.</size>\n" +
            "WASD move   Shift sprint   Ctrl crouch (silent)\n" +
            "F flashlight   RMB glance behind",
            5f);
    }

    static void EnsureCanvasAndHud()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("Canvas",
                typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        EnsureCrosshair(canvas);
        Hud.Build(canvas);
        PauseMenu.Build(canvas);
    }

    static void SpawnCreature()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null) return;

        // Don't double-spawn.
        var existing = Object.FindFirstObjectByType<CreatureAI>();
        GameObject creature;
        if (existing != null) creature = existing.gameObject;
        else
        {
            creature = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            creature.name = "Creature";
            creature.transform.localScale = new Vector3(0.55f, 1f, 0.55f);
            // Start hidden somewhere outside camera range; CreatureAI will reposition.
            creature.transform.position = new Vector3(-1000, 1, -1000);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.04f, 0.04f, 0.04f);
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetFloat("_Metallic", 0.2f);
            creature.GetComponent<Renderer>().material = mat;

            // Two glowing red eye lights.
            for (int i = -1; i <= 1; i += 2)
            {
                var eye = new GameObject("Eye_" + (i < 0 ? "L" : "R"));
                eye.transform.SetParent(creature.transform, false);
                eye.transform.localPosition = new Vector3(0.10f * i, 0.55f, 0.30f);
                var light = eye.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.13f, 0.13f);
                light.range = 1.2f;
                light.intensity = 3.5f;
            }

            creature.AddComponent<CreatureAI>();
        }

        var ai = creature.GetComponent<CreatureAI>();
        if (ai.breathingClip == null) ai.breathingClip = ProceduralAudio.MakeBreathing(7f);
        if (ai.chaseClip == null) ai.chaseClip = ProceduralAudio.MakeChase(6f);

        SetupProximityFx(ai, pc);
    }

    static void SetupProximityFx(CreatureAI ai, PlayerController pc)
    {
        if (pc.GetComponent<CreatureProximityFX>() != null) return;
        var cam = pc.GetComponentInChildren<Camera>();
        if (cam == null) return;

        // Heartbeat audio source as a child of the camera.
        var hb = new GameObject("Heartbeat");
        hb.transform.SetParent(cam.transform, false);
        var hbSrc = hb.AddComponent<AudioSource>();
        hbSrc.clip = ProceduralAudio.MakeHeartbeat(75f);
        hbSrc.loop = true;
        hbSrc.spatialBlend = 0f;
        hbSrc.volume = 0f;
        hbSrc.playOnAwake = false;

        var fx = pc.gameObject.AddComponent<CreatureProximityFX>();
        fx.creature = ai;
        fx.cameraTransform = cam.transform;
        fx.heartbeatSource = hbSrc;
        fx.volume = Object.FindFirstObjectByType<Volume>();
    }

    static void SetupPostProcessing()
    {
        // Find or create a Global Volume.
        var vol = Object.FindFirstObjectByType<Volume>();
        if (vol == null)
        {
            var go = new GameObject("Global Volume");
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 0;
        }

        if (vol.profile == null)
            vol.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var p = vol.profile;

        if (!p.TryGet<Vignette>(out var vig)) vig = p.Add<Vignette>();
        vig.active = true;
        vig.intensity.Override(0.45f);
        vig.smoothness.Override(0.3f);
        vig.color.Override(Color.black);

        if (!p.TryGet<FilmGrain>(out var grain)) grain = p.Add<FilmGrain>();
        grain.active = true;
        grain.type.Override(FilmGrainLookup.Medium2);
        grain.intensity.Override(0.25f);

        if (!p.TryGet<Bloom>(out var bloom)) bloom = p.Add<Bloom>();
        bloom.active = true;
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(0.35f);
        bloom.scatter.Override(0.7f);

        if (!p.TryGet<ColorAdjustments>(out var color)) color = p.Add<ColorAdjustments>();
        color.active = true;
        color.postExposure.Override(-0.3f);
        color.saturation.Override(-30f);
        color.contrast.Override(10f);

        if (!p.TryGet<ChromaticAberration>(out var chroma)) chroma = p.Add<ChromaticAberration>();
        chroma.active = true;
        chroma.intensity.Override(0f); // proximity FX raises this near the creature

        // URP needs post-processing toggled on the camera to render the stack.
        var cam = Camera.main;
        if (cam != null)
        {
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data == null) data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
        }
    }

    static void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 1f;
        RenderSettings.fogEndDistance = 13f;
        // Near-black charcoal so distant halls vanish into nothing.
        RenderSettings.fogColor = new Color(0.02f, 0.025f, 0.035f, 1f);
        RenderSettings.ambientLight = new Color(0.008f, 0.010f, 0.015f, 1f);
    }

    static void DimDirectionalLights()
    {
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional && l.intensity > 0.1f)
                l.intensity = 0.02f;
        }
    }

    static void SetupPlayerAudio()
    {
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc == null) return;

        var footsteps = ProceduralAudio.MakeFootsteps(6);
        if (pc.GetComponent<FootstepAudio>() == null)
        {
            var fa = pc.gameObject.AddComponent<FootstepAudio>();
            fa.footstepClips = footsteps;
        }
        else
        {
            var fa = pc.GetComponent<FootstepAudio>();
            if (fa.footstepClips == null || fa.footstepClips.Length == 0)
                fa.footstepClips = footsteps;
        }

        var scares = new[]
        {
            ProceduralAudio.MakeScare(1.2f, 110f),
            ProceduralAudio.MakeScare(1.8f, 72f),
            ProceduralAudio.MakeScare(0.9f, 160f),
        };
        if (pc.GetComponent<RandomScares>() == null)
        {
            var rs = pc.gameObject.AddComponent<RandomScares>();
            rs.scareClips = scares;
            rs.minInterval = 22f;
            rs.maxInterval = 55f;
        }
        else
        {
            var rs = pc.GetComponent<RandomScares>();
            if (rs.scareClips == null || rs.scareClips.Length == 0)
                rs.scareClips = scares;
        }
    }

    static void SetupAmbientAudio()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // Find or create a child named "Ambient" so we don't stack sources.
        Transform t = cam.transform.Find("Ambient");
        AudioSource src;
        if (t == null)
        {
            var go = new GameObject("Ambient");
            go.transform.SetParent(cam.transform, false);
            src = go.AddComponent<AudioSource>();
        }
        else
        {
            src = t.GetComponent<AudioSource>();
            if (src == null) src = t.gameObject.AddComponent<AudioSource>();
        }

        if (src.clip == null)
            src.clip = ProceduralAudio.MakeDrone(10f);
        src.loop = true;
        src.volume = 0.22f;
        src.spatialBlend = 0f;
        src.playOnAwake = false;
        if (!src.isPlaying) src.Play();
    }

    static void EnsureCrosshair(Canvas canvas)
    {
        if (canvas == null) return;
        if (canvas.transform.Find("Crosshair") != null) return;

        var go = new GameObject("Crosshair", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvas.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(4f, 4f);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.5f);
        img.raycastTarget = false;
    }
}
