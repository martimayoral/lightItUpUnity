using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class helpMsg
{
    public int timeMoves;
    public string action;
    public string message;
}

[System.Serializable]
class levelMsg
{
    public int level;
    public helpMsg[] helpMsgs;
}

[System.Serializable]
class levelsHelpData
{
    public levelMsg[] levelMsgs;
}

public static class SaveHelpText
{
    public static Dictionary<int, Dictionary<int, helpMsg>> levelMsgs;

    public static void LoadHelpTextData()
    {
        levelMsgs = new Dictionary<int, Dictionary<int, helpMsg>>();

        levelsHelpData levelsHelpData = JsonUtility.FromJson<levelsHelpData>(Resources.Load("helpText").ToString());

        foreach (levelMsg level in levelsHelpData.levelMsgs)
        {
            Dictionary<int, helpMsg> helpMsgs = new Dictionary<int, helpMsg>();

            foreach (helpMsg msg in level.helpMsgs)
            {
                Debug.Assert(!helpMsgs.ContainsKey(msg.timeMoves));
                helpMsgs.Add(msg.timeMoves, msg);
            }
            Debug.Assert(!levelMsgs.ContainsKey(level.level));
            levelMsgs.Add(level.level, helpMsgs);
        }

    }

}
