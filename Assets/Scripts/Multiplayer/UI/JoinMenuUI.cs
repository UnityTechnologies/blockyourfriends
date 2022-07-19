using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Handles the list of LobbyButtons and ensures it stays synchronized with the lobby list from the service.
    /// </summary>
    public class JoinMenuUI : ObserverPanel<LobbyServiceData>
    {
        [SerializeField] private LobbyButtonUI lobbyButtonPrefab;
        [SerializeField] private RectTransform lobbyButtonParent;
        [SerializeField] private TMP_InputField joinCodeField;
        [SerializeField] private JoinCreateLobbyUI joinCreateLobbyUI;
        /// <summary>
        /// Key: Lobby ID, Value Lobby UI
        /// </summary>
        private Dictionary<string, LobbyButtonUI> lobbyButtons = new Dictionary<string, LobbyButtonUI>();
        private Dictionary<string, LocalLobby> localLobby = new Dictionary<string, LocalLobby>();

        /// <summary>Contains some amount of information used to join an existing lobby.</summary>
        private LocalLobby.LobbyData localLobbySelected;

        public override void Start()
        {
            base.Start();
            joinCreateLobbyUI.onTabChanged.AddListener(OnTabChanged);
        }

        void OnTabChanged(JoinCreateTabs tabState)
        {
            if (tabState == JoinCreateTabs.Join)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void LobbyButtonSelected(LocalLobby lobby)
        {
            localLobbySelected = lobby.Data;
            OnJoinButtonPressed();
        }

        public void OnLobbyCodeInputFieldChanged(string newCode)
        {
            if (!string.IsNullOrEmpty(newCode))
                localLobbySelected = new LocalLobby.LobbyData(newCode.ToUpper());
        }

        public void OnJoinButtonPressed()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.JoinLobbyRequest, localLobbySelected);
            localLobbySelected = default;
        }

        public void OnRefresh()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.QueryLobbies, string.Empty);
        }

        public override void ObservedUpdated(LobbyServiceData observed)
        {
            ///Check for new entries, We take CurrentLobbies as the source of truth
            List<string> previousKeys = new List<string>(lobbyButtons.Keys);
            foreach (var codeLobby in observed.CurrentLobbies)
            {
                var lobbyCodeKey = codeLobby.Key;
                var lobbyData = codeLobby.Value;
                if (!lobbyButtons.ContainsKey(lobbyCodeKey))
                {
                    if (CanDisplay(lobbyData))
                        AddNewLobbyButton(lobbyCodeKey, lobbyData);
                }
                else
                {
                    if (CanDisplay(lobbyData))
                        UpdateLobbyButton(lobbyCodeKey, lobbyData);
                    else
                        RemoveLobbyButton(lobbyData);
                }

                previousKeys.Remove(lobbyCodeKey);
            }

            foreach (string key in previousKeys) // Need to remove any lobbies from the list that no longer exist.
                RemoveLobbyButton(localLobby[key]);
        }

        public void JoinMenuChangedVisibility(bool show)
        {
            if (show)
            {
                joinCodeField.text = "";
                OnRefresh();
            }
        }

        public void OnQuickJoin()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.QuickJoin, null);
        }

        private bool CanDisplay(LocalLobby lobby)
        {
            return lobby.Data.State == LobbyState.Lobby && !lobby.Private;
        }

        /// <summary>
        /// Instantiates UI element and initializes the observer with the LobbyData
        /// </summary>
        private void AddNewLobbyButton(string lobbyCode, LocalLobby lobby)
        {
            var lobbyButtonInstance = Instantiate(lobbyButtonPrefab, lobbyButtonParent);
            lobbyButtonInstance.GetComponent<LocalLobbyObserver>().BeginObserving(lobby);
            lobby.onDestroyed += RemoveLobbyButton; // Set up to clean itself
            lobbyButtonInstance.onLobbyPressed.AddListener(LobbyButtonSelected);
            lobbyButtons.Add(lobbyCode, lobbyButtonInstance);
            localLobby.Add(lobbyCode, lobby);
        }

        private void UpdateLobbyButton(string lobbyCode, LocalLobby lobby)
        {
            lobbyButtons[lobbyCode].UpdateLobby(lobby);
        }

        private void RemoveLobbyButton(LocalLobby lobby)
        {
            var lobbyID = lobby.LobbyID;
            var lobbyButton = lobbyButtons[lobbyID];
            lobbyButton.GetComponent<LocalLobbyObserver>().EndObserving();
            lobbyButtons.Remove(lobbyID);
            localLobby.Remove(lobbyID);
            Destroy(lobbyButton.gameObject);
        }
    }
}
