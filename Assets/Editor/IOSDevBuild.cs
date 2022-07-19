
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class IOSDevBuild : MonoBehaviour
{
    [MenuItem("Build/Build IOS Dev")]
    public static void BuildAndroidDev()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "~/Builds/BYF/IOSDevBuild";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "Dev");

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}
