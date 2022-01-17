using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelsController
{
    public static medalType[] levelMedals { get; private set; }
    public static int lastLevelCompleted { get; private set; }

    public const int nLevels = 17;

    public static void ResetData()
    {
        levelMedals = new medalType[nLevels];

        for (int i = 0; i < levelMedals.Length; i++)
        {
            levelMedals[i] = medalType.none;
        }

        lastLevelCompleted = 0;
    }


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

        for (int i = 0; i < levelMedals.Length; i++)
        {
            levelMedals[i] = (medalType)data.levelMedals[i];
        }

    }

    public static void changeMedal(int level, medalType medalType)
    {
        if(level == 0)
        {
            Debug.LogWarning("IS THIS EDITOR MODE?");
        }

        lastLevelCompleted = Mathf.Max(level, lastLevelCompleted);

        if (levelMedals[level] >= medalType)
            return;

        Debug.Log($"Changing medal ({medalType})");

        levelMedals[level] = medalType;


        SaveSystem.SaveLevelsData();
    }
}
