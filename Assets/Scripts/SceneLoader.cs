using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    static public int levelNum { get; private set; } // static bc it has to persist between scenes

    static public sLevel level { get; private set; }

    static public SceneLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public void LoadMenu()
    {
        StartCoroutine(LoadScene("Menu"));
    }

    public void LoadLevelEditor()
    {
        StartCoroutine(LoadScene("LevelEditor"));
    }

    public void LoadLevel(sLevel slevel)
    {
        levelNum = 0;
        level = slevel;
        StartCoroutine(LoadScene("Game"));
    }

    public void LoadLevel(int ln)
    {
        if (ln < LevelsController.nLevels)
        {
            levelNum = ln;
            print("Level load Level " + ln);
            level = LevelManager.GetSLevelFromFile(ln);
            StartCoroutine(LoadScene("Game"));
        }
        else
        {
            Debug.LogError("Trying to load a level that doesn't exist (" + ln + ")");
        }
    }

    IEnumerator LoadScene(string sceneName)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);
    }


}
