using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class JsonToScriptableObject : EditorWindow
{
    public UnityEngine.Object jsonFile;
    public ScriptableObject scriptableObjectTemplate;

    [MenuItem("Tools/JSON to ScriptableObject")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableObject>("JSON to ScriptableObject");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to ScriptableObject Converter", EditorStyles.boldLabel);

        jsonFile = EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        scriptableObjectTemplate = (ScriptableObject)EditorGUILayout.ObjectField("ScriptableObject Template", scriptableObjectTemplate, typeof(ScriptableObject), false);

        if (GUILayout.Button("Convert"))
        {
            ConvertJSONToScriptableObject();
        }
    }

    private void ConvertJSONToScriptableObject()
    {
        if (jsonFile == null || scriptableObjectTemplate == null)
        {
            Debug.LogError("Please assign all fields.");
            return;
        }

        string jsonFilePath = AssetDatabase.GetAssetPath(jsonFile);
        string jsonFileFullPath = Path.Combine(Application.dataPath, jsonFilePath.Replace("Assets/", ""));
        Type soType = scriptableObjectTemplate.GetType();

        try
        {
            string jsonData = File.ReadAllText(jsonFileFullPath);
            var listType = typeof(List<>).MakeGenericType(typeof(Dictionary<string, object>));
            var objects = JsonConvert.DeserializeObject(jsonData, listType) as IList<Dictionary<string, object>>;

            if (objects == null)
            {
                Debug.LogError("Failed to deserialize JSON data.");
                return;
            }

            foreach (var field in soType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elementType = field.FieldType.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(field.FieldType);

                    foreach (var objData in objects)
                    {
                        var element = Activator.CreateInstance(elementType);

                        foreach (var objField in elementType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            string jsonKey = objField.Name.TrimStart('_');
                            if (objData.ContainsKey(jsonKey))
                            {
                                var value = ConvertValue(objData[jsonKey], objField.FieldType);
                                objField.SetValue(element, value);
                            }
                        }

                        list.Add(element);
                    }

                    field.SetValue(scriptableObjectTemplate, list);
                }
            }

            EditorUtility.SetDirty(scriptableObjectTemplate);
            AssetDatabase.SaveAssets();
            Debug.Log("JSON to ScriptableObject conversion completed!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during JSON to ScriptableObject conversion: " + ex.Message);
        }
    }

    private object ConvertValue(object value, Type targetType)
    {
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value.ToString(), true);
        }
        else if (targetType == typeof(int))
        {
            return Convert.ToInt32(value);
        }
        else if (targetType == typeof(float))
        {
            return Convert.ToSingle(value);
        }
        else if (targetType == typeof(bool))
        {
            return Convert.ToBoolean(value);
        }
        else if (targetType == typeof(string))
        {
            return value.ToString();
        }
        else
        {
            return Convert.ChangeType(value, targetType);
        }
    }
}