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
            //Debug.LogWarning("DESACTIVAT");
        }

        if (GUILayout.Button("Save New map"))
        {
            Debug.LogWarning("DESACTIVAT");
            //script.SaveNewMapInEditor();
        }

        if (GUILayout.Button("Save Hint Level"))
        {
            script.SaveHintMapInEditor();
        }


        GUILayout.Label("Load");

        if (GUILayout.Button("Load Map"))
        {
            script.LoadMapInEditor(false);
        }
        if (GUILayout.Button("Load Hint Map"))
        {
            script.LoadMapInEditor(true);
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