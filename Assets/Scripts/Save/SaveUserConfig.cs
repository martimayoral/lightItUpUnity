using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public static class UserConfig
{
    public static float animationSpeed = 0.2f;

    public static float musicVolume = .5f;
    public static float soundVolume = 1f;

    public static readonly int onlineLoadBatchSize = 30;
    public static bool[] onlineMedalsOptions = { true, true, true, true };
    public static CloudFirestore.eOrderListBy orderOnlineListBy = CloudFirestore.eOrderListBy.TIMESTAMP;
    public static bool orderOnlineListAscending = true;
    public static string filterOnlineName = "";
}


[System.Serializable]
public class UserConfigData
{
    public float animationSpeed;
    public float musicVolume;
    public float soundVolume;
    public bool[] onlineMedalsOptions;
    public bool orderOnlineListAscending;
    public int orderOnlineListBy;

    public UserConfigData()
    {
        animationSpeed = UserConfig.animationSpeed;
        musicVolume = UserConfig.musicVolume;
        soundVolume = UserConfig.soundVolume;
        onlineMedalsOptions = UserConfig.onlineMedalsOptions;
        orderOnlineListAscending = UserConfig.orderOnlineListAscending;
        orderOnlineListBy = (int)UserConfig.orderOnlineListBy;
    }
}

public static class SaveUserConfig
{
    readonly static string path = Application.persistentDataPath + "/userConfigData.json";

    public static void SaveUserConfigData()
    {
        StreamWriter stream = File.CreateText(path);

        //SaveOnlineInfoDataList data = new SaveOnlineInfoDataList { saveOnlineInfoDataList = getListFromDic() };
        UserConfigData data = new UserConfigData();

        stream.Write(JsonUtility.ToJson(data));

        stream.Close();
    }

    public static void LoadUserConfigData()
    {
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);

            UserConfigData data = JsonUtility.FromJson<UserConfigData>(text);

            UserConfig.animationSpeed = data.animationSpeed;
            UserConfig.musicVolume = data.musicVolume;
            UserConfig.soundVolume = data.soundVolume;
            UserConfig.orderOnlineListBy = (CloudFirestore.eOrderListBy)data.orderOnlineListBy;
            UserConfig.orderOnlineListAscending = data.orderOnlineListAscending;

            if (data.onlineMedalsOptions != null)
                UserConfig.onlineMedalsOptions = data.onlineMedalsOptions;

        }
    }

}
