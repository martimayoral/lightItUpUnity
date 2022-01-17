using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] GameObject fullScreenSpinner;

    public static LoadingScreen Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartFullScreenSpinner() {
        fullScreenSpinner.SetActive(true);
    }

    public void StopAll()
    {
        fullScreenSpinner.SetActive(false);
    }
}
