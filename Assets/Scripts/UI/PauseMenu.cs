using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public Animator animator;

    public Image winStarsImage;
    private Sprite[] starsSprites;

    public Button getMoreMovesButton;
    public Button nextLevelButton;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI nextLevelName;

    bool currentLevelOnline;

    static public PauseMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        AdsManager.Instance.ShowBanner();
    }

    private void OnDestroy()
    {
        AdsManager.Instance.HideBanner();
    }

    // Start is called before the first frame update
    void Start()
    {
        starsSprites = Resources.LoadAll<Sprite>("UI/stars");
        getMoreMovesButton.interactable = true;

        currentLevelOnline = LevelsController.CurrentLevelIsOnline();

        LevelManager.Instance.LoadMap(LevelsController.currentLevel, false);

        // start all objects and values
        GameController.Instance.HardResetGame();

        GameController.gameState = eGameState.Play;
    }


    public void BtnPause()
    {
        if (GameController.gameState == eGameState.Pause)
            return;

        Debug.Log("PAUSING...");

        GameController.gameState = eGameState.Pause;
        Time.timeScale = 0f;
        animator.SetTrigger("Pause");
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        Debug.Log("RESUMING...");
        animator.SetTrigger("Resume");

        StartCoroutine(doAnimation());

        IEnumerator doAnimation()
        {
            yield return new WaitForSeconds(0.25f);
            GameController.gameState = eGameState.Play;
        }
    }

    public void BtnResume()
    {
        Resume();
    }

    public void BtnLoadMenu()
    {
        Time.timeScale = 1f;

        Debug.Log("LOADING MENU...");
        SceneLoader.Instance.LoadMenu(currentLevelOnline ? MenuController.eMenuPanel.USER_LEVELS : MenuController.eMenuPanel.WORLDS);
    }

    public void BtnRestart()
    {
        LevelManager.Instance.RestartLevel();
        getMoreMovesButton.interactable = true;
        Resume();
        Debug.Log("RESTARTING...");
    }

    public void Lose()
    {
        Debug.Log("LOSE...");
        animator.SetTrigger("Lose");
    }

    public void Win(medalType medal)
    {
        Debug.Log("WIN...");

        bool nextWorldUnlocked = false;
        int nextWorldNum = -1;

        if (!currentLevelOnline)
            nextWorldNum = LevelsController.getCurrentWorldNum() + 1;
        // check if the next world is unlocked

        if (!currentLevelOnline && LevelsController.isWorldLocked(nextWorldNum))
            nextWorldUnlocked = true;

        LevelsController.changeMedal(LevelsController.currentLevel.levelIndex, medal);

        // check if when changed the medal, the next world is unlocked
        if (!currentLevelOnline && LevelsController.isWorldLocked(nextWorldNum))
        {
            nextWorldUnlocked = !nextWorldUnlocked;
        }

        sLevel nextLevel = LevelsController.GetNextLevel();

        if (nextLevel != null)
        {
            nextLevelText.text = "Next Level ->";
            nextLevelName.text = nextLevel.levelName;
            nextLevelButton.interactable = true;

            if (nextWorldUnlocked)
            {
                nextLevelText.text = "World unlocked!";
                nextLevelName.text = "Go to the menu to check it out!";
                nextLevelButton.interactable = false;
            }

            if (!currentLevelOnline && LevelsController.isWorldLocked(LevelsController.getWorldNum(nextLevel)))
            {
                nextLevelText.text = "Blocked!";
                nextLevelName.text = "Next level is blocked";
                nextLevelButton.interactable = false;
            }
        }
        else
        {
            nextLevelText.text = "Congratulations!";
            nextLevelName.text = "More levels comming soon...";
            nextLevelButton.interactable = false;

            if (currentLevelOnline)
            {
                CloudFirestore.Instance.PopulateListAndDoActionAsync((isLast) =>
                {
                    if (!isLast)
                        Win(medal);
                });
            }
        }

        animator.SetTrigger("Win");
        winStarsImage.sprite = starsSprites[(int)medal];


    }

    public void BtnNextLevel()
    {
        Debug.Log("Next level...");
        nextLevelButton.interactable = false;
        SceneLoader.Instance.LoadNextLevel();
    }

    public void BtnAdToWinMoves()
    {
        AdsManager.Instance.PlayRewardedAdd(onRewardedAddSuccess);

        void onRewardedAddSuccess()
        {
            Debug.Log("On reward success");
            GameController.Instance.addMoves(3);
            Instance.Resume();
            getMoreMovesButton.interactable = false;
        }
    }


}
