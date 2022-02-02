using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelpController : MonoBehaviour
{
    public TextMeshProUGUI centeredText;

    Animator animator;

    Dictionary<int, helpMsg> helpTexts;
    int lastKeyUsed;

    // Start is called before the first frame update
    void Start()
    {
        if (LevelsController.CurrentLevelIsOnline())
            Destroy(gameObject);

        if (SaveHelpText.levelMsgs.ContainsKey(LevelsController.currentLevel.levelIndex))
            helpTexts = SaveHelpText.levelMsgs[LevelsController.currentLevel.levelIndex];
        else
            Destroy(gameObject);

        animator = GetComponent<Animator>();
        lastKeyUsed = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (helpTexts == null)
            return;

        int key = GameController.Instance.movesCount;

        if (key <= lastKeyUsed) return;

        if (helpTexts.ContainsKey(key))
        {
            helpMsg helpMsg = helpTexts[key];
            lastKeyUsed = key;

            Debug.Log(key + " , " + helpMsg.action);

            switch (helpMsg.action)
            {
                case "Begin":
                    animator.SetTrigger("In");
                    centeredText.text = helpMsg.message;
                    break;
                case "Change":
                    animator.SetTrigger("Out");
                    StartCoroutine(WaitToChangeText(helpMsg.message, 0.25f));
                    StartCoroutine(WaitToTriggerAnimation("In", 0.25f));
                    break;
                case "End":
                    animator.SetTrigger("Out");
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator WaitToChangeText(string message, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        centeredText.text = message;
    }
    IEnumerator WaitToTriggerAnimation(string animationTrigger, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        animator.SetTrigger(animationTrigger);
    }
}
