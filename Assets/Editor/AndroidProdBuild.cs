using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidProdBuild : MonoBehaviour
{
    [MenuItem("Build/Build Android Prod")]
    public static void BuildAndroidDev()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "~/Builds/BYF/AndroidProd.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "Prod");

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
