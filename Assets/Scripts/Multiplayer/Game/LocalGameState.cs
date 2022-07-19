using System;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Current state of the local game.
    /// Set as a flag to allow for the Inspector to select multiple valid states for various UI features.
    /// </summary>
    [Flags]
    public enum GameState
    {
        Menu = 1,
        Lobby = 2,
        JoinMenu = 4,
    }

    /// <summary>
    /// Awaits player input to change the local game data.
    /// </summary>
    [System.Serializable]
    public class LocalGameState : Observed<LocalGameState>
    {
        private GameState state = GameState.Menu;

        public GameState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnChanged(this);
                }
            }
        }

        public override void CopyObserved(LocalGameState oldObserved)
        {
            if (state == oldObserved.State)
                return;
            state = oldObserved.State;
            OnChanged(this);
        }
    }
}
