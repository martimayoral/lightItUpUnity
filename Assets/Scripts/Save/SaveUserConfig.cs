using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public static class UserConfig
{
    public static float animationSpeed = 0.2f;

    public static float musicVolume = .5f;
    public static float soundVolume = 1f;

    public static readonly int onlineLoadBatchSize = 6;
    public static bool[] onlineMedalsOptions = { true, true, true, true };
}


[System.Serializable]
public class UserConfigData
{
    public float animationSpeed;
    public float musicVolume;
    public float soundVolume;
    public bool[] onlineMedalsOptions;

    public UserConfigData()
    {
        animationSpeed = UserConfig.animationSpeed;
        musicVolume = UserConfig.musicVolume;
        soundVolume = UserConfig.soundVolume;
        onlineMedalsOptions = UserConfig.onlineMedalsOptions;
    }
}

public static class SaveUserConfig
{
    readonly static string path = Application.persistentDataPath + "/userConfigData.txt";

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

            if (data.onlineMedalsOptions != null)
                UserConfig.onlineMedalsOptions = data.onlineMedalsOptions;
        }
    }

}
