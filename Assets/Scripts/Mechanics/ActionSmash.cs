using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Mechanics
{
    public sealed class ActionSmash : MonoBehaviour, IActionParameter
    {
        [SerializeField] private float smashForce = 34f;
        [SerializeField] private float controlLockDuration = 0.3f;

        public bool TryExecute(Rigidbody2D playerRb)
        {
            playerRb.linearVelocity = new Vector2(0f, -6f);
            playerRb.AddForce(Vector2.down * smashForce, ForceMode2D.Impulse);
            playerRb.GetComponent<PlayerController>()?.LockHorizontalControl(controlLockDuration);
            return true;
        }

        public string GetActionName()
        {
            return "Smash";
        }

        public string GetActionDescription()
        {
            return "Drive down through weak floors.";
        }

        public Color GetActionColor()
        {
            return PresentationTheme.SmashRed;
        }
    }
}
