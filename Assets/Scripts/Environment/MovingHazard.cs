using UnityEngine;

namespace SpaceLoader.Environment
{
    public sealed class MovingHazard : MonoBehaviour
    {
        [SerializeField] private Vector2 axis = Vector2.right;
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float speed = 1f;

        private Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            Vector3 offset = (Vector3)(axis.normalized * Mathf.Sin(Time.time * speed) * amplitude);
            transform.position = startPosition + offset;
        }

        public void Configure(Vector2 movementAxis, float movementAmplitude, float movementSpeed)
        {
            axis = movementAxis;
            amplitude = movementAmplitude;
            speed = movementSpeed;
        }
    }
}
