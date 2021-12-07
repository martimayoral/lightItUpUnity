using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public static class UserConfig
{
    public static float animationSpeed = 0.2f;

    public static float musicVolume = .5f;
    public static float soundVolume = 1f;
}


[System.Serializable]
public class UserConfigData
{
    public float animationSpeed;
    public float musicVolume;
    public float soundVolume;

    public UserConfigData()
    {
        animationSpeed = UserConfig.animationSpeed;
        musicVolume = UserConfig.musicVolume;
        soundVolume = UserConfig.soundVolume;
    }
}

public static class SaveUserConfig
{
    readonly static string path = Application.persistentDataPath + "/userConfigData.save";

    public static void SaveUserConfigData()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);
        UserConfigData data = new UserConfigData();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void LoadUserConfigData()
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            UserConfigData data = formatter.Deserialize(stream) as UserConfigData;

            UserConfig.animationSpeed = data.animationSpeed;
            UserConfig.musicVolume = data.musicVolume;
            UserConfig.soundVolume = data.soundVolume;

            stream.Close();
        }
    }

}
