
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using AppleAuth.Editor;

public class PostBuildStep
{
    const string trackingDescription = "Your data will be used to provide you a better and personalized ad experience";

    [PostProcessBuild(0)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
            SignInWithApplePostBuildProcess(pathToXcode);
        }
            
    }

    static void SignInWithApplePostBuildProcess(string pathToXcode)
    {
        var projectPath = PBXProject.GetPBXProjectPath(pathToXcode);
        var project = new PBXProject();
        project.ReadFromString(System.IO.File.ReadAllText(projectPath));
        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
        manager.AddSignInWithAppleWithCompatibility(project.GetUnityMainTargetGuid());
        manager.WriteToFile();
    }

    static void AddPListValues(string pathToXcode)
    {
        string plistPath = pathToXcode + "/Info.plist";
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));
        PlistElementDict plistRoot = plistObj.root;
        plistRoot.SetString("NSUserTrackingUsageDescription", trackingDescription);
        File.WriteAllText(plistPath, plistObj.WriteToString());
    }
}
#endif