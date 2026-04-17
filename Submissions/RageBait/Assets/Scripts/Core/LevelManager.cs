using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static int CurrentLevel = 1;
    public static int MaxLevels = 3;

    [Header("Level Zones (auto-populated)")]
    public List<LevelZone> zones = new List<LevelZone>();

    [Header("Level Themes")]
    public Color[] levelFogColors = new Color[]
    {
        new Color(0.02f, 0.01f, 0.06f),  // L1: Deep purple-black
        new Color(0.06f, 0.01f, 0.01f),  // L2: Dark blood red
        new Color(0.01f, 0.01f, 0.06f),  // L3: Void blue-black
    };

    public Color[] levelAmbientColors = new Color[]
    {
        new Color(0.06f, 0.04f, 0.1f),   // L1: Purple ambient
        new Color(0.1f, 0.03f, 0.03f),   // L2: Red ambient
        new Color(0.03f, 0.03f, 0.1f),   // L3: Blue ambient
    };

    public string[] levelNames = new string[]
    {
        "LEVEL 1: TRUST BUILDING",
        "LEVEL 2: FIRST BETRAYAL",
        "LEVEL 3: TOTAL CHAOS"
    };

    public string[] levelSubtitles = new string[]
    {
        "// learn the rules...",
        "// everything you know is wrong",
        "// there are no rules"
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find level zones (include inactive objects!)
        if (zones.Count == 0)
        {
            // FindObjectsOfType with true includes inactive GameObjects (Unity 2020+)
            LevelZone[] found = Resources.FindObjectsOfTypeAll<LevelZone>();
            foreach (var z in found)
            {
                // Only include scene objects, not assets
                if (z.gameObject.scene.isLoaded)
                    zones.Add(z);
            }
            zones.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        }

        // Activate only the current level
        ActivateLevel(CurrentLevel);
    }

    public void ActivateLevel(int level)
    {
        CurrentLevel = level;

        // Activate/deactivate zones
        foreach (var zone in zones)
        {
            if (zone != null)
                zone.gameObject.SetActive(zone.levelNumber == level);
        }

        // Apply visual theme via NeonEnvironment
        int idx = Mathf.Clamp(level - 1, 0, levelFogColors.Length - 1);
        if (NeonEnvironment.Instance != null)
            NeonEnvironment.Instance.ApplyTheme(levelFogColors[idx], levelAmbientColors[idx]);
        else
        {
            RenderSettings.fogColor = levelFogColors[idx];
            RenderSettings.ambientLight = levelAmbientColors[idx];
            Camera cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = levelFogColors[idx];
        }

        // Adjust difficulty per level — use SetRule for deterministic betrayal
        ApplyLevelRules(level);

        // Show level intro
        if (RageBaitMessages.Instance != null)
        {
            string name = GetLevelName();
            string sub = level <= levelSubtitles.Length ? levelSubtitles[level - 1] : "";
            RageBaitMessages.Instance.ShowMessage(name + "\n" + sub, new Color(1f, 0.84f, 0f), 3f);
        }
    }

    public void CompleteLevel()
    {
        if (CurrentLevel < MaxLevels)
        {
            CurrentLevel++;
            // Reload scene to reset everything cleanly for next level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // All levels complete — win!
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerWin();
        }
    }

    public static string GetLevelName()
    {
        if (Instance != null && CurrentLevel >= 1 && CurrentLevel <= Instance.levelNames.Length)
            return Instance.levelNames[CurrentLevel - 1];

        switch (CurrentLevel)
        {
            case 1: return "LEVEL 1: TRUST BUILDING";
            case 2: return "LEVEL 2: FIRST BETRAYAL";
            case 3: return "LEVEL 3: TOTAL CHAOS";
            default: return "LEVEL " + CurrentLevel;
        }
    }

    /// Apply deterministic rules for each level. Called on level start AND after each respawn.
    public static void ApplyLevelRules(int level)
    {
        if (RuleEngine.Instance == null) return;

        if (level == 1)
        {
            // Trust building: everything kills — teach the player
            RuleEngine.Instance.ResetRules();
        }
        else if (level == 2)
        {
            // BETRAYAL: Fire HEALS, Spikes are SAFE (bounce), Enemies still kill
            RuleEngine.Instance.ResetRules();
            RuleEngine.Instance.SetRule(HazardType.Fire, HazardBehavior.Heal);
            RuleEngine.Instance.SetRule(HazardType.Spike, HazardBehavior.Bounce);
        }
        else
        {
            // CHAOS: Fire heals, Spike bounces, Enemy heals — nothing is what it seems
            RuleEngine.Instance.ResetRules();
            RuleEngine.Instance.SetRule(HazardType.Fire, HazardBehavior.Heal);
            RuleEngine.Instance.SetRule(HazardType.Spike, HazardBehavior.Bounce);
            RuleEngine.Instance.SetRule(HazardType.Enemy, HazardBehavior.Heal);
        }
    }

    public static void ResetToLevel1()
    {
        CurrentLevel = 1;
    }
}
