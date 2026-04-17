using UnityEngine;

// Per-run scoring. Reset by GameManager on Awake; read on Win.
public static class ScoreSystem
{
    public static int pagesCollected;
    public static int totalPages;
    public static int batteriesCollected;
    public static int totalBatteries;
    public static float runStartTime;
    public static int lastFinalScore;

    public static void BeginRun()
    {
        pagesCollected = 0;
        batteriesCollected = 0;
        totalPages = 0;
        totalBatteries = 0;
        runStartTime = Time.time;
        lastFinalScore = 0;
    }

    public static float Elapsed => Time.time - runStartTime;

    public static int Compute(out int timeBonus, out int pageBonus,
                              out int batteryBonus, out int completionBonus)
    {
        float t = Elapsed;
        // Target time scales with maze size: ~6s per cell baseline.
        float target = RunConfig.MazeSize * 6f;
        timeBonus = Mathf.Max(0, Mathf.RoundToInt(target - t)) * 8;
        pageBonus = pagesCollected * 100;
        batteryBonus = batteriesCollected * 30;
        completionBonus = (totalPages > 0 && pagesCollected == totalPages) ? 500 : 0;

        int raw = timeBonus + pageBonus + batteryBonus + completionBonus;
        int final = Mathf.RoundToInt(raw * RunConfig.DifficultyMultiplier);
        lastFinalScore = final;
        return final;
    }
}
