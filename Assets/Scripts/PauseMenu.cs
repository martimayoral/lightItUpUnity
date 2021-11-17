using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;

    public GameObject pausedUI;
    public GameObject notPausedUI;

    // Start is called before the first frame update
    void Start()
    {
        pausedUI.SetActive(false);
        notPausedUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pause()
    {
        gameIsPaused = true;
        pausedUI.SetActive(true);
        notPausedUI.SetActive(false);

    }

    public void Resume()
    {
        gameIsPaused = false;
        pausedUI.SetActive(false);
        notPausedUI.SetActive(true);

    }


}
