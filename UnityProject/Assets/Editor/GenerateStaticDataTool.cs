using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateStaticDataTool : EditorWindow
{
    public StaticDataAsset StaticDataAsset;

    [MenuItem("Window/GenerateStaticDataTool")]
    static void Init()
    {
        var window = (GenerateStaticDataTool)GetWindow(typeof(GenerateStaticDataTool));
        window.Show();
    }
    

    void OnGUI()
    {
        if (GUILayout.Button("Generate"))
        {

        }
    }

    [MenuItem("Window/GenerateStaticData")]
    static void Generate()
    {

    }
}