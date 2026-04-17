using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Effects")]
    public ParticleSystem deathParticles;
    public ParticleSystem healParticles;
    public ParticleSystem bounceParticles;

    [Header("Bounce")]
    public float bounceForce = 15f;

    [Header("Invincibility")]
    public float iFrameDuration = 0.5f;

    public event Action<int, int> OnHealthChanged;

    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;
    private PlayerController controller;
    private Renderer playerRenderer;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        playerRenderer = GetComponent<Renderer>();
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void OnTriggerEnter(Collider other)
    {
        Hazard hazard = other.GetComponent<Hazard>();
        if (hazard == null) return;

        HazardBehavior behavior = RuleEngine.Instance.GetBehavior(hazard.hazardType);

        switch (behavior)
        {
            case HazardBehavior.Kill:
                TakeDamage(1, hazard.hazardType);
                break;
            case HazardBehavior.Heal:
                Heal(1);
                break;
            case HazardBehavior.Bounce:
                Bounce();
                break;
        }
    }

    public void TakeDamage(int amount, HazardType? cause = null)
    {
        if (isInvincible || GameManager.Instance.CurrentState != GameState.Playing) return;

        CurrentHealth -= amount;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.15f, 0.2f);

        if (CurrentHealth <= 0)
        {
            Die(cause);
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        SoundManager.Instance?.PlayHeal();

        if (healParticles != null)
            healParticles.Play();

        // Show "???" surprise — player expected death but got healed
        if (RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage("???", new Color(0.2f, 1f, 0.4f), 1.2f);
    }

    public void Bounce()
    {
        SoundManager.Instance?.PlayBounce();

        if (controller != null)
            controller.ApplyBounce(bounceForce);

        if (bounceParticles != null)
            bounceParticles.Play();
    }

    public void Die(HazardType? cause = null)
    {
        if (GameManager.Instance.CurrentState == GameState.Dead) return;

        SoundManager.Instance?.PlayDeath();

        if (controller != null) controller.DisableControl();

        if (deathParticles != null)
        {
            deathParticles.transform.SetParent(null);
            deathParticles.Play();
            Destroy(deathParticles.gameObject, 2f);
        }

        DeathExplosion.SpawnAt(transform.position);

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.3f, 0.4f);

        gameObject.SetActive(false);
        GameManager.Instance.OnPlayerDeath(cause);
    }

    public void InstantKill()
    {
        Die(null);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        isInvincible = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        while (elapsed < iFrameDuration)
        {
            bool vis = !((int)(elapsed / 0.1f) % 2 == 0);
            foreach (Renderer r in childRenderers)
                r.enabled = vis;
            yield return new WaitForSeconds(0.05f);
            elapsed += 0.05f;
        }
        foreach (Renderer r in childRenderers)
            r.enabled = true;
        if (playerRenderer != null) playerRenderer.enabled = false;
        isInvincible = false;
    }
}
