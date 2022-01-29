using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ToolSelectComponent : MonoBehaviour
{
    public EditorController.Tool tool;

    [HideInInspector]
    public TileType type;

    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void setSelected(bool isSelected)
    {
        if (isSelected && tool != EditorController.Tool.None)
            image.color = new Color(0, 0, 0, image.color.a);
        else
            image.color = new Color(1, 1, 1, image.color.a);
    }


}

#if UNITY_EDITOR
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
#endif