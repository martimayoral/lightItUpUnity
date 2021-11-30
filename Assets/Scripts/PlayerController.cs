using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    [HideInInspector]
    public Sprite[] sprites; // where sprites from player will be stored
    int spriteNum;
    int spriteRenderNum;
    public SpriteRenderer spriteRenderer;


    public void reset()
    {
        spriteNum = 0;
        spriteRenderNum = 0;
        transform.position = initalPosition;
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(initalPosition);
        goalReached = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Start");

        coliderTile = Resources.Load<LevelTile>("LevelTiles/Ground");

        initalPosition = transform.position;
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

        // victory
        if (!goalReached)
            if (LevelManager.Instance.tilemap.GetTile(mapTilePos) == goalTile)
            {
                goalReached = true;
                spriteNum = 4;
                LevelManager.Instance.tilemap.SetTile(mapTilePos, goalDoneTile);
            }

        mapPosition = LevelManager.Instance.tilemap.WorldToCell(moveToPosition);
        moveState = moveStates.Moved;
    }

    // solve position problems if needed
    public void fixPosition()
    {
        transform.position = LevelManager.Instance.tilemap.CellToWorld(mapPosition) + new Vector3(0.5f, 0.5f);
    }

    public void Move(Vector3 translation)
    {
        Debug.Assert(hasMoved());

        Vector3 moveToPosition = transform.position + translation;

        if (moveState == moveStates.Moved)
        {
            //Move
            LeanTween.move(gameObject, moveToPosition, UserConfig.animationSpeed)
                .setEaseInOutSine();

            if (moveToPosition.x > transform.position.x + .1f)
                StartCoroutine(moveEffect(spriteDirections.Right));
            else if (moveToPosition.x + .1f < transform.position.x)
                StartCoroutine(moveEffect(spriteDirections.Left));

        }
        if (moveState == moveStates.Collided)
        {
            //Collide effect
            float speed = UserConfig.animationSpeed * 0.5f;
            LeanTween.move(gameObject, transform.position + translation * 0.1f, speed);
            LeanTween.move(gameObject, transform.position, speed).setDelay(speed);
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
}
