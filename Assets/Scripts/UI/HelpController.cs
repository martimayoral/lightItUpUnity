using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelpController : MonoBehaviour
{
    public TextMeshProUGUI centeredText;

    Animator animator;

    enum eHelpAction
    {
        Begin,
        Change,
        Clear,
        End
    }

    struct sHelpMsg
    {
        public eHelpAction action;
        public string message;
        public TextMeshProUGUI target;
        public sHelpMsg(TextMeshProUGUI target, eHelpAction action, string message)
        {
            this.target = target;
            this.action = action;
            this.message = message;
        }
        public sHelpMsg(eHelpAction action)
        {
            this.action = action;
            this.target = null;
            this.message = null;
        }
    }

    Dictionary<int, sHelpMsg> helpTexts;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        helpTexts = new Dictionary<int, sHelpMsg>();

        switch (SceneLoader.levelNum)
        {
            case 1:
                helpTexts.Add(0, new sHelpMsg(centeredText, eHelpAction.Begin, "Drag -> to move all light bulbs"));
                helpTexts.Add(5, new sHelpMsg(eHelpAction.Clear));
                break;
            case 2:
                helpTexts.Add(7, new sHelpMsg(centeredText, eHelpAction.Begin, "Try making as less moves as possible!"));
                helpTexts.Add(14, new sHelpMsg(eHelpAction.End));
                break;
            case 3:
                helpTexts.Add(0, new sHelpMsg(centeredText, eHelpAction.Begin, "The light bulbs collide with eachother"));
                helpTexts.Add(10, new sHelpMsg(eHelpAction.End));
                break;
            default:
                Destroy(this.gameObject);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int key = GameController.Instance.movesCount;
        if (helpTexts.ContainsKey(key))
        {
            sHelpMsg helpMsg = helpTexts[key];
            helpTexts.Remove(key);

            Debug.Log(key + " , " + helpMsg.action);

            switch (helpMsg.action)
            {
                case eHelpAction.Begin:
                    animator.SetTrigger("In");
                    helpMsg.target.text = helpMsg.message;
                    break;
                case eHelpAction.Change:
                    animator.SetTrigger("Out");
                    StartCoroutine(WaitToChangeText(helpMsg.target, helpMsg.message, 0.25f));
                    StartCoroutine(WaitToTriggerAnimation("In", 0.25f));
                    break;
                case eHelpAction.Clear:
                    animator.SetTrigger("Out");
                    break;
                case eHelpAction.End:
                    animator.SetTrigger("Out");
                    Destroy(this.gameObject, 1f);
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator WaitToChangeText(TextMeshProUGUI target, string message, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        target.text = message;
    }
    IEnumerator WaitToTriggerAnimation(string animationTrigger, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        animator.SetTrigger(animationTrigger);
    }
}
