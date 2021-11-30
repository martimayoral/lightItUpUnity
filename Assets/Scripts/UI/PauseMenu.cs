using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    Animator animator;

    public Image winStarsImage;
    private Sprite[] starsSprites;

    public Button getMoreMovesButton;
    public Button nextLevelButton;

    static public PauseMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        starsSprites = Resources.LoadAll<Sprite>("Other/Stars");
        getMoreMovesButton.interactable = true;
        animator = GetComponent<Animator>();
    }

    void PlayBtnSelect()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Select);
    }

    public void BtnPause()
    {
        if (GameController.gameState == eGameState.Pause)
            return;

        Debug.Log("PAUSING...");
        PlayBtnSelect();

        GameController.gameState = eGameState.Pause;
        Time.timeScale = 0f;
        animator.SetTrigger("Pause");
    }

    public void Resume()
    {
        animator = GetComponent<Animator>();

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
        PlayBtnSelect();
        Resume();
    }

    public void BtnLoadMenu()
    {
        Time.timeScale = 1f;

        PlayBtnSelect();
        Debug.Log("LOADING MENU...");
        SceneLoader.Instance.LoadMenu();
    }

    public void BtnRestart()
    {
        PlayBtnSelect();
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

        animator.SetTrigger("Win");
        winStarsImage.sprite = starsSprites[(int) medal];
    }

    public void BtnNextLevel()
    {
        PlayBtnSelect();
        Debug.Log("Next level...");
        SceneLoader.Instance.LoadLevel(SceneLoader.levelNum + 1);
    }

    public void BtnAdToWinMoves()
    {
        PlayBtnSelect();
        AdsManager.Instance.PlayRewardedAdd(onRewardedAddSuccess);
    }

    void onRewardedAddSuccess()
    {
        Debug.Log("On reward success");
        GameController.Instance.addMoves(3);
        PauseMenu.Instance.Resume();
        getMoreMovesButton.interactable = false;
    }
}