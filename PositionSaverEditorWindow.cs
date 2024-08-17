using UnityEditor;
using UnityEngine;

public class PositionSaverEditorWindow : EditorWindow
{
    private PositionData positionData;

    [MenuItem("Tools/Position Saver")]
    public static void ShowWindow()
    {
        GetWindow<PositionSaverEditorWindow>("Position Saver");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save Positions to ScriptableObject", EditorStyles.boldLabel);

        positionData = (PositionData)EditorGUILayout.ObjectField("Position Data", positionData, typeof(PositionData), false);

        if (GUILayout.Button("Save Selected Transforms"))
        {
            SaveSelectedTransforms();
        }
    }

    private void SaveSelectedTransforms()
    {
        if (positionData == null)
        {
            Debug.LogError("Please assign a PositionDataScriptableObject.");
            return;
        }

        Transform[] selectedTransforms = Selection.transforms;
        positionData.positions = new Vector3[selectedTransforms.Length];

        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            positionData.positions[i] = selectedTransforms[i].position;
        }

        EditorUtility.SetDirty(positionData);
        Debug.Log("Positions saved to ScriptableObject.");
    }
}
