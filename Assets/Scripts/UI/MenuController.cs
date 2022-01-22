using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    Animator animator;

    [Header("Levels Online")]
    [SerializeField] Transform levelInfo;
    [SerializeField] Transform levelInfoParent;
    [SerializeField] GameObject loadMoreLevels;
    [SerializeField] Button loadMoreLevelsButton;
    [SerializeField] ScrollRect levelsScrollRect;

    [Header("Settings Panel")]
    [SerializeField] GameObject loggedInUI;
    [SerializeField] GameObject logInUI;
    [SerializeField] Animator settingsPanelAnimator;

    public static MenuController Instance;

    void Awake()
    {
        Instance = this;
        LevelsController.InitOnlineLevelsList();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        UpdateLoggingUI();
        RefreshUserLevels();
    }

    public void ExitApp()
    {
        Debug.Log("QUIT");
        SceneLoader.Instance.ExitApp();
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

    // removes all level info in the UI
    void ClearUserLevels()
    {
        for (int i = 0; i + 1 < levelInfoParent.childCount; i++)
        {
            Destroy(levelInfoParent.GetChild(i).gameObject);
        }
    }

    // reset list, is done at the begining
    public void ResetUserLevelsList()
    {
        LevelsController.onlineLevelsList.Clear();
        CloudFirestore.latestDoc = null;

        ClearUserLevels();

        PopulateUserLevels();
    }

    // Instanciates the level
    public void PopulateLevel(sLevel level)
    {
        Transform levelObjectTransform = Instantiate(levelInfo, levelInfoParent);
        levelObjectTransform.GetComponent<LevelInfoController>().levelInfo = level;
    }

    // refresh the UI, not the list
    public void RefreshUserLevels(int start = 0)
    {

        DBGText.Write("A1");
        if (start == 0)
            ClearUserLevels();

        DBGText.Write("A2");
        int added = 0;

        DBGText.Write("A3: " + LevelsController.onlineLevelsList.Count);
        for (int i = start; i < LevelsController.onlineLevelsList.Count; i++)
        {
            DBGText.Write("A4." + i);
            sLevel level = LevelsController.onlineLevelsList[i];
            if (!LevelsController.LevelIsFiltered(level))
            {
                PopulateLevel(level);
                added++;
            }

        }
        DBGText.Write("A3");
        loadMoreLevels.transform.SetAsLastSibling();

        // if there was no many added, we add more, populating the levels
        //  if ((added < (UserConfig.onlineLoadBatchSize * 0.5)) && loadMoreLevelsButton.interactable)
        //  {
        //      PopulateUserLevels();
        //  }

    }

    public void PopulateUserLevels()
    {
        int initialSize = LevelsController.onlineLevelsList.Count;


        CloudFirestore.Instance.PopulateListAndDoActionAsync(
            (isLast) =>
                {

                    DBGText.Write("ACTION init");

                    DBGText.Write("A");

                    RefreshUserLevels(initialSize);
                    DBGText.Write("B");

                    loadMoreLevelsButton.interactable = (!isLast);
                    DBGText.Write("C");

                    // when is the first time, we have to set the sroll bar on top
                    if (initialSize == 0)
                    {
                        DBGText.Write("D");
                        StartCoroutine(ResetScrollBar());

                        DBGText.Write("E");
                        IEnumerator ResetScrollBar()
                        {
                            DBGText.Write("F");
                            yield return new WaitForEndOfFrame();
                            DBGText.Write("G");
                            levelsScrollRect.verticalNormalizedPosition = 1;
                        }
                    }
                    DBGText.Write("H");
                    DBGText.Write("ACTION end");
                },
            UserConfig.onlineLoadBatchSize);

    }


    public void BtnUserLevelsPanel()
    {
        if (LevelsController.onlineLevelsList.Count == 0)
            ResetUserLevelsList();

    }

}
