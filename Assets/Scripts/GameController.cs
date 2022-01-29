using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;


public enum eGameState
{
    Play,
    Pause,
    Win,
    Lose
}


// This class controlls everything that happens in the game

public class GameController : MonoBehaviour
{
    // device touch
    Vector2 initialTouch;

    // the players
    public static MovableObject[] movableObjects { get; private set; } // canviar
    public static PlayerController[] players { get; private set; }

    // game state
    public static eGameState gameState;

    // back moves
    public int movesCount { get; private set; }
    public Button undoButton;
    static int undoesToAd;

    // punctuation system
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI nextMedalText;
    public Image medalUI;
    Sprite[] medalSprites;
    public int movesLeft;
    struct medal
    {
        public medalType type;
        public Color medalColor;
        public int moves; // moves they have to reach
    }
    medal currentMedal;
    medal[] medals = new medal[]
    {
        new medal() { type= medalType.none, medalColor = GlobalVars.none, moves = 0 },
        new medal() { type= medalType.bronze, medalColor = GlobalVars.bronze },
        new medal() { type= medalType.silver, medalColor = GlobalVars.silver },
        new medal() { type= medalType.gold, medalColor = GlobalVars.gold }
    };

    // input buffer
    Queue<Vector3Int> inputBuffer;
    float lastInputTime = 0.0f;


