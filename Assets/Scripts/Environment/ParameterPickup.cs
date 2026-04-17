using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Environment
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ParameterPickup : MonoBehaviour
    {
        private IActionParameter parameter;

        private void Awake()
        {
            TryResolveParameter();
        }

        private void Start()
        {
            TryResolveParameter();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController playerController))
            {
                return;
            }

            if (parameter == null)
            {
                TryResolveParameter();
            }

            if (parameter == null)
            {
                return;
            }

            GameplayFeedbackService.Instance?.PlayOrbPickup(transform.position, parameter.GetActionColor());
            playerController.SetActionParameter(parameter);

            // Disable instead of destroying so the equipped action reference stays alive.
            gameObject.SetActive(false);
        }

        private void TryResolveParameter()
        {
            MonoBehaviour[] components = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is IActionParameter actionParameter)
                {
                    parameter = actionParameter;
                    return;
                }
            }
        }
    }
}
