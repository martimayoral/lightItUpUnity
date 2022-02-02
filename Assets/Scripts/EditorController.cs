using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class EditorController : MonoBehaviour
{
    readonly int maxMoves = 99;
    readonly int minMoves = 3;
    readonly int minStarMoves = 0;


    [System.Serializable]
    public enum Tool
    {
        None,
        Eraser,
        PaintTile
    }

    TileType tileTypeToPaint;
    Tool tool;
    ToolSelectComponent toolSelectedComponent;

    Tilemap tilemap;

    // level info
    static sLevel level;
    Scores scores;
    eLevelSize size;

    bool canPublish;

    public GameController gameController;
    public Animator animatorUI;

    [SerializeField] TextMeshProUGUI exitTitle;
    [SerializeField] TextMeshProUGUI exitSubtitle;
    [SerializeField] Button exitPublishButton;
    [SerializeField] Button menuPublishButton;
    [SerializeField] GameObject leftoverMovements;
    [SerializeField] TextMeshProUGUI leftoverMovementsText;
    int leftOverMoves;

    [Header("Moves Settings")]
    [SerializeField] TextMeshProUGUI settingsScoresMoves;
    [SerializeField] TextMeshProUGUI settingsScoresGold;
    [SerializeField] TextMeshProUGUI settingsScoresSilver;
    [SerializeField] TextMeshProUGUI settingsScoresBronze;

    [Header("Map size Setting")]
    [SerializeField] TextMeshProUGUI settingsMapSize;
    [SerializeField] Button lessMapSizeButton;
    [SerializeField] Button moreMapSizeButton;

    [Header("Publish")]
    [SerializeField] TextMeshProUGUI settingsPublishText;
    [SerializeField] TMP_InputField publishName;
    [SerializeField] TextMeshProUGUI publishingAs;
    [SerializeField] Button settingsPublishButton;

    private void Start()
    {
        tool = Tool.None;
        tilemap = FindObjectOfType<Tilemap>();

        canPublish = true;
        DisablePublish();
        publishingAs.text = "Publishing as " + AuthController.username;

        if (level == null)
        {
            Debug.Log("Initiating Editor");
            size = eLevelSize.Small;
            TileMapUtils.LoadMapBorders(tilemap, true, size);
            scores = new Scores() { goldMoves = 5, silverMoves = 4, bronzeMoves = 2, startingMoves = 30 };

            UpdateSLevel();
        }
        else
        {
            size = level.levelSize;
            scores = level.score;
            Debug.Log($"Loading level (moves = {scores.startingMoves})");

        }

        LevelManager.Instance.LoadMap(level, true);
        gameController.HardResetGame();

        UpdateSettingsText();
    }

    public void EnableLeftOverMovesButton(int moves)
    {
        if (moves == 0)
        {
            DisableLeftOverMovesButton();
            return;
        }

        leftoverMovements.SetActive(true);
        leftOverMoves = moves;

        if (moves > 0)
        {
            leftoverMovementsText.text = $"Reduce moves leftover ({moves})";
        }
        else
        {
            leftoverMovementsText.text = $"Add {-moves} more moves";
        }
    }

    public void DisableLeftOverMovesButton()
    {
        leftoverMovements.SetActive(false);
    }

    public void EnablePublish()
    {
        if (!canPublish)
        {
            canPublish = true;
            menuPublishButton.interactable = true;
            exitPublishButton.interactable = true;
        }

    }

    public void DisablePublish()
    {
        if (canPublish)
        {
            canPublish = false;
            menuPublishButton.interactable = false;
            exitPublishButton.interactable = false;

        }
    }

    private void Update()
    {
        if (tool == Tool.None)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            //Debug.Log("PHASE: " + touch.phase);

            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            Vector3Int tilemapPos = tilemap.WorldToCell(touchPos);

            TileBase previousCell = tilemap.GetTile(tilemapPos);

            if (previousCell != null)
                if (((LevelTile)previousCell).type == TileType.BlockedBaseWall)
                {
                    Debug.Log("Couldn't use tool " + tool);
                    return;
                }

            UseTool(tilemapPos, tool);
        }
    }

    void UseTool(Vector3Int tilemapPos, Tool tool)
    {
        DisablePublish();
        switch (tool)
        {
            case Tool.Eraser:
                tilemap.SetTile(tilemapPos, null);
                break;
            case Tool.PaintTile:
                TileMapUtils.SetTile(tilemap, tilemapPos, tileTypeToPaint);
                break;
            default:
                Debug.Log("Tool not registered (" + tool + ")");
                break;
        }

    }

    public void SelectTool(ToolSelectComponent toolSelectComponent)
    {
        if (toolSelectedComponent != null)
            toolSelectedComponent.setSelected(false);

        toolSelectedComponent = toolSelectComponent;
        toolSelectedComponent.setSelected(true);

        tool = toolSelectComponent.tool;
        tileTypeToPaint = toolSelectComponent.type;
        Debug.Log(toolSelectComponent.type);
    }

    void LoadEditorMap()
    {
        LevelManager.Instance.LoadMap(level, true);
    }

    void UpdateSLevel()
    {
        Debug.Log($"Updating slevel (moves = {scores.startingMoves})");

        level = TileMapUtils.CreateLevel(publishName.text, 0, tilemap, scores, size);
        level.creatorName = AuthController.username;
    }

    public void BtnReduceMovesLeftover()
    {
        if (leftOverMoves > 0)
        {
            int startingMoves = scores.startingMoves;
            scores.startingMoves = scores.goldMoves + GameController.Instance.movesCount;
            LevelManager.Instance.scores.startingMoves = scores.startingMoves;

            GameController.Instance.substractMoves(startingMoves - scores.startingMoves);
            settingsScoresMoves.text = scores.startingMoves.ToString();
            leftoverMovements.SetActive(false);
        }
        else
        {
            scores.startingMoves += (-leftOverMoves);
            LevelManager.Instance.scores.startingMoves = scores.startingMoves;

            GameController.Instance.addMoves(-leftOverMoves);
            settingsScoresMoves.text = scores.startingMoves.ToString();
            BtnResume();
            animatorUI.SetTrigger("playStart");
        }

    }

    // --- Test game ---
    public void BtnPlay()
    {
        tool = Tool.None;

        Dictionary<TileType, int> tileTypesCountDic = TileMapUtils.CountTileTypes(tilemap);

        int numLightBulbs = tileTypesCountDic[TileType.Red] + tileTypesCountDic[TileType.Green] + tileTypesCountDic[TileType.Blue];
        if (numLightBulbs == 0)
        {
            ToastController.Instance.ToastRed("You need to add some light bulb to Play", ToastController.eToastType.TOP);
            return;
        }

        if (tileTypesCountDic[TileType.Red] > tileTypesCountDic[TileType.GoalRed])
        {
            ToastController.Instance.ToastRed("You need to have more red light sources", ToastController.eToastType.TOP);
            return;
        }
        if (tileTypesCountDic[TileType.Green] > tileTypesCountDic[TileType.GoalGreen])
        {
            ToastController.Instance.ToastRed("You need to have more green light sources", ToastController.eToastType.TOP);
            return;
        }
        if (tileTypesCountDic[TileType.Blue] > tileTypesCountDic[TileType.GoalBlue])
        {
            ToastController.Instance.ToastRed("You need to have more blue light sources", ToastController.eToastType.TOP);
            return;
        }


        UpdateSLevel();


        LevelManager.Instance.LoadMap(level, false);

        Debug.Log("Play start");
        animatorUI.SetTrigger("playStart");

        gameController.HardResetGame();
    }

    public void Win(medalType medal)
    {
        Debug.Log("EDITOR CONTROLLER WIN");
        animatorUI.SetTrigger("playEnd");
        exitTitle.text = "You won!";
        if (medal != medalType.gold)
        {
            exitSubtitle.text = "Well done! but you need 3 stars to publish";
            DisablePublish();
        }
        else
        {
            exitSubtitle.text = "You can now publish the game";

            Debug.Log($"Moves left {gameController.movesLeft}, gold moves {scores.goldMoves} ");
            EnablePublish();
        }

        EnableLeftOverMovesButton(gameController.movesLeft - scores.goldMoves);

    }

    public void Lose()
    {
        Debug.Log("EDITOR CONTROLLER LOSE");
        animatorUI.SetTrigger("playEnd");
        exitTitle.text = "You lost";
        exitSubtitle.text = "You need 3 stars to publish";
        DisablePublish();

        if (scores.startingMoves >= maxMoves)
            DisableLeftOverMovesButton();
        else
            EnableLeftOverMovesButton(Mathf.Max(-10, scores.startingMoves - maxMoves));
    }

    public void BtnContinueEditing()
    {
        Debug.Log("Continue editing");
        animatorUI.SetTrigger("menu");
        gameController.DeleteMovableObj();

        LoadEditorMap();
    }

    public void BtnPause()
    {
        Debug.Log("Pause");
        animatorUI.SetTrigger("playPause");
        GameController.gameState = eGameState.Pause;
    }

    public void BtnResume()
    {
        Debug.Log("Resume");
        animatorUI.SetTrigger("playStart");

        StartCoroutine(doAnimation());

        IEnumerator doAnimation()
        {
            yield return new WaitForSeconds(0.25f);
            GameController.gameState = eGameState.Play;
        }
    }

    // --- settings moves ---
    public void ChangeMoves(AddMovesSelectComponent movesSelectComponent)
    {
        Debug.Log(movesSelectComponent.scoreType);
        int toAdd = movesSelectComponent.add ? 1 : -1;

        switch (movesSelectComponent.scoreType)
        {
            case Scores.scoreType.startingMoves:
                scores.startingMoves += toAdd;

                if (movesSelectComponent.add && scores.startingMoves <= maxMoves)
                    GameController.Instance.addMove();

                if (!movesSelectComponent.add && scores.startingMoves >= minMoves)
                    GameController.Instance.substractMove();
                break;
            case Scores.scoreType.goldMoves:
                scores.goldMoves += toAdd;
                break;
            case Scores.scoreType.silverMoves:
                scores.silverMoves += toAdd;
                break;
            case Scores.scoreType.bronzeMoves:
                scores.bronzeMoves += toAdd;
                break;
            default:
                break;
        }

        scores.startingMoves = Mathf.Clamp(scores.startingMoves, minMoves, maxMoves);
        scores.goldMoves = Mathf.Clamp(scores.goldMoves, minStarMoves, scores.startingMoves);
        scores.silverMoves = Mathf.Clamp(scores.silverMoves, minStarMoves, scores.goldMoves);
        scores.bronzeMoves = Mathf.Clamp(scores.bronzeMoves, minStarMoves, scores.silverMoves);

        GameController.Instance.SetScoreMoves(scores.goldMoves, scores.silverMoves, scores.bronzeMoves);
        GameController.Instance.UpdateCurrentMedal();
        GameController.Instance.UpdateMedals();

        LevelManager.Instance.scores = scores;

        DisablePublish();
        UpdateSettingsText();
    }

    void UpdateSettingsText()
    {
        settingsScoresMoves.text = scores.startingMoves.ToString();
        settingsScoresGold.text = scores.goldMoves.ToString();
        settingsScoresSilver.text = scores.silverMoves.ToString();
        settingsScoresBronze.text = scores.bronzeMoves.ToString();


        settingsMapSize.text = size.ToString();
        if (size == eLevelSize.Small)
            lessMapSizeButton.interactable = false;
        else if (size == eLevelSize.Large)
            moreMapSizeButton.interactable = false;
    }

    public void ChangeMapSize(bool more)
    {
        TileMapUtils.ClearMapBorders(tilemap, size);

        if (more)
            switch (size)
            {
                case eLevelSize.Small:
                    size = eLevelSize.Medium;
                    lessMapSizeButton.interactable = true;
                    break;
                case eLevelSize.Medium:
                    size = eLevelSize.Large;
                    moreMapSizeButton.interactable = false;
                    break;
                default:
                    break;
            }
        else
            switch (size)
            {
                case eLevelSize.Medium:
                    size = eLevelSize.Small;
                    lessMapSizeButton.interactable = false;
                    break;
                case eLevelSize.Large:
                    moreMapSizeButton.interactable = true;
                    size = eLevelSize.Medium;
                    break;
                default:
                    break;
            }

        settingsMapSize.text = size.ToString();
        TileMapUtils.LoadMapBorders(tilemap, true, size);
        TileMapUtils.ChangeCameraSize(FindObjectOfType<Camera>(), size);

    }

    // --- publish ---
    public void BtnOpenSettingsPublish()
    {
        if (canPublish)
        {
            settingsPublishText.text = "Publish";
            settingsPublishButton.interactable = true;
        }
        else
        {
            settingsPublishText.text = "you need to win with 3 stars to publish";
            settingsPublishButton.interactable = false;
        }
    }

    public void BtnPublish()
    {
        Debug.Log("Trying to publish " + publishName.text);

        UpdateSLevel();

        LoadingScreen.Instance.StartFullScreenSpinner();


        CloudFirestore.Instance.SaveLevel(level,
            () =>
            {
                // on success
                ToastController.Instance.ToastGreen("Level Published!", ToastController.eToastType.TOP);

                tilemap.ClearAllTiles();
                TileMapUtils.LoadMapBorders(tilemap, true, size);
                BtnLoadMenu();
            },
            () =>
                // on fail
                ToastController.Instance.ToastRed("Level failed to upload", ToastController.eToastType.TOP)
            );
    }

    public void BtnLoadMenu()
    {
        UpdateSLevel();
        SceneLoader.Instance.LoadMenu();
    }

}
