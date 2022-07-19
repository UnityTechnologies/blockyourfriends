using System;
using UnityEngine;
using Unity.Services.Mediation;

namespace BlockYourFriends.Ads
{
    public class InterstitialAd : MonoBehaviour
    {
        public string androidAdUnitId;
        public string IOSAdUnitId;

        private IInterstitialAd interstitialAd;

        public void Init()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                interstitialAd = MediationService.Instance.CreateInterstitialAd(androidAdUnitId);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                interstitialAd = MediationService.Instance.CreateInterstitialAd(IOSAdUnitId);
            }
            else if (Application.isEditor)
            {
                interstitialAd = MediationService.Instance.CreateInterstitialAd("ExampleAdUnitId");
            }

            // Subscribe callback methods to load events:
            interstitialAd.OnLoaded += InterstitialAdLoaded;
            interstitialAd.OnFailedLoad += InterstitialAdFailedToLoad;

            // Subscribe callback methods to show events:
            interstitialAd.OnShowed += InterstitialAdShown;
            interstitialAd.OnFailedShow += InterstitialAdFailedToShow;
            interstitialAd.OnClosed += InterstitialAdClosed;

            LoadInterstitialAd();
        }

        private void LoadInterstitialAd()
        {
            try
            {
                interstitialAd.LoadAsync();
            }
            catch (LoadFailedException ex)
            {
                Debug.Log("Interstitial ad failed to load: " + ex.Message);
            }
        }

        public void ShowInterstitialAd()
        {
            // Ensure the ad has loaded, then show it
            if (interstitialAd.AdState == AdState.Loaded)
            {
                try
                {
                    interstitialAd.ShowAsync();
                }
                catch (ShowFailedException ex)
                {
                    Debug.Log("Interstitial ad failed to show: " + ex.Message);
                }
            }
        }

        // Implement load event callback methods:
        private void InterstitialAdLoaded(object sender, EventArgs args)
        {
            //Debug.Log("Interstitial ad loaded");
            // Execute logic for when the ad has loaded
        }

        private void InterstitialAdFailedToLoad(object sender, LoadErrorEventArgs args)
        {
            //Debug.Log("Interstitial ad failed to load");
            // Execute logic for the ad failing to load
        }

        // Implement show event callback methods:
        private void InterstitialAdShown(object sender, EventArgs args)
        {

            //Debug.Log("Interstitial ad shown successfully");
            // Execute logic for the ad showing successfully
        }

        private void InterstitialAdFailedToShow(object sender, ShowErrorEventArgs args)
        {
            //Debug.Log("Interstitial ad failed to show");
            // Execute logic for the ad failing to show
        }

        private void InterstitialAdClosed(object sender, EventArgs e)
        {
            //Debug.Log("Interstitial ad has closed");
            // Execute logic after an ad has been closed
            LoadInterstitialAd();
        }
    }
}
