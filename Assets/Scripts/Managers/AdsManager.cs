using System;
using UnityEngine;

namespace BlockYourFriends.Ads
{
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager _instance;
        public static AdsManager Instance { get { return _instance; } }

        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;

        private int adCounter = 0;

        private bool adsRemoved;

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
        }

        private void Start()
        {
            EconomyManager.Instance.InventoryChecked += CheckIfAdsRemoved;
        }

        public bool AdsRemoved
        {
            get { return adsRemoved; }
            set { adsRemoved = value; }
        }

        public void CheckIfAdsRemoved()
        {
            if (adsRemoved)
            {
                //Debug.Log("Ads removed");
                return;
            }

            else InitializeAds();
        }

        private void InitializeAds()
        {
            //Debug.Log("Init ads");

            interstitialAd = GetComponent<InterstitialAd>();
            rewardedAd = GetComponent<RewardedAd>();

            interstitialAd.Init();
            rewardedAd.Init();
        }

        public int GetAdCounter()
        {
            if (adCounter % 2 == 0)
            {
                adCounter++;
                return 0;
            }

            else
            {
                adCounter++;
                return 1;
            }
        }

        public void ShowInterstitialAd()
        {
            if (interstitialAd != null)
                interstitialAd.ShowInterstitialAd();
        }

        public void ShowRewardedAd()
        {
            if (rewardedAd != null)
                rewardedAd.ShowRewardedAd();
        }
    }
}