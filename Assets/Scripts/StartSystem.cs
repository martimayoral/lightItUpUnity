using Firebase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitAndChangeScene());
    }

    IEnumerator InitAndChangeScene()
    {

        DBGText.Write("FirebaseApp Init");
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkAndFixDependenciesTask.IsCompleted);
        DBGText.Write("FirebaseApp initiated with result:" + checkAndFixDependenciesTask.Result.ToString());


        DBGText.Write("Loading online data...");
        SaveOnlineInfo.InitOnlineInfoData();
        LevelsController.InitOnlineLevelsList();
        DBGText.Write("Done!");

        DBGText.Write("Loading user data and config...");
        SaveHelpText.LoadHelpTextData();
        LevelsController.LoadData();
        SaveUserConfig.LoadUserConfigData();
        DBGText.Write("Done!");

        DBGText.Write("Waiting 2 seconds for dramatic effect...");
        yield return new WaitForSeconds(2.0f);
        DBGText.Write("Done!");

        DBGText.Write("Loading menu...");
        SceneLoader.Instance.LoadMenu();
    }

}
