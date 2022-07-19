using System;
using System.Collections;
using System.Linq;
using BlockYourFriends.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.ngo
{
    /// <summary>
    /// Once the NetworkManager has been spawned, we need something to manage the game state and setup other in-game objects
    /// that is itself a networked object, to track things like network connect events.
    /// </summary>
    public class InGameRunner : NetworkBehaviour
    {
        private static InGameRunner instance;
        public static InGameRunner Instance { get { return instance; } }

        private Action onConnectionVerified, onGameEnd;
        private int expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool hasConnected = false;

        [SerializeField]
        private PaddlesManager paddlesManager;
        [SerializeField]
        private BrickSetManager brickSetManager;

        private PlayerData localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.
        private LocalLobby localLobby;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }

            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
            {
                paddlesManager.Initialize();
                paddlesManager.GetHostScreenSize(new Vector2(Screen.width, Screen.height));
                brickSetManager.StartNewGame();
            }
            else
                UIManager.Instance.UpdateVisibilityWaitingForPlayer(true);
        }

        public override void OnDestroy()
        {
            instance = null;
        }

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameEnd, LobbyUser localUser, LocalLobby localLobby)
        {
            this.onConnectionVerified = onConnectionVerified;
            this.expectedPlayerCount = expectedPlayerCount;
            this.onGameEnd = onGameEnd;
            this.localLobby = localLobby;
            paddlesManager.Initialize(localUser, localLobby);
            localUserData = new PlayerData(localUser.DisplayName, 0, 0, (int)localUser.PaddleLocation);
        }

        public override void OnNetworkSpawn()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                return;

            localUserData = new PlayerData(localUserData.name, NetworkManager.Singleton.LocalClientId);
            VerifyConnection_ServerRpc(localUserData.id);

            //if (IsHost)
            //    UpdateVisibilityWaitingForPlayer_ServerRpc(true);
        }

        public override void OnNetworkDespawn()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                    return;

                if (IsHost)
                    UpdateVisibilityWaitingForPlayer_ServerRpc(false);

                onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
            }
        }

        private void FinishInitialize()
        {
            LobbyUser[] lobbyUsers = localLobby.LobbyUsers.Values.ToArray();
            string[] lobbyUserIDs = new string[4];
            for (int i = 0; i < lobbyUsers.Length; i++)
            {
                lobbyUserIDs[i] = lobbyUsers[i].ID;
            }

            paddlesManager.SetupPlayers_ServerRpc(lobbyUserIDs[0], lobbyUserIDs[1], lobbyUserIDs[2], lobbyUserIDs[3]);
            paddlesManager.SendHostScreenSize_ServerRpc(new Vector2(Screen.width, Screen.height));
            brickSetManager.StartNewGame();
            UpdateVisibilityWaitingForPlayer_ServerRpc(false);

            Debug.Log("NGO - FinishInitialize");
        }

        /// <summary>
        /// To verify the connection, invoke a server RPC call that then invokes a client RPC call. After this, the actual setup occurs.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);

            // While we could start pooling symbol objects now, incoming clients would be flooded with the Spawn calls.
            // This could lead to dropped packets such that the InGameRunner's Spawn call fails to occur, so we'll wait until all players join.
            // (Besides, we will need to display instructions, which has downtime during which symbol objects can be spawned.)
        }

        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if (clientId == localUserData.id)
                VerifyConnectionConfirm_ServerRpc(localUserData);
        }

        /// <summary>
        /// Once the connection is confirmed, spawn a player cursor and check if all players have connected.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(PlayerData clientData)
        {
            Debug.Log("NGO - VerifyConnectionConfirm_ServerRpc: " + clientData.name);

            bool areAllPlayersConnected = NetworkManager.ConnectedClients.Count >= expectedPlayerCount; // The game will begin at this point, or else there's a timeout for booting any unconnected players.
            VerifyConnectionConfirm_ClientRpc(clientData.id, areAllPlayersConnected);

            if (areAllPlayersConnected && IsHost)
                FinishInitialize();
        }

        [ClientRpc]
        private void VerifyConnectionConfirm_ClientRpc(ulong clientId, bool canBeginGame)
        {
            if (clientId == localUserData.id)
            {
                onConnectionVerified?.Invoke();
                hasConnected = true;
            }

            if (canBeginGame && hasConnected)
            {
                BeginGame();
            }
        }

        /// <summary>
        /// The game will begin either when all players have connected successfully or after a timeout.
        /// </summary>
        private void BeginGame()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.MinigameBeginning, null);
        }

        public void EndGame()
        {
            if (IsHost)
                StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsHost)
                return;
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.EndGame, null); // We only send this message if the game completes, since the player remains in the lobby in that case. If the player leaves with the back button, that instead sends them to the menu.
            onGameEnd();
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateVisibilityWaitingForPlayer_ServerRpc(bool isShow)
        {
            UpdateVisibilityWaitingForPlayer_ClientRpc(isShow);
        }

        [ClientRpc]
        public void UpdateVisibilityWaitingForPlayer_ClientRpc(bool isShow)
        {
            UIManager.Instance.UpdateVisibilityWaitingForPlayer(isShow);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangePlayerToAI_ServerRpc(string userID)
        {
            ChangePlayerToAI_ClientRpc(userID);
        }

        [ClientRpc]
        public void ChangePlayerToAI_ClientRpc(string userID)
        {
            if (IsHost)
            {
                foreach (Player player in BallManager.Instance.players)
                {
                    if (player.playerID == userID)
                    {
                        string randomNameOfAI = player.InitPlayer(true);
                        ChangePlayerToAI_ClientRpc(userID, randomNameOfAI);
                    }
                }
            }
        }

        [ClientRpc]
        public void ChangePlayerToAI_ClientRpc(string userID, string name)
        {
            if (!IsHost)
            {
                foreach (Player player in BallManager.Instance.players)
                {
                    if (player.playerID == userID)
                    {
                        player.InitPlayer(true, name);

                    }
                }
            }
        }
    }
}