using System.Collections;
using System.Collections.Generic;
using BlockYourFriends.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.ngo
{
    public class NetworkScorer : NetworkBehaviour
    {
        public void AddScore(PaddleLocation paddleLocation, int score)
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                AddScore_ClientRpc(paddleLocation, score);
            else if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                ScoreManager.Instance.AddScore(paddleLocation, score);
        }

        [ClientRpc]
        private void AddScore_ClientRpc(PaddleLocation paddleLocation, int score)
        {
            ScoreManager.Instance.AddScore(paddleLocation, score);
        }

        public void SetPlayerName(PaddleLocation paddleLocation, string playerName)
        {
            if (GameManager.Instance.currentGameplayMode == GameplayMode.MultiPlayer && IsHost)
                SetPlayerName_ClientRpc(paddleLocation, playerName);
            else if (GameManager.Instance.currentGameplayMode == GameplayMode.SinglePlayer)
                ScoreManager.Instance.SetPlayerName(paddleLocation, playerName);
        }

        [ClientRpc]
        private void SetPlayerName_ClientRpc(PaddleLocation paddleLocation, string playerName)
        {
            ScoreManager.Instance.SetPlayerName(paddleLocation, playerName);
        }
    }
}
