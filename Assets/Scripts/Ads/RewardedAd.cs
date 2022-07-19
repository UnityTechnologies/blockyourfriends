using System;
using UnityEngine;
using Unity.Services.Mediation;

namespace BlockYourFriends.Ads
{
    public class RewardedAd : MonoBehaviour
    {
        public string androidAdUnitId;
        public string IOSAdUnitId;

        private IRewardedAd rewardedAd;

        public void Init()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                rewardedAd = MediationService.Instance.CreateRewardedAd(androidAdUnitId);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                rewardedAd = MediationService.Instance.CreateRewardedAd(IOSAdUnitId);
            }
            else if (Application.isEditor)
            {
                rewardedAd = MediationService.Instance.CreateRewardedAd("ExampleAdUnitId");
            }

            // Subscribe callback methods to load events:
            rewardedAd.OnLoaded += RewardedAdLoaded;
            rewardedAd.OnFailedLoad += RewardedAdFailedToLoad;

            // Subscribe callback methods to show events:
            rewardedAd.OnShowed += RewardedAdShown;
            rewardedAd.OnFailedShow += RewardedAdFailedToShow;
            rewardedAd.OnUserRewarded += UserRewarded;
            rewardedAd.OnClosed += RewardedAdClosed;

            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            try
            {
                rewardedAd.LoadAsync();
            }
            catch (LoadFailedException ex)
            {
                Debug.Log("Rewarded ad failed to load: " + ex.Message);
            }
        }

        public void ShowRewardedAd()
        {
            // Ensure the ad has loaded, then show it
            if (rewardedAd.AdState == AdState.Loaded)
            {
                try
                {
                    rewardedAd.ShowAsync();
                }
                catch (ShowFailedException ex)
                {
                    Debug.Log("Rewarded ad failed to show: " + ex.Message);
                }
            }
        }

        // Implement load event callback methods:
        private void RewardedAdLoaded(object sender, EventArgs args)
        {
            //Debug.Log("Rewarded ad loaded");
            // Execute logic for when the ad has loaded
        }

        private void RewardedAdFailedToLoad(object sender, LoadErrorEventArgs args)
        {
            //Debug.Log("Rewarded ad failed to load");
            // Execute logic for the ad failing to load
        }

        // Implement show event callback methods:
        private void RewardedAdShown(object sender, EventArgs args)
        {
            //Debug.Log("Rewarded ad shown successfully");
            // Execute logic for the ad showing successfully
        }

        private void UserRewarded(object sender, RewardEventArgs args)
        {
            //Debug.Log("Rewarded ad has rewarded user");
            // Execute logic for rewarding the user
            UIManager.Instance.OnUserRewarded();
        }

        private void RewardedAdFailedToShow(object sender, ShowErrorEventArgs args)
        {
            //Debug.Log("Rewarded ad failed to show");
            // Execute logic for the ad failing to show
        }

        private void RewardedAdClosed(object sender, EventArgs e)
        {
            //Debug.Log("Rewarded ad is closed");
            // Execute logic for the user closing the ad
            LoadRewardedAd();
        }
    }
}