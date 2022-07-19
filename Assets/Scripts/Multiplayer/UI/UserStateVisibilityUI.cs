using System;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// User permission type. It's a flag enum to allow for the Inspector to select multiples for various UI features.
    /// </summary>
    [Flags]
    public enum UserPermission
    {
        Client = 1,
        Host = 2
    }

    /// <summary>
    /// Shows the UI when the LobbyUser matches some conditions, including having the target permissions.
    /// </summary>
    [RequireComponent(typeof(LobbyUserObserver))]
    public class UserStateVisibilityUI : ObserverPanel<LobbyUser>
    {
        [SerializeField] private UserStatus showThisWhen;
        [SerializeField] private UserPermission permissions;

        public override void ObservedUpdated(LobbyUser observed)
        {
            var hasStatusFlags = showThisWhen.HasFlag(observed.UserStatus);

            var hasPermissions = false;

            if (permissions.HasFlag(UserPermission.Host) && observed.IsHost)
            {
                hasPermissions = true;
            }
            else if (permissions.HasFlag(UserPermission.Client) && !observed.IsHost)
            {
                hasPermissions = true;
            }

            if (hasStatusFlags && hasPermissions)
                Show();
            else
                Hide();
        }
    }
}