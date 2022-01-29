using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


[System.Serializable]
public struct SaveOnlineInfoData
{
    public string id;
    public int medal;
}

[System.Serializable]
public struct SaveOnlineInfoDataList
{
    public List<SaveOnlineInfoData> saveOnlineInfoDataList;
}

public static class SaveOnlineInfo
{
    readonly static string path = Application.persistentDataPath + "/onlineLevelsPlayed.json";

    public static Dictionary<string, medalType> levelsPlayed;

    public static void AddLevelAndSave(string databaseId, medalType medal)
    {
        AddLevel(databaseId, medal);
        SaveOnlineInfoData();
    }

    static void AddLevel(string databaseId, medalType medal)
    {
        if (levelsPlayed.ContainsKey(databaseId))
        {
            if (levelsPlayed[databaseId] >= medal)
                return;
        }
        levelsPlayed[databaseId] = medal;
    }

    static List<SaveOnlineInfoData> getListFromDic()
    {
        List<SaveOnlineInfoData> list = new List<SaveOnlineInfoData>();

        foreach (var level in levelsPlayed)
        {
            list.Add(new SaveOnlineInfoData()
            {
                id = level.Key,
                medal = (int)level.Value
            });
        }

        return list;
    }

    public static void SaveOnlineInfoData()
    {
        StreamWriter stream = File.CreateText(path);

        SaveOnlineInfoDataList data = new SaveOnlineInfoDataList { saveOnlineInfoDataList = getListFromDic() };

        stream.Write(JsonUtility.ToJson(data));

        stream.Close();
    }


    public static void InitOnlineInfoData()
    {
        levelsPlayed = new Dictionary<string, medalType>();

        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);

            SaveOnlineInfoDataList data = JsonUtility.FromJson<SaveOnlineInfoDataList>(text);

            foreach (SaveOnlineInfoData save in data.saveOnlineInfoDataList)
            {
                levelsPlayed.Add(save.id, (medalType)save.medal);
            }
        }
    }

}
