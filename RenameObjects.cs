using UnityEngine;
using UnityEditor;

public class RenameObjects : EditorWindow
{
    private string baseName = "";

    [MenuItem("Tools/Rename Selected Objects")]
    public static void ShowWindow()
    {
        GetWindow<RenameObjects>("Rename Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Name for Objects", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField("Base Name", baseName);

        if (GUILayout.Button("Rename"))
        {
            RenameSelectedObjects();
        }
    }

    private void RenameSelectedObjects()
    {
        // 선택된 오브젝트들을 가져옵니다.
        GameObject[] selectedObjects = Selection.gameObjects;

        // 선택된 오브젝트가 없으면 경고 메시지를 출력합니다.
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        // 선택된 오브젝트들을 이름 오름차순으로 정렬합니다.
        System.Array.Sort(selectedObjects, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // 오브젝트 이름 뒤에 숫자를 붙입니다.
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].name = baseName + i.ToString();
        }

        // 하이어라키뷰를 업데이트합니다.
        EditorApplication.RepaintHierarchyWindow();
    }
}
