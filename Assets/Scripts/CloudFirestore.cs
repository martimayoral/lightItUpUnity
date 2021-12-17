using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

public class CloudFirestore : MonoBehaviour
{
    FirebaseFirestore db;
    Dictionary<string, object> user;
    public static CloudFirestore i;

    private void Awake()
    {
        i = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

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

    public void SaveLevel(sLevel slevel)
    {
        Debug.Log("SAVINGLEVEL");

        Dictionary<string, object> dlevel = SLevelToDLevel(slevel);

        db.Collection("Levels").Document().SetAsync(dlevel).ContinueWith(task =>
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

    // Update is called once per frame
    void Update()
    {

    }
}
