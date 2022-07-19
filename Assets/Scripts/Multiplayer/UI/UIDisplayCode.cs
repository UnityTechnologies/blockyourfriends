using System;
using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Watches a lobby or relay code for updates, displaying the current code to lobby members.
    /// </summary>
    public class UIDisplayCode : ObserverPanel<LocalLobby>
    {
        public enum CodeType { Lobby = 0, Relay = 1 }

        [SerializeField] private TMP_Text outputText;
        [SerializeField] private CodeType codeType;
        [SerializeField] private string fieldName;

        public override void ObservedUpdated(LocalLobby observed)
        {
            if (codeType==CodeType.Relay)
            {
                //Debug.Log("Relay code set to:" + observed.RelayCode);
            }
            string code = codeType == CodeType.Lobby ? observed.LobbyCode : observed.RelayCode;
            string displayText = string.Format("{0}: {1}", fieldName, code);

            if (!string.IsNullOrEmpty(code))
            {
                outputText.text = displayText;
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}
