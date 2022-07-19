using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// UI element that is displayed when the lobby is in a particular state (e.g. counting down, in-game).
    /// </summary>
    public class ShowWhenLobbyStateUI : ObserverPanel<LocalLobby>
    {
        [SerializeField] private LobbyState showThisWhen;

        public override void ObservedUpdated(LocalLobby observed)
        {
            if (showThisWhen.HasFlag(observed.State))
                Show();
            else
                Hide();
        }
    }
}