using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController: MonoBehaviour
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

    // the goal
    public LevelTile goalTile; 
    public LevelTile goalDoneTile;
    
    [HideInInspector]
    public bool goalReached;
    public Sprite offSprite; // light off (goal not reached)
    public Sprite onSprite; // light on (goal reached)
    SpriteRenderer spriteRenderer;

    public void resetSprite()
    {
        spriteRenderer.sprite = offSprite;
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        goalReached = false;
        resetSprite();

        Debug.Log("Player Start");

        coliderTile = Resources.Load<LevelTile>("LevelTiles/Ground");

        mapPosition = LevelManager.Instance.tilemap.WorldToCell(transform.position);
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
                spriteRenderer.sprite = onSprite;
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
        }
        if (moveState == moveStates.Collided)
        {
            //Collide effect
            float speed = UserConfig.animationSpeed * 0.5f;
            LeanTween.move(gameObject, transform.position + translation * 0.1f, speed);
            LeanTween.move(gameObject, transform.position, speed).setDelay(speed);
        }

    }
}
