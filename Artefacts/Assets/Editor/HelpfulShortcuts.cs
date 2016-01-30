using UnityEngine;
using System.Collections;
using UnityEditor;

public class HelpfulShortcuts
{
    private static string menuScene = "Assets/Scenes/Menu.unity";
    private static string playgroundScene = "Assets/Scenes/Playground.unity";


    [MenuItem("Custom tools/Shortcuts/Load menu scene #1")]
    public static void LoadMenu()
    {
        EditorApplication.SaveScene(EditorApplication.currentScene);
        EditorApplication.OpenScene(menuScene);
    }

    [MenuItem("Custom tools/Shortcuts/Load playground scene #2")]
    public static void LoadPlayground()
    {
        EditorApplication.SaveScene(EditorApplication.currentScene);
        EditorApplication.OpenScene(playgroundScene);
    }
    [MenuItem("Custom tools/Delete PlayerPrefs %#x" )]
    public static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}