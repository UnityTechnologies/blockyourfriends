using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Create a scriptable object to store all the versions used in this app and display those versions in the about popup at runtime
    [CreateAssetMenu(fileName = "VersionsUsed", menuName = "ScriptableObjects/Create VersionsUsed", order = 4)]
    public class VersionsUsed : ScriptableObject
    {
        public string engine;
        public string app;
        public string analytics;
        public string authentication;
        public string cloudcode;
        public string cloudsave;
        public string inAppPurchasing;
        public string lobby;
        public string mediation;
        public string relay;
        public string vivox;
    }
}
