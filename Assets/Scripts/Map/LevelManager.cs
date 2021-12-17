using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System;
using System.Text;

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

public struct sLevel
{
    public string levelName;
    public int levelIndex;
    public List<SavedTile> tiles;
    public Scores score;
    public eLevelSize levelSize;
}

// this class is in charge of starting the level and loading it
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private void Awake()
    {
        Instance = this;
        tilemap = GameObject.FindObjectOfType<Tilemap>();
    }

    public Tilemap tilemap;

    public Scores scores;

    public eLevelSize levelSize;

    List<SavedTile> goalTiles;

    [SerializeField] Transform playerPrefab;



#if UNITY_EDITOR
    public int levelNum;

    public void SaveMap()
    {
        sLevel newLevel = TileMapUtils.CreateLevel($"Level {levelNum}", levelNum, tilemap, scores, levelSize);

        var levelFile = ScriptableObject.CreateInstance<ScriptableLevel>();

        levelFile.name = $"/Level {levelNum}";
        levelFile.jsonFile = JsonUtility.ToJson(newLevel);

        print(levelFile.jsonFile);

        ScriptableObjectUnity.SaveLevelFile(levelFile);

        Debug.Log("Level Saved! :)");
    }
#endif


    public void LoadBase()
    {
        ClearMap();
        TileMapUtils.LoadMapBorders(tilemap, true, levelSize);
    }

    public void ClearMap()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (Tilemap map in maps)
        {
            map.ClearAllTiles();
        }

        goalTiles.Clear();
    }

    public void LoadMap(sLevel level, bool editing)
    {
        ClearMap();

        TileMapUtils.LoadMapBorders(tilemap, editing, level.levelSize);

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

                default:
                    TileMapUtils.SetTile(tilemap, savedTile.position, type);
                    break;
            }
        }

        setGoalTiles();

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

    public void LoadMap(int lvlNum, bool editing = false)
    {
        print($"Loading Level {lvlNum}");

        goalTiles = new List<SavedTile>();

        // locate file from resources
        var levelFile = Resources.Load<ScriptableLevel>($"Levels/Level {lvlNum}");
        if (levelFile == null)
        {
            Debug.LogError($"Level {lvlNum} does not exist");
            return;
        }

        sLevel level = JsonUtility.FromJson<sLevel>(levelFile.jsonFile);

        Debug.Assert(lvlNum == level.levelIndex);

        LoadMap(level, editing);

        Debug.Log($"{lvlNum} Loaded! :)");
    }


    private void setGoalTiles()
    {
        foreach (SavedTile savedTile in goalTiles)
            TileMapUtils.SetTile(tilemap, savedTile.position, (TileType)savedTile.tile);

    }

    public void RestartLevel()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            player.GetComponent<PlayerController>().reset();

        GameController.Instance.resetGame();
        setGoalTiles();
    }


}

#if UNITY_EDITOR

public static class ScriptableObjectUnity
{
    public static void SaveLevelFile(ScriptableLevel level)
    {
        AssetDatabase.CreateAsset(level, $"Assets/Resources/Levels/{level.name}.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif
