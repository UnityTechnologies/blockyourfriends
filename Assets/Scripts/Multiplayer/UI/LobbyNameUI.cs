using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Displays the name of the lobby.
    /// </summary>
    public class LobbyNameUI : ObserverPanel<LocalLobby>
    {
        [SerializeField] private TMP_Text lobbyNameText;

        public override void ObservedUpdated(LocalLobby observed)
        {
            lobbyNameText.SetText(observed.LobbyName);
        }
    }
}
