using System.IO;
using SpaceLoader.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceLoader.Editor
{
    public static class SpaceLoaderSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [InitializeOnLoadMethod]
        private static void EnsureSceneExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            bool missingScene = !File.Exists(ScenePath);
            bool missingBuildEntry = true;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == ScenePath)
                {
                    missingBuildEntry = false;
                    break;
                }
            }

            if (!missingScene && !missingBuildEntry)
            {
                return;
            }

            EditorApplication.delayCall += BuildBootstrapScene;
        }

        private static void BuildBootstrapScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GameObject bootstrap = new GameObject("GameBootstrap");
                bootstrap.AddComponent<LevelManager>();

                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
