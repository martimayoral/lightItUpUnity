using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovableObject
{

    // the goal
    [HideInInspector] public LevelTile goalTile;
    [HideInInspector] public LevelTile goalDoneTile;

    [HideInInspector] public bool goalReached;
    Vector3Int goalReachedPos;
    [HideInInspector] public Sprite[] sprites; // where sprites from player will be stored
    int spriteNum;
    int spriteRenderNum;
    public SpriteRenderer spriteRenderer;


    public override void ResetObj()
    {
        base.ResetObj();
        spriteNum = 0;
        spriteRenderNum = 0;
        goalReached = false;

    }

    // Start is called before the first frame update
    /*void Start()
    {
        Debug.Log("Player Start");

    }*/

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
        //Debug.Log($"SpriteUpdate: sn= {spriteNum} srn={spriteRenderNum}");
        spriteRenderer.sprite = sprites[spriteNum + spriteRenderNum];
    }

    protected override void MakeMoveToPosition(Vector3 position)
    {
        base.MakeMoveToPosition(position);

        // move sprite effect
        if (position.x > transform.position.x + .1f)
            StartCoroutine(moveEffect(directions.RIGHT));
        else if (position.x + .1f < transform.position.x)
            StartCoroutine(moveEffect(directions.LEFT));
    }

    public override void Move(Vector3Int translation)
    {
        base.Move(translation);

        if (moveState == moveStates.Moved)
        {

            if (!goalReached)
                if (LevelManager.Instance.tilemap.GetTile(mapPosition) == goalTile)
                {
                    goalReached = true;
                    goalReachedPos = mapPosition;
                    spriteNum += 4; // light on sprite

                    LevelManager.Instance.tilemap.SetTile(mapPosition, goalDoneTile);

                    Vector3 move = movesHistory.Pop();
                    move.z = 1f;
                    movesHistory.Push(move);
                }


        }

    }

    IEnumerator moveEffect(directions direction)
    {
        int toAdd = 0;
        if (direction == directions.RIGHT)
            toAdd = 8;
        if (direction == directions.LEFT)
            toAdd = 16;

        spriteNum += toAdd;
        yield return new WaitForSeconds(UserConfig.animationSpeed);
        spriteNum -= toAdd;
    }

    public override void UndoMove()
    {
        Debug.Log("PlayerUndo");

        if (movesHistory.Count == 0) return;

        Vector3 move = movesHistory.Pop();

        if (goalReached && move.z == 1f)
        {
            Debug.Log("UNDOING GOAL");

            move.z = 0;
            goalReached = false;
            LevelManager.Instance.tilemap.SetTile(goalReachedPos, goalTile);
            spriteNum = 0;
        }

        MakeUndo(move);
    }

}
