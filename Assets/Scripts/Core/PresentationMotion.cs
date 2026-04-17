using UnityEngine;

namespace SpaceLoader.Core
{
    public sealed class PresentationMotion : MonoBehaviour
    {
        [SerializeField] private Vector3 bobAxis = new Vector3(0f, 0.18f, 0f);
        [SerializeField] private float bobSpeed = 2.2f;
        [SerializeField] private float pulseAmount = 0.08f;
        [SerializeField] private float pulseSpeed = 2.6f;
        [SerializeField] private bool rotate;
        [SerializeField] private float rotationSpeed = 60f;

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private float randomOffset;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float time = Time.time + randomOffset;
            transform.localPosition = baseLocalPosition + bobAxis * Mathf.Sin(time * bobSpeed);

            float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
            transform.localScale = baseLocalScale * pulse;

            if (rotate)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
