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
        // ���õ� ������Ʈ���� �����ɴϴ�.
        GameObject[] selectedObjects = Selection.gameObjects;

        // ���õ� ������Ʈ�� ������ ��� �޽����� ����մϴ�.
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        // ���õ� ������Ʈ���� �̸� ������������ �����մϴ�.
        System.Array.Sort(selectedObjects, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // ������Ʈ �̸� �ڿ� ���ڸ� ���Դϴ�.
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].name = baseName + i.ToString();
        }

        // ���̾��Ű�並 ������Ʈ�մϴ�.
        EditorApplication.RepaintHierarchyWindow();
    }
}
