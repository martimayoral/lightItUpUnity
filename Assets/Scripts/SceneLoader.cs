using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneLoader : MonoBehaviour
{
    public Animator transition;
    public readonly float transitionTime = 1f;

    static public SceneLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ExitApp()
    {
        StartCoroutine(Exit());

        IEnumerator Exit()
        {
            transition.SetTrigger("Start");

            yield return new WaitForSeconds(transitionTime);

            Application.Quit();
        }
    }



    public void LoadMenu(MenuController.eMenuPanel panel = MenuController.eMenuPanel.SELECTION)
    {
        StartCoroutine(LoadScene("Menu"));
        MenuController.startingPanel = panel;
    }

    public void LoadLevelEditor()
    {
        StartCoroutine(LoadScene("LevelEditor"));
    }

    public void LoadNextLevel()
    {
        LevelsController.ChangeCurrentLevelForNext();
        LoadLevel();
    }
    public void LoadLevel(sLevel slevel)
    {
        LevelsController.ChangeCurrentLevel(slevel);
        LoadLevel();
    }

    public void LoadLevel(int ln)
    {
        if (LevelsController.ChangeCurrentLevel(ln))
        {
            print("Level load Level " + ln);
            LoadLevel();
        }
        else
        {
            Debug.LogError("Trying to load a level that doesn't exist (" + ln + ")");
        }
    }

    void LoadLevel()
    {
        Debug.Log("hint = 0");
        LevelsController.hintNum = 0;
        PauseMenu.hintAvailable = true;
        StartCoroutine(LoadScene("Game"));
    }

    public void LoadHint()
    {
        LevelsController.LoadHint();
        StartCoroutine(LoadScene("Game"));
    }



    IEnumerator LoadScene(string sceneName)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);
    }


}
