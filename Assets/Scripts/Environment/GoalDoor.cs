using SpaceLoader.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceLoader.Environment
{
    [DisallowMultipleComponent]
    public sealed class GoalDoor : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _))
            {
                return;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteRoom(transform.position);
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                nextSceneIndex = SceneManager.GetActiveScene().buildIndex;
            }

            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
