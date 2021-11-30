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

    // punctuation system
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI nextMedalText;
    int moves = 0;

    // game state
    public static eGameState gameState;

    // medals
    struct medal
    {
        public medalType type;
        public medalType nextMedal;
        public Color medalColor;
        public int moves;
    }
    medal currentMedal;
    medal[] medals = new medal[]
    {
        new medal() { type= medalType.none, medalColor = GlobalVars.none, nextMedal = medalType.none, moves = 0 },
        new medal() { type= medalType.bronze, medalColor = GlobalVars.bronze, nextMedal = medalType.none },
        new medal() { type= medalType.silver, medalColor = GlobalVars.silver, nextMedal = medalType.bronze },
        new medal() { type= medalType.gold, medalColor = GlobalVars.gold, nextMedal = medalType.silver }
    };

    // input buffer
    Queue<Vector3> inputBuffer;
    float lastInputTime = 0.0f;

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
        moves = LevelManager.Instance.scores.startingMoves;
        movesText.SetText(moves.ToString());
        currentMedal = medals[(int)medalType.gold];
        nextMedalText.color = currentMedal.medalColor;
        nextMedalText.SetText(currentMedal.moves.ToString());

        // create an imput buffer queue to make a "memory"
        inputBuffer = new Queue<Vector3>();
        initialTouch.x = float.PositiveInfinity;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Game Controller");

        // get the players that the level manager has instanciated
        lightBuibs = GameObject.FindGameObjectsWithTag("Player");

        if (lightBuibs.Length == 0) gameObject.SetActive(false);

        // set the moves value
        medals[(int)medalType.gold].moves = LevelManager.Instance.scores.goldMoves;
        medals[(int)medalType.silver].moves = LevelManager.Instance.scores.silverMoves;
        medals[(int)medalType.bronze].moves = LevelManager.Instance.scores.bronzeMoves;

        // set the initial text values and color
        resetGame();
    }

    public void addMoves(int nMoves)
    {
        moves += nMoves;
        movesText.SetText(moves.ToString());
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

    private void MovePlayers(Vector3 translation)
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

            moves--;
            movesText.SetText(moves.ToString());

            if (moves < currentMedal.moves)
            {
                currentMedal = medals[(int)currentMedal.nextMedal];
                nextMedalText.color = currentMedal.medalColor;
                nextMedalText.SetText(currentMedal.moves.ToString());
            }

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
            //Debug.Log("Input!");
            lastInputTime = Time.time;
            MovePlayers(inputBuffer.Dequeue());
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
            gameState = eGameState.Win;
            AudioManager.Instance.PlaySound(AudioManager.eSound.Win);

            Debug.Log("WON! Level: " + SceneLoader.levelNum + ", medal: " + currentMedal.type);
            LevelsController.changeMedal(SceneLoader.levelNum, currentMedal.type);
            PauseMenu.Instance.Win(currentMedal.type);
        }
        else if (moves < 1)
        {
            gameState = eGameState.Lose;
            AudioManager.Instance.PlaySound(AudioManager.eSound.Lose);

            Debug.Log("YOU LOST");
            PauseMenu.Instance.Lose();
        }
    }
}
