using BlockYourFriends.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// When inside a lobby, this will show information about a player, whether local or remote.
    /// </summary>
    [RequireComponent(typeof(LobbyUserObserver))]
    public class InLobbyUserUI : ObserverPanel<LobbyUser>
    {
        [SerializeField] private TMP_Text displayNameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image hostIcon;
        [SerializeField] private vivox.VivoxUserHandler vivoxUserHandler;
        [SerializeField] private InGamePlayerHUD inGamePlayerHUD;

        public bool IsAssigned => UserId != null;

        public string UserId { get; private set; }
        private LobbyUserObserver observer;

        public void SetUser(LobbyUser myLobbyUser)
        {
            Show();
            if (observer == null)
                observer = GetComponent<LobbyUserObserver>();
            observer.BeginObserving(myLobbyUser);
            UserId = myLobbyUser.ID;
            vivoxUserHandler.SetId(UserId);
        }

        public void OnUserLeft()
        {
            UserId = null;
            Hide();
            observer.EndObserving();
        }

        public override void ObservedUpdated(LobbyUser observed)
        {
            displayNameText.SetText(observed.DisplayName);
            inGamePlayerHUD.UpdatePlayerName(observed.DisplayName);
            statusText.SetText(SetStatusFancy(observed.UserStatus));
            hostIcon.enabled = observed.IsHost;
            //observed.PaddleLocation = m_PaddleLocation;
        }

        string SetStatusFancy(UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Lobby:
                    return "<color=#7EF1FD>in lobby</color>";
                case UserStatus.Ready:
                    return "<color=#F59FFC>ready</color>";
                case UserStatus.Connecting:
                    return "<color=#FDA09B>connecting...</color>";
                case UserStatus.InGame:
                    return "<color=#FFFFFF>in game</color>";
                default:
                    return "";
            }
        }
    }
}
