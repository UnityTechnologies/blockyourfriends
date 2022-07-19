using System;
using System.Collections;
using System.Collections.Generic;
using BlockYourFriends.Gameplay;
using TMPro;
using UnityEngine;

namespace BlockYourFriends
{
    public class ScoreManager : MonoBehaviour
    {
        private static ScoreManager _instance;
        public static ScoreManager Instance { get { return _instance; } }

        [Header("Score Texts")]
        [SerializeField] private TMP_Text Player1ScoreText;
        [SerializeField] private TMP_Text Player2ScoreText;
        [SerializeField] private TMP_Text Player3ScoreText;
        [SerializeField] private TMP_Text Player4ScoreText;

        [Header("Player Name Texts")]
        [SerializeField] private TMP_Text Player1NameText;
        [SerializeField] private TMP_Text Player2NameText;
        [SerializeField] private TMP_Text Player3NameText;
        [SerializeField] private TMP_Text Player4NameText;

        private Dictionary<PaddleLocation, int> scores;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }

            InitScoresDictionary();
        }

        public Dictionary<PaddleLocation, int> GetScores()
        {
            return scores;
        }

        private void InitScoresDictionary()
        {
            scores = new Dictionary<PaddleLocation, int>();
            scores.Add(PaddleLocation.bottom, 0);
            scores.Add(PaddleLocation.top, 0);
            scores.Add(PaddleLocation.right, 0);
            scores.Add(PaddleLocation.left, 0);
        }

        public void ResetScores()
        {
            SetScore(PaddleLocation.bottom, 0);
            SetScore(PaddleLocation.top, 0);
            SetScore(PaddleLocation.right, 0);
            SetScore(PaddleLocation.left, 0);
        }

        public void AddScore(PaddleLocation paddleLocation, int score)
        {
            int currentScore = scores[paddleLocation];
            currentScore += score;
            SetScore(paddleLocation, currentScore);
        }

        private void SetScore(PaddleLocation paddleLocation, int score)
        {
            scores[paddleLocation] = score;
            UpdateUI(paddleLocation, score);
        }

        private void UpdateUI(PaddleLocation paddleLocation, int score)
        {
            if (paddleLocation == PaddleLocation.bottom)
                Player1ScoreText.text = score.ToString();
            else if (paddleLocation == PaddleLocation.top)
                Player2ScoreText.text = score.ToString();
            else if (paddleLocation == PaddleLocation.right)
                Player3ScoreText.text = score.ToString();
            else if (paddleLocation == PaddleLocation.left)
                Player4ScoreText.text = score.ToString();
        }

        public void SetPlayerName(PaddleLocation paddleLocation, string playerName)
        {
            if (paddleLocation == PaddleLocation.bottom)
                Player1NameText.text = playerName;
            else if (paddleLocation == PaddleLocation.top)
                Player2NameText.text = playerName;
            else if (paddleLocation == PaddleLocation.right)
                Player3NameText.text = playerName;
            else if (paddleLocation == PaddleLocation.left)
                Player4NameText.text = playerName;
        }
    }
}
