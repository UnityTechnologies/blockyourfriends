using System;
using System.Collections.Generic;
using UnityEngine;
using LobbyRemote = Unity.Services.Lobbies.Models.Lobby;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Keep updated on changes to a joined lobby, at a speed compliant with Lobby's rate limiting.
    /// </summary>
    public class LobbyContentHeartbeat : IReceiveMessages
    {
        private LocalLobby localLobby;
        private LobbyUser localUser;
        private int awaitingQueryCount = 0;
        private bool shouldPushData = false;

        private const float ApprovalMaxTime = 20; // Used for determining if a user should timeout if they are unable to connect.
        private float lifetime = 0;

        public void BeginTracking(LocalLobby lobby, LobbyUser localUser)
        {
            localLobby = lobby;
            this.localUser = localUser;
            Locator.Get.UpdateSlow.Subscribe(OnUpdate, 6.5f);
            Locator.Get.Messenger.Subscribe(this);
            localLobby.onChanged += OnLocalLobbyChanged;
            shouldPushData = true; // Ensure the initial presence of a new player is pushed to the lobby; otherwise, when a non-host joins, the LocalLobby never receives their data until they push something new.
            lifetime = 0;
        }

        public void EndTracking()
        {
            shouldPushData = false;
            Locator.Get.UpdateSlow.Unsubscribe(OnUpdate);
            Locator.Get.Messenger.Unsubscribe(this);
            if (localLobby != null)
                localLobby.onChanged -= OnLocalLobbyChanged;
            localLobby = null;
            localUser = null;
            Debug.Log("End Tracking");
        }

        private void OnLocalLobbyChanged(LocalLobby changed)
        {
            if (string.IsNullOrEmpty(changed.LobbyID)) // When the player leaves, their LocalLobby is cleared out but maintained.
                EndTracking();
            shouldPushData = true;
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.ClientUserSeekingDisapproval)
            {
                bool shouldDisapprove = localLobby.State != LobbyState.Lobby; // By not refreshing, it's possible to have a lobby in the lobby list UI after its countdown starts and then try joining.
                if (shouldDisapprove)
                    (msg as Action<relay.Approval>)?.Invoke(relay.Approval.GameAlreadyStarted);
            }
        }

        /// <summary>
        /// If there have been any data changes since the last update, push them to Lobby. Regardless, pull for the most recent data.
        /// (Unless we're already awaiting a query, in which case continue waiting.)
        /// </summary>
        private void OnUpdate(float dt)
        {
            lifetime += dt;
            if (awaitingQueryCount > 0 || localLobby == null)
                return;
            if (localUser.IsHost)
                LobbyAsyncRequests.Instance.DoLobbyHeartbeat(dt);

            if (!localUser.IsApproved && lifetime > ApprovalMaxTime)
            {
                Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Connection attempt timed out!");
                Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
            }

            if (shouldPushData)
                PushDataToLobby();
            else
                OnRetrieve();


            void PushDataToLobby()
            {
                shouldPushData = false;

                if (localUser.IsHost)
                {
                    awaitingQueryCount++;
                    DoLobbyDataPush();
                }
                awaitingQueryCount++;
                DoPlayerDataPush();
            }

            void DoLobbyDataPush()
            {
                Debug.Log("Pushing Lobby Data");
                if (LobbyAsyncRequests.Instance.HasRelayTokenData() && localLobby.RelayCode == null)
                {
                    Debug.Log("Abort push, relay token exists in singleton");
                    return;
                }
                else
                {
                    LobbyAsyncRequests.Instance.UpdateLobbyDataAsync(RetrieveLobbyData(localLobby), () => { if (--awaitingQueryCount <= 0) OnRetrieve(); });
                }
            }

            void DoPlayerDataPush()
            {
                LobbyAsyncRequests.Instance.UpdatePlayerDataAsync(RetrieveUserData(localUser), () => { if (--awaitingQueryCount <= 0) OnRetrieve(); });
            }

            void OnRetrieve()
            {
                LobbyRemote lobbyRemote = LobbyAsyncRequests.Instance.CurrentLobby;
                if (lobbyRemote == null) return;
                if (lobbyRemote.Data == null & localLobby.LobbyCode != null)
                {
                    //if (localUser.IsHost)
                    //{
                        Debug.Log("Overwriting Lobby Data detected, aborting");
                        DoLobbyDataPush();
                        return;
                    //}
                }
                bool prevShouldPush = shouldPushData;
                var prevState = localLobby.State;
                lobby.ToLocalLobby.Convert(lobbyRemote, localLobby);
                shouldPushData = prevShouldPush;

                // If the host suddenly leaves, the Lobby service will automatically handle disconnects after about 30s, but we can try to do a disconnect sooner if we detect it.
                if (!localUser.IsHost)
                {
                    foreach (var lobbyUser in localLobby.LobbyUsers)
                    {
                        if (lobbyUser.Value.IsHost)
                            return;
                    }
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Host left the lobby! Disconnecting...");
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.EndGame, null);
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
                }
            }
        }

        private static Dictionary<string, string> RetrieveLobbyData(LocalLobby lobby)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("RelayCode", lobby.RelayCode);
            data.Add("RelayNGOCode", lobby.RelayNGOCode);
            data.Add("State", ((int)lobby.State).ToString()); // Using an int is smaller than using the enum state's name.
            data.Add("Color", ((int)lobby.Color).ToString());
            data.Add("State_LastEdit", lobby.Data.State_LastEdit.ToString());
            data.Add("Color_LastEdit", lobby.Data.Color_LastEdit.ToString());
            data.Add("RelayNGOCode_LastEdit", lobby.Data.RelayNGOCode_LastEdit.ToString());
            return data;
        }

        private static Dictionary<string, string> RetrieveUserData(LobbyUser user)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (user == null || string.IsNullOrEmpty(user.ID))
                return data;
            data.Add("DisplayName", user.DisplayName); // The lobby doesn't need to know any data beyond the name and state; Relay will handle the rest.
            data.Add("UserStatus", ((int)user.UserStatus).ToString());
            return data;
        }
    }
}
