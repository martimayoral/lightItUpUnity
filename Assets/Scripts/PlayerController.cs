using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    LevelTile coliderTile;

    // for all the light bulbs to move simultaniously
    //every time that there is a movement, if it has moved or not
    enum moveStates
    {
        Moved,
        NoMoved,
        Collided
    }
    moveStates moveState;
    public Vector3Int mapPosition { get; private set; }
    private Vector3 initalPosition;

    // the goal
    [HideInInspector]
    public LevelTile goalTile;
    [HideInInspector]
    public LevelTile goalDoneTile;

    [HideInInspector]
    public bool goalReached;
    Vector3Int goalReachedPos;
    [HideInInspector]
    public Sprite[] sprites; // where sprites from player will be stored
    int spriteNum;
    int spriteRenderNum;
    public SpriteRenderer spriteRenderer;

    // history
    Stack<Vector3> movesHistory;

    public void reset()
    {
        spriteNum = 0;
        spriteRenderNum = 0;
        transform.position = initalPosition;
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(initalPosition);
        goalReached = false;

        movesHistory.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Start");

        coliderTile = Resources.Load<LevelTile>("LevelTiles/Ground");

        initalPosition = transform.position;

        movesHistory = new Stack<Vector3>();

        reset();
    }

    private void OnEnable()
    {
        StartCoroutine(changeSprite());
    }

    private void OnDisable()
    {
        StopCoroutine(changeSprite());
    }

    IEnumerator changeSprite()
    {
        bool reverse = false;

        while (true)
        {
            yield return new WaitForSeconds(1.4f);

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(.15f);
                if (reverse)
                    spriteRenderNum = 3 - i;
                else
                    spriteRenderNum = i;
            }
            reverse = !reverse;
        }
    }

    private void Update()
    {
        spriteRenderer.sprite = sprites[spriteNum + spriteRenderNum];
    }


    public void clearMovement()
    {
        moveState = moveStates.NoMoved;
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(transform.position);
    }

    // returns the player that is in the cell position if there is any
    private PlayerController PlayerInCell(Vector3Int cellPosition)
    {
        foreach (GameObject lb in GameController.lightBuibs)
        {
            PlayerController pc = lb.GetComponent<PlayerController>();
            if (pc.mapPosition == cellPosition)
                return pc;
        }
        return null;
    }

    public bool hasMoved() { return moveState != moveStates.NoMoved; }

    public bool hasCollided() { return moveState == moveStates.Collided; }

    bool Collides(Vector3Int mapTilePos)
    {
        // check if collides with walls
        if (LevelManager.Instance.tilemap.GetTile(mapTilePos) == coliderTile)
        {
            moveState = moveStates.Collided;
            return true;
        }

        // check if colides with other light bulb
        PlayerController playerInCell = PlayerInCell(mapTilePos);
        if (playerInCell)
        {
            if (playerInCell.hasMoved())
            {
                moveState = moveStates.Collided;
            }
            return true;
        }
        return false;
    }

    public void PreMove(Vector3 translation)
    {
        if (hasMoved())
            return;

        Vector3 moveToPosition = transform.position + translation;
        Vector3Int mapTilePos = LevelManager.Instance.tilemap.WorldToCell(moveToPosition);

        // colisions
        if (Collides(mapTilePos))
        {
            return;
        }

        // update map position (used by the colision system)
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(moveToPosition);

        moveState = moveStates.Moved;
    }

    // solve position problems if needed
    //  public void fixPosition()
    //  {
    //      transform.position = LevelManager.Instance.tilemap.CellToWorld(mapPosition) + new Vector3(0.5f, 0.5f);
    //  }

    void MakeMoveToPosition(Vector3 position)
    {
        //Move
        LeanTween.move(gameObject, position, UserConfig.animationSpeed)
            .setEaseInOutSine();

        // move sprite effect
        if (position.x > transform.position.x + .1f)
            StartCoroutine(moveEffect(spriteDirections.Right));
        else if (position.x + .1f < transform.position.x)
            StartCoroutine(moveEffect(spriteDirections.Left));
    }

    public void Move(Vector3 translation)
    {
        Debug.Assert(hasMoved());

        Vector3 moveToPosition = transform.position + translation;

        if (moveState == moveStates.Moved)
        {
            //Debug.Log("PLAYER MOVE");

            // check if goal reached
            Vector3Int mapTilePos = LevelManager.Instance.tilemap.WorldToCell(moveToPosition);

            bool goalReachedNow = false;

            if (!goalReached)
                if (LevelManager.Instance.tilemap.GetTile(mapTilePos) == goalTile)
                {
                    goalReached = true;
                    goalReachedPos = mapTilePos;
                    spriteNum = 4; // light on sprite
                    LevelManager.Instance.tilemap.SetTile(mapTilePos, goalDoneTile);

                    goalReachedNow = true;
                }

            // Commit the movement
            MakeMoveToPosition(moveToPosition);

            // add to history their previous position, we use z of vector3 to show if the goal was reached in that movement
            movesHistory.Push(new Vector3(transform.position.x, transform.position.y, goalReachedNow ? 1f : 0f));
        }
        if (moveState == moveStates.Collided)
        {
            //Collide effect
            float speed = UserConfig.animationSpeed * 0.5f;
            LeanTween.move(gameObject, transform.position + translation * 0.1f, speed);
            LeanTween.move(gameObject, transform.position, speed).setDelay(speed);

            // add to history
            movesHistory.Push(new Vector3(0, 0, -1)); // z=-1 to show to the history that no move was done
        }

    }

    enum spriteDirections
    {
        Right = 1, Left
    }

    IEnumerator moveEffect(spriteDirections direction)
    {
        spriteNum += 8 * ((int)direction);
        yield return new WaitForSeconds(UserConfig.animationSpeed);
        spriteNum -= 8 * ((int)direction);
    }

    public void UndoMove()
    {
        if (movesHistory.Count == 0) return;

        Vector3 move = movesHistory.Pop();

        Debug.Log("Undoing move: " + move.ToString());

        if (goalReached && move.z == 1)
        {
            Debug.Log("UNDOING GOAL");

            move.z = 0;
            goalReached = false;
            LevelManager.Instance.tilemap.SetTile(goalReachedPos, goalTile);
            spriteNum = 0;
        }

        if (move.z == 0)
        {
            Debug.Log("MOVING UNDO: " + move.ToString());
            MakeMoveToPosition(move);
        }
    }

    public void PopMove()
    {
        Vector3 move = movesHistory.Pop();
        Debug.Log("Popping move: " + move);
    }
}
