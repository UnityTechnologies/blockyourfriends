using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using BlockYourFriends.UI;

[InitializeOnLoad]
public class GetVersions
{
    private static ListRequest Request;
    private static AboutPopup aboutPopup;

    static GetVersions()
    {
        Init();
    }

    private static void Init()
    {
        try
        {
            aboutPopup = GameObject.Find("AboutPopup").GetComponent<AboutPopup>();
        }
        catch (Exception) {}

        if (aboutPopup == null)
        {
            //Debug.Log("About popup not found!");
            return;
        }

        Request = Client.List();
        EditorApplication.update += SetVersions;
    }

    private static void SetVersions()
    {
        aboutPopup.versionsUsed.engine = Application.unityVersion;
        aboutPopup.versionsUsed.app = Application.version;

        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
                foreach (var package in Request.Result)
                {
                    SetPackageVersion(package.name, package.version);
                }

            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= SetVersions;
        }
    }

    private static void SetPackageVersion(string packageName, string version)
    {
        switch (packageName)
        {
            case "com.unity.services.analytics":
                aboutPopup.versionsUsed.analytics = version;
                break;
            case "com.unity.services.authentication":
                aboutPopup.versionsUsed.authentication = version;
                break;
            case "com.unity.services.cloudcode":
                aboutPopup.versionsUsed.cloudcode = version;
                break;
            case "com.unity.services.cloudsave":
                aboutPopup.versionsUsed.cloudsave = version;
                break;
            case "com.unity.purchasing":
                aboutPopup.versionsUsed.inAppPurchasing = version;
                break;
            case "com.unity.services.lobby":
                aboutPopup.versionsUsed.lobby = version;
                break;
            case "com.unity.services.mediation":
                aboutPopup.versionsUsed.mediation = version;
                break;
            case "com.unity.services.relay":
                aboutPopup.versionsUsed.relay = version;
                break;
            case "com.unity.services.vivox":
                aboutPopup.versionsUsed.vivox = version;
                break;
        }
    }
}