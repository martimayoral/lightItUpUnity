using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public enum TileType
{
    // none
    None = -1,

    // bg
    BaseWall = 0,
    BlockedBaseWall = 1,
    GroundOnlyY = 10,
    GroundOnlyX = 11,
    MovableBlock = 80,

    // Players
    Red = 100,
    Blue = 101,
    Green = 102,

    // Goals
    GoalRed = 200,
    GoalBlue = 201,
    GoalGreen = 202,

    // Hint (+10000)
    HINT_TILE = 9999,

    // red
    RHintArrowRIGHT,
    RHintArrowBOTTOM,
    RHintArrowLEFT,
    RHintArrowUP,

    RHintArrowLB,
    RHintArrowTL,
    RHintArrowRT,
    RHintArrowBR,

    RHintArrowRB,
    RHintArrowTR,
    RHintArrowLT,
    RHintArrowBL,

    RHintX,
    RHint1,
    RHint2,
    RHint3,
    RHint4,
    RHint5,

    // blue
    BHintArrowRIGHT = 10020,
    BHintArrowBOTTOM,
    BHintArrowLEFT,
    BHintArrowUP,

    BHintArrowLB,
    BHintArrowTL,
    BHintArrowRT,
    BHintArrowBR,

    BHintArrowRB,
    BHintArrowTR,
    BHintArrowLT,
    BHintArrowBL,

    BHintX,
    BHint1,
    BHint2,
    BHint3,
    BHint4,
    BHint5,

    // green
    GHintArrowRIGHT = 10040,
    GHintArrowBOTTOM,
    GHintArrowLEFT,
    GHintArrowUP,

    GHintArrowLB,
    GHintArrowTL,
    GHintArrowRT,
    GHintArrowBR,

    GHintArrowRB,
    GHintArrowTR,
    GHintArrowLT,
    GHintArrowBL,

    GHintX,
    GHint1,
    GHint2,
    GHint3,
    GHint4,
    GHint5,
}

public enum eLevelSize
{
    // manually set all tiles
    Flex,

    // borders set
    Small = 10,
    Medium = 15,
    Large = 20
}

public static class TileMapUtils
{
    readonly static Color hintRed = new Color(0.6f, 0.15f, 0.08f);
    readonly static Color hintBlue = new Color(0.06f, 0.32f, 0.49f);
    readonly static Color hintGreen = new Color(0.07f, 0.5f, 0.08f);

    readonly static Vector2Int mapMargin = new Vector2Int(10, 4);

    readonly static Vector2Int smallMapCorner = new Vector2Int(7, 3);
    readonly static int smallCameraSize = 5;

    readonly static Vector2Int mediumMapCorner = new Vector2Int(9, 4);
    readonly static int mediumCameraSize = 7;

    readonly static Vector2Int largeMapCorner = new Vector2Int(10, 5);
    readonly static int largeCameraSize = 8;


    public static void SetTile(Tilemap tilemap, Vector3Int tilePos, TileType tile)
    {
        SetTile(tilemap, new Vector2Int(tilePos.x, tilePos.y), tile);
    }

