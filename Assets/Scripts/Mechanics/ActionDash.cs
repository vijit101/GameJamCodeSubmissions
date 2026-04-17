using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Mechanics
{
    public sealed class ActionDash : MonoBehaviour, IActionParameter
    {
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private float dashLift = 3.5f;
        [SerializeField] private float controlLockDuration = 0.2f;

        public bool TryExecute(Rigidbody2D playerRb)
        {
            float input = Input.GetAxisRaw("Horizontal");
            float direction = Mathf.Abs(input) > 0.01f ? Mathf.Sign(input) : (playerRb.linearVelocity.x >= 0f ? 1f : -1f);

            float liftedVerticalSpeed = Mathf.Max(playerRb.linearVelocity.y, dashLift);
            playerRb.linearVelocity = new Vector2(0f, liftedVerticalSpeed);
            playerRb.AddForce(Vector2.right * direction * dashForce, ForceMode2D.Impulse);
            playerRb.GetComponent<PlayerController>()?.LockHorizontalControl(controlLockDuration);
            return true;
        }

        public string GetActionName()
        {
            return "Dash";
        }

        public string GetActionDescription()
        {
            return "Burst forward with lift.";
        }

        public Color GetActionColor()
        {
            return PresentationTheme.DashBlue;
        }
    }
}
