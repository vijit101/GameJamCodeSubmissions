using UnityEngine;

// Tiny event bus for noise events the player makes (footsteps, sprints,
// flashlight toggles). Creatures subscribe and investigate.
public static class NoiseSystem
{
    public delegate void NoiseHandler(Vector3 worldPos, float radius);
    public static event NoiseHandler OnNoise;

    public static void Emit(Vector3 worldPos, float radius)
    {
        if (radius <= 0f) return;
        OnNoise?.Invoke(worldPos, radius);
    }

    // Reset on scene load so we don't keep stale subscribers.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnDomainReload()
    {
        OnNoise = null;
    }
}
