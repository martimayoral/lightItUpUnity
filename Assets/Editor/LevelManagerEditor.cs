using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    bool overrideMap = false;
    public override void OnInspectorGUI()
    {
        // draw what it has by default
        DrawDefaultInspector();

        LevelManager script = (LevelManager) target;

        if (GUILayout.Button("Save Map"))
        {
            if(!overrideMap)
                script.NextLevel();
            script.SaveMap();
        }

        if (GUILayout.Button("Clear Map"))
        {
            script.ClearMap();
        }

        if (GUILayout.Button("Load Map"))
        {
            script.LoadMap(true);
        }

        overrideMap = GUILayout.Toggle(overrideMap, "Override");
    }


}
