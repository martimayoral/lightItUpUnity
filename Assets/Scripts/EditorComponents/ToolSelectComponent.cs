using UnityEngine;
using UnityEditor;

public class ToolSelectComponent : MonoBehaviour
{
    public EditorController.Tool tool;

    [HideInInspector]
    public TileType type;
}

[CustomEditor(typeof(ToolSelectComponent))]
public class ToolSelectComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // draw what it has by default
        DrawDefaultInspector();

        ToolSelectComponent script = (ToolSelectComponent)target;

        if (script.tool == EditorController.Tool.PaintTile)
        {
            script.type = (TileType)EditorGUILayout.EnumPopup("Type", script.type);
            EditorUtility.SetDirty(script);
        }

    }
}
