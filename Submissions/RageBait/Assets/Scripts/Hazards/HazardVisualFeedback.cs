using System.Collections;
using UnityEngine;

public class HazardVisualFeedback : MonoBehaviour
{
    public ParticleSystem ambientParticles;
    public float colorTransitionSpeed = 5f;

    private Renderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private Color targetColor;
    private Color currentColor;
    private Color targetEmission;

    private static readonly Color KillColor = new Color(1f, 0.15f, 0.1f);
    private static readonly Color HealColor = new Color(0.1f, 1f, 0.3f);
    private static readonly Color BounceColor = new Color(1f, 1f, 0.1f);

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        currentColor = KillColor;
        targetColor = KillColor;
        targetEmission = KillColor * 2f;
    }

    public void UpdateVisuals(HazardBehavior behavior)
    {
        switch (behavior)
        {
            case HazardBehavior.Kill:
                targetColor = KillColor;
                targetEmission = KillColor * 2f;
                break;
            case HazardBehavior.Heal:
                targetColor = HealColor;
                targetEmission = HealColor * 2f;
                break;
            case HazardBehavior.Bounce:
                targetColor = BounceColor;
                targetEmission = BounceColor * 2f;
                break;
        }

        // Only animate if this object is active — prevents errors from inactive level zones
        if (!gameObject.activeInHierarchy)
        {
            // Apply color instantly without coroutine
            currentColor = targetColor;
            ApplyColor(currentColor);
            UpdateParticleColor(targetColor);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TransitionColor());
        UpdateParticleColor(targetColor);
    }

    private IEnumerator TransitionColor()
    {
        while (Vector4.Distance(currentColor, targetColor) > 0.01f)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            ApplyColor(currentColor);
            yield return null;
        }
        currentColor = targetColor;
        ApplyColor(currentColor);
    }

    private void ApplyColor(Color color)
    {
        if (meshRenderer == null) return;

        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", color);
        propBlock.SetColor("_EmissionColor", targetEmission);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    private void UpdateParticleColor(Color color)
    {
        if (ambientParticles == null) return;

        var main = ambientParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
    }
}
