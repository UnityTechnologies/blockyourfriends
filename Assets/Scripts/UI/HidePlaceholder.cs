using UnityEngine;
using TMPro;

namespace BlockYourFriends.UI
{
    public class HidePlaceholder : MonoBehaviour
    {
        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        public void OnSelect()
        {
            if (inputField != null)
                inputField.placeholder.gameObject.SetActive(false);
        }

        public void OnDeselect()
        {
            if (inputField != null)
                inputField.placeholder.gameObject.SetActive(true);
        }
    }
}