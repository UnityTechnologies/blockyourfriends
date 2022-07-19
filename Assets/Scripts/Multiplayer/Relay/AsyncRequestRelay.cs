using System;
using Unity.Services.Relay;

namespace BlockYourFriends.Multiplayer.relay
{
    public class AsyncRequestRelay : AsyncRequest
    {
        private static AsyncRequestRelay instance;
        public static AsyncRequestRelay Instance
        {
            get
            {   if (instance == null)
                    instance = new AsyncRequestRelay();
                return instance;
            }
        }

        /// <summary>
        /// The Relay service will wrap HTTP errors in RelayServiceExceptions. We can filter on RelayServiceException.Reason for custom behavior.
        /// </summary>
        protected override void ParseServiceException(Exception e)
        {
            if (!(e is RelayServiceException))
                return;
            var relayEx = e as RelayServiceException;
            if (relayEx.Reason == RelayExceptionReason.Unknown)
                Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Relay Error: Relay service had an unknown error.");
            else
                Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, $"Relay Error: {relayEx.Message}");
        }
    }
}