    public static void SetTile(Tilemap tilemap, Vector2Int tilePos, TileType tile)
    {
        switch (tile)
        {
            case TileType.None:
                SetTile(tilemap, tilePos, null);
                break;
            case TileType.BaseWall:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/Ground"));
                break;
            case TileType.BlockedBaseWall:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/BlockedGround"));
                break;
            case TileType.GroundOnlyX:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/GroundOnlyX"));
                break;
            case TileType.GroundOnlyY:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/GroundOnlyY"));
                break;
            case TileType.MovableBlock:
                SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/MovableBlock"));
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


            case TileType.RHintArrowRIGHT:
            case TileType.RHintArrowLEFT:
            case TileType.RHintArrowBOTTOM:
            case TileType.RHintArrowUP:
                SetHintTile("HintArrowRight", TileType.RHintArrowRIGHT, hintRed);
                break;
            case TileType.BHintArrowRIGHT:
            case TileType.BHintArrowLEFT:
            case TileType.BHintArrowBOTTOM:
            case TileType.BHintArrowUP:
                SetHintTile("HintArrowRight", TileType.BHintArrowRIGHT, hintBlue);
                break;
            case TileType.GHintArrowRIGHT:
            case TileType.GHintArrowLEFT:
            case TileType.GHintArrowBOTTOM:
            case TileType.GHintArrowUP:
                SetHintTile("HintArrowRight", TileType.GHintArrowRIGHT, hintGreen);
                break;

            case TileType.RHintArrowLB:
            case TileType.RHintArrowBR:
            case TileType.RHintArrowRT:
            case TileType.RHintArrowTL:
                SetHintTile("HintArrowLB", TileType.RHintArrowLB, hintRed);
                break;
            case TileType.BHintArrowLB:
            case TileType.BHintArrowBR:
            case TileType.BHintArrowRT:
            case TileType.BHintArrowTL:
                SetHintTile("HintArrowLB", TileType.BHintArrowLB, hintBlue);
                break;
            case TileType.GHintArrowLB:
            case TileType.GHintArrowBR:
            case TileType.GHintArrowRT:
            case TileType.GHintArrowTL:
                SetHintTile("HintArrowLB", TileType.GHintArrowLB, hintGreen);
                break;

            case TileType.RHintArrowRB:
            case TileType.RHintArrowTR:
            case TileType.RHintArrowLT:
            case TileType.RHintArrowBL:
                SetHintTile("HintArrowRB", TileType.RHintArrowRB, hintRed);
                break;
            case TileType.BHintArrowRB:
            case TileType.BHintArrowTR:
            case TileType.BHintArrowLT:
            case TileType.BHintArrowBL:
                SetHintTile("HintArrowRB", TileType.BHintArrowRB, hintBlue);
                break;
            case TileType.GHintArrowRB:
            case TileType.GHintArrowTR:
            case TileType.GHintArrowLT:
            case TileType.GHintArrowBL:
                SetHintTile("HintArrowRB", TileType.GHintArrowRB, hintGreen);
                break;

            case TileType.GHintX:
            case TileType.GHint1:
            case TileType.GHint2:
            case TileType.GHint3:
            case TileType.GHint4:
            case TileType.GHint5:
                SetHintTile(tile.ToString().Remove(0, 1), tile, hintGreen);
                break;
            case TileType.BHintX:
            case TileType.BHint1:
            case TileType.BHint2:
            case TileType.BHint3:
            case TileType.BHint4:
            case TileType.BHint5:
                SetHintTile(tile.ToString().Remove(0, 1), tile, hintBlue);
                break;
            case TileType.RHintX:
            case TileType.RHint1:
            case TileType.RHint2:
            case TileType.RHint3:
            case TileType.RHint4:
            case TileType.RHint5:
                SetHintTile(tile.ToString().Remove(0, 1), tile, hintRed);
                break;

            default:
                Debug.LogError("Tile " + tile + " not found");
                break;
        }

        void SetHintTile(string assetName, TileType baseType, Color color)
        {
            SetTile(tilemap, tilePos, Resources.Load<LevelTile>("LevelTiles/HintTiles/" + assetName));

            //Debug.Log(tile + ": Rotating " + (90f * (tile - baseType)));

            if (tile != baseType)
                RotateTile(tilemap, tilePos, 90f * (tile - baseType));

            tilemap.SetColor(new Vector3Int(tilePos.x, tilePos.y, 0), color);
        }
    }

