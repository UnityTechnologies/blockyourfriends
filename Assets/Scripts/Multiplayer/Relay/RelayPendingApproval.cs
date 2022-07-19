using System;
using Unity.Networking.Transport;

namespace BlockYourFriends.Multiplayer.relay
{
    /// <summary>
    /// The Relay host doesn't need to know what might approve or disapprove of a pending connection, so this will
    /// broadcast a message that approval is being sought, and if nothing disapproves, the connection will be permitted.
    /// </summary>
    public class RelayPendingApproval : IDisposable
    {
        private const float WaitTime = 0.1f;

        private NetworkConnection pendingConnection;
        private bool hasDisposed = false;
        private Action<NetworkConnection, Approval> onResult;

        public string ID { get; private set; }

        public RelayPendingApproval(NetworkConnection conn, Action<NetworkConnection, Approval> onResult, string id)
        {
            pendingConnection = conn;
            this.onResult = onResult;
            ID = id;
            Locator.Get.UpdateSlow.Subscribe(Approve, WaitTime);
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ClientUserSeekingDisapproval, (Action<Approval>)Disapprove);
        }
        ~RelayPendingApproval() { Dispose(); }

        private void Approve(float unused)
        {
            try
            {
                onResult?.Invoke(pendingConnection, Approval.OK);
            }
            finally
            {
                Dispose();
            }
        }

        public void Disapprove(Approval reason)
        {
            try
            {
                onResult?.Invoke(pendingConnection, reason);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (!hasDisposed)
            {
                Locator.Get.UpdateSlow.Unsubscribe(Approve);
                hasDisposed = true;
            }
        }
    }
}
