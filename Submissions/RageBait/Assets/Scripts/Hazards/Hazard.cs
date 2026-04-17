using UnityEngine;

public class Hazard : MonoBehaviour
{
    public HazardType hazardType;

    private HazardVisualFeedback visualFeedback;

    void Start()
    {
        visualFeedback = GetComponent<HazardVisualFeedback>();

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (RuleEngine.Instance != null)
        {
            RuleEngine.Instance.OnRulesChanged += UpdateVisuals;
            UpdateVisuals();
        }
    }

    void OnDestroy()
    {
        if (RuleEngine.Instance != null)
            RuleEngine.Instance.OnRulesChanged -= UpdateVisuals;
    }

    private void UpdateVisuals()
    {
        if (visualFeedback == null) return;
        HazardBehavior behavior = RuleEngine.Instance.GetBehavior(hazardType);
        visualFeedback.UpdateVisuals(behavior);
    }
}
