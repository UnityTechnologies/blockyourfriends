using System;
using BlockYourFriends.Gameplay;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Current state of the user in the lobby.
    /// This is a Flags enum to allow for the Inspector to select multiples for various UI features.
    /// </summary>
    [Flags]
    public enum UserStatus
    {
        None = 0,
        Connecting = 1, // User has joined a lobby but has not yet connected to Relay.
        Lobby = 2,      // User is in a lobby and connected to Relay.
        Ready = 4,      // User has selected the ready button, to ready for the "game" to start.
        InGame = 8,     // User is part of a "game" that has started.
        Menu = 16       // User is not in a lobby, in one of the main menus.
    }

    public enum InGameSlot
    {
        Bottom,
        Top,
        Right,
        Left
    }

    /// <summary>
    /// Data for a local player instance. This will update data and is observed to know when to push local player changes to the entire lobby.
    /// </summary>
    [Serializable]
    public class LobbyUser : Observed<LobbyUser>
    {
        public LobbyUser(bool isHost = false, string displayName = null, string id = null, EmoteType emote = EmoteType.None, UserStatus userStatus = UserStatus.Menu, bool isApproved = false, PaddleLocation paddlePosition = PaddleLocation.bottom)
        {
            data = new UserData(isHost, displayName, id, emote, userStatus, isApproved, paddlePosition);
        }

        #region Local UserData

        public struct UserData
        {
            public bool IsHost { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }
            public EmoteType Emote { get; set; }
            public UserStatus UserStatus { get; set; }
            public bool IsApproved { get; set; }
            public PaddleLocation PaddlePosition { get; set; }

            public UserData(bool isHost, string displayName, string id, EmoteType emote, UserStatus userStatus, bool isApproved, PaddleLocation paddlePosition)
            {
                IsHost = isHost;
                DisplayName = displayName;
                ID = id;
                Emote = emote;
                UserStatus = userStatus;
                IsApproved = isApproved;
                PaddlePosition = paddlePosition;
            }
        }

        private UserData data;

        public void ResetState()
        {
            data = new UserData(false, data.DisplayName, data.ID, EmoteType.None, UserStatus.Menu, false, PaddleLocation.bottom); // ID and DisplayName should persist since this might be the local user.
        }

        #endregion

        /// <summary>
        /// Used for limiting costly OnChanged actions to just the members which actually changed.
        /// </summary>
        [Flags]
        public enum UserMembers
        {
            IsHost = 1,
            DisplayName = 2,
            Emote = 4,
            ID = 8,
            UserStatus = 16,
            IsApproved = 32,
            PaddleLocation = 48,
        }

        private UserMembers lastChanged;
        public UserMembers LastChanged => lastChanged;

        public bool IsHost
        {
            get { return data.IsHost; }
            set
            {
                if (data.IsHost != value)
                {
                    data.IsHost = value;
                    lastChanged = UserMembers.IsHost;
                    OnChanged(this);
                    if (value)
                        IsApproved = true;
                }
            }
        }

        public string DisplayName
        {
            get => data.DisplayName;
            set
            {
                if (data.DisplayName != value)
                {
                    data.DisplayName = value;
                    lastChanged = UserMembers.DisplayName;
                    OnChanged(this);
                }
            }
        }

        public EmoteType Emote
        {
            get => data.Emote;
            set
            {
                if (data.Emote != value)
                {
                    data.Emote = value;
                    lastChanged = UserMembers.Emote;
                    OnChanged(this);
                }
            }
        }

        public string ID
        {
            get => data.ID;
            set
            {
                if (data.ID != value)
                {
                    data.ID = value;
                    lastChanged = UserMembers.ID;
                    OnChanged(this);
                }
            }
        }

        UserStatus m_userStatus = UserStatus.Menu;
        public UserStatus UserStatus
        {
            get => m_userStatus;
            set
            {
                if (m_userStatus != value)
                {
                    m_userStatus = value;
                    lastChanged = UserMembers.UserStatus;
                    OnChanged(this);
                }
            }
        }

        public bool IsApproved // Clients joining the lobby should be approved by the host before they can interact.
        {
            get => data.IsApproved;
            set
            {
                if (!data.IsApproved && value) // Don't be un-approved except by a call to ResetState.
                {
                    data.IsApproved = value;
                    lastChanged = UserMembers.IsApproved;
                    OnChanged(this);
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ClientUserApproved, null);
                }
            }
        }

        public PaddleLocation PaddleLocation
        {
            get => data.PaddlePosition;
            set
            {
                if (data.PaddlePosition != value)
                {
                    data.PaddlePosition = value;
                    lastChanged = UserMembers.PaddleLocation;
                    OnChanged(this);
                }
            }
        }

        public override void CopyObserved(LobbyUser observed)
        {
            UserData data = observed.data;
            int lastChanged = // Set flags just for the members that will be changed.
                (this.data.IsHost == data.IsHost ?           0 : (int)UserMembers.IsHost) |
                (this.data.DisplayName == data.DisplayName ? 0 : (int)UserMembers.DisplayName) |
                (this.data.ID == data.ID ?                   0 : (int)UserMembers.ID) |
                (this.data.Emote == data.Emote ?             0 : (int)UserMembers.Emote) |
                (this.data.UserStatus == data.UserStatus ?   0 : (int)UserMembers.UserStatus) |
                (this.data.PaddlePosition == data.PaddlePosition ? 0 : (int)UserMembers.PaddleLocation);

            if (lastChanged == 0) // Ensure something actually changed.
                return;

            this.data = data;
            this.lastChanged = (UserMembers)lastChanged;

            OnChanged(this);
        }
    }
}
