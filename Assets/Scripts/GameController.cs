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
    public static GameObject[] lightBuibs { get; private set; }


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
    int movesLeft;
    struct medal
    {
        public medalType type;
        public Color medalColor;
        public int moves;
    }
    medal currentMedal;
    medal[] medals = new medal[]
    {
        new medal() { type= medalType.none, medalColor = GlobalVars.none, moves = -1 },
        new medal() { type= medalType.bronze, medalColor = GlobalVars.bronze },
        new medal() { type= medalType.silver, medalColor = GlobalVars.silver },
        new medal() { type= medalType.gold, medalColor = GlobalVars.gold }
    };

    // input buffer
    Queue<Vector3> inputBuffer;
    float lastInputTime = 0.0f;

    // editor
    public bool editing;

    static public GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void resetGame()
    {
        Debug.Log("Reseting game");

        // game state
        gameState = eGameState.Play;

        // moves and medals
        movesLeft = LevelManager.Instance.scores.startingMoves;
        movesText.SetText(movesLeft.ToString());
        currentMedal = medals[(int)medalType.gold];
        medalUI.sprite = medalSprites[(int)medalType.gold];
        nextMedalText.color = currentMedal.medalColor;
        nextMedalText.SetText(currentMedal.moves.ToString());

        // create an imput buffer queue to make a "memory"
        inputBuffer = new Queue<Vector3>();
        initialTouch.x = float.PositiveInfinity;

        // moves history
        undoButton.interactable = false;
        movesCount = 0;
        //foreach()
    }


    public void DestoyPlayers()
    {
        foreach (var lb in lightBuibs)
        {
            Destroy(lb);
        }
        lightBuibs = null;
    }

    public void HardResetGame()
    {
        // get the players that the level manager has instanciated
        lightBuibs = GameObject.FindGameObjectsWithTag("Player");

        // set the moves value
        medals[(int)medalType.gold].moves = LevelManager.Instance.scores.goldMoves;
        medals[(int)medalType.silver].moves = LevelManager.Instance.scores.silverMoves;
        medals[(int)medalType.bronze].moves = LevelManager.Instance.scores.bronzeMoves;

        resetGame();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Game Controller");

        // sprites
        medalSprites = Resources.LoadAll<Sprite>("Other/Stars");

        // load level -------------------- MOVE TO ANOTHER COMPONENT WITHOUT EDITING
        if (!editing)
            LevelManager.Instance.LoadMap(SceneLoader.levelNum);

        // start all objects and values
        HardResetGame();
    }


    public void BtnUndo()
    {
        inputBuffer.Enqueue(new Vector3(0, 0, -1));
    }

    void UndoMove()
    {
        Debug.Log("Undo (moves done: " + movesCount);
        AudioManager.Instance.PlaySound(AudioManager.eSound.MoveReverse);

        if (movesCount > 0)
        {
            foreach (GameObject lb in lightBuibs)
            {
                lb.GetComponent<PlayerController>().UndoMove();
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

    private void predictPlayerMoves(Vector3 translation)
    {
        foreach (GameObject lb in lightBuibs)
        {
            lb.GetComponent<PlayerController>().clearMovement();
        }

        bool exit = false;
        while (!exit)
        {
            exit = true;
            foreach (GameObject lb in lightBuibs)
            {
                PlayerController pc = lb.GetComponent<PlayerController>();
                pc.PreMove(translation);

                // if all moved
                exit = exit && pc.hasMoved();
            }
        }
    }

    // moves players returns true if some moved
    private bool MovePlayers(Vector3 translation)
    {
        // predict moves (for who colides first)
        predictPlayerMoves(translation);

        // move players
        foreach (GameObject lb in lightBuibs)
            lb.GetComponent<PlayerController>().Move(translation);

        // check if they all collided and if not count the move
        bool allCollided = true;
        foreach (GameObject lb in lightBuibs)
        {
            if (!lb.GetComponent<PlayerController>().hasCollided())
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

    void substractMove()
    {
        movesLeft--;
        movesText.SetText(movesLeft.ToString());

        // change next medal if necessary
        if (movesLeft < currentMedal.moves)
        {
            currentMedal = medals[(int)currentMedal.type - 1];
            nextMedalText.color = currentMedal.medalColor;
            nextMedalText.SetText(currentMedal.moves.ToString());
            medalUI.sprite = medalSprites[(int)currentMedal.type];
        }
    }

    public void addMoves(int nMoves)
    {
        for (int i = 0; i < nMoves; i++)
        {
            addMove();
        }
    }
    void addMove()
    {
        movesLeft++;
        movesText.SetText(movesLeft.ToString());

        // change next medal if necessary
        if (currentMedal.type != medalType.gold)
            if (movesLeft > medals[(int)currentMedal.type + 1].moves)
            {
                currentMedal = medals[(int)currentMedal.type + 1];
                nextMedalText.color = currentMedal.medalColor;
                nextMedalText.SetText(currentMedal.moves.ToString());
                medalUI.sprite = medalSprites[(int)currentMedal.type];
            }
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
                    inputBuffer.Enqueue(new Vector3(Mathf.Sign(touchDif.x), 0, 0));
                }
                if (Mathf.Abs(touchDif.y) > Mathf.Abs(touchDif.x))
                {
                    inputBuffer.Enqueue(new Vector3(0, Mathf.Sign(touchDif.y), 0));
                }

            }
        }

        // input action
        if (inputBuffer.Count > 0 && (Time.time - lastInputTime > UserConfig.animationSpeed * 1.2))
        {

            // reset last input
            lastInputTime = Time.time;

            // get the move
            Vector3 move = inputBuffer.Dequeue();

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
                    undoesToAd = 2;
                    undoButton.interactable = true;
                }
                else
                    foreach (GameObject lb in lightBuibs)
                        lb.GetComponent<PlayerController>().PopMove();
            }
        }

        // check if game state is won (all bulbs have reached the goal)
        bool winState = true;

        if (lightBuibs.Length == 0)
            winState = false;

        foreach (GameObject lb in lightBuibs)
        {
            if (!lb.GetComponent<PlayerController>().goalReached)
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
