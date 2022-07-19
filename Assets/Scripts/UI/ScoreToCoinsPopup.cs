using UnityEngine;
using TMPro;
using BlockYourFriends.Gameplay;
using BlockYourFriends.Ads;
using System.Threading.Tasks;
using BlockYourFriends.Utility;
using BlockYourFriends.Multiplayer;

namespace BlockYourFriends.UI
{
    public class ScoreToCoinsPopup : MonoBehaviour
    {
        [SerializeField]
        private GameObject
            rewardedAdButton,
            interstitialAdButton,
            continueButton;

        [SerializeField]
        private TextMeshProUGUI
            scoreText,
            coinsText;

        private int score = 0;
        private int oldCoins = 0;
        private int newCoins = 0;
        private int pointsPerCoin = 50;

        private enum ButtonToShow { InterstitialAdButton, RewardedAdButton, ContinueButton }
        private ButtonToShow buttonToShow;

        private void Awake()
        {
            HideButtons();
        }

        public void OnEndGame()
        {
            GetScore();
            SetUI();
        }

        private void GetScore()
        {
            score = PlayerManager.Instance.GetCurrentPlayer().GetCurrentPlayerScore();
        }

        private async void SetUI()
        {
            var adsRemoved = AdsManager.Instance.AdsRemoved;

            //if (adsRemoved)
            //    Debug.Log("Ads removed");
            //else Debug.Log("Ads not removed");

            if (score > 0)
            {
                if (adsRemoved)
                    buttonToShow = ButtonToShow.ContinueButton;

                else
                {
                    var adCounter = AdsManager.Instance.GetAdCounter();

                    if (adCounter == 0)
                        buttonToShow = ButtonToShow.InterstitialAdButton;

                    else if (adCounter == 1)
                        buttonToShow = ButtonToShow.RewardedAdButton;
                }

                await Task.Delay(200);
                await ScoreToCoins();
            }
            else
            {
                if (adsRemoved)
                    buttonToShow = ButtonToShow.ContinueButton;

                else buttonToShow = ButtonToShow.InterstitialAdButton;
            }

            ShowButton();
        }

        private async Task ScoreToCoins()
        {
            var s = 0;
            var c = 0;
            var increment = score / 50;

            while (s < score)
            {
                s += increment;
                scoreText.text = s.ToString();

                c += increment;

                while (c >= pointsPerCoin)
                {
                    newCoins++;
                    coinsText.text = newCoins.ToString();
                    c -= pointsPerCoin;
                }

                await Task.Yield();
            }
        }

        private void ShowButton()
        {
            switch (buttonToShow)
            {
                case ButtonToShow.InterstitialAdButton:
                    interstitialAdButton.SetActive(true);
                    break;
                case ButtonToShow.RewardedAdButton:
                    rewardedAdButton.SetActive(true);
                    continueButton.SetActive(true);
                    break;
                case ButtonToShow.ContinueButton:
                    continueButton.SetActive(true);
                    break;
            }
        }

        public async void OnUserRewarded()
        {
            rewardedAdButton.SetActive(false);

            await Task.Delay(200);
            await DoubleCoins();

            coinsText.color = Color.green;
            // todo play sound
        }

        private async Task DoubleCoins()
        {
            oldCoins = newCoins;
            newCoins *= 2;

            while (oldCoins < newCoins)
            {
                oldCoins++;
                coinsText.text = oldCoins.ToString();
                await Task.Yield();
            }
            BYFAnalytics.Instance.RewardedProvided(oldCoins);
        }

        public void SaveAndReset()
        {
            ShopManager.Instance.AddCoins(newCoins);
            BYFAnalytics.Instance.ReportCoinsEarned(newCoins);
            newCoins = 0;
            scoreText.text = "0";
            coinsText.text = "0";
            coinsText.color = Color.white;

            HideButtons();
        }

        private void HideButtons()
        {
            rewardedAdButton.SetActive(false);
            interstitialAdButton.SetActive(false);
            continueButton.SetActive(false);
        }
    }
}
