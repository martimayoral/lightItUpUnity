using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    // hints
    public static int hintNum = 0;


    /// <summary>
    /// Returns true if current level is from online online
    /// </summary>
    public static bool CurrentLevelIsOnline()
    {
        return LevelIsOnline(currentLevel);
    }

    public static bool LevelIsOnline(sLevel level)
    {
        Debug.Log("Level is online?? " + (level is OnlineLevel).ToString());
        return level is OnlineLevel;
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

    public static void LoadHint()
    {
        hintNum++;
        Debug.Log("Exists " + $"Assets/Resources/HintLevels/Level {currentLevel.levelIndex}.{hintNum + 1}.json? " + File.Exists($"Assets/Resources/HintLevels/Level {currentLevel.levelIndex}.{hintNum + 1}.json").ToString());
        PauseMenu.hintAvailable = File.Exists($"Assets/Resources/HintLevels/Level {currentLevel.levelIndex}.{hintNum + 1}.json");
        ChangeCurrentLevel(LevelManager.GetSLevelFromFile($"HintLevels/Level {currentLevel.levelIndex}.{hintNum}", currentLevel.levelIndex));
    }

    public static sLevel GetNextLevel()
    {
        if (CurrentLevelIsOnline())
        {
            int nextLevelIndex = currentLevel.levelIndex + 1;
            foreach (sLevel level in OnlineLevelsController.onlineLevelsList.GetRange(nextLevelIndex, OnlineLevelsController.onlineLevelsList.Count - nextLevelIndex))
            {
                if (!OnlineLevelsController.LevelIsFiltered((OnlineLevel)level))
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
            levelMedals[i] = medalType.NONE;
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
            SaveOnlineInfo.AddLevelAndSave(((OnlineLevel)currentLevel).levelId, medalType);
        }
        else
        {
            //if (medalType == medalType.JUST_TRIED)
            //    return;

            lastLevelCompleted = Mathf.Max(levelNum, lastLevelCompleted);

            if (levelMedals[levelNum] >= medalType)
                return;

            numOfflineStars += medalType - levelMedals[levelNum];

            Debug.Log($"Changing medal ({medalType})");

            levelMedals[levelNum] = medalType;

            SaveSystem.SaveLevelsData();

            Firebase.Analytics.FirebaseAnalytics
                .LogEvent(
                    "change_medal_event",
                    new Firebase.Analytics.Parameter[] {
                        new Firebase.Analytics.Parameter("level", levelNum),
                        new Firebase.Analytics.Parameter("medal", medalType.ToString()),
                    }
                );
        }
    }

    public static void OnTimePlayed(int levelNum)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("level_played", "level_id", levelNum);
    }
}
