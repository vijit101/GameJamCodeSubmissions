#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-shot editor bootstrap that creates anything the guide asks for but the
// SampleScene doesn't already have:
//   - Assets/Scenes/StartScene.unity with a menu canvas + StartMenu controller
//   - Both scenes in Build Settings (StartScene = 0, the game scene = 1)
// Runs on editor load via [InitializeOnLoad]. Idempotent — does nothing if the
// assets already exist.
[InitializeOnLoad]
static class ProjectBootstrap
{
    const string StartScenePath = "Assets/Scenes/StartScene.unity";
    const string SamplePath = "Assets/Scenes/SampleScene.unity";
    const string GameScenePath = "Assets/Scenes/GameScene.unity";

    static ProjectBootstrap()
    {
        EditorApplication.delayCall += Run;
    }

    static void Run()
    {
        EnsureStartScene();
        EnsureBuildSettings();
    }

    static void EnsureStartScene()
    {
        if (File.Exists(StartScenePath)) return;
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");

        // Open an additive scene so we don't disturb whatever the user has loaded.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var previouslyActive = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SetActiveScene(scene);

        BuildStartScene();

        EditorSceneManager.SaveScene(scene, StartScenePath);
        EditorSceneManager.SetActiveScene(previouslyActive);
        EditorSceneManager.CloseScene(scene, true);

        AssetDatabase.Refresh();
        Debug.Log("[ProjectBootstrap] Created " + StartScenePath);
    }

    static void BuildStartScene()
    {
        // Camera with solid black clear.
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.transform.position = new Vector3(0, 1, -10);

        // EventSystem so UI buttons could work if added.
        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // Canvas.
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Solid black background panel.
        var bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(canvasGo.transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = Color.black;

        AddText(canvasGo.transform, "Title", "MAZE RUNNER", 80, Color.white, new Vector2(0, 150));
        AddText(canvasGo.transform, "Subtitle", "Find the exit. Before the dark finds you.",
            24, new Color(0.53f, 0.53f, 0.53f), new Vector2(0, 60));
        AddText(canvasGo.transform, "Controls",
            "WASD  -  Move\nMouse  -  Look\nShift  -  Sprint\nF  -  Flashlight\n\nPress ENTER to start",
            18, new Color(0.33f, 0.33f, 0.33f), new Vector2(0, -150));

        var controller = new GameObject("MenuController");
        controller.AddComponent<StartMenu>();
    }

    static void AddText(Transform parent, string name, string value, float size, Color color, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1200, 200);
        rt.anchoredPosition = anchoredPos;
    }

    static void EnsureBuildSettings()
    {
        // Use whichever of GameScene.unity / SampleScene.unity exists.
        string gamePath = File.Exists(GameScenePath) ? GameScenePath :
                          File.Exists(SamplePath) ? SamplePath : null;

        var wanted = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        if (File.Exists(StartScenePath))
            wanted.Add(new EditorBuildSettingsScene(StartScenePath, true));
        if (gamePath != null)
            wanted.Add(new EditorBuildSettingsScene(gamePath, true));

        if (wanted.Count == 0) return;

        var existing = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        bool matches = existing.Length == wanted.Count;
        if (matches)
        {
            for (int i = 0; i < wanted.Count; i++)
                if (existing[i] != wanted[i].path) { matches = false; break; }
        }

        if (!matches)
        {
            EditorBuildSettings.scenes = wanted.ToArray();
            Debug.Log("[ProjectBootstrap] Build Settings updated: " +
                string.Join(", ", wanted.Select(s => s.path)));
        }
    }
}
#endif
