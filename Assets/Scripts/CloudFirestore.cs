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

    Dictionary<string, object> SLevelToDLevel(sLevel slevel)
    {
        List<object> tiles = new List<object>();

        foreach (SavedTile tile in slevel.tiles)
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
            {"Name", slevel.levelName },
            {"Creator", slevel.creatorName },
            {"Size", slevel.levelSize},
            {"Scores", new Dictionary<string, object>
                {
                    {"Starting Moves", slevel.score.startingMoves },
                    {"Gold Moves", slevel.score.goldMoves },
                    {"Silver Moves", slevel.score.silverMoves },
                    {"Bronze Moves", slevel.score.bronzeMoves }
                }
            },
            {"Tiles", tiles}
        };
    }

    static sLevel DLevelToSLevel(Dictionary<string, object> dlevel, string levelId)
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

        return new sLevel
        {
            levelIndex = 0,
            levelSize = (eLevelSize)Convert.ToInt32(dlevel["Size"]),
            levelName = Convert.ToString(dlevel["Name"]),
            creatorName = Convert.ToString(dlevel["Creator"]),
            score = new Scores
            {
                startingMoves = Convert.ToInt32(scores["Starting Moves"]),
                goldMoves = Convert.ToInt32(scores["Gold Moves"]),
                silverMoves = Convert.ToInt32(scores["Silver Moves"]),
                bronzeMoves = Convert.ToInt32(scores["Bronze Moves"]),
            },
            tiles = savedTiles
        };
    }

    public void DoActionForEachLevelAsync(Action<sLevel> action)
    {
        Query allLevelsQuery = db.Collection("Levels");
        allLevelsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allLevelsQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allLevelsQuerySnapshot.Documents)
            {
                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));

                Dictionary<string, object> dlevel = documentSnapshot.ToDictionary();

                sLevel slevel = DLevelToSLevel(dlevel, documentSnapshot.Id);

                slevel.levelId = documentSnapshot.Id;

                action.Invoke(slevel);
            }
        });
    }

    public static DocumentSnapshot latestDoc;
    public void PopulateListAndDoActionAsync(Action<bool> action)
    {
        Debug.Log("Start at " + LevelsController.onlineLevelsList.Count);

        Query allLevelsQuery;


        if (latestDoc != null)
            allLevelsQuery = db.Collection("Levels").OrderBy("Name").StartAfter(latestDoc).Limit(UserConfig.onlineLoadBatchSize);
        else
            allLevelsQuery = db.Collection("Levels").OrderBy("Name").Limit(UserConfig.onlineLoadBatchSize);

        StartCoroutine(PLADAA());

        IEnumerator PLADAA()
        {
            var task = allLevelsQuery.GetSnapshotAsync();

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

                sLevel slevel = DLevelToSLevel(dlevel, documentSnapshot.Id);

                slevel.levelId = documentSnapshot.Id;

                slevel.levelIndex = LevelsController.onlineLevelsList.Count;

                latestDoc = documentSnapshot;
                count++;

                DBGText.Write("Level Added");
                LevelsController.onlineLevelsList.Add(slevel);
            }
            bool isLast = UserConfig.onlineLoadBatchSize != count;

            DBGText.Write("Invoke Action");
            action.Invoke(isLast);
            DBGText.Write("Action Invoked");

            yield return new WaitForSeconds(0.3f);
            LoadingScreen.Instance.StopAll();
        }
    }

    public void SaveLevel(sLevel slevel, Action onSuccess, Action onFail)
    {
        Debug.Log("SAVINGLEVEL");

        Dictionary<string, object> dlevel = SLevelToDLevel(slevel);

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

}
