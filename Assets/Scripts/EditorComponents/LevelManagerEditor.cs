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

        GUILayout.Label("Save");

        if (GUILayout.Button("Save Map"))
        {
            script.SaveMapInEditor();
        }

        if (GUILayout.Button("Save New map"))
        {
            script.SaveNewMapInEditor();
        }


        GUILayout.Label("Load");

        if (GUILayout.Button("Load Map"))
        {
            script.LoadMapInEditor(script.levelNum, true);
        }

        GUILayout.Label("Other");

        if (GUILayout.Button("Clear Map"))
        {
            script.LoadBase();
        }

        if (GUILayout.Button("Save online data"))
        {
            script.SaveOnlineData();
        }

    }


}

#endif