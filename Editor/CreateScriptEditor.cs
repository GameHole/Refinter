using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
using System.Text;
using UnityEditor.Callbacks;
[InitializeOnLoad]
public class EntitiesEditor 
{
    [MenuItem("Assets/Create/Interface",false,1)]
    public static void CreateCmpScript()
    {
        var path = AssetDatabase.GUIDToAssetPath("2ab4c1d5848abc845a9a47d7276209a8");
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
        ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
        GetSelectedPathOrFallback() + "/NewInterface.cs", null, path);
    }
    [MenuItem("Assets/Create/C# Class", false, 1)]
    public static void CreateIAndIScript()
    {
        var path = AssetDatabase.GUIDToAssetPath("71c585e740c19b74eac3d3c65feecb45");
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
        ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
        GetSelectedPathOrFallback() + "/NewClass.cs", null, path);
    }
    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered< Object >(SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
    static EntitiesEditor()
    {
        string[] guids = AssetDatabase.FindAssets("namespace");
        if (guids.Length > 0)
        {
            nameSpace = File.ReadAllText(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        //SetOrder("095a6725417571149bf564fb1e5f61f1", -200);
    }
    static void SetOrder(string guid, int order)
    {
        var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
        if (MonoImporter.GetExecutionOrder(mono) != order)
            MonoImporter.SetExecutionOrder(mono, order);
    }
    public readonly static string nameSpace = "Default";
}


class MyDoCreateScriptAsset : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
    {
       
        string fullPath = Path.GetFullPath(pathName);
        //StreamReader streamReader = new StreamReader(resourceFile);
        string text = File.ReadAllText(resourceFile);// streamReader.ReadToEnd();
        //streamReader.Close();
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        text = Regex.Replace(text, "#NAME#", fileNameWithoutExtension);
        text = Regex.Replace(text, "#NAMESPACE#", EntitiesEditor.nameSpace);
        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}