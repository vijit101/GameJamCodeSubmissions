using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Environment
{
    [DisallowMultipleComponent]
    public sealed class BreakableBlock : MonoBehaviour
    {
        [SerializeField] private float minimumBreakImpact = 6f;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent(out PlayerController _))
            {
                return;
            }

            if (other.relativeVelocity.y > -minimumBreakImpact)
            {
                GameplayFeedbackService.Instance?.PlayMechanicFail();
                return;
            }

            GameplayFeedbackService.Instance?.PlayMechanicSuccess(transform.position, PresentationTheme.BreakableAmber);
            Destroy(gameObject);
        }
    }
}
