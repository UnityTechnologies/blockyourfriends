using System;
using BlockYourFriends.Multiplayer.relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.ngo
{
    /*
     * To use Netcode for GameObjects (NGO), we use the Relay adapter for UTP, attached to a NetworkManager. This needs to be provided the Allocation info before we bind to it.
     * In actual use, if you are using NGO for your game's networking, you would not also use the RelayUtpSetupHost/RelayUtpSetupClient at all, since their direct data transmission would be unnecessary.
     * We keep both versions for this sample to demonstrate how each is set up, whether you want to just use Lobby + Relay or use NGO as well.
     */

    /// <summary>
    /// Host logic: Request a new Allocation, and then pass its info to the UTP adapter for NGO.
    /// </summary>
    public class RelayUtpNGOSetupHost : MonoBehaviour // This is a MonoBehaviour so that it can be added to the InGameRunner object for easier cleanup on game end.
    {
        private SetupInGame setupInGame;
        private LocalLobby localLobby;
        private Action onJoin;

        public void Initialize(SetupInGame setupInGame, LocalLobby lobby, Action onJoin)
        {
            this.setupInGame = setupInGame;
            localLobby = lobby;
            this.onJoin = onJoin;

            RelayAPIInterface.AllocateAsync(localLobby.MaxPlayerCount, OnAllocation);
        }

        private void OnAllocation(Allocation allocation)
        {
            RelayAPIInterface.GetJoinCodeAsync(allocation.AllocationId, OnRelayCode);
            bool isSecure = false;
            var endpoint = RelayUtpSetup.GetEndpointForAllocation(allocation.ServerEndpoints, allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);
            setupInGame.SetRelayServerData(RelayUtpSetup.AddressFromEndpoint(endpoint), endpoint.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.ConnectionData, isSecure);
            onJoin?.Invoke();
        }

        private void OnRelayCode(string relayCode)
        {
            localLobby.RelayNGOCode = relayCode;
        }
    }

    /// <summary>
    /// Client logic: Wait to receive the Relay code for NGO, and then pass the allocation info to the UTP adapter.
    /// </summary>
    public class RelayUtpNGOSetupClient : MonoBehaviour // This is also a MonoBehaviour for access to OnDestroy, to ensure unsubscription from the local lobby on game end.
    {
        private SetupInGame setupInGame;
        private LocalLobby localLobby;
        private Action onJoin;

        public void Initialize(SetupInGame setupInGame, LocalLobby lobby, Action onJoin)
        {
            this.setupInGame = setupInGame;
            localLobby = lobby;
            this.onJoin = onJoin;

            localLobby.onChanged += OnLobbyChange;
        }
        public void OnDestroy()
        {
            localLobby.onChanged -= OnLobbyChange;
        }

        private void OnLobbyChange(LocalLobby lobby)
        {
            if (localLobby.RelayNGOCode != null)
            {
                RelayAPIInterface.JoinAsync(localLobby.RelayNGOCode, OnJoin);
                localLobby.onChanged -= OnLobbyChange;
            }
        }

        private void OnJoin(JoinAllocation joinAllocation)
        {
            if (joinAllocation == null || this == null) // The returned JoinAllocation is null if allocation failed. This would be destroyed already if you quit the lobby while Relay is connecting.
                return;

            var endpoint = RelayUtpSetup.GetEndpointForAllocation(joinAllocation.ServerEndpoints, joinAllocation.RelayServer.IpV4, joinAllocation.RelayServer.Port, out bool isSecure);
            setupInGame.SetRelayServerData(RelayUtpSetup.AddressFromEndpoint(endpoint), endpoint.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure);
            onJoin?.Invoke();
        }
    }
}
