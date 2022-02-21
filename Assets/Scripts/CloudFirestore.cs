using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using System;

public class CloudFirestore : MonoBehaviour
{
    FirebaseFirestore db;
    Dictionary<string, object> user;
    public static CloudFirestore Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }


    // DATABASE FIRESTORE

    public void SaveData()
    {
        Debug.Log("SAVINGDATA");

        user = new Dictionary<string, object>
        {
            {"UserName", "usernametext" }
        };

        db.Collection("Users").Document().SetAsync(user).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Succesfully added");
            }
            else
            {
                Debug.Log("not succesfully");
            }
        });
    }

    Dictionary<string, object> OLevelToDLevel(OnlineLevel oLevel)
    {
        List<object> tiles = new List<object>();

        foreach (SavedTile tile in oLevel.tiles)
        {
            tiles.Add(
                new Dictionary<string, object>
                {
                {"Type", tile.tile },
                {"x", tile.position.x },
                {"y", tile.position.y }
                });
        }

        return new Dictionary<string, object>
        {
            {"Name", oLevel.levelName },
            {"Creator", oLevel.creatorName },
            {"Size", oLevel.levelSize},
            {"Scores", new Dictionary<string, object>
                {
                    {"Starting Moves", oLevel.score.startingMoves },
                    {"Gold Moves", oLevel.score.goldMoves },
                    {"Silver Moves", oLevel.score.silverMoves },
                    {"Bronze Moves", oLevel.score.bronzeMoves }
                }
            },
            {"Tiles", tiles},
            {"Stats", new Dictionary<string, object>
                {
                    {"Times Played", 0 },
                    {"Gold Medals", 0 },
                    {"Wins", 0 },
                    {"Difficulty", -1}
                }
            },
            {"Timestamp", Timestamp.GetCurrentTimestamp() }
        };
    }

    OnlineLevel DLevelToOLevel(Dictionary<string, object> dlevel, string levelId)
    {
        Debug.Log("Tranforming level " + (string)dlevel["Name"]);

        Dictionary<string, object> scores = (Dictionary<string, object>)dlevel["Scores"];


        List<object> tiles = (List<object>)dlevel["Tiles"];
        List<SavedTile> savedTiles = new List<SavedTile>();

        foreach (object tile in tiles)
        {
            Dictionary<string, object> dtile = (Dictionary<string, object>)tile;

            savedTiles.Add(new SavedTile
            {
                position = new Vector2Int(Convert.ToInt32(dtile["x"]), Convert.ToInt32(dtile["y"])),
                tile = Convert.ToInt32(dtile["Type"])
            });
        }

        OnlineLevel onlineLevel = new OnlineLevel
        {
            levelIndex = 0,
            levelSize = (eLevelSize)Convert.ToInt32(dlevel["Size"]),
            levelName = Convert.ToString(dlevel["Name"]),
            score = new Scores
            {
                startingMoves = Convert.ToInt32(scores["Starting Moves"]),
                goldMoves = Convert.ToInt32(scores["Gold Moves"]),
                silverMoves = Convert.ToInt32(scores["Silver Moves"]),
                bronzeMoves = Convert.ToInt32(scores["Bronze Moves"]),
            },
            tiles = savedTiles,

            creatorName = Convert.ToString(dlevel["Creator"]),
            levelId = levelId

        };

        if (dlevel.ContainsKey("Stats"))
        {
            Dictionary<string, object> stats = (Dictionary<string, object>)dlevel["Stats"];
            onlineLevel.stats = new OnlineLevel.Stats
            {
                goldMedals = Convert.ToInt32(stats["Gold Medals"]),
                timesPlayed = Convert.ToInt32(stats["Times Played"]),
                wins = Convert.ToInt32(stats["Wins"])
            };
        }

        if (dlevel.ContainsKey("Timestamp"))
        {
            Timestamp timestamp = (Timestamp)dlevel["Timestamp"];
            onlineLevel.createdAt = timestamp.ToDateTime().ToShortDateString();
        }
        else
        {
            CreateTimestamp(levelId);
            onlineLevel.createdAt = Timestamp.GetCurrentTimestamp().ToDateTime().ToShortDateString();
        }

        return onlineLevel;
    }

    public void DoActionForEachLevelAsync(Action<OnlineLevel> action)
    {
        Query allLevelsQuery = db.Collection("Levels");
        allLevelsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allLevelsQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allLevelsQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));

                Dictionary<string, object> dlevel = documentSnapshot.ToDictionary();

                OnlineLevel slevel = DLevelToOLevel(dlevel, documentSnapshot.Id);

                action.Invoke(slevel);
            }
        });
    }

    public static DocumentSnapshot latestDoc;
    public enum eOrderListBy
    {
        TIMESTAMP,
        NAME,
        CREATOR_NAME,
        TIMES_PLAYED,
        DIFFICULTY,
        LEVEL_SIZE,
        MOVES
    }
    public void PopulateListAndDoActionAsync(Action<bool> action)
    {
        Debug.Log("Start at " + OnlineLevelsController.onlineLevelsList.Count);

        Query filteredLevelsQuery;

        eOrderListBy orderListByEnum = UserConfig.orderOnlineListBy;
        Debug.Log("Ordering by " + orderListByEnum);
        string orderListBy;

        switch (orderListByEnum)
        {
            case eOrderListBy.TIMESTAMP:
                orderListBy = "Timestamp";
                break;
            case eOrderListBy.NAME:
                orderListBy = "Name";
                break;
            case eOrderListBy.CREATOR_NAME:
                orderListBy = "Creator";
                break;
            case eOrderListBy.TIMES_PLAYED:
                orderListBy = "Stats.Times Played";
                break;
            case eOrderListBy.DIFFICULTY:
                orderListBy = "Stats.Difficulty";
                break;
            case eOrderListBy.LEVEL_SIZE:
                orderListBy = "Size";
                break;
            case eOrderListBy.MOVES:
                orderListBy = "Scores.Starting Moves";
                break;
            default:
                orderListBy = "Timestamp";
                break;
        }

        filteredLevelsQuery = db.Collection("Levels");

        if (UserConfig.filterOnlineName != "")
        {
            filteredLevelsQuery = filteredLevelsQuery.WhereEqualTo("Name", UserConfig.filterOnlineName);
        }
        else
        {
            if (UserConfig.orderOnlineListAscending)
                filteredLevelsQuery = filteredLevelsQuery.OrderBy(orderListBy);
            else
                filteredLevelsQuery = filteredLevelsQuery.OrderByDescending(orderListBy);
        }


        if (latestDoc != null)
            filteredLevelsQuery = filteredLevelsQuery.StartAfter(latestDoc);

        filteredLevelsQuery = filteredLevelsQuery.Limit(UserConfig.onlineLoadBatchSize);


        StartCoroutine(PLADAA());

        IEnumerator PLADAA()
        {
            var task = filteredLevelsQuery.GetSnapshotAsync();

            DBGText.Write("Starting query");

            LoadingScreen.Instance.StartFullScreenSpinner();
            yield return new WaitUntil(() => task.IsCompleted);

            QuerySnapshot allLevelsQuerySnapshot = task.Result;

            DBGText.Write("Query result: " + task.Result);

            if (task.Exception != null)
                DBGText.Write("QUERY EXCEPTION: " + task.Exception);

            int count = 0;

            foreach (DocumentSnapshot documentSnapshot in allLevelsQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                DBGText.Write("Document data for {0} document: " + documentSnapshot.Id);

                Dictionary<string, object> dlevel = documentSnapshot.ToDictionary();

                OnlineLevel slevel = DLevelToOLevel(dlevel, documentSnapshot.Id);

                slevel.levelId = documentSnapshot.Id;

                slevel.levelIndex = OnlineLevelsController.onlineLevelsList.Count;

                latestDoc = documentSnapshot;
                count++;

                DBGText.Write("Level Added");
                OnlineLevelsController.onlineLevelsList.Add(slevel);
            }
            bool isLast = UserConfig.onlineLoadBatchSize != count;

            DBGText.Write("Invoke Action");
            action.Invoke(isLast);
            DBGText.Write("Action Invoked");

            yield return new WaitForSeconds(0.3f);
            LoadingScreen.Instance.StopAll();
        }
    }

    public void SaveLevel(OnlineLevel slevel, Action onSuccess, Action onFail)
    {
        Debug.Log("SAVINGLEVEL");

        Dictionary<string, object> dlevel = OLevelToDLevel(slevel);

        StartCoroutine(ISaveLevel());

        IEnumerator ISaveLevel()
        {
            var createLevelTask = db.Collection("Levels").Document().SetAsync(dlevel);

            LoadingScreen.Instance.StartFullScreenSpinner();

            yield return new WaitUntil(() => createLevelTask.IsCompleted);

            LoadingScreen.Instance.StopAll();

            if (createLevelTask.Exception == null)
            {
                onSuccess.Invoke();
            }
            else
            {
                onFail.Invoke();
            }
        }
    }


    void CreateTimestamp(string databaseId)
    {
        DocumentReference docRef = db.Collection("Levels").Document(databaseId);

        StartCoroutine(IUpdateLevel());

        IEnumerator IUpdateLevel()
        {
            Dictionary<string, object> update = new Dictionary<string, object>{
                {"Timestamp", Timestamp.GetCurrentTimestamp() }
            };

            var mergeTask = docRef.SetAsync(update, SetOptions.MergeAll);

            yield return new WaitUntil(() => mergeTask.IsCompleted);

            if (mergeTask.Exception != null)
            {
                Debug.Log("Something Failed Updating Field");
            }
        }
    }

    // update
    public void UpdateStatsField(string databaseId, int timesPlayedToAdd, int winsToAdd, int goldMedalToAdd)
    {
        DocumentReference docRef = db.Collection("Levels").Document(databaseId);

        StartCoroutine(IUpdateLevel());

        IEnumerator IUpdateLevel()
        {
            var task = docRef.GetSnapshotAsync();
            yield return new WaitUntil(() => task.IsCompleted);


            if (task.Exception != null)
            {
                Debug.Log(String.Format("Document {0} does not exist!", databaseId));
                yield break;
            }

            DocumentSnapshot snapshot = task.Result;
            if (!snapshot.Exists)
            {
                Debug.Log(String.Format("Document {0} does not exist!", databaseId));
                yield break;
            }

            Dictionary<string, object> dLevel = snapshot.ToDictionary();

            Debug.Assert(timesPlayedToAdd >= 0);

            Dictionary<string, int> stats = new Dictionary<string, int>
            {
                {"Times Played", timesPlayedToAdd },
                {"Gold Medals", goldMedalToAdd },
                {"Wins", winsToAdd },
                {"Difficulty", 0 }
            };

            if (dLevel.ContainsKey("Stats"))
            {
                Dictionary<string, object> dLevelStats = (Dictionary<string, object>)dLevel["Stats"];
                stats["Times Played"] += Convert.ToInt32(dLevelStats["Times Played"]);
                stats["Wins"] += Convert.ToInt32(dLevelStats["Wins"]);
                stats["Gold Medals"] += Convert.ToInt32(dLevelStats["Gold Medals"]);
            }

            stats["Difficulty"] = OnlineLevelsController.CalculateDifficulty(stats["Times Played"], stats["Wins"]);

            Dictionary<string, object> update = new Dictionary<string, object>{
                { "Stats", stats }
            };

            var mergeTask = docRef.SetAsync(update, SetOptions.MergeAll);

            yield return new WaitUntil(() => mergeTask.IsCompleted);

            if (mergeTask.Exception != null)
            {
                Debug.Log("Something Failed Updating Field");
            }
        }
    }

}
