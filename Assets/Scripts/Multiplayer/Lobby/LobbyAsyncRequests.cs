using System;
using System.Collections.Generic;
using BlockYourFriends.Multiplayer.lobby;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// An abstraction layer between the direct calls into the Lobby API and the outcomes you actually want. E.g. you can request to get a readable list of 
    /// current lobbies and not need to make the query call directly.
    /// </summary>
    public class LobbyAsyncRequests
    {
        // Just doing a singleton since static access is all that's really necessary but we also need to be able to subscribe to the slow update loop.
        private static LobbyAsyncRequests instance;

        public static LobbyAsyncRequests Instance
        {
            get
            {
                if (instance == null)
                    instance = new LobbyAsyncRequests();
                return instance;
            }
        }

        public LobbyAsyncRequests()
        {
            Locator.Get.UpdateSlow.Subscribe(UpdateLobby, 2f); // Shouldn't need to unsubscribe since this instance won't be replaced. 0.5s is arbitrary; the rate limits are tracked later.
        }

        #region Once connected to a lobby, cache the local lobby object so we don't query for it for every lobby operation.

        // (This assumes that the player will be actively in just one lobby at a time, though they could passively be in more.)
        private string currentLobbyId = null;
        private Lobby lastKnownLobby;
        public Lobby CurrentLobby => lastKnownLobby;

        public void BeginTracking(string lobbyId)
        {
            currentLobbyId = lobbyId;
        }

        public void EndTracking()
        {
            currentLobbyId = null;
            lastKnownLobby = null;
            heartbeatTime = 0;
        }

        private void UpdateLobby(float unused)
        {
            if (!string.IsNullOrEmpty(currentLobbyId))
                RetrieveLobbyAsync(currentLobbyId, OnComplete);

            void OnComplete(Lobby lobby)
            {
                if (lobby != null)
                {
                    lastKnownLobby = lobby;
                }
            }
        }

        #endregion

        #region Adding some code to force a lobby refresh when a relay token is added and check for the existance to prevent overwriting it

        public void UpdateLobbyExt()
        {
            UpdateLobby(2f); //Manually force an update (useful for when relay connection is completed so the relay code doesn't get overridden due to connection timing
            Debug.Log("Forcing Lobby Data Refresh");
        }

        public bool HasRelayTokenData()
        {
            if (lastKnownLobby == null || lastKnownLobby.Data == null)
            {
                return false;
            }
            return lastKnownLobby.Data.ContainsKey("RelayCode");
        }

        #endregion

        #region Lobby API calls are rate limited, and some other operations might want an alert when the rate limits have passed.

        // Note that some APIs limit to 1 call per N seconds, while others limit to M calls per N seconds. We'll treat all APIs as though they limited to 1 call per N seconds.
        // Also, this is seralized, so don't reorder the values unless you know what that will affect.
        public enum RequestType
        {
            Query = 0,
            Join,
            QuickJoin,
            Host
        }

        public RateLimitCooldown GetRateLimit(RequestType type)
        {
            if (type == RequestType.Join)
                return rateLimitJoin;
            else if (type == RequestType.QuickJoin)
                return rateLimitQuickJoin;
            else if (type == RequestType.Host)
                return rateLimitHost;
            return rateLimitQuery;
        }

        private RateLimitCooldown rateLimitQuery = new RateLimitCooldown(2f); // Used for both the lobby list UI and the in-lobby updating. In the latter case, updates can be cached.
        private RateLimitCooldown rateLimitJoin = new RateLimitCooldown(3f);
        private RateLimitCooldown rateLimitQuickJoin = new RateLimitCooldown(3f);
        private RateLimitCooldown rateLimitHost = new RateLimitCooldown(3f);

        #endregion

        private static Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LobbyUser player)
        {
            Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();
            PlayerDataObject dataObjName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, player.DisplayName);
            data.Add("DisplayName", dataObjName);
            return data;
        }

        /// <summary>
        /// Attempt to create a new lobby and then join it.
        /// </summary>
        public void CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, LobbyUser localUser, Action<Lobby> onSuccess, Action onFailure)
        {
            if (!rateLimitHost.CanCall())
            {
                onFailure?.Invoke();
                UnityEngine.Debug.LogWarning("Create Lobby hit the rate limit.");
                return;
            }

            string uasId = AuthenticationService.Instance.PlayerId;
            LobbyAPIInterface.CreateLobbyAsync(uasId, lobbyName, maxPlayers, isPrivate, CreateInitialPlayerData(localUser), OnLobbyCreated);

            void OnLobbyCreated(Lobby response)
            {
                if (response == null)
                    onFailure?.Invoke();
                else
                    onSuccess?.Invoke(response); // The Create request automatically joins the lobby, so we need not take further action.
            }
        }

        /// <summary>
        /// Attempt to join an existing lobby. Either ID xor code can be null.
        /// </summary>
        public void JoinLobbyAsync(string lobbyId, string lobbyCode, LobbyUser localUser, Action<Lobby> onSuccess, Action onFailure)
        {
            if (!rateLimitJoin.CanCall() ||
                (lobbyId == null && lobbyCode == null))
            {
                onFailure?.Invoke();
                UnityEngine.Debug.LogWarning("Join Lobby hit the rate limit.");
                return;
            }

            string uasId = AuthenticationService.Instance.PlayerId;
            if (!string.IsNullOrEmpty(lobbyId))
                LobbyAPIInterface.JoinLobbyAsync_ById(uasId, lobbyId, CreateInitialPlayerData(localUser), OnLobbyJoined);
            else
                LobbyAPIInterface.JoinLobbyAsync_ByCode(uasId, lobbyCode, CreateInitialPlayerData(localUser), OnLobbyJoined);

            void OnLobbyJoined(Lobby response)
            {
                if (response == null)
                    onFailure?.Invoke();
                else
                    onSuccess?.Invoke(response);
            }
        }

        /// <summary>
        /// Attempt to join the first lobby among the available lobbies that match the filtered limitToColor.
        /// </summary>
        public void QuickJoinLobbyAsync(LobbyUser localUser, LobbyColor limitToColor = LobbyColor.None, Action<Lobby> onSuccess = null, Action onFailure = null)
        {
            if (!rateLimitQuickJoin.CanCall())
            {
                onFailure?.Invoke();
                UnityEngine.Debug.LogWarning("Quick Join Lobby hit the rate limit.");
                return;
            }

            var filters = LobbyColorToFilters(limitToColor);
            string uasId = AuthenticationService.Instance.PlayerId;
            LobbyAPIInterface.QuickJoinLobbyAsync(uasId, filters, CreateInitialPlayerData(localUser), OnLobbyJoined);

            void OnLobbyJoined(Lobby response)
            {
                if (response == null)
                    onFailure?.Invoke();
                else
                    onSuccess?.Invoke(response);
            }
        }

        /// <summary>
        /// Used for getting the list of all active lobbies, without needing full info for each.
        /// </summary>
        /// <param name="onListRetrieved">If called with null, retrieval was unsuccessful. Else, this will be given a list of contents to display, as pairs of a lobby code and a display string for that lobby.</param>
        public void RetrieveLobbyListAsync(Action<QueryResponse> onListRetrieved, Action<QueryResponse> onError = null, LobbyColor limitToColor = LobbyColor.None)
        {
            if (!rateLimitQuery.CanCall())
            {
                onListRetrieved?.Invoke(null);
                rateLimitQuery.EnqueuePendingOperation(() => { RetrieveLobbyListAsync(onListRetrieved, onError, limitToColor); });
                UnityEngine.Debug.LogWarning("Retrieve Lobby list hit the rate limit. Will try again soon...");
                return;
            }

            var filters = LobbyColorToFilters(limitToColor);

            LobbyAPIInterface.QueryAllLobbiesAsync(filters, OnLobbyListRetrieved);

            void OnLobbyListRetrieved(QueryResponse response)
            {
                if (response != null)
                    onListRetrieved?.Invoke(response);
                else
                    onError?.Invoke(response);
            }
        }

        private List<QueryFilter> LobbyColorToFilters(LobbyColor limitToColor)
        {
            List<QueryFilter> filters = new List<QueryFilter>();
            if (limitToColor == LobbyColor.Orange)
                filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Orange).ToString(), QueryFilter.OpOptions.EQ));
            else if (limitToColor == LobbyColor.Green)
                filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Green).ToString(), QueryFilter.OpOptions.EQ));
            else if (limitToColor == LobbyColor.Blue)
                filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Blue).ToString(), QueryFilter.OpOptions.EQ));
            return filters;
        }

        /// <param name="onComplete">If no lobby is retrieved, or if this call hits the rate limit, this is given null.</param>
        private void RetrieveLobbyAsync(string lobbyId, Action<Lobby> onComplete)
        {
            if (!rateLimitQuery.CanCall())
            {
                onComplete?.Invoke(null);
                UnityEngine.Debug.LogWarning("Retrieve Lobby hit the rate limit.");
                return;
            }
            LobbyAPIInterface.GetLobbyAsync(lobbyId, OnGet);

            void OnGet(Lobby response)
            {
                try
                {
                    onComplete?.Invoke(response); //added exception handling.
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        /// Attempt to leave a lobby, and then delete it if no players remain.
        /// </summary>
        /// <param name="onComplete">Called once the request completes, regardless of success or failure.</param>
        public void LeaveLobbyAsync(string lobbyId, Action onComplete)
        {
            string uasId = AuthenticationService.Instance.PlayerId;
            LobbyAPIInterface.LeaveLobbyAsync(uasId, lobbyId, OnLeftLobby);
            Debug.Log("Left Lobby by Request");
            void OnLeftLobby()
            {
                onComplete?.Invoke();

                // Lobbies will automatically delete the lobby if unoccupied, so we don't need to take further action.
            }
        }

        /// <param name="data">Key-value pairs, which will overwrite any existing data for these keys. Presumed to be available to all lobby members but not publicly.</param>
        public void UpdatePlayerDataAsync(Dictionary<string, string> data, Action onComplete)
        {
            if (!ShouldUpdateData(() => { UpdatePlayerDataAsync(data, onComplete); }, onComplete, false))
                return;

            string playerId = Locator.Get.Identity.GetSubIdentity(Auth.IIdentityType.Auth).GetContent("id");
            Dictionary<string, PlayerDataObject> dataCurr = new Dictionary<string, PlayerDataObject>();
            foreach (var dataNew in data)
            {
                PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: dataNew.Value);
                if (dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key, dataObj);
            }

            LobbyAPIInterface.UpdatePlayerAsync(lastKnownLobby.Id, playerId, dataCurr, (result) => {
                if (result != null)
                    lastKnownLobby = result; // Store the most up-to-date lobby now since we have it, instead of waiting for the next heartbeat.
                onComplete?.Invoke();
            }, null, null);
        }

        /// <summary>
        /// Lobby can be provided info about Relay (or any other remote allocation) so it can add automatic disconnect handling.
        /// </summary>
        public void UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo, Action onComplete)
        {
            if (!ShouldUpdateData(() => { UpdatePlayerRelayInfoAsync(allocationId, connectionInfo, onComplete); }, onComplete, true)) // Do retry here since the RelayUtpSetup that called this might be destroyed right after this.
                return;
            string playerId = Locator.Get.Identity.GetSubIdentity(Auth.IIdentityType.Auth).GetContent("id");
            LobbyAPIInterface.UpdatePlayerAsync(lastKnownLobby.Id, playerId, new Dictionary<string, PlayerDataObject>(), (r) => { onComplete?.Invoke(); }, allocationId, connectionInfo);
        }

        /// <param name="data">Key-value pairs, which will overwrite any existing data for these keys. Presumed to be available to all lobby members but not publicly.</param>
        public void UpdateLobbyDataAsync(Dictionary<string, string> data, Action onComplete)
        {
            if (!ShouldUpdateData(() => { UpdateLobbyDataAsync(data, onComplete); }, onComplete, false))
                return;

            Lobby lobby = lastKnownLobby;
            Dictionary<string, DataObject> dataCurr = lobby.Data ?? new Dictionary<string, DataObject>();

			var shouldLock = false;
            foreach (var dataNew in data)
            {
                // Special case: We want to be able to filter on our color data, so we need to supply an arbitrary index to retrieve later. Uses N# for numerics, instead of S# for strings.
                DataObject.IndexOptions index = dataNew.Key == "Color" ? DataObject.IndexOptions.N1 : 0;
                DataObject dataObj = new DataObject(DataObject.VisibilityOptions.Public, dataNew.Value, index); // Public so that when we request the list of lobbies, we can get info about them for filtering.
                if (dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key, dataObj);
                
                //Special Use: Get the state of the Local lobby so we can lock it from appearing in queries if it's not in the "Lobby" State
                if (dataNew.Key == "State")
                {
                    Enum.TryParse(dataNew.Value, out LobbyState lobbyState);
                    shouldLock = lobbyState != LobbyState.Lobby;
                }
            }

            string newRelayCode = dataCurr.ContainsKey("RelayCode") == true ? dataCurr["RelayCode"].Value : "MissingRelayCode";

            if (newRelayCode != null)
            {
                LobbyAPIInterface.UpdateLobbyAsync(lobby.Id, dataCurr, shouldLock, (result) =>
                {
                    if (result != null)
                        lastKnownLobby = result;
                    onComplete?.Invoke();
                });
            }
        }

        /// <summary>
        /// If we are in the middle of another operation, hold onto any pending ones until after that.
        /// If we aren't in a lobby yet, leave it to the caller to decide what to do, since some callers might need to retry and others might not.
        /// </summary>
        private bool ShouldUpdateData(Action caller, Action onComplete, bool shouldRetryIfLobbyNull)
        {
            if (rateLimitQuery.IsInCooldown)
            {
                rateLimitQuery.EnqueuePendingOperation(caller);
                return false;
            }

            Lobby lobby = lastKnownLobby;
            if (lobby == null)
            {
                if (shouldRetryIfLobbyNull)
                    rateLimitQuery.EnqueuePendingOperation(caller);
                onComplete?.Invoke();
                return false;
            }

            return true;
        }

        private float heartbeatTime = 0;
        private const float heartbeatPeriod = 5; // The heartbeat must be rate-limited to 5 calls per 30 seconds. We'll aim for longer in case periods don't align.

        /// <summary>
        /// Lobby requires a periodic ping to detect rooms that are still active, in order to mitigate "zombie" lobbies.
        /// </summary>
        public void DoLobbyHeartbeat(float dt)
        {
            heartbeatTime += dt;
            if (heartbeatTime > heartbeatPeriod)
            {
                heartbeatTime -= heartbeatPeriod;
                LobbyAPIInterface.HeartbeatPlayerAsync(lastKnownLobby.Id);
            }
        }

        public class RateLimitCooldown : Observed<RateLimitCooldown>
        {
            private float timeSinceLastCall = float.MaxValue;
            private readonly float cooldownTime;
            private Queue<Action> pendingOperations = new Queue<Action>();

            public void EnqueuePendingOperation(Action action)
            {
                pendingOperations.Enqueue(action);
            }

            private bool isInCooldown = false;

            public bool IsInCooldown
            {
                get => isInCooldown;
                private set
                {
                    if (isInCooldown != value)
                    {
                        isInCooldown = value;
                        OnChanged(this);
                    }
                }
            }

            public RateLimitCooldown(float cooldownTime)
            {
                this.cooldownTime = cooldownTime;
            }

            public bool CanCall()
            {
                if (timeSinceLastCall < cooldownTime)
                {
                    return false;
                }
                else
                {
                    Locator.Get.UpdateSlow.Subscribe(OnUpdate, cooldownTime);
                    timeSinceLastCall = 0;
                    IsInCooldown = true;
                    return true;
                }
            }

            private void OnUpdate(float dt)
            {
                timeSinceLastCall += dt;
                if (timeSinceLastCall >= cooldownTime)
                {
                    IsInCooldown = false;
                    if (!isInCooldown) // It's possible that by setting IsInCooldown, something called CanCall immediately, in which case we want to stay on UpdateSlow.
                    {
                        Locator.Get.UpdateSlow.Unsubscribe(OnUpdate); // Note that this is after IsInCooldown is set, to prevent an Observer from kicking off CanCall again immediately.
                        int numPending = pendingOperations.Count; // It's possible a pending operation will re-enqueue itself or new operations, which should wait until the next loop.
                        for (; numPending > 0; numPending--)
                            pendingOperations.Dequeue()?.Invoke(); // Note: If this ends up enqueuing many operations, we might need to batch them and/or ensure they don't all execute at once.
                    }
                }
            }

            public override void CopyObserved(RateLimitCooldown oldObserved)
            {
                /* This behavior isn't needed; we're just here for the OnChanged event management. */
            }
        }
    }
}
