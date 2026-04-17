using SpaceLoader.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceLoader.Environment
{
    [DisallowMultipleComponent]
    public sealed class Hazard : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            HandlePlayerContact(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandlePlayerContact(other.gameObject);
        }

        private static void HandlePlayerContact(GameObject other)
        {
            if (!other.TryGetComponent(out PlayerController _))
            {
                return;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
