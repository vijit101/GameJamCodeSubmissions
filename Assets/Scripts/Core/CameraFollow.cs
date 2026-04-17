using UnityEngine;

namespace SpaceLoader.Core
{
    [DisallowMultipleComponent]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        private Vector3 velocity;
        private Rect roomBounds = new Rect(-12f, -7f, 24f, 14f);
        private Vector2 objectivePoint;
        private float targetOrthographicSize = 5.6f;
        private float shakeTimer;
        private float shakeStrength;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Camera cameraComponent = GetComponent<Camera>();
            if (cameraComponent == null)
            {
                return;
            }

            cameraComponent.orthographicSize = Mathf.Lerp(
                cameraComponent.orthographicSize,
                targetOrthographicSize,
                Time.deltaTime * PresentationTheme.CameraTransitionSizeSpeed);

            Vector2 focus = target.position;
            Vector2 objectiveDelta = objectivePoint - focus;

            if (objectiveDelta.magnitude > PresentationTheme.CameraFocusZoneRadius)
            {
                focus += objectiveDelta.normalized * Mathf.Min(
                    objectiveDelta.magnitude - PresentationTheme.CameraFocusZoneRadius,
                    PresentationTheme.CameraObjectivePull);
            }

            float verticalExtent = cameraComponent.orthographicSize;
            float horizontalExtent = verticalExtent * cameraComponent.aspect;
            float minX = roomBounds.xMin + horizontalExtent + PresentationTheme.CameraEdgePadding;
            float maxX = roomBounds.xMax - horizontalExtent - PresentationTheme.CameraEdgePadding;
            float minY = roomBounds.yMin + verticalExtent + PresentationTheme.CameraEdgePadding;
            float maxY = roomBounds.yMax - verticalExtent - PresentationTheme.CameraEdgePadding;

            float clampedX = minX <= maxX ? Mathf.Clamp(focus.x, minX, maxX) : roomBounds.center.x;
            float clampedY = minY <= maxY ? Mathf.Clamp(focus.y, minY, maxY) : roomBounds.center.y;

            Vector3 targetPosition = new Vector3(clampedX, clampedY, 0f) + offset;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector2 shakeOffset = Random.insideUnitCircle * shakeStrength;
                targetPosition += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, PresentationTheme.CameraSmoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void ConfigureRoom(Rect bounds, Vector2 objective, float orthographicSize)
        {
            roomBounds = bounds;
            objectivePoint = objective;
            targetOrthographicSize = orthographicSize;
        }

        public void Shake(float strength, float duration)
        {
            shakeStrength = Mathf.Max(shakeStrength, strength);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }
    }
}
