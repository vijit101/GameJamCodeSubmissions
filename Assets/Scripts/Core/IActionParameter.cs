using UnityEngine;

namespace SpaceLoader.Core
{
    public interface IActionParameter
    {
        bool TryExecute(Rigidbody2D playerRb);
        string GetActionName();
        string GetActionDescription();
        Color GetActionColor();
    }
}
