using TMPro;
using UnityEngine;

namespace BlockYourFriends.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class SetVersion : MonoBehaviour
    {
        [SerializeField] private string versionName;

        public void SetText(string version)
        {
            GetComponent<TMP_Text>().text = $"{versionName}: {version}";
        }
    }
}