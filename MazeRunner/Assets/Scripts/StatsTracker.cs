using UnityEngine;

// Lifetime stats persisted via PlayerPrefs.
public static class StatsTracker
{
    const string PrefRuns       = "MazeRunner.Stat.Runs";
    const string PrefEscapes    = "MazeRunner.Stat.Escapes";
    const string PrefDeaths     = "MazeRunner.Stat.Deaths";
    const string PrefBestScore  = "MazeRunner.Stat.BestScore";
    const string PrefAllPagesRuns = "MazeRunner.Stat.AllPagesRuns";

    static string BestTimeKey(RunConfig.Difficulty d) => $"MazeRunner.Best.Time.{d}";
    static string BestScoreKey(RunConfig.Difficulty d) => $"MazeRunner.Best.Score.{d}";

    public static int Runs    => PlayerPrefs.GetInt(PrefRuns, 0);
    public static int Escapes => PlayerPrefs.GetInt(PrefEscapes, 0);
    public static int Deaths  => PlayerPrefs.GetInt(PrefDeaths, 0);
    public static int BestScoreOverall => PlayerPrefs.GetInt(PrefBestScore, 0);
    public static int AllPagesRuns => PlayerPrefs.GetInt(PrefAllPagesRuns, 0);

    public static float BestTime(RunConfig.Difficulty d) =>
        PlayerPrefs.GetFloat(BestTimeKey(d), float.MaxValue);

    public static int BestScore(RunConfig.Difficulty d) =>
        PlayerPrefs.GetInt(BestScoreKey(d), 0);

    public static void RegisterRunStart()
    {
        PlayerPrefs.SetInt(PrefRuns, Runs + 1);
        PlayerPrefs.Save();
    }

    public static bool RegisterWin(float timeSeconds, int score, bool collectedAllPages)
    {
        PlayerPrefs.SetInt(PrefEscapes, Escapes + 1);

        bool newTime = timeSeconds < BestTime(RunConfig.difficulty);
        if (newTime) PlayerPrefs.SetFloat(BestTimeKey(RunConfig.difficulty), timeSeconds);

        bool newScore = score > BestScore(RunConfig.difficulty);
        if (newScore) PlayerPrefs.SetInt(BestScoreKey(RunConfig.difficulty), score);

        if (score > BestScoreOverall) PlayerPrefs.SetInt(PrefBestScore, score);

        if (collectedAllPages)
            PlayerPrefs.SetInt(PrefAllPagesRuns, AllPagesRuns + 1);

        PlayerPrefs.Save();
        return newTime || newScore;
    }

    public static void RegisterDeath()
    {
        PlayerPrefs.SetInt(PrefDeaths, Deaths + 1);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(PrefRuns);
        PlayerPrefs.DeleteKey(PrefEscapes);
        PlayerPrefs.DeleteKey(PrefDeaths);
        PlayerPrefs.DeleteKey(PrefBestScore);
        PlayerPrefs.DeleteKey(PrefAllPagesRuns);
        foreach (RunConfig.Difficulty d in System.Enum.GetValues(typeof(RunConfig.Difficulty)))
        {
            PlayerPrefs.DeleteKey(BestTimeKey(d));
            PlayerPrefs.DeleteKey(BestScoreKey(d));
        }
        PlayerPrefs.Save();
    }
}
