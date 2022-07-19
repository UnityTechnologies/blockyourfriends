using System;
using System.Collections;
using BlockYourFriends.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.ngo
{
    /// <summary>
    /// Once the local player is in a lobby and that lobby has entered the In-Game state, this will load in whatever is necessary to actually run the game part.
    /// This will exist in the game scene so that it can hold references to scene objects that spawned prefab instances will need.
    /// </summary>
    public class SetupInGame : MonoBehaviour, IReceiveMessages
    {
        [SerializeField] private GameObject prefabNetworkManager = default;
        [SerializeField] private GameObject prefabInGameLogic = default;
        [SerializeField] private GameObject[] disableWhileInGame = default;

        private GameObject inGameManagerObj;
        private NetworkManager networkManager;
        private InGameRunner inGameRunner;

        private bool doesNeedCleanup = false;
        private bool hasConnectedViaNGO = false;
        private bool isGameEnding = false;

        private Action<UnityTransport> initializeTransport;
        private LocalLobby lobby;
        private LobbyUser localUser;


        public void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
        }

        public void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }

        private void SetMenuVisibility(bool areVisible)
        {
            foreach (GameObject go in disableWhileInGame)
                go.SetActive(areVisible);
        }

        /// <summary>
        /// The prefab with the NetworkManager contains all of the assets and logic needed to set up the NGO minigame.
        /// The UnityTransport needs to also be set up with a new Allocation from Relay.
        /// </summary>
        private void CreateNetworkManager()
        {
            isGameEnding = false;
            inGameManagerObj = GameObject.Instantiate(prefabNetworkManager);
            networkManager = inGameManagerObj.GetComponentInChildren<NetworkManager>();
            inGameRunner = GameObject.Instantiate(prefabInGameLogic).GetComponentInChildren<InGameRunner>();
            inGameRunner.Initialize(OnConnectionVerified, lobby.PlayerCount, OnGameEnd, localUser, lobby);

            UnityTransport transport = inGameManagerObj.GetComponentInChildren<UnityTransport>();
            if (localUser.IsHost)
                inGameManagerObj.AddComponent<RelayUtpNGOSetupHost>().Initialize(this, lobby, () => { initializeTransport(transport); networkManager.StartHost(); });
            else
                inGameManagerObj.AddComponent<RelayUtpNGOSetupClient>().Initialize(this, lobby, () => { initializeTransport(transport); networkManager.StartClient(); });
        }

        private void OnConnectionVerified()
        {
            hasConnectedViaNGO = true;
        }

        // These are public for use in the Inspector.
        public void OnLobbyChange(LocalLobby lobby)
        {
            this.lobby = lobby; // Most of the time this is redundant, but we need to get multiple members of the lobby to the Relay setup components, so might as well just hold onto the whole thing.
        }

        public void OnLocalUserChange(LobbyUser user)
        {
            localUser = user; // Same, regarding redundancy.
        }

        /// <summary>
        /// Once the Relay Allocation is created, this passes its data to the UnityTransport.
        /// </summary>
        public void SetRelayServerData(string address, int port, byte[] allocationBytes, byte[] key, byte[] connectionData, byte[] hostConnectionData, bool isSecure)
        {
            initializeTransport = (transport) => { transport.SetRelayServerData(address, (ushort)port, allocationBytes, key, connectionData, hostConnectionData, isSecure); };
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.ConfirmInGameState)
            {
                doesNeedCleanup = true;
                SetMenuVisibility(false);
                CreateNetworkManager();
            }

            else if (type == MessageType.MinigameBeginning)
            {
                if (!hasConnectedViaNGO)
                {
                    // If this player hasn't successfully connected via NGO, forcibly exit the minigame.
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Failed to join the game.");
                    OnGameEnd();
                }
            }

            else if (type == MessageType.ChangeGameState)
            {
                // Once we're in-game, any state change reflects the player leaving the game, so we should clean up.
                OnGameEnd();
            }
        }

        /// <summary>
        /// Return to the lobby after the game, whether due to the game ending or due to a failed connection.
        /// </summary>
        private void OnGameEnd()
        {
            if (isGameEnding)
                return;

            isGameEnding = true;

            if (doesNeedCleanup)
            {
                if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && inGameRunner != null)
                    inGameRunner.ChangePlayerToAI_ServerRpc(localUser.ID);

                StartCoroutine(DoTheRest());

                IEnumerator DoTheRest()
                {
                    yield return null;
                    GameObject.Destroy(inGameManagerObj); // Since this destroys the NetworkManager, that will kick off cleaning up networked objects.
                    GameObject.Destroy(inGameRunner.transform.parent.gameObject);
                    GameObject.Destroy(BallManager.Instance.gameObject);
                    ExplosionManager.Instance.ResetPlayers();
                    ScoreManager.Instance.ResetScores();
                    SetMenuVisibility(true);
                    lobby.RelayNGOCode = null;
                    doesNeedCleanup = false;
                    PlayerManager.Instance.ResetView();
                    MusicManager.Instance.StopPlayingMusic();

                    yield return new WaitForSeconds(1);
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
                }
            }
        }

        public void InterruptGame()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer)
            {
                doesNeedCleanup = true;
                OnGameEnd();
            }

            GameManager.Instance.GameInterrupted();
            GameManager.Instance.EndGame();

            GameObject[] currentBrickSets = GameObject.FindGameObjectsWithTag("BrickSet");
            foreach (GameObject brickSet in currentBrickSets)
            {
                Destroy(brickSet);
            }

            GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject powerUp in powerUps)
            {
                Destroy(powerUp);
            }
        }
    }
}
