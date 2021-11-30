using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadData());
    }

    IEnumerator LoadData()
    {
        LevelsController.LoadData();

        yield return new WaitForSeconds(2.0f);

        SceneLoader.Instance.LoadMenu();
    }

}
