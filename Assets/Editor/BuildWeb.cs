using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.WebGL;
using UnityEngine;

public static class BuildWeb
{
    const string ReleaseFolder = "Builds/Web";
    const string DevelopmentFolder = "Builds/WebDev";
    const string MetaFolder = "Builds/Meta";
    const string SiteFolder = "Builds/Site";

    [MenuItem("SWARMSHIFT/Build Web (release)")]
    public static void BuildRelease()
    {
        Build(OutputPath(ReleaseFolder), BuildOptions.None);
    }

    [MenuItem("SWARMSHIFT/Build Web (development)")]
    public static void BuildDevelopment()
    {
        Build(OutputPath(DevelopmentFolder), BuildOptions.Development);
    }

    [MenuItem("SWARMSHIFT/Build Web (Facebook Instant Games)")]
    public static void BuildMeta()
    {
        BuildGzipVariant("PROJECT:FBInstantTemplate", MetaFolder, "SWARMSHIFT_META");
    }

    [MenuItem("SWARMSHIFT/Build Web (swarmshift.pasindug.com)")]
    public static void BuildSite()
    {
        BuildGzipVariant("PROJECT:SiteTemplate", SiteFolder, "SWARMSHIFT_WEB");
    }

    static void BuildGzipVariant(string variantTemplate, string folder, string define)
    {
        string template = PlayerSettings.WebGL.template;
        WebGLCompressionFormat compression = PlayerSettings.WebGL.compressionFormat;
        bool fallback = PlayerSettings.WebGL.decompressionFallback;
        try
        {
            PlayerSettings.WebGL.template = variantTemplate;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true; // these hosts are not known to send Content-Encoding for Unity's files
            Build(OutputPath(folder), BuildOptions.None, new[] { define });
        }
        finally
        {
            PlayerSettings.WebGL.template = template;
            PlayerSettings.WebGL.compressionFormat = compression;
            PlayerSettings.WebGL.decompressionFallback = fallback;
        }
    }

    static void Build(string outputPath, BuildOptions options, string[] defines = null)
    {
        UserBuildSettings.codeOptimization = WasmCodeOptimization.DiskSizeLTO;
        var buildOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = options,
            extraScriptingDefines = defines
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Web build {summary.result} with {summary.totalErrors} errors");
        }

        Debug.Log($"Web build written to {outputPath}: {summary.totalSize} bytes in {summary.totalTime}");
    }

    // unity build --execute-method hands its --output-path to Unity as -buildOutput
    static string OutputPath(string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-buildOutput");
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
    }
}
