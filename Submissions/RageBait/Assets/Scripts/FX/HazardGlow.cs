using UnityEngine;

public class HazardGlow : MonoBehaviour
{
    private Light glowLight;
    private Hazard hazard;

    private static readonly Color KillGlow = new Color(1f, 0.1f, 0.05f);
    private static readonly Color HealGlow = new Color(0.1f, 1f, 0.3f);
    private static readonly Color BounceGlow = new Color(1f, 1f, 0.1f);

    void Start()
    {
        hazard = GetComponent<Hazard>();

        glowLight = gameObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.range = 5f;
        glowLight.intensity = 2f;
        glowLight.color = KillGlow;

        if (RuleEngine.Instance != null)
        {
            RuleEngine.Instance.OnRulesChanged += UpdateGlow;
            UpdateGlow();
        }
    }

    void OnDestroy()
    {
        if (RuleEngine.Instance != null)
            RuleEngine.Instance.OnRulesChanged -= UpdateGlow;
    }

    void Update()
    {
        if (glowLight != null)
            glowLight.intensity = 1.5f + Mathf.Sin(Time.time * 3f) * 0.5f;
    }

    void UpdateGlow()
    {
        if (hazard == null || glowLight == null) return;
        HazardBehavior b = RuleEngine.Instance.GetBehavior(hazard.hazardType);
        switch (b)
        {
            case HazardBehavior.Kill: glowLight.color = KillGlow; break;
            case HazardBehavior.Heal: glowLight.color = HealGlow; break;
            case HazardBehavior.Bounce: glowLight.color = BounceGlow; break;
        }
    }
}
