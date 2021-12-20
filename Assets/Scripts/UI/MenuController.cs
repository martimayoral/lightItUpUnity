using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    Animator animator;

    [SerializeField] Transform levelInfo;
    [SerializeField] Transform levelInfoParent;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void BtnLoadLevelEditor()
    {

        SceneLoader.Instance.LoadLevelEditor();
    }

    public void BtnChangePanel(string triger)
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Select);
        animator.SetTrigger(triger);
    }

    public void BtnSaveUserConfig()
    {
        SaveUserConfig.SaveUserConfigData();
    }

    public void BtnUserLevelsPanel()
    {
        for (int i = 0; i < levelInfoParent.childCount; i++)
        {
            Destroy(levelInfoParent.GetChild(i).gameObject);
        }

        Action<sLevel> OnLevelLoad = (level) =>
         {
             Debug.Log(String.Format(":) {0}", level.levelName));

             Transform levelObjectTransform = Instantiate(levelInfo, levelInfoParent);
             levelObjectTransform.GetComponent<LevelInfoController>().levelInfo = level;
         };

        CloudFirestore.Instance.DoActionForEachLevelAsync(OnLevelLoad);
    }

}
