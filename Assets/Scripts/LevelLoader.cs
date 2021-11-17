using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    static public LevelLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public void LoadMenu()
    {
        StartCoroutine(LoadScene(0));
    }
    public void LoadLevel()
    {
        StartCoroutine(LoadScene(1));
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneIndex);
    }

}
