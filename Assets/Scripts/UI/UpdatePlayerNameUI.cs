using TMPro;
using UnityEngine;

namespace BlockYourFriends.UI
{
    public class UpdatePlayerNameUI : MonoBehaviour
    {
        private TextMeshProUGUI playerNameText;
        private TMP_InputField playerNameInput;

        private void Awake()
        {
            playerNameText = GetComponent<TextMeshProUGUI>();
            playerNameInput = GetComponent<TMP_InputField>();

            UIManager.PlayerNameChanged += UpdatePlayerNameText;
        }

        private void UpdatePlayerNameText(string newName)
        {
            if (playerNameText != null)
                playerNameText.text = newName;
            if (playerNameInput != null)
                playerNameInput.text = newName;
        }
    }
}