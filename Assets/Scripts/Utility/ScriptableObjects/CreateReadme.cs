using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Utility.SO
{
    //Create a Readme object for refreence as needed (Just a fancy way to make a bit of text!)
    [CreateAssetMenu(fileName = "Readme", menuName = "ScriptableObjects/Create Readme", order = 1)]
    public class CreateReadme : ScriptableObject
    {
        [TextArea]
        public string information = "";
    }
}
