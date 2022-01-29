using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelControllerUI : MonoBehaviour
{
    public Image image;
    public int levelId;

    bool locked;

    // Start is called before the first frame update
    void Start()
    {
        if (LevelsController.levelMedals != null)
        {
            if (LevelsController.levelMedals.Length < levelId)
            {
                // image.sprite = Resources.LoadAll<Sprite>("UI/stars")[0];
                Debug.Log("User medal " + levelId + " not found");
                return;
            }

            image.sprite = Resources.LoadAll<Sprite>("UI/stars")[(int)LevelsController.levelMedals[levelId]];
        }
        else
        {
            Debug.LogWarning("Could not load Level medals");
        }

        locked = LevelsController.lastLevelCompleted + 1 < levelId;

        GetComponent<CanvasGroup>().interactable = !locked;

    }

    public void OnClick()
    {
        if (!locked)
        {
            SceneLoader.Instance.LoadLevel(levelId);
        }
    }

}
