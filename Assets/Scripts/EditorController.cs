using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class EditorController : MonoBehaviour
{
    readonly int maxMoves = 50;
    readonly int minMoves = 1;


    [System.Serializable]
    public enum Tool
    {
        None,
        Eraser,
        PaintTile
    }

    TileType tileTypeToPaint;

    Tool tool;

    Tilemap tilemap;

    // level info
    sLevel level;
    Scores scores;
    eLevelSize size;

    bool canPublish;

    public GameController gameController;
    public Animator animatorUI;

    public TextMeshProUGUI exitTitle;
    public TextMeshProUGUI exitSubtitle;
    public Button exitPublishButton;
    public Button menuPublishButton;
    public GameObject leftoverMovements;
    public TextMeshProUGUI leftoverMovementsText;

    [Header("Moves Settings")]
    public TextMeshProUGUI settingsScoresMoves;
    public TextMeshProUGUI settingsScoresGold;
    public TextMeshProUGUI settingsScoresSilver;
    public TextMeshProUGUI settingsScoresBronze;

    [Header("Publish")]
    public TextMeshProUGUI settingsPublishText;
    public TMP_InputField publishName;
    public TextMeshProUGUI publishingAs;
    public Button settingsPublishButton;

    private void Start()
    {
        tool = Tool.None;
        tilemap = GameObject.FindObjectOfType<Tilemap>();

        canPublish = true;
        DisablePublish();
        publishingAs.text = "Publishing as " + AuthController.username;

        size = eLevelSize.Small;
        TileMapUtils.LoadMapBorders(tilemap, true, size);

        scores = new Scores() { goldMoves = 5, silverMoves = 4, bronzeMoves = 2, startingMoves = 20 };
        UpdateScoresText();
    }

    public void EnablePublish(bool reduceMovementsOption = false)
    {
        if (!canPublish)
        {
            canPublish = true;
            menuPublishButton.interactable = true;
            exitPublishButton.interactable = true;
        }

        leftoverMovements.SetActive(reduceMovementsOption);
    }

    public void DisablePublish()
    {
        if (canPublish)
        {
            canPublish = false;
            menuPublishButton.interactable = false;
            exitPublishButton.interactable = false;

            leftoverMovements.SetActive(false);
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

    public void SelectTool(ToolSelectComponent tool)
    {
        this.tool = tool.tool;
        this.tileTypeToPaint = tool.type;
        Debug.Log(tool.type);
    }

    void LoadEditorMap()
    {
        LevelManager.Instance.LoadMap(level, true);
    }

    void UpdateSLevel()
    {
        level = TileMapUtils.CreateLevel(publishName.text, 0, tilemap, scores, size);
        level.creatorName = AuthController.username;
    }

    public void BtnReduceMovesLeftover()
    {
        int startingMoves = scores.startingMoves;
        scores.startingMoves = scores.goldMoves + GameController.Instance.movesCount;
        LevelManager.Instance.scores.startingMoves = scores.startingMoves;

        GameController.Instance.substractMoves(startingMoves - scores.startingMoves);
        settingsScoresMoves.text = scores.startingMoves.ToString();
        leftoverMovements.SetActive(false);
    }

    // --- Test game ---
    public void BtnPlay()
    {
        tool = Tool.None;

        if (TileMapUtils.CountLBStarts(tilemap) == 0)
        {
            ToastController.Instance.ToastRed("You need to add some light bulb to Play");
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
            EnablePublish(gameController.movesLeft != scores.goldMoves);
            leftoverMovementsText.text = $"Reduce moves leftover ({GameController.Instance.movesLeft - scores.goldMoves})";
        }
    }

    public void Lose()
    {
        Debug.Log("EDITOR CONTROLLER LOSE");
        animatorUI.SetTrigger("playEnd");
        exitTitle.text = "You lost";
        exitSubtitle.text = "You need 3 stars to publish";
        DisablePublish();
    }

    public void BtnContinueEditing()
    {
        Debug.Log("Continue editing");
        animatorUI.SetTrigger("menu");
        gameController.DestoyPlayers();

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
                if (movesSelectComponent.add)
                    GameController.Instance.addMove();
                else
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
        scores.goldMoves = Mathf.Clamp(scores.goldMoves, minMoves, scores.startingMoves);
        scores.silverMoves = Mathf.Clamp(scores.silverMoves, minMoves, scores.goldMoves);
        scores.bronzeMoves = Mathf.Clamp(scores.bronzeMoves, minMoves, scores.silverMoves);

        GameController.Instance.SetScoreMoves(scores.goldMoves, scores.silverMoves, scores.bronzeMoves);
        GameController.Instance.UpdateCurrentMedal();
        GameController.Instance.UpdateMedals();

        LevelManager.Instance.scores = scores;

        DisablePublish();
        UpdateScoresText();
    }

    void UpdateScoresText()
    {
        settingsScoresMoves.text = scores.startingMoves.ToString();
        settingsScoresGold.text = scores.goldMoves.ToString();
        settingsScoresSilver.text = scores.silverMoves.ToString();
        settingsScoresBronze.text = scores.bronzeMoves.ToString();
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
                animatorUI.SetTrigger("menu");
                ToastController.Instance.ToastGreen("Level Published!");
            },
            () =>
                // on fail
                ToastController.Instance.ToastRed("Level failed to upload")
            );
    }

    public void BtnLoadMenu()
    {
        SceneLoader.Instance.LoadMenu();
    }
}
