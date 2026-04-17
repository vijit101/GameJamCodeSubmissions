using SpaceLoader.Core;
using UnityEngine;

namespace SpaceLoader.Environment
{
    public sealed class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] private float beltSpeed = -4f;

        public void Configure(float speed)
        {
            beltSpeed = speed;
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent(out PlayerController _))
            {
                return;
            }

            Vector3 delta = Vector3.right * beltSpeed * Time.fixedDeltaTime;
            other.transform.position += delta;
        }
    }
}
