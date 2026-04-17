using System;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    public event Action<int> OnPhaseChanged;

    public int CurrentPhase { get; private set; } = 0;

    private static readonly string[] PhaseNames = new string[]
    {
        "TRUST BUILDING",
        "RULE MUTATION",
        "MIXED RULES",
        "CONTROLLED CHAOS"
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDeathCountChanged += EvaluatePhase;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDeathCountChanged -= EvaluatePhase;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDeathCountChanged += EvaluatePhase;
    }

    private void EvaluatePhase(int deathCount)
    {
        int newPhase;
        if (deathCount == 0) newPhase = 0;
        else if (deathCount <= 2) newPhase = 1;
        else if (deathCount <= 5) newPhase = 2;
        else newPhase = 3;

        if (newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }

    public static string GetPhaseName(int phase)
    {
        if (phase >= 0 && phase < PhaseNames.Length)
            return PhaseNames[phase];
        return "UNKNOWN";
    }
}
