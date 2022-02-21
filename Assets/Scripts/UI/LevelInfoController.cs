using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfoController : MonoBehaviour
{
    public OnlineLevel levelInfo;

    [Header("Basic info")]
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI creatorNameText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] Image medalImage;

    [Header("More info")]
    [SerializeField] TextMeshProUGUI createdAtText;
    [SerializeField] TextMeshProUGUI timesPlayedText;
    [SerializeField] TextMeshProUGUI winsPrText;
    [SerializeField] TextMeshProUGUI winsPrGoldText;

    [SerializeField] TextMeshProUGUI movesText;
    [SerializeField] TextMeshProUGUI bronzeMovesText;
    [SerializeField] TextMeshProUGUI silverMovesText;
    [SerializeField] TextMeshProUGUI goldMovesText;

    [SerializeField] TextMeshProUGUI nBlueText;
    [SerializeField] TextMeshProUGUI nGreenText;
    [SerializeField] TextMeshProUGUI nRedText;

    [SerializeField] TextMeshProUGUI sizeText;


    bool moreInfoActive;

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

        infoText.color = Color.white;
        switch (UserConfig.orderOnlineListBy)
        {
            case CloudFirestore.eOrderListBy.TIMES_PLAYED:
                infoText.text = levelInfo.stats.timesPlayed.ToString();
                break;
            case CloudFirestore.eOrderListBy.LEVEL_SIZE:
                infoText.text = levelInfo.levelSize.ToString();
                break;
            case CloudFirestore.eOrderListBy.MOVES:
                int nMoves = levelInfo.score.startingMoves - levelInfo.score.goldMoves;
                infoText.text = nMoves.ToString() + (nMoves == 1 ? " move" : "moves");
                break;
            default:
                if (levelInfo.stats.timesPlayed == 0)
                {
                    infoText.text = "-% win";
                }
                else
                {
                    int pr = levelInfo.stats.wins * 100 / levelInfo.stats.timesPlayed;
                    if (pr < 20)
                    {
                        infoText.color = new Color(.8f, 0, 0);
                    }
                    else if (pr < 50)
                    {
                        infoText.color = Color.yellow;
                    }
                    else if (pr > 75)
                    {
                        infoText.color = new Color(0, .8f, 0);
                    }
                    infoText.text = pr.ToString() + "% win";
                }
                break;
        }

        if (SaveOnlineInfo.levelsPlayed.ContainsKey(levelInfo.levelId))
        {
            medalType medal = SaveOnlineInfo.levelsPlayed[levelInfo.levelId];
            if (medal < 0) medal = medalType.NONE;
            medalImage.sprite = Resources.LoadAll<Sprite>("UI/stars")[(int)medal];
        }

        moreInfoActive = false;
    }

    public void ShowInfo()
    {
        if (!moreInfoActive)
        {
            //show more info
            createdAtText.text = levelInfo.createdAt;
            timesPlayedText.text = levelInfo.stats.timesPlayed.ToString();
            if (levelInfo.stats.timesPlayed == 0)
            {
                winsPrText.text = "0%";
                winsPrGoldText.text = "0%";
            }
            else
            {
                winsPrText.text = (levelInfo.stats.wins * 100 / levelInfo.stats.timesPlayed).ToString() + "%";
                winsPrGoldText.text = (levelInfo.stats.goldMedals * 100 / levelInfo.stats.timesPlayed).ToString() + "%";
            }
            movesText.text = levelInfo.score.startingMoves.ToString();
            bronzeMovesText.text = levelInfo.score.bronzeMoves.ToString();
            silverMovesText.text = levelInfo.score.silverMoves.ToString();
            goldMovesText.text = levelInfo.score.goldMoves.ToString();

            var tileTypes = TileMapUtils.CountTileTypes(levelInfo.tiles);
            nBlueText.text = tileTypes[TileType.Blue].ToString();
            nGreenText.text = tileTypes[TileType.Green].ToString();
            nRedText.text = tileTypes[TileType.Red].ToString();

            sizeText.text = levelInfo.levelSize.ToString();
        }

        moreInfoActive = true;
    }

    public void BtnOnClick()
    {
        OnlineLevelsController.AddTimePlayed(levelInfo);
        SceneLoader.Instance.LoadLevel(levelInfo);
    }
}
