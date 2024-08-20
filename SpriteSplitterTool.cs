using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSplitterTool : EditorWindow
{
    private Texture2D sourceTexture;
    private int rows = 1;
    private int columns = 1;

    [MenuItem("Tools/Sprite Splitter")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSplitterTool>("Sprite Splitter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Splitter", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
        rows = EditorGUILayout.IntField("Rows", rows);
        columns = EditorGUILayout.IntField("Columns", columns);

        if (GUILayout.Button("Split Sprite"))
        {
            if (sourceTexture != null)
            {
                SplitSprite();
            }
            else
            {
                Debug.LogError("Please assign a source texture.");
            }
        }
    }

    private void SplitSprite()
    {
        // 원본 텍스처를 읽기 가능한 새 텍스처로 복사
        Texture2D readableTexture = CopyTexture(sourceTexture);

        int spriteWidth = readableTexture.width / columns;
        int spriteHeight = readableTexture.height / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect spriteRect = new Rect(x * spriteWidth, readableTexture.height - (y + 1) * spriteHeight, spriteWidth, spriteHeight);
                Texture2D newTexture = new Texture2D(spriteWidth, spriteHeight);
                newTexture.SetPixels(readableTexture.GetPixels((int)spriteRect.x, (int)spriteRect.y, spriteWidth, spriteHeight));
                newTexture.Apply();

                byte[] bytes = newTexture.EncodeToPNG();
                string path = AssetDatabase.GetAssetPath(sourceTexture);
                string directory = Path.GetDirectoryName(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string newPath = Path.Combine(directory, $"{fileName}_split_{y}_{x}.png");
                File.WriteAllBytes(newPath, bytes);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Sprite splitting complete!");
    }

    private Texture2D CopyTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
