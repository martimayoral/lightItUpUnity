using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;

public class MovableObject : MonoBehaviour
{
    // for all the light bulbs to move simultaniously
    //every time that there is a movement, if it has moved or not
    protected enum moveStates
    {
        Moved,
        NoMoved,
        Collided
    }
    protected moveStates moveState;
    public Vector3Int mapPosition { get; private set; }
    private Vector3 initalPosition;

    readonly float SMALL_SCALE = 0.6f;

    // history
    protected Stack<Vector3> movesHistory;

    public virtual void ResetObj()
    {
        transform.position = initalPosition;
        transform.localScale = new Vector3(1f, 1f, 1f);

        mapPosition = LevelManager.Instance.tilemap.WorldToCell(initalPosition);

        movesHistory.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Movable object Start");

        initalPosition = transform.position;

        movesHistory = new Stack<Vector3>();

        ResetObj();
    }


    public void ClearMovement()
    {
        moveState = moveStates.NoMoved;
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(transform.position);
    }

    // returns the player that is in the cell position if there is any
    private MovableObject PlayerInCell(Vector3Int cellPosition)
    {
        foreach (MovableObject mo in GameController.movableObjects)
        {
            if (mo.mapPosition == cellPosition)
                return mo;
        }
        return null;
    }

    public bool hasMoved() { return moveState != moveStates.NoMoved; }

    public bool hasCollided() { return moveState == moveStates.Collided; }

    bool Collides(Vector3Int direction)
    {
        LevelTile currentTile = LevelManager.GetTile(mapPosition);

        // check if can move
        if (currentTile != null)
        {
            switch (currentTile.type)
            {
                case TileType.GroundOnlyY:
                    if (direction.x != 0)
                    {
                        moveState = moveStates.Collided;
                        return true;
                    }
                    break;
                case TileType.GroundOnlyX:
                    if (direction.y != 0)
                    {
                        moveState = moveStates.Collided;
                        return true;
                    }
                    break;
                default:
                    break;
            }
        }

        Vector3Int mapTilePos = mapPosition + direction;
        LevelTile toTile = LevelManager.GetTile(mapTilePos);

        // check if collides with walls
        if (toTile != null)
        {
            switch (toTile.type)
            {
                case TileType.BlockedBaseWall:
                case TileType.BaseWall:
                    moveState = moveStates.Collided;
                    return true;
                case TileType.GroundOnlyY:
                    if (direction.x != 0)
                    {
                        moveState = moveStates.Collided;
                        return true;
                    }
                    break;
                case TileType.GroundOnlyX:
                    if (direction.y != 0)
                    {
                        moveState = moveStates.Collided;
                        return true;
                    }
                    break;
                default:
                    break;
            }
        }



        // check if colides with other light bulb
        MovableObject objectInCell = PlayerInCell(mapTilePos);
        if (objectInCell)
        {
            if (objectInCell.hasMoved())
            {
                moveState = moveStates.Collided;
            }
            return true;
        }
        return false;
    }

    public void PreMove(Vector3Int translation)
    {
        if (hasMoved())
            return;

        Vector3 moveToPosition = transform.position + translation;

        // colisions
        if (Collides(translation))
        {
            return;
        }

        // update map position (used by the colision system)
        mapPosition = LevelManager.Instance.tilemap.WorldToCell(moveToPosition);

        moveState = moveStates.Moved;
    }


    protected virtual void MakeMoveToPosition(Vector3 position)
    {
        //Move
        LeanTween.move(gameObject, position, UserConfig.animationSpeed)
            .setEaseInOutSine();

        LevelTile currentTile = LevelManager.GetTile(mapPosition);
        if (transform.localScale.x > SMALL_SCALE + .01f)
        {
            if (currentTile != null)
                if ((currentTile.type == TileType.GroundOnlyX) || (currentTile.type == TileType.GroundOnlyY))
                    LeanTween.scale(gameObject, new Vector3(SMALL_SCALE, SMALL_SCALE, 1f), UserConfig.animationSpeed).setEaseInOutSine();
        }
        else
        {
            if (currentTile == null)
                LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), UserConfig.animationSpeed).setEaseInOutSine();
        }
    }

    public virtual void Move(Vector3Int translation)
    {
        Debug.Assert(hasMoved());

        Vector3 moveToPosition = transform.position + translation;

        if (moveState == moveStates.Moved)
        {
            // Commit the movement
            MakeMoveToPosition(moveToPosition);



            movesHistory.Push(new Vector3(transform.position.x, transform.position.y, 0f));
        }
        if (moveState == moveStates.Collided)
        {
            //Collide effect
            float speed = UserConfig.animationSpeed * 0.5f;
            LeanTween.move(gameObject, transform.position + ((Vector3)translation) * 0.1f, speed);
            LeanTween.move(gameObject, transform.position, speed).setDelay(speed);

            // add to history
            movesHistory.Push(new Vector3(0, 0, -1)); // z=-1 to show to the history that no move was done
        }

    }

    public virtual void UndoMove()
    {
        Debug.Log("Virtual Undo");

        if (movesHistory.Count == 0) return;

        Vector3 move = movesHistory.Pop();

        MakeUndo(move);
    }

    protected void MakeUndo(Vector3 move)
    {
        if (move.z == 0)
        {
            mapPosition = LevelManager.Instance.tilemap.WorldToCell(move);
            MakeMoveToPosition(move);
        }
    }

    public void PopMove()
    {
        Vector3 move = movesHistory.Pop();
        Debug.Log("Popping move: " + move);
    }
}
