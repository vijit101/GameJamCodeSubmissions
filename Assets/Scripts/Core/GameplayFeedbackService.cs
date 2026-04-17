using System.Collections;
using UnityEngine;

namespace SpaceLoader.Core
{
    [DisallowMultipleComponent]
    public sealed class GameplayFeedbackService : MonoBehaviour
    {
        public static GameplayFeedbackService Instance { get; private set; }

        private CameraFollow cameraFollow;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetCamera(CameraFollow follow)
        {
            cameraFollow = follow;
        }

        public void PlayOrbPickup(Vector2 position, Color color)
        {
            SpawnBurst(position, color, 7, 1.6f, 0.32f);
        }

        public void PlayActionSwap(string actionName, Color color)
        {
            UI.UIManager.Instance?.PulseAction(color);
            UI.UIManager.Instance?.ShowBanner($"Selected: {actionName}", color, 0.65f);
        }

        public void PlayActionSwap(string actionName, Color color, int remainingCharges, int maximumCharges)
        {
            UI.UIManager.Instance?.PulseAction(color);
            UI.UIManager.Instance?.ShowBanner($"Selected: {actionName} ({remainingCharges}/{maximumCharges})", color, 0.75f);
        }

        public void PlayMechanicUse(Vector2 position, Color color)
        {
            SpawnBurst(position, color, 5, 1.1f, 0.24f);
        }

        public void PlayChargeConsumed(string actionName, int remainingCharges, int maximumCharges, Color color)
        {
            UI.UIManager.Instance?.PulseAction(color);
            UI.UIManager.Instance?.ShowBanner($"{actionName} used ({remainingCharges}/{maximumCharges})", color, 0.65f);
        }

        public void PlayMechanicSuccess(Vector2 position, Color color)
        {
            SpawnBurst(position, color, 8, 1.8f, 0.38f);
            UI.UIManager.Instance?.PulseAction(color);
            UI.UIManager.Instance?.ShowBanner("Weak floor broken", color, 0.6f);
            cameraFollow?.Shake(0.14f, 0.12f);
        }

        public void PlayMechanicFail()
        {
            UI.UIManager.Instance?.PulseAction(PresentationTheme.FailureOrange);
        }

        public void PlayMechanicFail(string message)
        {
            UI.UIManager.Instance?.PulseAction(PresentationTheme.FailureOrange);
            UI.UIManager.Instance?.ShowBanner(message, PresentationTheme.FailureOrange, 0.7f);
        }

        public void PlayOutOfCharges(string actionName, Color color)
        {
            UI.UIManager.Instance?.PulseAction(PresentationTheme.FailureOrange);
            UI.UIManager.Instance?.ShowBanner($"{actionName} depleted", PresentationTheme.FailureOrange, 0.85f);
        }

        public void PlayRoomComplete(Vector2 position, Color color)
        {
            SpawnBurst(position, color, 12, 2.4f, 0.5f);
            UI.UIManager.Instance?.ShowBanner("Room Complete", color, 0.9f);
            cameraFollow?.Shake(0.2f, 0.18f);
        }

        private void SpawnBurst(Vector2 position, Color color, int count, float speed, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count + Random.Range(-0.2f, 0.2f);
                Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * Random.Range(0.7f, 1.2f);
                CreateParticle(position, velocity, color, lifetime, Random.Range(0.08f, 0.18f));
            }
        }

        private void CreateParticle(Vector2 position, Vector2 velocity, Color color, float lifetime, float scale)
        {
            GameObject particle = new GameObject("FeedbackParticle");
            particle.transform.position = position;
            SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.CircleSprite;
            renderer.color = color;
            renderer.sortingOrder = 50;
            particle.transform.localScale = Vector3.one * scale;

            FeedbackParticle motion = particle.AddComponent<FeedbackParticle>();
            motion.Initialize(velocity, lifetime);
        }

        private sealed class FeedbackParticle : MonoBehaviour
        {
            private Vector2 velocity;
            private float lifetime;
            private float age;
            private SpriteRenderer renderer;

            public void Initialize(Vector2 initialVelocity, float duration)
            {
                velocity = initialVelocity;
                lifetime = duration;
                renderer = GetComponent<SpriteRenderer>();
            }

            private void Update()
            {
                age += Time.deltaTime;
                transform.position += (Vector3)(velocity * Time.deltaTime);
                velocity *= 0.93f;

                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = Mathf.Clamp01(1f - age / lifetime);
                    renderer.color = color;
                }

                if (age >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
