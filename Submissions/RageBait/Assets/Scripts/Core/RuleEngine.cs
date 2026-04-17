using System;
using System.Collections.Generic;
using UnityEngine;

public class RuleEngine : MonoBehaviour
{
    public static RuleEngine Instance { get; private set; }

    public event Action OnRulesChanged;

    private Dictionary<HazardType, HazardBehavior> rules = new Dictionary<HazardType, HazardBehavior>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ResetRules();
    }

    public void ResetRules()
    {
        rules[HazardType.Fire] = HazardBehavior.Kill;
        rules[HazardType.Spike] = HazardBehavior.Kill;
        rules[HazardType.Enemy] = HazardBehavior.Kill;
        OnRulesChanged?.Invoke();
    }

    public HazardBehavior GetBehavior(HazardType type)
    {
        return rules.ContainsKey(type) ? rules[type] : HazardBehavior.Kill;
    }

    public void SetRule(HazardType type, HazardBehavior behavior)
    {
        rules[type] = behavior;
        OnRulesChanged?.Invoke();
    }

    public Dictionary<HazardType, HazardBehavior> GetAllRules()
    {
        return new Dictionary<HazardType, HazardBehavior>(rules);
    }

    public void MutateRule(HazardType causeOfDeath)
    {
        HazardBehavior current = rules[causeOfDeath];
        List<HazardBehavior> options = new List<HazardBehavior>();
        foreach (HazardBehavior b in Enum.GetValues(typeof(HazardBehavior)))
        {
            if (b != current) options.Add(b);
        }
        rules[causeOfDeath] = options[UnityEngine.Random.Range(0, options.Count)];
        OnRulesChanged?.Invoke();
        SoundManager.Instance?.PlayRuleChange();
    }

    public void MutateRandomRule()
    {
        HazardType[] types = (HazardType[])Enum.GetValues(typeof(HazardType));
        HazardType randomType = types[UnityEngine.Random.Range(0, types.Length)];
        MutateRule(randomType);
    }
}
