#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Invoked from the command line:
//   Unity -batchmode -quit -projectPath ... -executeMethod BuildScript.Linux
// Produces Build/Linux/MazeRunner.x86_64 and its _Data folder.
public static class BuildScript
{
    public static void Linux()
    {
        BuildStandalone(BuildTarget.StandaloneLinux64,
            "Build/Linux/MazeRunner.x86_64");
    }

    public static void Windows()
    {
        BuildStandalone(BuildTarget.StandaloneWindows64,
            "Build/Windows/MazeRunner.exe");
    }

    public static void Mac()
    {
        BuildStandalone(BuildTarget.StandaloneOSX,
            "Build/Mac/MazeRunner.app");
    }

    public static void WebGL()
    {
        // Configure WebGL-friendly settings before we build.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.memorySize = 512;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.runInBackground = true;

        string outputPath = "Build/WebGL";
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        var scenes = GetScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No scenes in Build Settings.");
            EditorApplication.Exit(2);
            return;
        }

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        Debug.Log($"[BuildScript] Building WebGL to {outputPath} with {scenes.Length} scenes.");
        var report = BuildPipeline.BuildPlayer(opts);

        Debug.Log($"[BuildScript] Result: {report.summary.result}, " +
                  $"size {report.summary.totalSize} bytes, " +
                  $"time {report.summary.totalTime}");

        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    static void BuildStandalone(BuildTarget target, string outputPath)
    {
        // Make sure the folder exists.
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var scenes = GetScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No scenes in Build Settings.");
            EditorApplication.Exit(2);
            return;
        }

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.None,
        };

        Debug.Log($"[BuildScript] Building {target} to {outputPath} with {scenes.Length} scenes.");
        var report = BuildPipeline.BuildPlayer(opts);

        Debug.Log($"[BuildScript] Result: {report.summary.result}, " +
                  $"size {report.summary.totalSize} bytes, " +
                  $"time {report.summary.totalTime}");

        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    static string[] GetScenes()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled && !string.IsNullOrEmpty(s.path)) list.Add(s.path);
        return list.ToArray();
    }
}
#endif
