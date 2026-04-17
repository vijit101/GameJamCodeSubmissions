using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Mechanics
{
    public sealed class ActionSpring : MonoBehaviour, IActionParameter
    {
        [SerializeField] private float jumpForce = 15f;

        public bool TryExecute(Rigidbody2D playerRb)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            return true;
        }

        public string GetActionName()
        {
            return "Spring";
        }

        public string GetActionDescription()
        {
            return "Leap upward.";
        }

        public Color GetActionColor()
        {
            return PresentationTheme.SpringGreen;
        }
    }
}
