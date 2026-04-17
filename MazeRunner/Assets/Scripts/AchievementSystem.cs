using System;
using System.Collections.Generic;
using UnityEngine;

// Achievement unlock tracking + toast notifications. Persisted via PlayerPrefs.
public static class AchievementSystem
{
    public enum Id
    {
        FirstEscape,
        SubTwoMinute,
        Collector,
        Survivor,             // 5+ minutes alive in maze
        BraveSoul,            // Escape with flashlight off >60s in single run
        HardEscape,
        DailyChallenge,
        EndlessFloor3,
        AllPages5Times,       // collect every page in 5 runs (cumulative)
    }

    public struct Info
    {
        public string title;
        public string description;
        public Info(string t, string d) { title = t; description = d; }
    }

    public static readonly Dictionary<Id, Info> Catalog = new()
    {
        { Id.FirstEscape,    new("First Escape",     "You made it out alive.") },
        { Id.SubTwoMinute,   new("Speed Demon",      "Escape in under 2 minutes.") },
        { Id.Collector,      new("Collector",        "Collect every page in a single run.") },
        { Id.Survivor,       new("Survivor",         "Survive 5 minutes in the maze.") },
        { Id.BraveSoul,      new("Brave Soul",       "Spend 60 seconds with the flashlight off in one run.") },
        { Id.HardEscape,     new("In the Dark",      "Escape on Hard difficulty.") },
        { Id.DailyChallenge, new("Today's Special",  "Complete a daily challenge.") },
        { Id.EndlessFloor3,  new("Going Deeper",     "Reach floor 4 in endless mode.") },
        { Id.AllPages5Times, new("Archivist",        "Collect every page in 5 runs.") },
    };

    static string Key(Id id) => "MazeRunner.Ach." + id;

    public static bool IsUnlocked(Id id) => PlayerPrefs.GetInt(Key(id), 0) == 1;

    public static void Unlock(Id id)
    {
        if (IsUnlocked(id)) return;
        PlayerPrefs.SetInt(Key(id), 1);
        PlayerPrefs.Save();
        var info = Catalog[id];
        Hud.Toast($"<b>{info.title}</b>\n{info.description}", 4f);
    }

    public static int UnlockedCount()
    {
        int n = 0;
        foreach (Id id in Enum.GetValues(typeof(Id)))
            if (IsUnlocked(id)) n++;
        return n;
    }

    public static int TotalCount() => Enum.GetValues(typeof(Id)).Length;
}
