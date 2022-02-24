using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class OnlineLevelsController
{
    public static List<OnlineLevel> onlineLevelsList { get; private set; }

    public static void InitOnlineLevelsList()
    {
        if (onlineLevelsList == null)
            onlineLevelsList = new List<OnlineLevel>();
    }

    public static int GetNumOnlineStars()
    {
        int stars = 0;
        foreach (int score in SaveOnlineInfo.levelsPlayed.Values)
            stars += score;

        return stars;
    }

    /// <summary>
    /// True if a level can't be played because user has filtered it
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    /*
    public static bool LevelIsFiltered(OnlineLevel level)
    {
        medalType medal;

        // is filtered if user has played and has the stars filtered
        if (SaveOnlineInfo.levelsPlayed.ContainsKey(level.levelId))
        {
            medal = SaveOnlineInfo.levelsPlayed[level.levelId];
        }

        // if level hasn't been won
        if (medal == medalType.JUST_TRIED)
        {
            // if the filter selected is to not show the levels not played
            if (!UserConfig.onlineMedalsOptions[(int)medalType.NONE])
                return true;

            else return false;
        }

        if (!UserConfig.onlineMedalsOptions[(int)medal])
            return true;

        return false;
    }*/


    /// <summary>
    /// True if a level can't be played because user has filtered it
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static bool LevelIsFiltered(OnlineLevel level)
    {
        bool isFiltered = false;

        // is filtered if user has played and has the stars filtered
        if (SaveOnlineInfo.levelsPlayed.ContainsKey(level.levelId))
        {
            if (!UserConfig.onlineMedalsOptions[(int)SaveOnlineInfo.levelsPlayed[level.levelId]])
                isFiltered = true;
        }
        else
        {
            // if the filter selected is to not show the levels not played
            if (!UserConfig.onlineMedalsOptions[(int)medalType.NONE])
                isFiltered = true;
        }

        return isFiltered;
    }


    public static void OnTimePlayed(string levelId)
    {
        CloudFirestore.Instance.UpdateStatsField(levelId, 1, 0, 0);

        Firebase.Analytics.FirebaseAnalytics.LogEvent("online_level_played", "level_id", levelId);
    }

    public static int CalculateDifficulty(int timesPlayed, int wins)
    {
        if (timesPlayed == 0)
            return -1;

        Debug.Assert(wins <= timesPlayed);

        float difficulty = 1f - ((float)wins / (float)timesPlayed);
        difficulty *= 1000000f;
        return Mathf.FloorToInt(difficulty);
    }
}
