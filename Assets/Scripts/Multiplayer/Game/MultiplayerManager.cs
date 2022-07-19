using BlockYourFriends.Gameplay;
using BlockYourFriends.Multiplayer.relay;
using BlockYourFriends.Multiplayer.vivox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Sets up and runs the entire sample.
    /// </summary>
    public class MultiplayerManager : MonoBehaviour, IReceiveMessages
    {
        #region UI elements that observe the local state. These should be assigned the observers in the scene during Start.
        [SerializeField] private List<LocalGameStateObserver> gameStateObservers = new List<LocalGameStateObserver>();
        [SerializeField] private List<LocalLobbyObserver> localLobbyObservers = new List<LocalLobbyObserver>();
        [SerializeField] private List<LobbyUserObserver> localUserObservers = new List<LobbyUserObserver>();
        [SerializeField] private List<LobbyServiceDataObserver> lobbyServiceObservers = new List<LobbyServiceDataObserver>();
        [SerializeField] private List<VivoxUserHandler> vivoxUserHandlers;
        #endregion // UI elements that observe the local state. These should be assigned the observers in the scene during Start.

        private LocalGameState localGameState = new LocalGameState();
        private LobbyUser localUser;
        private LocalLobby localLobby;

        private LobbyServiceData lobbyServiceData = new LobbyServiceData();
        private LobbyContentHeartbeat lobbyContentHeartbeat = new LobbyContentHeartbeat();
        private RelayUtpSetup relaySetup;
        private RelayUtpClient relayClient;

        private VivoxSetup vivoxSetup = new VivoxSetup();

        /// <summary>Rather than a setter, this is usable in-editor. It won't accept an enum, however.</summary>
        public void SetLobbyColorFilter(int color)
        {
            lobbyColorFilter = (LobbyColor)color;
        }

        private LobbyColor lobbyColorFilter;

        #region Setup
        private void Awake()
        {
            // Do some arbitrary operations to instantiate singletons.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var unused = Locator.Get;
#pragma warning restore IDE0059

            Locator.Get.Provide(new Auth.Identity(OnAuthSignIn));
            Application.wantsToQuit += OnWantToQuit;
        }

        private void Start()
        {
            localLobby = new LocalLobby { State = LobbyState.Lobby };
            localUser = new LobbyUser();
            localUser.DisplayName = "New Player";
            Locator.Get.Messenger.Subscribe(this);
            BeginObservers();
        }

        private void OnAuthSignIn()
        {
            localUser.ID = Locator.Get.Identity.GetSubIdentity(Auth.IIdentityType.Auth).GetContent("id");
            localUser.DisplayName = NameGenerator.GetName(localUser.ID);
            localLobby.AddPlayer(localUser); // The local LobbyUser object will be hooked into UI before the LocalLobby is populated during lobby join, so the LocalLobby must know about it already when that happens.
            GameManager.Instance.singlePlayerName = localUser.DisplayName;
            StartVivoxLogin();
        }

        private void BeginObservers()
        {
            foreach (var gameStateObs in gameStateObservers)
                gameStateObs.BeginObserving(localGameState);
            foreach (var serviceObs in lobbyServiceObservers)
                serviceObs.BeginObserving(lobbyServiceData);
            foreach (var lobbyObs in localLobbyObservers)
                lobbyObs.BeginObserving(localLobby);
            foreach (var userObs in localUserObservers)
                userObs.BeginObserving(localUser);
        }
        #endregion //Setup

        /// <summary>
        /// Primarily used for UI elements to communicate state changes, this will receive messages from arbitrary providers for user interactions.
        /// </summary>
        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.CreateLobbyRequest)
            {
                LocalLobby.LobbyData createLobbyData = (LocalLobby.LobbyData)msg;
                LobbyAsyncRequests.Instance.CreateLobbyAsync(createLobbyData.LobbyName, createLobbyData.MaxPlayerCount, createLobbyData.Private, localUser, (r) =>
                    {   lobby.ToLocalLobby.Convert(r, localLobby);
                        OnCreatedLobby();
                    },
                    OnFailedJoin);
            }
            else if (type == MessageType.JoinLobbyRequest)
            {
                LocalLobby.LobbyData lobbyInfo = (LocalLobby.LobbyData)msg;
                LobbyAsyncRequests.Instance.JoinLobbyAsync(lobbyInfo.LobbyID, lobbyInfo.LobbyCode, localUser, (r) =>
                    {
                        lobby.ToLocalLobby.Convert(r, localLobby);
                        if (localLobby.LobbyUsers.Count == 2)
                            localUser.PaddleLocation = PaddleLocation.top;
                        else if (localLobby.LobbyUsers.Count == 3)
                            localUser.PaddleLocation = PaddleLocation.right;
                        else if (localLobby.LobbyUsers.Count == 4)
                            localUser.PaddleLocation = PaddleLocation.left;
                        OnJoinedLobby();
                    },
                    OnFailedJoin);
            }
            else if (type == MessageType.QueryLobbies)
            {
                lobbyServiceData.State = LobbyQueryState.Fetching;
                LobbyAsyncRequests.Instance.RetrieveLobbyListAsync(
                    qr => {
                        if (qr != null)
                            OnLobbiesQueried(lobby.ToLocalLobby.Convert(qr));
                    },
                    er => {
                        OnLobbyQueryFailed();
                    },
                    lobbyColorFilter);
            }
            else if (type == MessageType.QuickJoin)
            {
                LobbyAsyncRequests.Instance.QuickJoinLobbyAsync(localUser, lobbyColorFilter, (r) =>
                    {   lobby.ToLocalLobby.Convert(r, localLobby);
                        OnJoinedLobby();
                    },
                    OnFailedJoin);
            }
			else if (type == MessageType.RenameRequest)
            {
                string name = (string)msg;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Empty Name not allowed."); // Lobby error type, then HTTP error type.
                    return;
            	}
                localUser.DisplayName = (string)msg;
            }       
            else if (type == MessageType.ClientUserApproved)
            {   ConfirmApproval();
            }
            else if (type == MessageType.UserSetEmote)
            {   EmoteType emote = (EmoteType)msg;
                localUser.Emote = emote;
            }
            else if (type == MessageType.LobbyUserStatus)
            {   localUser.UserStatus = (UserStatus)msg;
            }
            else if (type == MessageType.StartCountdown)
            {   localLobby.State = LobbyState.CountDown;
            }
            else if (type == MessageType.CancelCountdown)
            {   localLobby.State = LobbyState.Lobby;
            }
            else if (type == MessageType.CompleteCountdown)
            {   if (relayClient is RelayUtpHost)
                    (relayClient as RelayUtpHost).SendInGameState();
            }
            else if (type == MessageType.ChangeGameState)
            {   SetGameState((GameState)msg);
            }
            else if (type == MessageType.ConfirmInGameState)
            {   localUser.UserStatus = UserStatus.InGame;
                localLobby.State = LobbyState.InGame;
            }
            else if (type == MessageType.EndGame)
            {   localLobby.State = LobbyState.Lobby;
                SetUserLobbyState();
            }
		}

        private void SetGameState(GameState state)
        {
            bool isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) && localGameState.State == GameState.Lobby;
            localGameState.State = state;
            if (isLeavingLobby)
                OnLeftLobby();
        }

        private void OnLobbiesQueried(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            foreach (var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID, lobby);

            lobbyServiceData.State = LobbyQueryState.Fetched;
            lobbyServiceData.CurrentLobbies = newLobbyDict;
        }

        private void OnLobbyQueryFailed()
        {
            lobbyServiceData.State = LobbyQueryState.Error;
        }

        private void OnCreatedLobby()
        {
            localUser.IsHost = true;
            OnJoinedLobby();
        }

        private void OnJoinedLobby()
        {
            LobbyAsyncRequests.Instance.BeginTracking(localLobby.LobbyID);
            lobbyContentHeartbeat.BeginTracking(localLobby, localUser);
            SetUserLobbyState();

            // The host has the opportunity to reject incoming players, but to do so the player needs to connect to Relay without having game logic available.
            // In particular, we should prevent players from joining voice chat until they are approved.
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Connecting);
            if (localUser.IsHost)
            {
                Debug.Log("Joined, Is Host");
                StartRelayConnection();
                StartVivoxJoin();
            }
            else
            {
                Debug.Log("Joined Not Host");
                StartRelayConnection();
            }
        }

        private void OnLeftLobby()
        {
            localUser.ResetState();
            LobbyAsyncRequests.Instance.LeaveLobbyAsync(localLobby?.LobbyID, ResetLocalLobby);
            lobbyContentHeartbeat.EndTracking();
            LobbyAsyncRequests.Instance.EndTracking();
            vivoxSetup.LeaveLobbyChannel();

            if (relaySetup != null)
            {   Component.Destroy(relaySetup);
                relaySetup = null;
            }

            if (relayClient != null)
            {
                relayClient.Dispose();
                StartCoroutine(FinishCleanup());

                // We need to delay slightly to give the disconnect message sent during Dispose time to reach the host, so that we don't destroy the connection without it being flushed first.
                IEnumerator FinishCleanup()
                {
                    yield return null;
                    Component.Destroy(relayClient);
                    relayClient = null;
                }
            }

            //Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
        }

        /// <summary>
        /// Back to Join menu if we fail to join for whatever reason.
        /// </summary>
        private void OnFailedJoin()
        {
            SetGameState(GameState.JoinMenu);
        }

        private void StartVivoxLogin()
        {
            vivoxSetup.Initialize(vivoxUserHandlers, OnVivoxLoginComplete);

            void OnVivoxLoginComplete(bool didSucceed)
            {
                if (!didSucceed)
                {   Debug.LogError("Vivox login failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartVivoxLogin, localLobby.LobbyID));
                    return;
                }
            }
        }

        private void StartVivoxJoin() //removing pending fix to Vivox/Auth2.0 issue
        {
            vivoxSetup.JoinLobbyChannel(localLobby.LobbyID, OnVivoxJoinComplete);

            void OnVivoxJoinComplete(bool didSucceed)
            {
                if (!didSucceed)
                {
                    Debug.LogError("Vivox connection failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartVivoxJoin, localLobby.LobbyID));
                    return;
                }
            }
        }

        private void StartRelayConnection()
        {
            if (localUser.IsHost)
            {
                try
                {
                    relaySetup = gameObject.AddComponent<RelayUtpSetupHost>();
                }
                catch
                {
                    Debug.Log("Failed to add RelayUtpSetupHost Component");
                }
            }
            else
            {
                try
                {
                    relaySetup = gameObject.AddComponent<RelayUtpSetupClient>();
                }
                catch
                {
                    Debug.Log("Failed to add RelayUtpSetupHost Component");
                }
            }

            Debug.Log("Starting Relay Connection");
            relaySetup.BeginRelayJoin(localLobby, localUser, OnRelayConnected);

            void OnRelayConnected(bool didSucceed, RelayUtpClient client)
            {
                Debug.Log("On Relay Connected");
                Component.Destroy(relaySetup);
                relaySetup = null;

                if (!didSucceed)
                {   Debug.LogError("Relay connection failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartRelayConnection, localLobby.LobbyID));
                    return;
                }

                relayClient = client;
                if (localUser.IsHost)
                {
                    CompleteRelayConnection();
                    LobbyAsyncRequests.Instance.UpdateLobbyExt();
                }
                else
                    Debug.Log("Client is now waiting for approval...");
            }
        }

        private IEnumerator RetryConnection(Action doConnection, string lobbyId)
        {
            yield return new WaitForSeconds(5);
            if (localLobby != null && localLobby.LobbyID == lobbyId && !string.IsNullOrEmpty(lobbyId)) // Ensure we didn't leave the lobby during this waiting period.
                doConnection?.Invoke();
        }

        private void ConfirmApproval()
        {
            if (!localUser.IsHost && localUser.IsApproved)
            {
                Debug.Log("Approval confirmed");
                CompleteRelayConnection();
                StartVivoxJoin();
            }
        }

        private void CompleteRelayConnection()
        {
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Lobby);
        }

        private void SetUserLobbyState()
        {
            SetGameState(GameState.Lobby);
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Lobby);
        }

        private void ResetLocalLobby()
        {
            localLobby.CopyObserved(new LocalLobby.LobbyData(), new Dictionary<string, LobbyUser>());
            localLobby.AddPlayer(localUser); // As before, the local player will need to be plugged into UI before the lobby join actually happens.
            localLobby.RelayServer = null;
        }

        #region Teardown
        /// <summary>
        /// In builds, if we are in a lobby and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
        /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
        /// </summary>
        private IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        private bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(localLobby?.LobbyID);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        private void OnDestroy()
        {
            ForceLeaveAttempt();
            
        }

        private void ForceLeaveAttempt()
        {
            Locator.Get.Messenger.Unsubscribe(this);
            if (!string.IsNullOrEmpty(localLobby?.LobbyID))
            {
                LobbyAsyncRequests.Instance.LeaveLobbyAsync(localLobby?.LobbyID, null);
                localLobby = null;
            }
        }
        #endregion //Teardown
    }
}
