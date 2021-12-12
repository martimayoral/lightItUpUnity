using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public enum TileType
{
    // bg
    BaseWall = 0,
    BlockedBaseWall = 1,

    // Players
    Red = 100,
    Blue = 101,
    Green = 102,

    // Goals
    GoalRed = 200,
    GoalBlue = 201,
    GoalGreen = 202
}


public static class TileMapUtils
{
    public static void SetTile(Tilemap tilemap, Vector3Int tilePos, TileType tile)
    {
        SetTile(tilemap, new Vector2Int(tilePos.x, tilePos.y), tile);
    }

    public static void SetTile(Tilemap tilemap, Vector2Int tilePos, TileType tile)
    {
        switch (tile)
        {
            case TileType.BaseWall:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/Ground"));
                break;
            case TileType.BlockedBaseWall:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/BlockedGround"));
                break;
            case TileType.Red:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/StartRed"));
                break;
            case TileType.Blue:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/StartBlue"));
                break;
            case TileType.Green:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/StartGreen"));
                break;
            case TileType.GoalRed:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/GoalRed"));
                break;
            case TileType.GoalBlue:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/GoalBlue"));
                break;
            case TileType.GoalGreen:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/GoalGreen"));
                break;
            default:
                Debug.LogError("Tile " + tile + " not found");
                break;
        }
    }

    public static void SetTile(Tilemap tilemap, Vector2Int tilePos, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tile);
    }

    public static sLevel CreateLevel(string name, int levelNum, Tilemap tilemap, Scores scores)
    {
        sLevel newLevel = new sLevel()
        {
            levelName = name,
            levelIndex = levelNum,
            tiles = GetTilesFromMap(tilemap).ToList(),
            score = scores
        };

        return newLevel;

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    var levelTile = map.GetTile<LevelTile>(pos);
                    yield return new SavedTile()
                    {
                        position = new Vector2Int(pos.x, pos.y),
                        tile = (int)levelTile.type
                    };
                }
            }
        }
    }

}
