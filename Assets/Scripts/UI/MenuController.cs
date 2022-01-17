using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    Animator animator;

    [Header("Level")]
    [SerializeField] Transform levelInfo;
    [SerializeField] Transform levelInfoParent;

    [Header("Settings Panel")]
    [SerializeField] GameObject loggedInUI;
    [SerializeField] GameObject logInUI;
    [SerializeField] Animator settingsPanelAnimator;

    public static MenuController Instance;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        UpdateLoggingUI();
    }

    public void ExitApp()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }

    public void UpdateLoggingUI()
    {
        bool loggedIn = AuthController.Instance.isLoggedIn();
        Debug.Log("Updating logging UI " + (loggedIn ? "(Logged in)" : "(Not Logged in)"));
        if (loggedIn)
        {
            loggedInUI.SetActive(true);
            logInUI.SetActive(false);
        }
        else
        {
            loggedInUI.SetActive(false);
            logInUI.SetActive(true);
        }
    }
    public void SettingsPanelShowInit()
    {
        settingsPanelAnimator.SetTrigger("Init");
    }
    public void BtnLoadLevelEditor()
    {
        if (AuthController.username != null && AuthController.username != "")
            SceneLoader.Instance.LoadLevelEditor();
        else
        {
            ToastController.Instance.ToastWhite("Log in to create a level");
        }
    }

    public void BtnChangePanel(string triger)
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Select);
        animator.SetTrigger(triger);
    }

    public void BtnSaveUserConfig()
    {
        SaveUserConfig.SaveUserConfigData();
    }

    public void BtnUserLevelsPanel()
    {
        for (int i = 0; i < levelInfoParent.childCount; i++)
        {
            Destroy(levelInfoParent.GetChild(i).gameObject);
        }

        Action<sLevel> OnLevelLoad = (level) =>
         {
             Debug.Log(String.Format(":) {0}", level.levelName));

             Transform levelObjectTransform = Instantiate(levelInfo, levelInfoParent);
             levelObjectTransform.GetComponent<LevelInfoController>().levelInfo = level;
         };

        CloudFirestore.Instance.DoActionForEachLevelAsync(OnLevelLoad);
    }

}
