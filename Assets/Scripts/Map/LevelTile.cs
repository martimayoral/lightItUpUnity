using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "New Level Tile" , menuName = "2D/Tiles/Level Tile")]
public class LevelTile : AnimatedTile
{
    public TileType type;
}

public enum TileType
{
    // bg
    BaseWall = 0,

    // Players
    Red = 100,
    Blue = 101,
    Green = 102,

    // Goals
    GoalRed = 200,
    GoalBlue = 201,
    GoalGreen = 202
}
