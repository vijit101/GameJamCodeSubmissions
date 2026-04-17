using UnityEngine;

namespace SpaceLoader.Core
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLevelManagerExists()
        {
            if (Object.FindAnyObjectByType<LevelManager>() != null)
            {
                return;
            }

            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<LevelManager>();
        }
    }
}
