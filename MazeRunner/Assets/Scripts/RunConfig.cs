using System;
using UnityEngine;

// Static configuration for the next run. The Start scene writes to it; the
// game scene reads from it on Awake. Persists across scene loads but resets
// when the editor reloads (which is fine — defaults are sensible).
public static class RunConfig
{
    public enum Difficulty { Easy, Medium, Hard }

    public static Difficulty difficulty = Difficulty.Medium;
    public static int seed = 0;
    public static bool dailyMode = false;
    public static int endlessLevel = 0; // 0 = first map of a run

    // Mutators (toggleable from Start menu in future)
    public static bool extraCreature = false;

    public static int MazeSize
    {
        get
        {
            int baseSize = difficulty switch
            {
                Difficulty.Easy => 10,
                Difficulty.Medium => 15,
                Difficulty.Hard => 20,
                _ => 15
            };
            return Mathf.Min(40, baseSize + endlessLevel * 2);
        }
    }

    public static float CreatureSpawnDelay => difficulty switch
    {
        Difficulty.Easy => 90f,
        Difficulty.Medium => 45f,
        Difficulty.Hard => 15f,
        _ => 45f
    };

    public static int PageCount => Mathf.Clamp(MazeSize / 3, 3, 10);
    public static int BatteryCount => Mathf.Clamp(MazeSize / 5, 2, 6);

    public static float DifficultyMultiplier => difficulty switch
    {
        Difficulty.Easy => 1f,
        Difficulty.Medium => 2f,
        Difficulty.Hard => 3.5f,
        _ => 1f
    };

    public static int TodaysSeed()
    {
        var d = DateTime.UtcNow.Date;
        return d.Year * 10000 + d.Month * 100 + d.Day;
    }

    public static int RandomSeed() => (int)(DateTime.UtcNow.Ticks & int.MaxValue);

    public static string SeedLabel
    {
        get
        {
            if (dailyMode)
            {
                var d = DateTime.UtcNow.Date;
                return $"Daily {d:yyyy-MM-dd}";
            }
            return endlessLevel > 0 ? $"Floor {endlessLevel + 1}" : $"Seed {seed}";
        }
    }

    public static string DifficultyLabel => difficulty switch
    {
        Difficulty.Easy => "EASY",
        Difficulty.Medium => "MEDIUM",
        Difficulty.Hard => "HARD",
        _ => "MEDIUM"
    };
}
