using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlockYourFriends.Utility
{
    public static class PreExport
    {
        public static void IOSProd()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "Prod");
        }

        public static void IOSDev()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "Dev");
        }

        public static void AndroidProd()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "Prod");
        }

        public static void AndroidDev()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "Dev");
        }

    }
}
