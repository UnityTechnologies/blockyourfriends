using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Handles the menu for a player creating a new lobby.
    /// </summary>
    public class CreateMenuUI : UIPanelBase
    {
        public JoinCreateLobbyUI joinCreateLobbyUI;
        private LocalLobby.LobbyData serverRequestData = new LocalLobby.LobbyData { LobbyName = "New Lobby", MaxPlayerCount = 4 };

        public override void Start()
        {
            base.Start();
            joinCreateLobbyUI.onTabChanged.AddListener(OnTabChanged);
        }

        void OnTabChanged(JoinCreateTabs tabState)
        {
            if (tabState == JoinCreateTabs.Create)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void SetServerName(string serverName)
        {
            serverRequestData.LobbyName = serverName;
        }

        public void SetServerPrivate(bool priv)
        {
            serverRequestData.Private = priv;
        }

        public void OnCreatePressed()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.CreateLobbyRequest, serverRequestData);
        }
    }
}
