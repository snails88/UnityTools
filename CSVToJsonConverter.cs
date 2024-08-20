using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

public class CSVToJsonConverter : EditorWindow
{
    public string csvFilePath = "Assets/Resources/Data/.csv";
    public string jsonFilePath = "Assets/Resources/Data/.json";

    [MenuItem("Tools/Convert CSV to JSON")]
    public static void ShowWindow()
    {
        GetWindow<CSVToJsonConverter>("CSV to JSON Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV to JSON Converter", EditorStyles.boldLabel);

        csvFilePath = EditorGUILayout.TextField("CSV File Path", csvFilePath);
        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);

        if (GUILayout.Button("Convert"))
        {
            ConvertCSVToJson();
        }
    }

    private void ConvertCSVToJson()
    {
        try
        {
            List<Dictionary<string, object>> csvData = CSVReader.ReadCSVFile(csvFilePath);
            string jsonData = JsonConvert.SerializeObject(csvData, Formatting.Indented);
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log("CSV to JSON conversion completed!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error during CSV to JSON conversion: " + ex.Message);
        }
    }
}

public class CSVReader
{
    public static List<Dictionary<string, object>> ReadCSVFile(string filePath)
    {
        var csvData = new List<Dictionary<string, object>>();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                string[] headers = reader.ReadLine().Split(',');

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var entry = new Dictionary<string, object>();

                    for (int i = 0; i < headers.Length; i++)
                    {
                        entry[headers[i]] = ParseValue(values[i]);
                    }

                    csvData.Add(entry);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error reading CSV file: " + ex.Message);
            throw;
        }

        return csvData;
    }

    private static object ParseValue(string value)
    {
        // Try parsing as bool
        if (bool.TryParse(value, out bool boolResult))
        {
            return boolResult;
        }

        // Try parsing as integer
        if (int.TryParse(value, out int intResult))
        {
            return intResult;
        }

        // Try parsing as float
        if (float.TryParse(value, out float floatResult))
        {
            return floatResult;
        }

        // If all else fails, return as string
        return value;
    }
}