    static public GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // sprites
        medalSprites = Resources.LoadAll<Sprite>("UI/stars");
    }

    public void resetGame()
    {
        Debug.Log("Reseting game");

        // game state
        gameState = eGameState.Play;

        // moves and medals
        movesLeft = LevelManager.Instance.scores.startingMoves;
        UpdateMovesUI();
        currentMedal = medals[(int)medalType.gold];
        medalUI.sprite = medalSprites[(int)medalType.gold];
        nextMedalText.color = currentMedal.medalColor;
        nextMedalText.SetText(currentMedal.moves.ToString());

        // create an imput buffer queue to make a "memory"
        inputBuffer = new Queue<Vector3Int>();
        initialTouch.x = float.PositiveInfinity;

        // moves history
        undoButton.interactable = false;
        movesCount = 0;
    }


    public void DeleteMovableObj()
    {
        foreach (var mo in movableObjects)
        {
            Destroy(mo.gameObject);
        }
        movableObjects = null;
    }

    public void HardResetGame()
    {
        // get the players that the level manager has instanciated
        GameObject[] movableGObjects = GameObject.FindGameObjectsWithTag("Player");
        movableObjects = new MovableObject[movableGObjects.Length];
        for (int i = 0; i < movableGObjects.Length; i++)
        {
            movableObjects[i] = movableGObjects[i].GetComponent<MovableObject>();
        }


        // set the moves value
        SetScoreMoves(LevelManager.Instance.scores.goldMoves, LevelManager.Instance.scores.silverMoves, LevelManager.Instance.scores.bronzeMoves);

        resetGame();
    }

    public void UpdateCurrentMedal()
    {
        if (movesLeft >= medals[(int)medalType.gold].moves)
            currentMedal = medals[(int)medalType.gold];
        else if (movesLeft >= medals[(int)medalType.silver].moves)
            currentMedal = medals[(int)medalType.silver];
        else if (movesLeft >= medals[(int)medalType.bronze].moves)
            currentMedal = medals[(int)medalType.bronze];
        else
            currentMedal = medals[(int)medalType.none];
    }

    public void SetScoreMoves(int gold, int silver, int bronze)
    {
        medals[(int)medalType.gold].moves = gold;
        medals[(int)medalType.silver].moves = silver;
        medals[(int)medalType.bronze].moves = bronze;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Game Controller");


        gameState = eGameState.Pause;
    }


    public void BtnUndo()
    {
        inputBuffer.Enqueue(new Vector3Int(0, 0, -1));
    }

    void UndoMove()
    {
        Debug.Log("Undo (moves done: " + movesCount);
        AudioManager.Instance.PlaySound(AudioManager.eSound.MoveReverse);

        if (movesCount > 0)
        {
            foreach (MovableObject mo in movableObjects)
            {
                mo.UndoMove();
            }
            addMove();

            movesCount--;
            undoesToAd--;
            if (undoesToAd == 0)
            {
                if (AdsManager.Instance)
                    AdsManager.Instance.PlayAdd();
            }
        }

        if (movesCount <= 0)
        {
            undoButton.interactable = false;
        }
    }

    private void predictPlayerMoves(Vector3Int translation)
    {
        foreach (MovableObject mo in movableObjects)
        {
            mo.ClearMovement();
        }

        bool exit = false;
        while (!exit)
        {
            exit = true;
            foreach (MovableObject mo in movableObjects)
            {
                mo.PreMove(translation);

                // if all moved
                exit = exit && mo.hasMoved();
            }
        }
    }

    // moves players returns true if some moved
    private bool MovePlayers(Vector3Int translation)
    {
        // predict moves (for who colides first)
        predictPlayerMoves(translation);

        // move players
        foreach (MovableObject mo in movableObjects)
            mo.Move(translation);

        // check if they all collided and if not count the move
        bool allCollided = true;
        foreach (MovableObject mo in movableObjects)
        {
            if (!mo.hasCollided())
                allCollided = false;
        }


        // count the move
        if (allCollided)
            AudioManager.Instance.PlaySound(AudioManager.eSound.NoMove);
        else
        {
            AudioManager.Instance.PlaySound(AudioManager.eSound.Move);
        }

        return !allCollided;
    }

    public void UpdateMedals()
    {
        nextMedalText.color = currentMedal.medalColor;
        nextMedalText.SetText(currentMedal.moves.ToString());
        medalUI.sprite = medalSprites[(int)currentMedal.type];
    }


    public void substractMoves(int nMoves)
    {
        for (int i = 0; i < nMoves; i++)
        {
            substractMove();
        }
    }

    public void substractMove()
    {
        movesLeft--;
        movesText.SetText(movesLeft.ToString());


        if (movesLeft < currentMedal.moves)
        {
            currentMedal = medals[(int)currentMedal.type - 1];
            UpdateMedals();
        }
    }

    public void addMoves(int nMoves)
    {
        for (int i = 0; i < nMoves; i++)
        {
            addMove();
        }
    }
    public void addMove()
    {
        movesLeft++;
        UpdateMovesUI();

        if (currentMedal.type != medalType.gold)
        {
            if (movesLeft >= medals[(int)currentMedal.type + 1].moves)
            {
                currentMedal = medals[(int)currentMedal.type + 1];
                UpdateMedals();
            }
        }
    }

    public void UpdateMovesUI()
    {
        movesText.SetText(movesLeft.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(gameState);
        if (gameState != eGameState.Play)
            return;


        // player movement
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            //Debug.Log("PHASE: " + touch.phase);

            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                initialTouch = touchPos;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                if (initialTouch.x == float.PositiveInfinity)
                    return;

                Vector2 touchDif = touchPos - initialTouch;

                if (Mathf.Abs(touchDif.x) > Mathf.Abs(touchDif.y))
                {
                    inputBuffer.Enqueue(new Vector3Int((int)Mathf.Sign(touchDif.x), 0, 0));
                }
                if (Mathf.Abs(touchDif.y) > Mathf.Abs(touchDif.x))
                {
                    inputBuffer.Enqueue(new Vector3Int(0, (int)Mathf.Sign(touchDif.y), 0));
                }

            }
        }

        // input action
        if (inputBuffer.Count > 0 && (Time.time - lastInputTime > UserConfig.animationSpeed * 1.2))
        {

            // reset last input
            lastInputTime = Time.time;

            // get the move
            Vector3Int move = inputBuffer.Dequeue();

            // it is an undo move
            if (move.z == -1)
            {
                Debug.Log("IT IS AN UNDO MOVE");
                UndoMove();
            }
            else
            {

                // move players and count move
                // check if they could move
                if (MovePlayers(move))
                {
                    substractMove();

                    // history
                    movesCount++;
                    undoesToAd = 20;
                    undoButton.interactable = true;
                }
                else
                    foreach (MovableObject mo in movableObjects)
                        mo.PopMove();
            }
        }

        // check if game state is won (all bulbs have reached the goal)
        bool winState = true;

        if (movableObjects.Length == 0)
            winState = false;

        foreach (MovableObject mo in movableObjects)
        {
            if (mo is PlayerController)
                if (!((PlayerController)mo).goalReached)
                    winState = false;
        }
        if (winState)
        {
            Debug.Log("YOU WON");

            gameState = eGameState.Win;
            AudioManager.Instance.PlaySound(AudioManager.eSound.Win);

            gameObject.SendMessage("Win", currentMedal.type);
        }
        else if (movesLeft < 1)
        {
            Debug.Log("YOU LOST");

            gameState = eGameState.Lose;
            AudioManager.Instance.PlaySound(AudioManager.eSound.Lose);

            gameObject.SendMessage("Lose");
        }
    }
}
