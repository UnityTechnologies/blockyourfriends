﻿using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Show or hide a UI element based on the current GameState (e.g. in a lobby).
    /// </summary>
    [RequireComponent(typeof(LocalGameStateObserver))]
    public class GameStateVisibilityUI : ObserverPanel<LocalGameState>
    {
        [SerializeField]
        private GameState ShowThisWhen;

        public override void ObservedUpdated(LocalGameState observed)
        {
            if (!ShowThisWhen.HasFlag(observed.State))
                Hide();
            else
            {
                Show();
            }
        }
    }
}
