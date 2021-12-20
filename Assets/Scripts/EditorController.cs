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

    // moves settings
    public TextMeshProUGUI settingsScoresMoves;
    public TextMeshProUGUI settingsScoresGold;
    public TextMeshProUGUI settingsScoresSilver;
    public TextMeshProUGUI settingsScoresBronze;

    // publish
    public TextMeshProUGUI settingsPublishText;
    public TMP_InputField publishName;
    public Button settingsPublishButton;

    private void Start()
    {
        tool = Tool.None;
        tilemap = GameObject.FindObjectOfType<Tilemap>();

        canPublish = false;

        size = eLevelSize.Small;
        TileMapUtils.LoadMapBorders(tilemap, true, size);

        scores = new Scores() { bronzeMoves = 2, goldMoves = 5, silverMoves = 4, startingMoves = 20 };
        UpdateScoresText();
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
        canPublish = false;
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
        level.creatorName = "Creator";
    }

    // --- Test game ---
    public void BtnPlay()
    {
        tool = Tool.None;

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
            exitSubtitle.text = "You need 3 stars to publish";
        else
        {
            exitSubtitle.text = "You can now publish the game";
            canPublish = true;
        }
    }

    public void Lose()
    {
        Debug.Log("EDITOR CONTROLLER LOSE");
        animatorUI.SetTrigger("playEnd");
        exitTitle.text = "You lost";
        exitSubtitle.text = "You need 3 stars to publish";
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

        CloudFirestore.Instance.SaveLevel(level);
    }

    public void BtnLoadMenu()
    {
        SceneLoader.Instance.LoadMenu();
    }
}
