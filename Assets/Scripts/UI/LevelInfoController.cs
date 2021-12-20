using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelInfoController : MonoBehaviour
{

    public sLevel levelInfo;

    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI creatorNameText;
    [SerializeField] TextMeshProUGUI sizeText;

    // Start is called before the first frame update
    void Start()
    {
        if(levelInfo.levelName == null)
        {
            Debug.Log("Level name was not defined for level info controller");
            return;
        }

        levelNameText.text = levelInfo.levelName;
        creatorNameText.text = levelInfo.creatorName.ToString();
        sizeText.text = levelInfo.levelSize.ToString();
    }

    public void BtnOnClick()
    {
        SceneLoader.Instance.LoadLevel(levelInfo);
    }
}
