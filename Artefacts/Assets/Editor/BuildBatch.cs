using UnityEditor;
using System.Diagnostics;

public class ScriptBatch
{
    [MenuItem("Custom tools/Build client and Play in Editor &p")]
    public static void BuildGame()
    {
        var menuScene = "Assets/Scenes/Menu.unity";
        var playgroundScene = "Assets/Scenes/Playground.unity";

        // Get filename.
        string path = @"C:\ThesisBuild\";
        string[] levels = new string[] { menuScene, playgroundScene };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "/Artefacts.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

        // Run the game (Process class from System.Diagnostics).
        Process proc = new Process();
        proc.StartInfo.FileName = path + "Artefacts.exe";
        proc.Start();

        EditorApplication.SaveScene(EditorApplication.currentScene);
        EditorApplication.OpenScene(menuScene);
        EditorApplication.isPlaying = true;
    }
}