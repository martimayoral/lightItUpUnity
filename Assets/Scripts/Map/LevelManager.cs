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
}

public struct sLevel
{
    public string levelName;
    public int levelIndex;
    public List<SavedTile> tiles;
    public Scores score;
}

// this class is in charge of starting the level and loading it
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private void Awake()
    {
        Instance = this;
        tilemap = GameObject.FindObjectOfType<Tilemap>();
        levelNum = SceneLoader.levelNum;
        print($"Loading Level {levelNum}");
        LoadMap();
    }

    public Tilemap tilemap;
    public int levelNum;
    public Scores scores;

    List<SavedTile> goalTiles;

    [SerializeField] Transform playerPrefab;



#if UNITY_EDITOR
    public void SaveMap()
    {
        sLevel newLevel = new sLevel()
        {
            levelName = $"Level {levelNum}",
            levelIndex = levelNum,
            tiles = GetTilesFromMap(tilemap).ToList(),
            score = new Scores()
            {
                startingMoves = scores.startingMoves,
                goldMoves = scores.goldMoves,
                silverMoves = scores.silverMoves,
                bronzeMoves = scores.bronzeMoves
            }
        };

        var levelFile = ScriptableObject.CreateInstance<ScriptableLevel>();

        levelFile.name = $"/Level {levelNum}";
        levelFile.jsonFile = JsonUtility.ToJson(newLevel);

        print(levelFile.jsonFile);

        ScriptableObjectUnity.SaveLevelFile(levelFile);


        Debug.Log("Level Saved! :)");

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

    public void LoadBase()
    {
        int a = levelNum;
        levelNum = -1;
        LoadMap(true);
        levelNum = a;
    }
#endif

    public void ClearMap()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (Tilemap map in maps)
        {
            map.ClearAllTiles();
        }
    }



    public void LoadMap(bool editing = false)
    {
        goalTiles = new List<SavedTile>();

        // locate file from resources
        var levelFile = Resources.Load<ScriptableLevel>($"Levels/Level {levelNum}");
        if (levelFile == null)
        {
            Debug.LogError($"Level { levelNum} does not exist");
            return;
        }

        ClearMap();

        sLevel level = JsonUtility.FromJson<sLevel>(levelFile.jsonFile);

        scores = level.score;
        levelNum = level.levelIndex;

        foreach (var savedTile in level.tiles)
        {
            switch ((TileType)savedTile.tile)
            {
                case TileType.BaseWall:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/Ground"));
                    break;
                case TileType.GoalBlue:
                case TileType.GoalRed:
                case TileType.GoalGreen:
                    goalTiles.Add(savedTile);
                    break;

                // this ones spawn and move the players
                case TileType.Green:
                    if (editing)
                        setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/StartGreen"));
                    else
                        spawnAndMovePlayer(savedTile.position, "Green");
                    break;
                case TileType.Blue:
                    if (editing)
                        setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/StartBlue"));
                    else
                        spawnAndMovePlayer(savedTile.position, "Blue");
                    break;
                case TileType.Red:
                    if (editing)
                        setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/StartRed"));
                    else
                        spawnAndMovePlayer(savedTile.position, "Red");
                    break;


                default: throw new System.ArgumentOutOfRangeException();
            }


        }

        setGoalTiles();


        void spawnAndMovePlayer(Vector2Int tilePos, string name)
        {
            print("Spawn and move " + name);

            PlayerController pc =
                Instantiate(
                    playerPrefab,
                    new Vector3(tilePos.x + 0.5f, tilePos.y + 0.5f, 0),
                    Quaternion.identity,
                    GameObject.Find("/Players").transform)
                .GetComponent<PlayerController>();

            pc.goalTile = Resources.Load<LevelTile>("LevelTiles/Goal" + name);
            pc.goalDoneTile = Resources.Load<LevelTile>("LevelTiles/Start" + name);

            pc.sprites = Resources.LoadAll<Sprite>("Sprites/" + name);

        }

        Debug.Log($"{levelNum} Loaded! :)");
    }

    void setTile(Vector2Int tilePos, TileBase tile)
    {
        tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tile);
    }

    private void setGoalTiles()
    {
        foreach (SavedTile savedTile in goalTiles)
            switch ((TileType)savedTile.tile)
            {
                case TileType.GoalBlue:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalBlue"));
                    break;
                case TileType.GoalRed:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalRed"));
                    break;
                case TileType.GoalGreen:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalGreen"));
                    break;
                default: throw new System.ArgumentOutOfRangeException();
            }
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
