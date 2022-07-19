using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BlockYourFriends.Multiplayer.Auth;
using Unity.Services.Authentication;
using System;


namespace BlockYourFriends.Utility
{
    public class ServicesConnectStatus : MonoBehaviour
    {
        //This class controls the display of the connection status on the main menu UI
        //The environment label DEV is applied for development environment to highlight that the build is a development build
        //Once the Authentication Service is intiialized, User Info is queried to see if any external IDs exist, if so then there is
        //data for Apple or Google and we have a valid live service connection

        private string environment = "Prod";
        public TMP_Text environmentLabel;
        public Sprite cloudConnectedSprite;
        public Sprite cloudDisconnectedSprite;
        public Image cloudImage;

        private void Start()
        {
            environmentLabel.gameObject.SetActive(false);
#if Dev
            environment = "Dev";
#endif
#if Prod
            environment = "";
       
#endif
            environmentLabel.text = environment;
        }

        private void OnEnable()
        {
            SubIdentity_Authentication.Initialized += SetConnectionStatus;
        }

        private void OnDisable()
        {
            SubIdentity_Authentication.Initialized -= SetConnectionStatus;
        }

        private async void SetConnectionStatus()
        {
            try
            {
                PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
                if (playerInfo.Identities == null)
                {
                    cloudImage.sprite = cloudDisconnectedSprite;
                }
                else
                {
                    cloudImage.sprite = cloudConnectedSprite;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error getting userinfo: " + e.Message);
            }
        }
    }
}


