using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Displays the player's name.
    /// </summary>
    public class UserNameUI : ObserverPanel<LobbyUser>
    {
        [SerializeField] TMP_Text textField;
        [SerializeField] TMP_InputField inputField;

        private bool isSavedValueChecked = false;

        public override void ObservedUpdated(LobbyUser observed)
        {
            string playerID = observed.ID;
            string nameValue = string.Empty;

            if (!isSavedValueChecked)
            {
                if (PlayerPrefs.HasKey(playerID))
                    nameValue = PlayerPrefs.GetString(playerID);

                isSavedValueChecked = true;
            }

            if (string.IsNullOrEmpty(nameValue))
                nameValue = observed.DisplayName;

            if (textField != null)
            textField.SetText(nameValue);

            if (inputField != null)
                inputField.text = nameValue;
        }
    }
}
