using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfoController : MonoBehaviour
{
    public sLevel levelInfo;

    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI creatorNameText;
    [SerializeField] TextMeshProUGUI sizeText;
    [SerializeField] Image medalImage;

    // Start is called before the first frame update
    void Start()
    {
        if (levelInfo.levelName == null)
        {
            Debug.Log("Level name was not defined for level info controller");
            return;
        }

        levelNameText.text = levelInfo.levelName;
        creatorNameText.text = levelInfo.creatorName.ToString();
        sizeText.text = levelInfo.levelSize.ToString();

        if (SaveOnlineInfo.levelsPlayed.ContainsKey(levelInfo.levelId))
        {
            medalType medal = SaveOnlineInfo.levelsPlayed[levelInfo.levelId];
            medalImage.sprite = Resources.LoadAll<Sprite>("Other/Stars")[(int)medal];
        }

    }

    public void BtnOnClick()
    {
        SceneLoader.Instance.LoadLevel(levelInfo);
    }
}
