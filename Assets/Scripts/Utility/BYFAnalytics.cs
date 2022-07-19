using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using BlockYourFriends.Gameplay;

namespace BlockYourFriends.Utility
{
    //This singleton manages the Analytics calls to Unity Analytics 2.0 in one universally accessible class
    //Custom Events are used for data visualization on the Unity Dashboard

    public class BYFAnalytics : Singleton<BYFAnalytics>
    {
        private int storedGameLength = 5;
 
        public void ReportBricksDestroyed(string levelName, int maxBricks, float percentageDestroyed, float levelTimer)
        {
            Dictionary<string, object> blocksDestroyedParams = new Dictionary<string, object>()
            {
                {"levelName" , levelName},
                {"maxBricks" , maxBricks},
                {"percentageDestroyed" , percentageDestroyed},
                {"levelTimeLimit", levelTimer},

            };
            AnalyticsService.Instance.CustomData("blocksDestroyedTotal", blocksDestroyedParams);
        }

        public void ReportLevelEnd(string levelName, string levelAdvanceReason)
        {
            Dictionary<string, object> levelCompleteParams = new Dictionary<string, object>()
            {
                {"levelName" , levelName},
                {"levelAdvanceReason", levelAdvanceReason },

            };
            AnalyticsService.Instance.CustomData("level_complete", levelCompleteParams);
        }

        public void EndOfGameReporting(int gameLength)
        { //complete the end of game reporting for Analytics
            storedGameLength = gameLength;
            if (ScoreManager.Instance.GetScores() == null)
            {
                Debug.LogError("No score manager available");
                return;
            }

            Dictionary<PaddleLocation, int> scores = ScoreManager.Instance.GetScores();
            int p1Score = scores[PaddleLocation.bottom];
            int p2Score = scores[PaddleLocation.top];
            int p3Score = scores[PaddleLocation.right];
            int p4Score = scores[PaddleLocation.left];

            int aiScore = 0;
            int playerScore = 0;
            int aiCount = 0;
            int playerCount = 0;

            PlayerManager.Instance.GetPlayerInfo();
            foreach (KeyValuePair<PaddleLocation, int> score in scores)
            {
                if (PlayerManager.Instance.GetPlayer(score.Key)==null)
                {
                    Debug.Log("No player data for" + score.Key.ToString());
                }
                else if (PlayerManager.Instance.GetPlayer(score.Key).IsAI())
                {
                    aiScore += scores[score.Key];
                    aiCount++;
                }
                else
                {
                    playerScore += scores[score.Key];
                    playerCount++;
                }
            }
            PlayerManager.Instance.ResetPlayerInfo();

            if (aiCount!=0 && playerCount!=0)
            {
                //only relevant if there is at least one Player and one AI... Should always be a player though :)
                float averagePlayerScore = playerScore / playerCount;
                float averageAIScore = aiScore / aiCount;
                ReportAIVSPlayerScoreSpread(gameLength, averagePlayerScore, averageAIScore);
            }
            else
            {
                Debug.Log("No AI/Player comp reported. AI Count:" + aiCount + "\n Player Count:" + playerCount);
            }

            ReportPositionScoreSpread(gameLength, p1Score, p2Score, p3Score, p4Score);
        }

        public void ReportPositionScoreSpread(int gameLength, int p1Score, int p2Score, int p3Score, int p4Score)
        {
            //Repot score spread based on position of paddle to check for imbalance based on position
            //We could optionally check if the player is the owner to avoid multiple reporting from the same game, however having the data replicated isn't a bad choice either
            //since each Human Player is having that same experience (thus it represents a unique, albeit the same, data set).
            //0 - Bottom, 1 - Top, 2 - Right, 3 - Left

            Dictionary<string, object> reportScoreSpreadParams = new Dictionary<string, object>()
            {
                {"gameLength" , gameLength},
                {"bottomPlayerScore",  p1Score},
                {"topPlayerScore", p2Score},
                {"rightPlayerScore", p3Score},
                {"leftPlayerScore", p4Score},
            };
            AnalyticsService.Instance.CustomData("report_position_score_spread", reportScoreSpreadParams);
        }

        public void ReportStoreActivity(float timeOpen, string purchasedItem, string purchaseType)
        {
            Dictionary<string, object> reportStoreActivityParams = new Dictionary<string, object>()
            {
                {"timeOpen" , timeOpen},
                {"purchasedItem", purchasedItem },
                {"purchaseType", purchaseType },
            };
            AnalyticsService.Instance.CustomData("report_store_activity", reportStoreActivityParams);
        }

        public void RewardedProvided(int baseCoins)
        {
            Dictionary<string, object> reportStoreActivityParams = new Dictionary<string, object>()
            {
                {"coinBalance", baseCoins }
            };
            AnalyticsService.Instance.CustomData("rewardProvided", reportStoreActivityParams);
        }

        public void ReportCoinsEarned(int coinValue)
        {
            Dictionary<string, object> reportCoinsEarnedParams = new Dictionary<string, object>()
            {
                {"coinsEarned" , coinValue},
                {"gameLength", storedGameLength },
            };
            Debug.Log("Reporting Coins Earned:" + coinValue + " at level length:" + storedGameLength);
            AnalyticsService.Instance.CustomData("report_coins_earned", reportCoinsEarnedParams);
        }

        public void ReportAIVSPlayerScoreSpread(int gameLength, float averagePlayerScore, float averageAIScore)
        {
            //Report AI versus Human player scores at the end of a game (balance testing)
            Dictionary<string, object> reportScoreSpreadParams = new Dictionary<string, object>()
            {
                {"gameLength" , gameLength},
                {"aiAverageScore", averageAIScore},
                {"playerAverageScore", averagePlayerScore},
            };
            AnalyticsService.Instance.CustomData("report_AI_vs_Player_Score_Spread", reportScoreSpreadParams);
        }

        public void ReportBallSpawned()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() //Track total number of balls spawned (with parameter to indicate balls in play at the time)
            {
                { "active_balls", BallManager.Instance.GetBallsInPlayCount()-1},
            };
            AnalyticsService.Instance.CustomData("ball_spawned", parameters);
        }
    }
}
