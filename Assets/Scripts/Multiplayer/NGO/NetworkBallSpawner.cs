using System.Collections;
using System.Collections.Generic;
using BlockYourFriends;
using BlockYourFriends.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.ngo
{
    public class NetworkBallSpawner : NetworkBehaviour
    {
        public void StartNewLevel()
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                BallManager.Instance.StartNewLevel();
            else if (IsHost)
                StartNewLevel_ServerRpc();
        }

        [ServerRpc]
        private void StartNewLevel_ServerRpc()
        {
            BallManager.Instance.StartNewLevel();
        }

    }
}
