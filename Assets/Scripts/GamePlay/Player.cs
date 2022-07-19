using UnityEngine;
using BlockYourFriends.Multiplayer;
using BlockYourFriends.Multiplayer.ngo;

namespace BlockYourFriends.Gameplay
{
    public class Player : MonoBehaviour
    {
        public string playerName;
        public bool isAI;
        public string playerID;
        public bool isCurrentPlayer;
        private Color playerColor;
        private int playerScore;
        private PaddleSwitch paddleSwitch;

        [Tooltip("Paddle Position of this paddle")]
        public PaddleLocation paddlePos;

        [SerializeField] private string playerDefaultName;
        [SerializeField] private NetworkScorer networkScorer;
        [SerializeField] private NetworkSkinUpdater networkSkinUpdater;

        private PaddleControl paddleControl;
        private AIPaddleControl aiPaddleControl;

        public PaddleControl PaddleControl => paddleControl;

        public void InitPlayer(string name, Color paddleColor, bool isAI)
        {
            playerName = name;
            playerColor = paddleColor;
		    transform.GetChild(0).GetComponent<SpriteRenderer>().color = playerColor;
            this.isAI = isAI;
            SetupPaddleControl();
        }

        public void InitPlayer(string name, string playerID, bool isAI, bool isCurrentPlayer = false)
        {
            playerName = name;
            this.playerID = playerID;
            this.isAI = isAI;
            this.isCurrentPlayer = isCurrentPlayer;
            SetupPaddleControl();
            SetupScoresUI();

            if (isCurrentPlayer)
            {
                PlayerManager.Instance.SetupView(paddlePos);
                PlayerManager.Instance.SetCurrentPlayer(this);
                SetPaddle();
            }
        }

        public string InitPlayer(bool isAI)
        {
            playerName = NameGenerator.GetRandomNameForAI();
            networkScorer.SetPlayerName(paddlePos, playerName);
            this.isAI = isAI;
            SetupPaddleControl();
            SetupScoresUI();

            return playerName;
        }

        public void InitPlayer(bool isAI, string name)
        {
            playerName = name;
            this.isAI = isAI;
            SetupPaddleControl();
            SetupScoresUI();
        }

        public void InitNetworkPlayer()
        {
            playerName = playerDefaultName;
            SetupPaddleControl();
            SetupScoresUI();
        }

        private void Awake()
        {
            paddleControl = GetComponent<PaddleControl>();
            aiPaddleControl = GetComponent<AIPaddleControl>();
            SetupPaddleControl();
        }

        private void SetupPaddleControl()
        {
            if (paddleControl != null)
            {
                paddleControl.enabled = !isAI;
                if (isAI)
                    paddleControl.DisableBallLauncher();
            }

            if (aiPaddleControl != null)
                aiPaddleControl.enabled = isAI;
        }

        private void DisablePaddleControls()
        {
            if (paddleControl != null)
            {
                paddleControl.enabled = false;
                paddleControl.DisableBallLauncher();
            }

            if (aiPaddleControl != null)
                aiPaddleControl.enabled = false;
        }

        public Color GetPlayerColor()
        {
            return playerColor;
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public bool IsAI()
        {
            return isAI;
        }

        private void SetupScoresUI()
        {
            ScoreManager.Instance.SetPlayerName(paddlePos, playerName);
        }

        public void AddScore(int score)
        {
            networkScorer.AddScore(paddlePos, score);
            if (isCurrentPlayer)
                CurrentPlayerScore(score);
        }

        private void CurrentPlayerScore(int score)
        {
            playerScore += score;
        }

        public int GetCurrentPlayerScore()
        {
            return playerScore;
        }

        public void SetPaddle()
        {
            PlayerItem activePaddle = ShopManager.Instance.ActivePaddle;
            networkSkinUpdater.OnSkinUpdated(paddlePos, activePaddle);
            SetPaddle(activePaddle);
        }

        public void SetPaddle(PlayerItem activePaddle)
        {
            paddleSwitch = GetComponent<PaddleSwitch>();

            if (activePaddle == PlayerItem.BasePaddle)
                BasePaddle();
            else if (activePaddle == PlayerItem.BronzePaddle)
                BronzePaddle();
            else if (activePaddle == PlayerItem.SilverPaddle)
                SilverPaddle();
            else if (activePaddle == PlayerItem.GoldPaddle)
                GoldPaddle();
        }

        public void BasePaddle()
        {
            paddleSwitch.BasePaddle();
        }

        public void BronzePaddle()
        {
            paddleSwitch.BronzePaddle();
        }

        public void SilverPaddle()
        {
            paddleSwitch.SilverPaddle();
        }

        public void GoldPaddle()
        {
            paddleSwitch.GoldPaddle();
        }
    }
}
