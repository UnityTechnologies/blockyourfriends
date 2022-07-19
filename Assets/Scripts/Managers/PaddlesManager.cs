using System.Collections;
using System.Collections.Generic;
using BlockYourFriends.Multiplayer;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using System.Security.Cryptography;
using System;
using System.Text;

namespace BlockYourFriends.Gameplay
{
    public class PaddlesManager : NetworkBehaviour
    {
        public LobbyUser localUser { get; private set; }
        public LocalLobby localLobby { get; private set; }
        public Vector2 screenSizeDif { get; private set; }

        private List<LobbyUser> m_LobbyUsersList;

        [SerializeField] private Player[] players;

        public void Initialize(LobbyUser user, LocalLobby lobby)
        {
            localUser = user;
            localLobby = lobby;

            m_LobbyUsersList = lobby.LobbyUsers.Values.ToList();
        }

        public void Initialize()
        {
            players[0].InitPlayer(GameManager.Instance.singlePlayerName, "", false, true);
            players[1].InitPlayer(true);
            players[2].InitPlayer(true);
            players[3].InitPlayer(true);

            BallManager.Instance.players = players;
        }

        public override void OnNetworkSpawn()
        {
            BallManager.Instance.players = players;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetupPlayers_ServerRpc(string player1ID, string player2ID, string player3ID, string player4ID)
        {
            SetupPlayers_ClientRpc(player1ID, player2ID, player3ID, player4ID);
        }

        [ClientRpc]
        public void SetupPlayers_ClientRpc(string player1ID, string player2ID, string player3ID, string player4ID)
        {
            string[] playerIDs = { player1ID, player2ID, player3ID, player4ID };

            for (int i = 0; i < playerIDs.Length; i++)
            {
                string playerID = playerIDs[i];

                if (!string.IsNullOrEmpty(playerID))
                    players[i].InitPlayer(GetDisplayName(playerID), playerID, false, playerID == localUser.ID);
                else
                {
                    if (IsHost)
                        players[i].InitPlayer(true);
                    else
                        players[i].InitNetworkPlayer();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendHostScreenSize_ServerRpc(Vector2 screenSize)
        {
            GetHostScreenSize_ClientRpc(screenSize);
        }

        [ClientRpc]
        public void GetHostScreenSize_ClientRpc(Vector2 screenSize)
        {
            GetHostScreenSize(screenSize);
        }

        public void GetHostScreenSize(Vector2 screenSize)
        {
            screenSizeDif = new Vector2(screenSize.x / Screen.width, screenSize.y / Screen.height);
        }

        private string GetDisplayName(string userID)
        {
            string displayName = string.Empty;
            foreach (LobbyUser lobbyUser in m_LobbyUsersList)
            {
                if (lobbyUser.ID == userID)
                {
                    displayName = lobbyUser.DisplayName;
                }
            }

            return displayName;
        }

        [ServerRpc(RequireOwnership = false)]
        public void BallSpawn_ServerRpc(int playerLocation)
        {
            BallSpawn_ClientRpc(playerLocation);
        }

        [ClientRpc]
        public void BallSpawn_ClientRpc(int playerLocation)
        {
            if (IsHost)
                players[playerLocation].PaddleControl.DoBallSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableBallLauncher_ServerRpc(int playerLocation)
        {
            EnableBallLauncher_ClientRpc(playerLocation);
        }

        [ClientRpc]
        public void EnableBallLauncher_ClientRpc(int playerLocation)
        {
            players[playerLocation].PaddleControl.DoEnableBallLauncher();
        }

        [ServerRpc(RequireOwnership = false)]
        public void DisableBallLauncher_ServerRpc(int playerLocation)
        {
            DisableBallLauncher_ClientRpc(playerLocation);
        }

        [ClientRpc]
        public void DisableBallLauncher_ClientRpc(int playerLocation)
        {
            players[playerLocation].PaddleControl.DoDisableBallLauncher();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveLeft_ServerRpc(int playerLocation)
        {
            players[playerLocation].PaddleControl.DoMoveLeft();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MoveRight_ServerRpc(int playerLocation)
        {
            players[playerLocation].PaddleControl.DoMoveRight();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MovePaddle_ServerRpc(int playerLocation, Vector2 moveDelta)
        {
            players[playerLocation].PaddleControl.DoMovePaddle(moveDelta);
        }
    }
}
