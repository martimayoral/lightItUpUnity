using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MenuController : MonoBehaviour
{
    public static int levelNum = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                levelNum %= 5; //TBD
                levelNum++;
                print("loading level " + levelNum);
                LevelLoader.Instance.LoadLevel();
            }        
        }
    }
}
