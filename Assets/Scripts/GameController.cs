using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;


// This class controlls everything that happens in the game

public class GameController : MonoBehaviour
{
    // device touch
    Vector2 initialTouch;

    // the players
    public static GameObject[] lightBuibs { get; private set; }

    // punctuation system
    public GameObject movesText;
    public GameObject nextMedalText;
    int moves = 0;
    public enum medalType
    {
        gold, silver, bronze, none
    }

    struct medal
    {
        public medalType nextMedal;
        public Color medalColor;
        public int moves;
    }
    medal currentMedal;
    medal[] medals = new medal[]
    {
        new medal() { medalColor = GlobalVars.gold, nextMedal = medalType.silver },
        new medal() { medalColor = GlobalVars.silver, nextMedal = medalType.bronze },
        new medal() { medalColor = GlobalVars.bronze, nextMedal = medalType.none },
        new medal() { medalColor = GlobalVars.none, nextMedal = medalType.none, moves = 0 },
    };

    // input buffer
    Queue<Vector3> inputBuffer;
    float lastInputTime = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Game Controller");

        // create an imput buffer queue to make a "memory"
        inputBuffer = new Queue<Vector3>();

        // get the players that the level manager has instanciated
        lightBuibs = GameObject.FindGameObjectsWithTag("Player"); 

        // set the moves value
        medals[(int)medalType.gold].moves = LevelManager.Instance.scores.goldMoves;
        medals[(int)medalType.silver].moves = LevelManager.Instance.scores.silverMoves;
        medals[(int)medalType.bronze].moves = LevelManager.Instance.scores.bronzeMoves;

        // set the initial text values and color
        moves = LevelManager.Instance.scores.startingMoves;
        movesText.GetComponent<TextMeshProUGUI>().SetText(moves.ToString());
        currentMedal = medals[(int)medalType.gold];
        nextMedalText.GetComponent<TextMeshProUGUI>().color = currentMedal.medalColor;
        nextMedalText.GetComponent<TextMeshProUGUI>().SetText(currentMedal.moves.ToString());
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
        if (!allCollided)
        {
            moves--;
            movesText.GetComponent<TextMeshProUGUI>().SetText(moves.ToString());

            if (moves < currentMedal.moves)
            {
                currentMedal = medals[(int)currentMedal.nextMedal];
                nextMedalText.GetComponent<TextMeshProUGUI>().color = currentMedal.medalColor;
                nextMedalText.GetComponent<TextMeshProUGUI>().SetText(currentMedal.moves.ToString());
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
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
        if(inputBuffer.Count > 0 && (Time.time - lastInputTime > UserConfig.animationSpeed * 1.2))
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
            LevelLoader.Instance.LoadMenu();
    }
}
