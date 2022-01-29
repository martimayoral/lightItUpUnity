using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldUIController : MonoBehaviour
{


    [SerializeField] int worldNum;
    [SerializeField] CanvasGroup lockGroup;
    [SerializeField] TextMeshProUGUI lockText;

    [SerializeField] Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        if (LevelsController.currentWorld == worldNum)
            animator.SetTrigger("inRight");

        if(LevelsController.isWorldLocked(worldNum))
            lockGroup.alpha = 1;
        else
            lockGroup.alpha = 0;

        lockText.text = LevelsController.numOfflineStars + "/" + LevelsController.worldMinStars[worldNum];
    }


    public void InRight()
    {
        animator.SetTrigger("inRight");
    }
    public void InLeft()
    {
        animator.SetTrigger("inLeft");
    }
    public void OutRight()
    {
        animator.SetTrigger("outRight");
    }
    public void OutLeft()
    {
        animator.SetTrigger("outLeft");
    }
}