    public static void SetTile(Tilemap tilemap, Vector2Int tilePos, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tile);
    }

    public static void RotateTile(Tilemap tilemap, Vector2Int tilePos, float rotation)
    {
        tilemap.SetTransformMatrix(new Vector3Int(tilePos.x, tilePos.y, 0), Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, rotation), Vector3.one));
    }

    public static sLevel CreateLevel(string name, int levelNum, Tilemap tilemap, Scores scores, eLevelSize size)
    {
        sLevel newLevel = new sLevel()
        {
            levelName = name,
            levelIndex = levelNum,
            tiles = GetTilesFromMap(tilemap).ToList(),
            score = scores,
            levelSize = size
        };

        return newLevel;

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (!IsBorder(pos.x, pos.y, size))
                    if (map.HasTile(pos))
                    {
                        var levelTile = map.GetTile<LevelTile>(pos);
                        int type = (int)levelTile.type;
                        if (levelTile.type > TileType.HINT_TILE)
                        {
                            if (map.GetColor(pos).b > .4f)
                                type += 20;
                            if (map.GetColor(pos).g > .4f)
                                type += 40;

                            type += (int)Mathf.Floor(map.GetTransformMatrix(pos).rotation.eulerAngles.z / 90f);
                        }


                        yield return new SavedTile()
                        {
                            position = new Vector2Int(pos.x, pos.y),
                            tile = type
                        };
                    }
            }
        }
    }

    static bool IsBorder(int x, int y, Vector2Int initialCorner, Vector2Int finalCorner)
    {
        return (x < initialCorner.x || x >= finalCorner.x || y < initialCorner.y || y >= finalCorner.y);
    }
    static bool IsBorder(int x, int y, eLevelSize size)
    {
        return IsBorder(x, y, -1 * GetCorner(size), GetCorner(size));
    }

    static void LoadMapBorders(Tilemap tilemap, TileType tileType, Vector2Int corner, Vector2Int margins)
    {
        Vector2Int initialCorner = -1 * corner;
        Vector2Int finalCorner = corner;
        for (int x = initialCorner.x - margins.x; x < (finalCorner.x + margins.x); x++)
        {
            for (int y = initialCorner.y - margins.y; y < (finalCorner.y + margins.y); y++)
            {
                if (IsBorder(x, y, initialCorner, finalCorner))
                    TileMapUtils.SetTile(tilemap, new Vector2Int(x, y), tileType);
            }
        }
    }

    public static void LoadMapBorders(Tilemap tilemap, bool editing, eLevelSize size)
    {
        LoadMapBorders(tilemap, editing ? TileType.BlockedBaseWall : TileType.BaseWall, size);
    }

    public static void LoadMapBorders(Tilemap tilemap, TileType tileType, eLevelSize size)
    {
        LoadMapBorders(tilemap, tileType, GetCorner(size), mapMargin);
    }

    static Vector2Int GetCorner(eLevelSize size)
    {
        switch (size)
        {
            case eLevelSize.Flex:
                // we don't want any border in flex
                return -mapMargin;
            case eLevelSize.Small:
                return smallMapCorner;
            case eLevelSize.Medium:
                return mediumMapCorner;
            case eLevelSize.Large:
                return largeMapCorner;
            default:
                Debug.LogError($"Load Map border {size} not defined");
                return new Vector2Int();
        }
    }

    public static Dictionary<TileType, int> CountTileTypes(Tilemap tilemap)
    {
        Dictionary<TileType, int> dic = new Dictionary<TileType, int>();

        foreach (TileType tileType in System.Enum.GetValues(typeof(TileType)))
        {
            dic.Add(tileType, 0);
        }

        foreach (LevelTile tile in tilemap.GetTilesBlock(tilemap.cellBounds))
        {
            if (tile != null)
                dic[tile.type]++;
        }

        return dic;
    }

    public static void ClearMapBorders(Tilemap tilemap, eLevelSize size)
    {
        LoadMapBorders(tilemap, TileType.None, size);
    }

    public static void ChangeCameraSize(Camera camera, eLevelSize size)
    {
        switch (size)
        {
            case eLevelSize.Flex:
            case eLevelSize.Small:
                camera.orthographicSize = smallCameraSize;
                break;
            case eLevelSize.Medium:
                camera.orthographicSize = mediumCameraSize;
                break;
            case eLevelSize.Large:
                camera.orthographicSize = largeCameraSize;
                break;
            default:
                break;
        }
    }

}
