using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class LegacyFontTextureExporter : EditorWindow
{
    private Font legacyFont;
    private string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private int fontSize = 32;
    private int spacingPixels = 2;
    private int rowsPerTexture = 10;

    [MenuItem("Tools/Legacy Font Texture Exporter")]
    public static void ShowWindow()
    {
        GetWindow<LegacyFontTextureExporter>("Font Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Legacy Font Texture Exporter", EditorStyles.boldLabel);

        legacyFont = (Font)EditorGUILayout.ObjectField("Legacy Font", legacyFont, typeof(Font), false);
        characters = EditorGUILayout.TextField("Characters", characters);
        fontSize = EditorGUILayout.IntField("Font Size", fontSize);
        spacingPixels = EditorGUILayout.IntField("Spacing (pixels)", spacingPixels);
        rowsPerTexture = EditorGUILayout.IntField("Rows Per Texture", rowsPerTexture);

        if (GUILayout.Button("Export Texture"))
        {
            ExportTexture();
        }
    }

    private void ExportTexture()
    {
        if (legacyFont == null)
        {
            EditorUtility.DisplayDialog("Error", "Legacy font is not assigned!", "OK");
            return;
        }

        Texture2D fontAtlasTexture = ConvertToTexture2D(legacyFont.material.mainTexture);

        legacyFont.RequestCharactersInTexture(characters, fontSize);

        List<CharacterData> charDataList = new List<CharacterData>();
        int maxCharWidth = 0;
        int maxCharHeight = 0;

        foreach (char c in characters)
        {
            CharacterInfo charInfo;
            if (legacyFont.GetCharacterInfo(c, out charInfo, fontSize))
            {
                Rect uvRect = new Rect(
                    charInfo.uvBottomLeft.x * fontAtlasTexture.width,
                    charInfo.uvBottomLeft.y * fontAtlasTexture.height,
                    (charInfo.uvTopRight.x - charInfo.uvBottomLeft.x) * fontAtlasTexture.width,
                    (charInfo.uvTopRight.y - charInfo.uvBottomLeft.y) * fontAtlasTexture.height
                );

                int charWidth = Mathf.CeilToInt(uvRect.width);
                int charHeight = Mathf.CeilToInt(uvRect.height);

                charDataList.Add(new CharacterData
                {
                    Character = c,
                    UvRect = uvRect,
                    Width = charWidth,
                    Height = charHeight
                });

                maxCharWidth = Mathf.Max(maxCharWidth, charWidth);
                maxCharHeight = Mathf.Max(maxCharHeight, charHeight);
            }
        }

        int charsPerRow = Mathf.CeilToInt((float)charDataList.Count / rowsPerTexture);
        int actualRows = Mathf.CeilToInt((float)charDataList.Count / charsPerRow);

        int textureWidth = (maxCharWidth + spacingPixels) * charsPerRow - spacingPixels;
        int textureHeight = (maxCharHeight + spacingPixels) * actualRows - spacingPixels;

        Texture2D fontTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        Color[] clearColors = new Color[textureWidth * textureHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }
        fontTexture.SetPixels(clearColors);

        int x = 0, y = textureHeight - maxCharHeight;
        int charCount = 0;

        foreach (var charData in charDataList)
        {
            Color[] charPixels = fontAtlasTexture.GetPixels(
                Mathf.FloorToInt(charData.UvRect.x),
                Mathf.FloorToInt(charData.UvRect.y),
                charData.Width,
                charData.Height
            );

            fontTexture.SetPixels(x, y, charData.Width, charData.Height, charPixels);

            charCount++;
            x += maxCharWidth + spacingPixels;

            if (charCount % charsPerRow == 0)
            {
                x = 0;
                y -= maxCharHeight + spacingPixels;
            }
        }

        fontTexture.Apply();

        string path = EditorUtility.SaveFilePanel("Save Texture", "", "ExportedLegacyFontTexture", "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] bytes = fontTexture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Texture exported successfully!\nSize: {textureWidth}x{textureHeight}", "OK");
        }
    }

    private Texture2D ConvertToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

    private struct CharacterData
    {
        public char Character;
        public Rect UvRect;
        public int Width;
        public int Height;
    }
}