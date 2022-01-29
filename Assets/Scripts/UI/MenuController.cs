using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    Animator animator;

    [Header("Campaign Levels")]
    [SerializeField] WorldUIController[] worldUIControllers;
    [SerializeField] CanvasGroup nextWorldButton;
    [SerializeField] CanvasGroup prevWorldButton;
    [SerializeField] TextMeshProUGUI offlineNumStars;

    [Header("Levels Online")]
    [SerializeField] Transform levelInfo;
    [SerializeField] Transform levelInfoParent;
    [SerializeField] GameObject loadMoreLevels;
    [SerializeField] Button loadMoreLevelsButton;
    [SerializeField] ScrollRect levelsScrollRect;
    [SerializeField] TextMeshProUGUI onlineNumStars;

    [Header("Settings Panel")]
    [SerializeField] GameObject loggedInUI;
    [SerializeField] GameObject logInUI;
    [SerializeField] Animator settingsPanelAnimator;


    public static MenuController Instance;

    public enum eMenuPanel
    {
        SELECTION,
        WORLDS,
        USER_LEVELS
    }
    public static eMenuPanel startingPanel = eMenuPanel.SELECTION;

    void Awake()
    {
        Instance = this;
        LevelsController.InitOnlineLevelsList();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        UpdateLoggingUI();
        RefreshUserLevels();

        if (LevelsController.currentWorld == 0)
            prevWorldButton.interactable = false;
        if (LevelsController.currentWorld == worldUIControllers.Length - 1)
            nextWorldButton.interactable = false;

        offlineNumStars.text = LevelsController.numOfflineStars.ToString();
        onlineNumStars.text = LevelsController.GetNumOnlineStars().ToString();
        StartPanel();
    }

    void StartPanel()
    {
        switch (startingPanel)
        {
            case eMenuPanel.WORLDS:
                animator.SetTrigger("Worlds");
                break;
            case eMenuPanel.USER_LEVELS:
                animator.SetTrigger("UserLevels");
                break;
            default:
                break;
        }
        startingPanel = eMenuPanel.SELECTION;
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
        Debug.Log("Refreshing user levels");
        if (start == 0)
            ClearUserLevels();

        int added = 0;

        for (int i = start; i < LevelsController.onlineLevelsList.Count; i++)
        {
            sLevel level = LevelsController.onlineLevelsList[i];
            if (!LevelsController.LevelIsFiltered(level))
            {
                PopulateLevel(level);
                added++;
            }

        }
        loadMoreLevels.transform.SetAsLastSibling();

        //if there was no many added, we add more, populating the levels
        if (LevelsController.onlineLevelsList.Count != 0 && UserConfig.onlineMedalsOptions[(int)medalType.none])
        {
            Debug.Log($"Added: {added} < {UserConfig.onlineLoadBatchSize * 0.5}?");
            if ((added < (UserConfig.onlineLoadBatchSize * 0.5)) && loadMoreLevelsButton.interactable)
            {
                Debug.Log("Yes!");
                PopulateUserLevels();
            }
        }

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
                });

    }


    public void BtnUserLevelsPanel()
    {
        if (LevelsController.onlineLevelsList.Count == 0)
            ResetUserLevelsList();

    }

    public void NextWorld()
    {
        int nextWorld = LevelsController.currentWorld + 1;
        if (nextWorld >= worldUIControllers.Length)
        {
            Debug.LogError("World not found");
            return;
        }

        worldUIControllers[LevelsController.currentWorld].OutLeft();
        worldUIControllers[nextWorld].InRight();

        if (nextWorld == worldUIControllers.Length - 1)
        {
            nextWorldButton.interactable = false;
        }

        prevWorldButton.interactable = true;

        LevelsController.currentWorld = nextWorld;
    }

    public void PrevWorld()
    {
        int prevWorld = LevelsController.currentWorld - 1;
        if (prevWorld < 0)
        {
            Debug.LogError("World not found");
            return;
        }

        worldUIControllers[LevelsController.currentWorld].OutRight();
        worldUIControllers[prevWorld].InLeft();

        if (prevWorld == 0)
            prevWorldButton.interactable = false;

        nextWorldButton.interactable = true;

        LevelsController.currentWorld = prevWorld;
    }


}
