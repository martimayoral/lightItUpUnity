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


public class LevelManager : MonoBehaviour
{
    public GameObject gameController;
    public static LevelManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Tilemap tilemap;
    public int levelNum;
    public Scores scores;

    [SerializeField] Transform playerPrefab;


    public void Start()
    {
        print("Loading tilemap");
        tilemap = GameObject.FindObjectOfType<Tilemap>();
        levelNum = MenuController.levelNum;
        LoadMap();
    }

    public void SaveMap()
    {
        sLevel newLevel = new sLevel() {
            levelName = $"Level {levelNum}",
            levelIndex = levelNum,
            tiles = GetTilesFromMap(tilemap).ToList(),
            score = new Scores() {
                startingMoves = scores.startingMoves,
                goldMoves = scores.goldMoves, 
                silverMoves = scores.silverMoves,
                bronzeMoves = scores.bronzeMoves
            }
    };

        /* ----- SAVE TO LOCAL APP
        StreamWriter sw = new StreamWriter(Application.dataPath + $"/Level {_levelIndex}");
        if (sw == null) {
            Debug.LogError("Could not create a Stream Writer for saving the level");
            return;
        }

        sw.Write(JsonUtility.ToJson(newLevel));
        sw.Close();
        */

        var levelFile = ScriptableObject.CreateInstance<ScriptableLevel>();

        levelFile.name = $"/Level {levelNum}";
        levelFile.jsonFile = JsonUtility.ToJson(newLevel);
        
        print(levelFile.jsonFile);

        ScriptableObjectUnity.SaveLevelFile(levelFile);

        Debug.Log("Level Saved! :)");

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach(var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    var levelTile = map.GetTile<LevelTile>(pos);
                    yield return new SavedTile()
                    {
                        position = new Vector2Int(pos.x,pos.y),
                        tile = (int) levelTile.type
                    };
                }
            }
        }

    }

    public void ClearMap()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach(Tilemap map in maps)
        {
            map.ClearAllTiles();
        }
    }

    private void LevelLoaded()
    {
        gameController.SetActive(true);
    }

    public void LoadMap(bool editing = false)
    {
        /* ----- LOAD FROM LOCAL APP
        string levelPath = Application.dataPath + $"/Level {_levelIndex}";

        if (!File.Exists(levelPath))
        {
            Debug.LogError($"Level { _levelIndex} does not exist");
            return;
        }

        StreamReader sr = new StreamReader(levelPath);

        string jsonString = sr.ReadToEnd();
        */

        // locate file from resources
        var levelFile = Resources.Load<ScriptableLevel>($"Levels/Level {levelNum}");
        if(levelFile == null)
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
            switch ((TileType) savedTile.tile)
            {
                case TileType.BaseWall:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/Ground"));
                    break;
                case TileType.GoalBlue:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalBlue"));
                    break;
                case TileType.GoalRed:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalRed"));
                    break;
                case TileType.GoalGreen:
                    setTile(savedTile.position, Resources.Load<LevelTile>("LevelTiles/GoalGreen"));
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

            LevelLoaded();
        }

        void setTile(Vector2Int tilePos, TileBase tile)
        {
            tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tile);
        }

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

            pc.offSprite = Resources.Load<Sprite>("Sprites/" + name );
            pc.onSprite = Resources.LoadAll<Sprite>("Sprites/" + name)[4];

        }

        Debug.Log($"{levelNum} Loaded! :)");
    }

    public void NextLevel()
    {
        levelNum++;
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
