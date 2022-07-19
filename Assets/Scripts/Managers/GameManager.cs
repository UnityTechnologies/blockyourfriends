using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends;
using TMPro;
using BlockYourFriends.Utility;
using System;
using BlockYourFriends.UI;
using BlockYourFriends.Multiplayer;
using BlockYourFriends.Ads;
using BlockYourFriends.Gameplay;

namespace BlockYourFriends
{
    // The GameManager handles the overal game setup and game related events (Start/End game, Any events on level changes such as freezing the player

    public enum GameplayMode
    {
        SinglePlayer,
        MultiPlayer
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameObject gameLogicPrefab;
        [SerializeField] private GameObject[] uiToHideInSingleplayer;

        public GameplayMode currentGameplayMode { get; private set; }

        [HideInInspector]
        public string singlePlayerName = "Single Player";

        private GameObject gameLogic;

        private bool gameInterrupted;

        public void OnGameTypeSelected(int playMode)
        {
            currentGameplayMode = (GameplayMode)playMode;

            if (currentGameplayMode == GameplayMode.SinglePlayer)
            {
                SetupSinglePlayer();
            }
        }

        public void ActivateAIPaddles() //send a message to the paddles to increase ball count... this will activate the AI. Not super relevant for the same use later on but for the demo it's needed :)
        {
            AIPaddleControl[] aiPaddles = FindObjectsOfType<AIPaddleControl>();
            foreach (AIPaddleControl aiPaddle in aiPaddles)
            {
                aiPaddle.IncreaseBallCount();
            }
        }

        private void SetupSinglePlayer()
        {
            foreach (GameObject objectToHide in uiToHideInSingleplayer)
            {
                objectToHide.SetActive(false);
            }
            gameLogic = Instantiate(gameLogicPrefab);
        }

        public void StartGame()
        {
            //Starting music here, might be a better place for this via UI control :)


            //Unfreeze inputs as level starts
            PaddleControl[] paddles = FindObjectsOfType<PaddleControl>();
            AIPaddleControl[] aiControllers = FindObjectsOfType<AIPaddleControl>();
            if (paddles.Length <= 0)
            {
                Debug.LogError("No players");
            }
            else
            {
                foreach (PaddleControl paddle in paddles)
                {
                    if (paddle.enabled)
                    {
                        //paddle.ResetPlayerPosition();
                        paddle.UnFreezePlayerInput();
                    }
                }
            }
            if (aiControllers.Length <= 0)
            {
                Debug.Log("No AI Controllers");
            }
            else
            {
                foreach (AIPaddleControl aiControl in aiControllers)
                {
                    if (aiControl.enabled)
                    {
                        //aiControl.ResetAIPosition();
                        aiControl.UnFreezeAIMovement();
                    }
                }
            }
        }

        public void AdvanceLevel()
        // Freeze inputs and clear balls in preparation for the next level
        {
            PaddleControl[] paddles = FindObjectsOfType<PaddleControl>();
            AIPaddleControl[] aiControllers = FindObjectsOfType<AIPaddleControl>();

            if (paddles.Length <= 0)
            {
                Debug.LogError("No players");
            }
            else
            {
                foreach (PaddleControl paddle in paddles)
                {
                    if (paddle.enabled)
                        paddle.FreezePlayerInput();
                }
            }

            if (aiControllers.Length <= 0)
            {
                Debug.Log("No AI Controllers");
            }
            else
            {
                foreach (AIPaddleControl aiControl in aiControllers)
                {
                    if (aiControl.enabled)
                        aiControl.FreezeAIMovement();
                }
            }

            Ball[] balls = FindObjectsOfType<Ball>();
            if (balls.Length <= 0)
            {
                //Debug.Log("No balls, sad cat");
            }
            else
            {
                foreach (Ball ball in balls)
                {
                    ball.DestroyBall(true);
                }
            }
        }

        public void GameInterrupted()
        {
            gameInterrupted = true;
        }

        public void EndGame()
        {
            UIManager.Instance.ShowMainMenu();
            if (!gameInterrupted)
                UIManager.Instance.ShowScoreToCoinsPopup();
            else gameInterrupted = false;
            ScoreManager.Instance.ResetScores();
            MusicManager.Instance.StopPlayingMusic();
            LevelAdvanceUI.Instance.DisableTransitionUI();

            if (currentGameplayMode == GameplayMode.SinglePlayer)
            {
                foreach (GameObject objectToShow in uiToHideInSingleplayer)
                {
                    objectToShow.SetActive(true);
                }
                Destroy(gameLogic);
            }
            else
            {
                //Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.Menu);
            }
        }

        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
