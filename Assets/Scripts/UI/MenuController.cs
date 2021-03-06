using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Components;

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
    [Header("Filter Levels Online")]
    [SerializeField] TMP_InputField filterName;
    [SerializeField] LocalizeStringEvent filterOrderByButtonText;
    [SerializeField] Toggle filterBiggestFirstToggle;


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
        OnlineLevelsController.InitOnlineLevelsList();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        UpdateLoggingUI();
        RefreshUserLevels(0, false);

        if (LevelsController.currentWorld == 0)
            prevWorldButton.interactable = false;
        if (LevelsController.currentWorld == worldUIControllers.Length - 1)
            nextWorldButton.interactable = false;

        offlineNumStars.text = LevelsController.numOfflineStars.ToString();
        onlineNumStars.text = OnlineLevelsController.GetNumOnlineStars().ToString();
        StartPanel();

        // CloudFirestore.Instance.UpdateStatsField("ulR4e49qRtEVFc11YsYj", 1, 1, 0);
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
        DBGText.Write("Updating logging UI " + (loggedIn ? "(Logged in)" : "(Not Logged in)"));
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
        //if (AuthController.username != null && AuthController.username != "")
        SceneLoader.Instance.LoadLevelEditor();
        //else
        //{
        //    ToastController.Instance.ToastWhite("Log in to create a level");
        //}
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
        OnlineLevelsController.onlineLevelsList.Clear();
        CloudFirestore.latestDoc = null;

        ClearUserLevels();

        PopulateUserLevels();
    }

    // Instanciates the level
    public void PopulateLevel(OnlineLevel level)
    {
        Transform levelObjectTransform = Instantiate(levelInfo, levelInfoParent);
        levelObjectTransform.GetComponent<LevelInfoController>().levelInfo = level;
    }

    // refresh the UI, not the list
    public void RefreshUserLevels(int start = 0, bool populate = true)
    {
        Debug.Log("Refreshing user levels");
        if (start == 0)
            ClearUserLevels();

        int added = 0;

        for (int i = start; i < OnlineLevelsController.onlineLevelsList.Count; i++)
        {
            OnlineLevel level = OnlineLevelsController.onlineLevelsList[i];
            if (!OnlineLevelsController.LevelIsFiltered(level))
            {
                PopulateLevel(level);
                added++;
            }

        }
        loadMoreLevels.transform.SetAsLastSibling();

        if (!populate)
            return;

        //if there was no many added, we add more, populating the levels
        if (OnlineLevelsController.onlineLevelsList.Count != 0 && UserConfig.onlineMedalsOptions[(int)medalType.NONE])
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
        int initialSize = OnlineLevelsController.onlineLevelsList.Count;


        CloudFirestore.Instance.PopulateListAndDoActionAsync(
            (isLast) =>
                {

                    RefreshUserLevels(initialSize);

                    loadMoreLevelsButton.interactable = (!isLast);

                    // when is the first time, we have to set the sroll bar on top
                    if (initialSize == 0)
                    {
                        StartCoroutine(ResetScrollBar());

                        IEnumerator ResetScrollBar()
                        {
                            yield return new WaitForEndOfFrame();
                            levelsScrollRect.verticalNormalizedPosition = 1;
                        }
                    }
                });
    }



    public void BtnUserLevelsPanel()
    {
        if (OnlineLevelsController.onlineLevelsList.Count == 0)
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

    // settings user levels
    public void OpenFilterPanel()
    {
        filterName.text = UserConfig.filterOnlineName;

        filterOrderByButtonText.StringReference.TableEntryReference = OptionOrderToString(UserConfig.orderOnlineListBy);

        filterBiggestFirstToggle.isOn = UserConfig.orderOnlineListAscending;
    }

    public void BtnFilter()
    {
        UserConfig.orderOnlineListBy = OptionOrderToEnum(filterOrderByButtonText.StringReference.TableEntryReference);
        UserConfig.filterOnlineName = filterName.text;
        UserConfig.orderOnlineListAscending = filterBiggestFirstToggle.isOn;

        SaveUserConfig.SaveUserConfigData();
        ResetUserLevelsList();
    }

    public void BtnChangeOrderBy()
    {
        CloudFirestore.eOrderListBy orderListBy = OptionOrderToEnum(filterOrderByButtonText.StringReference.TableEntryReference);

        orderListBy = (CloudFirestore.eOrderListBy)(((int)orderListBy + 1) % Enum.GetNames(typeof(CloudFirestore.eOrderListBy)).Length);

        filterOrderByButtonText.StringReference.TableEntryReference = OptionOrderToString(orderListBy);
    }

    string OptionOrderToString(CloudFirestore.eOrderListBy orderListBy)
    {
        switch (orderListBy)
        {
            case CloudFirestore.eOrderListBy.TIMESTAMP:
                return "creation time";
            case CloudFirestore.eOrderListBy.NAME:
                return "level name";
            case CloudFirestore.eOrderListBy.CREATOR_NAME:
                return "creator name";
            case CloudFirestore.eOrderListBy.TIMES_PLAYED:
                return "times played";
            case CloudFirestore.eOrderListBy.DIFFICULTY:
                return "dificulty";
            case CloudFirestore.eOrderListBy.MOVES:
                return "number of moves";
            case CloudFirestore.eOrderListBy.LEVEL_SIZE:
                return "level size";
            default:
                return "creation time";
        }
    }
    CloudFirestore.eOrderListBy OptionOrderToEnum(string orderListBy)
    {
        switch (orderListBy)
        {
            case "creation time":
                return CloudFirestore.eOrderListBy.TIMESTAMP;
            case "level name":
                return CloudFirestore.eOrderListBy.NAME;
            case "creator name":
                return CloudFirestore.eOrderListBy.CREATOR_NAME;
            case "times played":
                return CloudFirestore.eOrderListBy.TIMES_PLAYED;
            case "dificulty":
                return CloudFirestore.eOrderListBy.DIFFICULTY;
            case "number of moves":
                return CloudFirestore.eOrderListBy.MOVES;
            case "level size":
                return CloudFirestore.eOrderListBy.LEVEL_SIZE;
            default:
                return CloudFirestore.eOrderListBy.NAME;
        }
    }

    public void BtnChangeLanguage()
    {
        LanguageManager.NextLanguage();
    }
}
