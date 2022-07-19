using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Utility;
using BlockYourFriends.Gameplay;


//This class will manage the spawning of balls, both the overal limits of balls in play as well who has the current spawn rights to new balls

namespace BlockYourFriends
{
    public class BallManager : Singleton<BallManager>
    {
        //Default values
        private const int defaultMaxBallsInPlay = 2;
        private const float defaultBallHoldTimer = 5f;

        //time in seconds that a player can hold a ball
        private float ballHoldTimer = 5f;

        //SET PER LEVEL
        //Maximum balls allowed in the level
        private int maxBallsInPlay = 2;

        //track if the level is being cleaned up (don't assign new balls)
        private bool cleanup = false;

        //list of players in the game, all players (AI and human) have player script attached
        public Player[] players = new Player[4];

        //Count of balls in play
        public int ballsInPlayCount;

        //Ball Spawn Speed (can be adjusted if desired by level manager)
        private float ballSpawnSpeed = 10f;

        //Ball Spawn Distance (from paddle) factor of unit vector
        private float ballSpawnDistance = 2f;

        #region Basic getters and setters
        public void SetBallHoldTimer(float newTimer)
        {
            ballHoldTimer = newTimer;
        }

        public float GetBallHoldTimer()
        {
            return ballHoldTimer;
        }

        public void SetMaxBallsInPlay(int newMax)
        {
            maxBallsInPlay = newMax;
        }

        public int GetMaxBallsInPlay()
        {
            return maxBallsInPlay;
        }

        public void BallSpawned()
        {
            ballsInPlayCount++;
            BYFAnalytics.Instance.ReportBallSpawned();
        }
        #endregion

        private void Awake()
        {
            cleanInstance = true;
        }

        #region StartNewLevel (with overloads)
        public void StartNewLevel(int maxBalls, float holdTimer)
        {
            cleanup = true;
            ballsInPlayCount = 0;
            RemoveAllBallAllocations();
            SetMaxBallsInPlay(maxBalls);
            SetBallHoldTimer(holdTimer);
            RandomlyAssignStartingBalls();
            cleanup = false;
            //all balls have been cleaned up by the level advance function
        }

        public void StartNewLevel(float holdTimer)
        {
            cleanup = true;
            ballsInPlayCount = 0;
            RemoveAllBallAllocations();
            SetMaxBallsInPlay(defaultMaxBallsInPlay);
            SetBallHoldTimer(holdTimer);
            RandomlyAssignStartingBalls();
            cleanup = false;
            //all balls have been cleaned up by the level advance function
        }

        public void StartNewLevel(int maxBalls)
        {
            cleanup = true;
            ballsInPlayCount = 0;
            RemoveAllBallAllocations();
            SetMaxBallsInPlay(maxBalls);
            SetBallHoldTimer(defaultBallHoldTimer);
            RandomlyAssignStartingBalls();
            cleanup = false;
            //all balls have been cleaned up by the level advance function
        }

        public void StartNewLevel()
        {
            cleanup = true;
            ballsInPlayCount = 0;
            RemoveAllBallAllocations();
            SetMaxBallsInPlay(defaultMaxBallsInPlay);
            SetBallHoldTimer(defaultBallHoldTimer);
            RandomlyAssignStartingBalls();
            cleanup = false;
            //all balls have been cleaned up by the level advance function
        }
        #endregion

        public Vector2 GetStartingVelocity(PaddleLocation paddlePos)
        {
            if (paddlePos==PaddleLocation.top)
            {
                return new Vector2(0, -1) * ballSpawnSpeed;
            }
            else if (paddlePos == PaddleLocation.bottom)
            {
                return new Vector2(0, 1) * ballSpawnSpeed;
            }
            if (paddlePos == PaddleLocation.left)
            {
                return new Vector2(1, 0) * ballSpawnSpeed;
            }
            else //right side
            {
                return new Vector2(-1, 0) * ballSpawnSpeed;
            }
        }

        public Vector2 GetStartingPosition(PaddleLocation paddlePos)
        {
            if (paddlePos == PaddleLocation.top)
            {
                return new Vector2(0, -1) * ballSpawnDistance;
            }
            else if (paddlePos == PaddleLocation.bottom)
            {
                return new Vector2(0, 1) * ballSpawnDistance;
            }
            if (paddlePos == PaddleLocation.left)
            {
                return new Vector2(1, 0) * ballSpawnDistance;
            }
            else //right side
            {
                return new Vector2(-1, 0) * ballSpawnDistance;
            }
        }

        private void RemoveAllBallAllocations()
        {
            foreach (Player player in players)
            {
                if (player.IsAI())
                {
                    player.gameObject.GetComponent<AIPaddleControl>().SetBallsAvailable(0);
                }
                else //human player
                {
                    player.gameObject.GetComponent<PaddleControl>().SetBallsAvailable(0);
                }
            }
        }

        public int GetBallsInPlayCount()
        {
            return ballsInPlayCount;
        }

        private void RandomlyAssignStartingBalls()
        {
            for (int i =0; i <maxBallsInPlay; i++)
            {
                int playerToGiveBallTo = Random.Range(0, players.Length);
                if (players[playerToGiveBallTo].IsAI())
                {
                    players[playerToGiveBallTo].gameObject.GetComponent<AIPaddleControl>().AddBallAvailable();
                }
                else //human player
                {
                    players[playerToGiveBallTo].gameObject.GetComponent<PaddleControl>().AddBallAvailable();
                }
            }
        }

        private void RandomlyAssignNewBall()
        {
            int playerToGiveBallTo = Random.Range(0, players.Length);
            if (players[playerToGiveBallTo].IsAI())
            {
                players[playerToGiveBallTo].gameObject.GetComponent<AIPaddleControl>().AddBallAvailable();
            }
            else //human player
            {
                players[playerToGiveBallTo].gameObject.GetComponent<PaddleControl>().AddBallAvailable();
            }
        }

        public void RemoveBall() // refactor (useless extra call)
        {
            ballsInPlayCount--;
            UpdateAIPanelBallCounter();
        }

        private void UpdateAIPanelBallCounter()
        {
            AIPaddleControl[] aiPaddles = FindObjectsOfType<AIPaddleControl>();
            foreach (AIPaddleControl aiPaddle in aiPaddles)
            {
                aiPaddle.OnBallCountChanged();
            }
        }

        public void RemoveBallInPlay()
        {
            if (!cleanup)
            {
                RemoveBall();
                //Debug.Log("Ball Removed");
                ballsInPlayCount--;
                RandomlyAssignNewBall();
            }
        }

    }
}
