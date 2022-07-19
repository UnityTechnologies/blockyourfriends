using System;
using Unity.Services.Lobbies;

namespace BlockYourFriends.Multiplayer.lobby
{
    public class AsyncRequestLobby : AsyncRequest
    {
        private static AsyncRequestLobby instance;
        public static AsyncRequestLobby Instance
        {
            get
            {   if (instance == null)
                    instance = new AsyncRequestLobby();
                return instance;
            }
        }

        /// <summary>
        /// The Lobby service will wrap HTTP errors in LobbyServiceExceptions. We can filter on LobbyServiceException.Reason for custom behavior.
        /// </summary>
        protected override void ParseServiceException(Exception e)
        {
            if (!(e is LobbyServiceException))
                return;
            var lobbyEx = e as LobbyServiceException;
            if (lobbyEx.Reason == LobbyExceptionReason.RateLimited) // We have other ways of preventing players from hitting the rate limit, so the developer-facing 429 error is sufficient here.
                return;
            Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, $"Lobby Error: {lobbyEx.Message}."); // Lobby error type, then HTTP error type.
        }
    }
}
