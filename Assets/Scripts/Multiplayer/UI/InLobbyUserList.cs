using System.Collections.Generic;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Contains the InLobbyUserUI instances while showing the UI for a lobby.
    /// </summary>
    [RequireComponent(typeof(LocalLobbyObserver))]
    public class InLobbyUserList : ObserverPanel<LocalLobby>
    {
        [SerializeField] private List<InLobbyUserUI> userUIObjects = new List<InLobbyUserUI>();
        private List<string> currentUsers = new List<string>(); // Just for keeping track more easily of which users are already displayed.

        /// <summary>
        /// When the observed data updates, we need to detect changes to the list of players.
        /// </summary>
        public override void ObservedUpdated(LocalLobby observed)
        {
            for (int id = currentUsers.Count - 1; id >= 0; id--) // We might remove users if they aren't in the new data, so iterate backwards.
            {
                string userId = currentUsers[id];
                if (!observed.LobbyUsers.ContainsKey(userId))
                {
                    foreach (var ui in userUIObjects)
                    {
                        if (ui.UserId == userId)
                        {
                            ui.OnUserLeft();
                            OnUserLeft(userId);
                        }
                    }
                }
            }

            foreach (var lobbyUserKvp in observed.LobbyUsers) // If there are new players, we need to hook them into the UI.
            {
                if (currentUsers.Contains(lobbyUserKvp.Key))
                    continue;
                currentUsers.Add(lobbyUserKvp.Key);

                foreach (var pcu in userUIObjects)
                {
                    if (pcu.IsAssigned)
                        continue;
                    pcu.SetUser(lobbyUserKvp.Value);
                    break;
                }
            }
        }

        void OnUserLeft(string userID)
        {
            if (!currentUsers.Contains(userID))
                return;
            currentUsers.Remove(userID);
        }
    }
}
