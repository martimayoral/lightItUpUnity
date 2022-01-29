using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelsController
{
    public static medalType[] levelMedals { get; private set; }
    public static int lastLevelCompleted { get; private set; }

    public static int numOfflineStars;

    public static int currentWorld = 0;
    readonly static public int[] worldMinStars = { 0, 16 * 3 - 12, 16 * 6 - 3 };
    public static readonly int nLevels = 33;

    public static sLevel currentLevel { get; private set; }

    public static List<sLevel> onlineLevelsList { get; private set; }

    public static void InitOnlineLevelsList()
    {
        if (onlineLevelsList == null)
            onlineLevelsList = new List<sLevel>();
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
    public static bool LevelIsFiltered(sLevel level)
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
            if (!UserConfig.onlineMedalsOptions[(int)medalType.none])
                isFiltered = true;
        }

        return isFiltered;
    }

    /// <summary>
    /// Returns true if current level is from online online
    /// </summary>
    public static bool CurrentLevelIsOnline()
    {
        return LevelIsOnline(currentLevel);
    }

    public static bool LevelIsOnline(sLevel level)
    {
        return level.levelId.Length > 0;
    }

    public static void ChangeCurrentLevelForNext()
    {
        sLevel nextLevel = GetNextLevel();
        if (nextLevel != null)
        {
            ChangeCurrentLevel(nextLevel);
        }
    }

    public static bool ChangeCurrentLevel(int levelNum)
    {
        if (levelNum < nLevels)
        {
            ChangeCurrentLevel(LevelManager.GetSLevelFromFile(levelNum));
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void ChangeCurrentLevel(sLevel currentLevel)
    {
        Debug.Log("Change current level for " + currentLevel.levelIndex);
        LevelsController.currentLevel = currentLevel;
    }

    public static sLevel GetNextLevel()
    {
        if (CurrentLevelIsOnline())
        {
            int nextLevelIndex = currentLevel.levelIndex + 1;
            foreach (sLevel level in onlineLevelsList.GetRange(nextLevelIndex, onlineLevelsList.Count - nextLevelIndex))
            {
                if (!LevelIsFiltered(level))
                    return level;
            }
            return null;
        }
        else
        {
            if (currentLevel.levelIndex + 1 < nLevels)
                return LevelManager.GetSLevelFromFile(currentLevel.levelIndex + 1);
            else
                return null;
        }
    }

    /// <summary>
    /// Resets player local save data
    /// </summary>
    public static void ResetData()
    {
        levelMedals = new medalType[nLevels];

        for (int i = 0; i < levelMedals.Length; i++)
        {
            levelMedals[i] = medalType.none;
        }

        lastLevelCompleted = 0;
    }

    /// <summary>
    /// Gets the data from the system using <see cref="SaveSystem"/>
    /// </summary>
    public static void LoadData()
    {
        // save data on start load data in the first screen
        LevelsData data = SaveSystem.LoadLevelsData();

        if (data == null)
        {
            // data file not found, may be the first time
            Debug.Log("Data file not found");
            ResetData();
            return;
        }

        Debug.Log("Data found!" + Application.persistentDataPath);

        Debug.Log("Last level completed: " + lastLevelCompleted + ", n level medals: " + data.levelMedals.Length);
        lastLevelCompleted = data.lastLevelCompleted;


        levelMedals = new medalType[nLevels];
        numOfflineStars = 0;

        for (int i = 0; i < levelMedals.Length; i++)
        {
            int thisLevelMedal;
            if (i < data.levelMedals.Length)
                thisLevelMedal = data.levelMedals[i];
            else
                thisLevelMedal = 0;

            levelMedals[i] = (medalType)thisLevelMedal;
            numOfflineStars += thisLevelMedal;
        }

    }

    public static bool isWorldLocked(int worldNum)
    {
        if (worldNum >= worldMinStars.Length)
        {
            Debug.Log($"World {worldNum} not found");
            return true;
        }

        return numOfflineStars < worldMinStars[worldNum];
    }

    public static int getWorldNum(int levelNum)
    {
        Debug.Log($"Level {levelNum} is of the world {Mathf.FloorToInt((levelNum - 1) / 16)}");
        return Mathf.FloorToInt((levelNum - 1) / 16);
    }

    public static int getRealLevelNum(int levelNum)
    {
        return ((levelNum - 1) % 16) + 1;
    }

    public static int getCurrentWorldNum()
    {
        Debug.Assert(!CurrentLevelIsOnline());
        return getWorldNum(currentLevel.levelIndex);
    }

    public static int getWorldNum(sLevel level)
    {
        if (LevelIsOnline(level))
        {
            Debug.LogWarning("Trying to get the world num of an online level");
            return 0;
        }

        return getWorldNum(level.levelIndex);
    }


    /// <summary>
    /// Changes the medal type of a local level
    /// </summary>
    /// <param name="levelNum">the level number</param>
    /// <param name="medalType">the medal type to assign</param>
    public static void changeMedal(int levelNum, medalType medalType)
    {
        if (CurrentLevelIsOnline())
        {
            SaveOnlineInfo.AddLevelAndSave(currentLevel.levelId, medalType);
        }
        else
        {
            lastLevelCompleted = Mathf.Max(levelNum, lastLevelCompleted);

            if (levelMedals[levelNum] >= medalType)
                return;

            numOfflineStars += medalType - levelMedals[levelNum];

            Debug.Log($"Changing medal ({medalType})");

            levelMedals[levelNum] = medalType;

            SaveSystem.SaveLevelsData();
        }
    }
}
