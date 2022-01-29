using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System;
using System.Text;
using System.IO;

[Serializable]
public struct SavedTile
{
    public Vector2Int position;
    public int tile;
}

[Serializable]
public struct Scores
{
    public int startingMoves, goldMoves, silverMoves, bronzeMoves;
    public enum scoreType { startingMoves, goldMoves, silverMoves, bronzeMoves }
}

public class sLevel
{
    public string levelName;
    public string creatorName;
    public int levelIndex; // if campaing, is the level number. if online, is the number on the list
    public List<SavedTile> tiles;
    public Scores score;
    public eLevelSize levelSize;
    public string levelId;
}

// this class is in charge of starting the level and loading it
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private void Awake()
    {
        Instance = this;
        tilemap = FindObjectOfType<Tilemap>();
        levelCamera = FindObjectOfType<Camera>();
    }

    public Tilemap tilemap;
    public Camera levelCamera;

    public Scores scores;

    public eLevelSize levelSize;

    List<SavedTile> goalTiles = new List<SavedTile>();

    [SerializeField] Transform playerPrefab;
    [SerializeField] Transform movableBlockPrefab;



#if UNITY_EDITOR
    public int levelNum;

    public void SaveMapInEditor()
    {
        sLevel newLevel = TileMapUtils.CreateLevel("", 0, tilemap, scores, levelSize);

        ScriptableObjectUnity.SaveLevelFile($"Level {levelNum}", JsonUtility.ToJson(newLevel));

        Debug.Log("Level Saved! :)");
    }

    public void SaveNewMapInEditor()
    {
        sLevel newLevel = TileMapUtils.CreateLevel("", 0, tilemap, scores, levelSize);

        levelNum = 1;
        while (File.Exists($"Assets/Resources/Levels/Level {levelNum}.json"))
            levelNum++;

        ScriptableObjectUnity.SaveLevelFile($"Level {levelNum}", JsonUtility.ToJson(newLevel));

        Debug.Log("Level Saved! :)");
    }

    public void LoadMapInEditor(int lvlNum, bool editing = false)
    {
        print($"Loading Level {lvlNum}");

        sLevel level;
        level = GetSLevelFromFile(lvlNum);

        Debug.Assert(lvlNum == level.levelIndex);

        LoadMap(level, editing);

        Debug.Log($"{lvlNum} Loaded! :)");
    }

    public void SaveOnlineData()
    {
        int i = 0;
        foreach (sLevel level in LevelsController.onlineLevelsList)
        {
            i++;
            string name = "online/" + i + ". " + level.levelName + "  " + level.levelId;
            level.creatorName = "";
            level.levelName = "";
            level.levelIndex = 0;
            level.levelId = "";
            ScriptableObjectUnity.SaveLevelFile(name, JsonUtility.ToJson(level));

        }
    }
#endif

    public static LevelTile GetTile(Vector3Int mapPos)
    {
        return (LevelTile)Instance.tilemap.GetTile(mapPos);
    }

    public void LoadBase()
    {
        ClearMap();
        TileMapUtils.ChangeCameraSize(levelCamera, levelSize);
        TileMapUtils.LoadMapBorders(tilemap, true, levelSize);
    }

    public void ClearMap()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (Tilemap map in maps)
        {
            map.ClearAllTiles();
        }

        if (goalTiles != null)
            goalTiles.Clear();
    }

    public void LoadMap(sLevel level, bool editing)
    {
        Debug.Log("Loading map");
        ClearMap();

        TileMapUtils.LoadMapBorders(tilemap, editing, level.levelSize);

        TileMapUtils.ChangeCameraSize(levelCamera, level.levelSize);

        scores = level.score;
        levelSize = level.levelSize;

        TileType type;
        foreach (var savedTile in level.tiles)
        {
            type = (TileType)savedTile.tile;
            switch (type)
            {
                case TileType.BlockedBaseWall:
                    TileMapUtils.SetTile(tilemap, savedTile.position, TileType.BaseWall);
                    break;

                case TileType.GoalBlue:
                case TileType.GoalRed:
                case TileType.GoalGreen:
                    goalTiles.Add(savedTile);
                    break;

                // this ones spawn and move the players
                case TileType.Green:
                case TileType.Blue:
                case TileType.Red:
                    if (editing)
                        TileMapUtils.SetTile(tilemap, savedTile.position, type);
                    else
                        spawnAndMovePlayer(savedTile.position, type);
                    break;

                case TileType.MovableBlock:
                    if (editing)
                        TileMapUtils.SetTile(tilemap, savedTile.position, type);
                    else
                        SpawnAndMoveEntity(savedTile.position);
                    break;


                default:
                    TileMapUtils.SetTile(tilemap, savedTile.position, type);
                    break;
            }
        }

        setGoalTiles();

        void SpawnAndMoveEntity(Vector2Int tilePos)
        {
            Instantiate(
                    movableBlockPrefab,
                    new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, 0),
                    Quaternion.identity,
                    GameObject.Find("/Players").transform);
        }

        void spawnAndMovePlayer(Vector2Int tilePos, TileType type)
        {
            string name;

            switch (type)
            {
                case TileType.Red:
                    name = "Red";
                    break;
                case TileType.Blue:
                    name = "Blue";
                    break;
                case TileType.Green:
                    name = "Green";
                    break;
                default:
                    Debug.LogError("Type " + type + " can't be a player");
                    return;
            }

            print("Spawn and move " + name);

            PlayerController pc =
                Instantiate(
                    playerPrefab,
                    new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, 0),
                    Quaternion.identity,
                    GameObject.Find("/Players").transform)
                .GetComponent<PlayerController>();

            pc.goalTile = Resources.Load<LevelTile>("LevelTiles/Goal" + name);
            pc.goalDoneTile = Resources.Load<LevelTile>("LevelTiles/GoalDone" + name);

            pc.sprites = Resources.LoadAll<Sprite>("Sprites/" + name);
        }

        Debug.Log($"Level Loaded! :)");
    }

    public static sLevel GetSLevelFromFile(int lvlNum)
    {
        // locate file from resources
        var levelFile = Resources.Load<TextAsset>($"Levels/Level {lvlNum}");
        if (levelFile == null)
        {
            Debug.LogError($"Level {lvlNum} does not exist");
            return new sLevel();
        }

        Debug.Log(levelFile.text);

        sLevel level = JsonUtility.FromJson<sLevel>(levelFile.text);
        level.levelIndex = lvlNum;
        level.levelName = $"W{LevelsController.getWorldNum(lvlNum) + 1} - Level {LevelsController.getRealLevelNum(lvlNum)}";
        return level;
    }


    private void setGoalTiles()
    {
        foreach (SavedTile savedTile in goalTiles)
            TileMapUtils.SetTile(tilemap, savedTile.position, (TileType)savedTile.tile);

    }

    public void RestartLevel()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            player.GetComponent<MovableObject>().ResetObj();

        GameController.Instance.resetGame();
        setGoalTiles();
    }


}

#if UNITY_EDITOR

public static class ScriptableObjectUnity
{
    public static void SaveLevelFile(string name, string level)
    {
        StreamWriter stream = File.CreateText($"Assets/Resources/Levels/{name}.json");

        stream.Write(level);

        stream.Close();

        AssetDatabase.Refresh();
    }
}

#endif
