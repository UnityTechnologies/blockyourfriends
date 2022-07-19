using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    [Flags] // Some UI elements will want to specify multiple states in which to be active, so this is Flags.
    public enum LobbyState
    {
        Lobby = 1,
        CountDown = 2,
        InGame = 4
    }

    public enum LobbyColor { None = 0, Orange = 1, Green = 2, Blue = 3 }

    /// <summary>
    /// A local wrapper around a lobby's remote data, with additional functionality for providing that data to UI elements and tracking local player objects.
    /// (The way that the Lobby service handles its data doesn't necessarily match our needs, so we need to map from that to this LocalLobby for use in the sample code.)
    /// </summary>
    [System.Serializable]
    public class LocalLobby : Observed<LocalLobby>
    {
        private Dictionary<string, LobbyUser> lobbyUsers = new Dictionary<string, LobbyUser>();
        public Dictionary<string, LobbyUser> LobbyUsers => lobbyUsers;

        #region LocalLobbyData
        public struct LobbyData
        {
            public string LobbyID { get; set; }
            public string LobbyCode { get; set; }
            public string RelayCode { get; set; }
            public string RelayNGOCode { get; set; }
            public string LobbyName { get; set; }
            public bool Private { get; set; }
            public int MaxPlayerCount { get; set; }
            public LobbyState State { get; set; }
            public LobbyColor Color { get; set; }
            public long State_LastEdit { get; set; }
            public long Color_LastEdit { get; set; }
            public long RelayNGOCode_LastEdit { get; set; }

            public LobbyData(LobbyData existing)
            {
                LobbyID = existing.LobbyID;
                LobbyCode = existing.LobbyCode;
                RelayCode = existing.RelayCode;
                RelayNGOCode = existing.RelayNGOCode;
                LobbyName = existing.LobbyName;
                Private = existing.Private;
                MaxPlayerCount = existing.MaxPlayerCount;
                State = existing.State;
                Color = existing.Color;
                State_LastEdit = existing.State_LastEdit;
                Color_LastEdit = existing.Color_LastEdit;
                RelayNGOCode_LastEdit = existing.RelayNGOCode_LastEdit;
            }

            public LobbyData(string lobbyCode)
            {
                Debug.Log("Lobby Data Created");
                LobbyID = null;
                LobbyCode = lobbyCode;
                RelayCode = null;
                RelayNGOCode = null;
                LobbyName = null;
                Private = false;
                MaxPlayerCount = -1;
                State = LobbyState.Lobby;
                Color = LobbyColor.None;
                State_LastEdit = 0;
                Color_LastEdit = 0;
                RelayNGOCode_LastEdit = 0;
            }
        }

        private LobbyData data;
        public LobbyData Data
        {
            get { return new LobbyData(data); }
        }

        ServerAddress relayServer;

        /// <summary>Used only for visual output of the Relay connection info. The obfuscated Relay server IP is obtained during allocation in the RelayUtpSetup.</summary>
        public ServerAddress RelayServer
        {
            get => relayServer;
            set
            {
                relayServer = value;
                OnChanged(this);
            }
        }

        #endregion

        public void AddPlayer(LobbyUser user)
        {
            if (lobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogError($"Cant add player {user.DisplayName}({user.ID}) to lobby: {LobbyID} twice");
                return;
            }

            DoAddPlayer(user);
            OnChanged(this);
        }

        private void DoAddPlayer(LobbyUser user)
        {
            lobbyUsers.Add(user.ID, user);
            user.onChanged += OnChangedUser;
        }

        public void RemovePlayer(LobbyUser user)
        {
            DoRemoveUser(user);
            OnChanged(this);
        }

        private void DoRemoveUser(LobbyUser user)
        {
            if (!lobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
                return;
            }

            lobbyUsers.Remove(user.ID);
            user.onChanged -= OnChangedUser;
        }

        private void OnChangedUser(LobbyUser user)
        {
            OnChanged(this);
        }

        public string LobbyID
        {
            get => data.LobbyID;
            set
            {
                data.LobbyID = value;
                OnChanged(this);
            }
        }

        public string LobbyCode
        {
            get => data.LobbyCode;
            set
            {
                data.LobbyCode = value;
                OnChanged(this);
            }
        }

        public string RelayCode
        {
            get => data.RelayCode;
            set
            {
                data.RelayCode = value;
                OnChanged(this);
            }
        }

        public string RelayNGOCode
        {
            get => data.RelayNGOCode;
            set
            {
                data.RelayNGOCode = value;
                data.RelayNGOCode_LastEdit = DateTime.Now.Ticks;
                OnChanged(this);
            }
        }

        public string LobbyName
        {
            get => data.LobbyName;
            set
            {
                data.LobbyName = value;
                OnChanged(this);
            }
        }

        public LobbyState State
        {
            get => data.State;
            set
            {
                data.State = value;
                data.State_LastEdit = DateTime.Now.Ticks;
                OnChanged(this);
            }
        }
        
        public bool Private
        {
            get => data.Private;
            set
            {
                data.Private = value;
                OnChanged(this);
            }
        }

        public int PlayerCount => lobbyUsers.Count;

        public int MaxPlayerCount
        {
            get => data.MaxPlayerCount;
            set
            {
                data.MaxPlayerCount = value;
                OnChanged(this);
            }
        }

        public LobbyColor Color
        {
            get => data.Color;
            set
            {
                if (data.Color != value)
                {   data.Color = value;
                    data.Color_LastEdit = DateTime.Now.Ticks;
                    OnChanged(this);
                }
            }
        }

        public void CopyObserved(LobbyData data, Dictionary<string, LobbyUser> currUsers)
        {
            // It's possible for the host to edit the lobby in between the time they last pushed lobby data and the time their pull for new lobby data completes.
            // If that happens, the edit will be lost, so instead we maintain the time of last edit to detect that case.
            var pendingState = data.State;
            var pendingColor = data.Color;
            var pendingNgoCode = data.RelayNGOCode;
            if (this.data.State_LastEdit > data.State_LastEdit)
                pendingState = this.data.State;
            if (this.data.Color_LastEdit > data.Color_LastEdit)
                pendingColor = this.data.Color;
            if (this.data.RelayNGOCode_LastEdit > data.RelayNGOCode_LastEdit)
                pendingNgoCode = this.data.RelayNGOCode;
            this.data = data;
            this.data.State = pendingState;
            this.data.Color = pendingColor;
            this.data.RelayNGOCode = pendingNgoCode;

            if (currUsers == null)
                lobbyUsers = new Dictionary<string, LobbyUser>();
            else
            {
                List<LobbyUser> toRemove = new List<LobbyUser>();
                foreach (var oldUser in lobbyUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                        oldUser.Value.CopyObserved(currUsers[oldUser.Key]);
                    else
                        toRemove.Add(oldUser.Value);
                }

                foreach (var remove in toRemove)
                {
                    DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!lobbyUsers.ContainsKey(currUser.Key))
                        DoAddPlayer(currUser.Value);
                }
            }

            OnChanged(this);
        }

        // This ends up being called from the lobby list when we get data about a lobby without having joined it yet.
        public override void CopyObserved(LocalLobby oldObserved)
        {
            CopyObserved(oldObserved.Data, oldObserved.lobbyUsers);
        }
    }
}
