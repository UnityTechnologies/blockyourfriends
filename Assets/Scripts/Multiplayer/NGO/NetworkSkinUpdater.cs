using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using BlockYourFriends.Gameplay;

namespace BlockYourFriends.Multiplayer.ngo
{
    public class NetworkSkinUpdater : NetworkBehaviour
    {
        [SerializeField] private Player[] players;

        public void OnSkinUpdated(PaddleLocation paddleLocation, PlayerItem activePaddle)
        {
            UpdateSkin_ServerRpc(paddleLocation, activePaddle);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateSkin_ServerRpc(PaddleLocation paddleLocation, PlayerItem activePaddle)
        {
            UpdateSkin_ClientRpc(paddleLocation, activePaddle);
        }

        [ClientRpc]
        private void UpdateSkin_ClientRpc(PaddleLocation paddleLocation, PlayerItem activePaddle)
        {
            players[(int)paddleLocation].SetPaddle(activePaddle);
        }
    }
}
