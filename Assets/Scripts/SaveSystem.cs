using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class LevelsData
{
    public int lastLevelCompleted;
    public int[] levelMedals;

    public LevelsData()
    {
        lastLevelCompleted = LevelsController.lastLevelCompleted;

        levelMedals = new int[LevelsController.levelMedals.Length];

        for (int i = 0; i < LevelsController.levelMedals.Length; i++)
        {
            levelMedals[i] = (int) LevelsController.levelMedals[i];
        }
    }
}

public static class SaveSystem
{
    readonly static string path = Application.persistentDataPath + "medals.save";

    public static void SaveLevelsData ()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);
        LevelsData data = new LevelsData();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static LevelsData LoadLevelsData()
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            LevelsData data = formatter.Deserialize(stream) as LevelsData;

            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("Savefile not found");
            return null;
        }

    }

}
