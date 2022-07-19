using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.relay
{
    /// <summary>
    /// Responsible for setting up a connection with Relay using Unity Transport (UTP). A Relay Allocation is created by the host, and then all players
    /// bind UTP to that Allocation in order to send data to each other.
    /// Must be a MonoBehaviour since the binding process doesn't have asynchronous callback options.
    /// </summary>
    public abstract class RelayUtpSetup : MonoBehaviour
    {
        protected bool isRelayConnected = false;
        protected NetworkDriver networkDriver;
        protected List<NetworkConnection> connections;
        protected NetworkEndPoint endpointForServer;
        protected LocalLobby localLobby;
        protected LobbyUser localUser;
        protected Action<bool, RelayUtpClient> onJoinComplete;

        public static string AddressFromEndpoint(NetworkEndPoint endpoint)
        {
            return endpoint.Address.Split(':')[0];
        }

        public void BeginRelayJoin(LocalLobby localLobby, LobbyUser localUser, Action<bool, RelayUtpClient> onJoinComplete)
        {
            Debug.Log("Beginning RelayJoin to Lobby:"+localLobby.LobbyName+" with ID"+localLobby.RelayCode);
            this.localLobby = localLobby;
            this.localUser = localUser;
            this.onJoinComplete = onJoinComplete;
            JoinRelay();
        }

        protected abstract void JoinRelay();


        /// <summary>
        /// Determine the server endpoint for connecting to the Relay server, for either an Allocation or a JoinAllocation.
        /// If DTLS encryption is available, and there's a secure server endpoint available, use that as a secure connection. Otherwise, just connect to the Relay IP unsecured.
        /// </summary>
        public static NetworkEndPoint GetEndpointForAllocation(List<RelayServerEndpoint> endpoints, string ip, int port, out bool isSecure)
        {
            #if ENABLE_MANAGED_UNITYTLS
                foreach (RelayServerEndpoint endpoint in endpoints)
                {
                    if (endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
                    {
                        isSecure = true;
                        return NetworkEndPoint.Parse(endpoint.Host, (ushort)endpoint.Port);
                    }
                }
            #endif
            isSecure = false;
            return NetworkEndPoint.Parse(ip, (ushort)port);
        }

        /// <summary>
        /// Shared behavior for binding to the Relay allocation, which is required for use.
        /// Note that a host will send bytes from the Allocation it creates, whereas a client will send bytes from the JoinAllocation it receives using a relay code.
        /// </summary>
        protected void BindToAllocation(NetworkEndPoint serverEndpoint, byte[] allocationIdBytes, byte[] connectionDataBytes, byte[] hostConnectionDataBytes, byte[] hmacKeyBytes, int connectionCapacity, bool isSecure)
        {
            RelayAllocationId   allocationId       = ConvertAllocationIdBytes(allocationIdBytes);
            RelayConnectionData connectionData     = ConvertConnectionDataBytes(connectionDataBytes);
            RelayConnectionData hostConnectionData = ConvertConnectionDataBytes(hostConnectionDataBytes);
            RelayHMACKey        key                = ConvertHMACKeyBytes(hmacKeyBytes);

            var relayServerData = new RelayServerData(ref serverEndpoint, 0, ref allocationId, ref connectionData, ref hostConnectionData, ref key, isSecure);
            relayServerData.ComputeNewNonce(); // For security, the nonce value sent when authenticating the allocation must be increased.
            var networkSettings = new NetworkSettings();

            networkDriver = NetworkDriver.Create(networkSettings.WithRelayParameters(ref relayServerData));
            connections = new List<NetworkConnection>(connectionCapacity);

            if (networkDriver.Bind(NetworkEndPoint.AnyIpv4) != 0)
                Debug.LogError("Failed to bind to Relay allocation.");
            else
                StartCoroutine(WaitForBindComplete());
        }

        private IEnumerator WaitForBindComplete()
        {
            while (!networkDriver.Bound)
            {
                networkDriver.ScheduleUpdate().Complete();
                yield return null;
            }
            OnBindingComplete();
        }

        protected abstract void OnBindingComplete();

        #region UTP uses pointers instead of managed arrays for performance reasons, so we use these helper functions to convert them.
        unsafe private static RelayAllocationId ConvertAllocationIdBytes(byte[] allocationIdBytes)
        {
            fixed (byte* ptr = allocationIdBytes)
            {
                return RelayAllocationId.FromBytePointer(ptr, allocationIdBytes.Length);
            }
        }

        unsafe private static RelayConnectionData ConvertConnectionDataBytes(byte[] connectionData)
        {
            fixed (byte* ptr = connectionData)
            {
                return RelayConnectionData.FromBytePointer(ptr, RelayConnectionData.k_Length);
            }
        }

        unsafe private static RelayHMACKey ConvertHMACKeyBytes(byte[] hmac)
        {
            fixed (byte* ptr = hmac)
            {
                return RelayHMACKey.FromBytePointer(ptr, RelayHMACKey.k_Length);
            }
        }
        #endregion

        private void OnDestroy()
        {
            if (!isRelayConnected && networkDriver.IsCreated)
                networkDriver.Dispose();
        }
    }

    /// <summary>
    /// Host logic: Request a new Allocation, and then both bind to it and request a join code. Once those are both complete, supply data back to the lobby.
    /// </summary>
    public class RelayUtpSetupHost : RelayUtpSetup
    {
        [Flags]
        private enum JoinState { None = 0, Bound = 1, Joined = 2 }
        private JoinState joinState = JoinState.None;
        private Allocation allocation;

        protected override void JoinRelay()
        {
            RelayAPIInterface.AllocateAsync(localLobby.MaxPlayerCount, OnAllocation);
        }

        private void OnAllocation(Allocation allocation)
        {
            this.allocation = allocation;
            RelayAPIInterface.GetJoinCodeAsync(allocation.AllocationId, OnRelayCode);
            bool isSecure = false;
            endpointForServer = GetEndpointForAllocation(allocation.ServerEndpoints, allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);
            BindToAllocation(endpointForServer, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.ConnectionData, allocation.Key, 16, isSecure);
        }

        private void OnRelayCode(string relayCode)
        {
            localLobby.RelayCode = relayCode;
            localLobby.RelayServer = new ServerAddress(AddressFromEndpoint(endpointForServer), endpointForServer.Port);
            joinState |= JoinState.Joined;
            CheckForComplete();
        }

        protected override void OnBindingComplete()
        {
            if (networkDriver.Listen() != 0)
            {
                Debug.LogError("RelayUtpSetupHost failed to bind to the Relay Allocation.");
                onJoinComplete(false, null);
            }
            else
            {
                Debug.Log("Relay host is bound.");
                joinState |= JoinState.Bound;
                CheckForComplete();
            }
        }

        private void CheckForComplete()
        {
            if (joinState == (JoinState.Joined | JoinState.Bound) && this != null) // this will equal null (i.e. this component has been destroyed) if the host left the lobby during the Relay connection sequence.
            {
                isRelayConnected = true;
                RelayUtpHost host = gameObject.AddComponent<RelayUtpHost>();
                host.Initialize(networkDriver, connections, localUser, localLobby);
                onJoinComplete(true, host);
                LobbyAsyncRequests.Instance.UpdatePlayerRelayInfoAsync(allocation.AllocationId.ToString(), localLobby.RelayCode, null);
            }
        }
    }

    /// <summary>
    /// Client logic: Wait until the Relay join code is retrieved from the lobby's shared data. Then, use that code to get the Allocation to bind to, and
    /// then create a connection to the host.
    /// </summary>
    public class RelayUtpSetupClient : RelayUtpSetup
    {
        private JoinAllocation allocation;

        protected override void JoinRelay()
        {
            localLobby.onChanged += OnLobbyChange;
        }

        private void OnLobbyChange(LocalLobby lobby)
        {
            Debug.Log("OnLobbyChanged. Relay Code:"+localLobby.RelayCode);
            if (localLobby.RelayCode != null)
            {
                RelayAPIInterface.JoinAsync(localLobby.RelayCode, OnJoin);
                localLobby.onChanged -= OnLobbyChange;
            }
        }

        private void OnJoin(JoinAllocation joinAllocation)
        {
            if (joinAllocation == null || this == null) // The returned JoinAllocation is null if allocation failed. this would be destroyed already if you quit the lobby while Relay is connecting.
                return;
            allocation = joinAllocation;
            bool isSecure = false;
            endpointForServer = GetEndpointForAllocation(joinAllocation.ServerEndpoints, joinAllocation.RelayServer.IpV4, joinAllocation.RelayServer.Port, out isSecure);
            BindToAllocation(endpointForServer, joinAllocation.AllocationIdBytes, joinAllocation.ConnectionData, joinAllocation.HostConnectionData, joinAllocation.Key, 1, isSecure);
            localLobby.RelayServer = new ServerAddress(AddressFromEndpoint(endpointForServer), endpointForServer.Port);
        }

        protected override void OnBindingComplete()
        {
            StartCoroutine(ConnectToServer());
        }

        private IEnumerator ConnectToServer()
        {
            // Once the client is bound to the Relay server, send a connection request.
            connections.Add(networkDriver.Connect(endpointForServer));
            while (networkDriver.GetConnectionState(connections[0]) == NetworkConnection.State.Connecting)
            {
                networkDriver.ScheduleUpdate().Complete();
                yield return null;
            }
            if (networkDriver.GetConnectionState(connections[0]) != NetworkConnection.State.Connected)
            {
                Debug.LogError("RelayUtpSetupClient could not connect to the host.");
                onJoinComplete(false, null);
            }
            else if (this != null)
            {
                isRelayConnected = true;
                RelayUtpClient client = gameObject.AddComponent<RelayUtpClient>();
                client.Initialize(networkDriver, connections, localUser, localLobby);
                onJoinComplete(true, client);
                Debug.Log("Providing Relay code:" + localLobby.RelayCode);
                LobbyAsyncRequests.Instance.UpdatePlayerRelayInfoAsync(allocation.AllocationId.ToString(), localLobby.RelayCode, null);
            }
        }
    }
}
