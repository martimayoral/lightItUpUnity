using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // draw what it has by default
        DrawDefaultInspector();

        LevelManager script = (LevelManager)target;

        if (GUILayout.Button("Save Map"))
        {
            script.SaveMapInEditor();
        }

        if (GUILayout.Button("Clear Map"))
        {
            script.LoadBase();
        }

        if (GUILayout.Button("Load Map"))
        {
            script.LoadMapInEditor(script.levelNum, true);
        }
    }


}

#endif