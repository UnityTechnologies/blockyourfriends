using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication.Models;
using Unity.Services.Authentication;
using System;
using Unity.Services.Core;
using BlockYourFriends.Multiplayer.Auth;
using BlockYourFriends.Gameplay;
using AppleAuth;

namespace BlockYourFriends.Utility
{
    //This class manages Unity Authentication to the Google and Apple Services via the Connect panel in the options menu
    //As of 04/22/2022 this functionality is a work in progress and is not fully operational
    //07/12/2022 - Updates to script to fit new approach as well as GPGS plugin (v11)

    public class ServicesConnect : MonoBehaviour
    {
        public TMP_Text mainText;
        [Tooltip("THe default text displayed for the connection status")]
        public string defaultText;
        [Tooltip("The Default connection state text (to be replaced when status is validated). This should correspond to the element of the text above that represents connection state")]
        public string defaultServiceState;
        [Tooltip("The Default Service Type text (to be replaced when status is validated). This should correspond to the element of the text above that represents service type")]
        public string defaultServiceType;

        private string googleToken;
        private string appleToken;

        private string appleServices = "Apple";
        private string googleServices = "Google Play Games";

        private string connectedText = "connected";
        private string disconnectedText = "disconnected";

        private LinkedState linkedState = LinkedState.FTUE;
        private const string linkedStateRef = "linkedState";

        [Header("References")]
        [Tooltip("Apple Game Center Icon")]
        public Sprite appleSprite;
        [Tooltip("Google Play Service Icon")]
        public Sprite googleSprite;
        [Tooltip("Icon to display when platofrm not supported")]
        public Sprite notSupported;
        [Tooltip("Reference to the sprite to be replaced")]
        public Image spriteRef;
        [Tooltip("Reference to the Error panel that will pop up if the service fails to connect")]
        public GameObject errorPanel;
        [Tooltip("Reference to the text of the error panel")]
        public TMP_Text errorText;

        public Button connectButton;

        public TMP_Text connectionButtonText;

        private void OnEnable()
        {
            mainText.text = defaultText;
            SubIdentity_Authentication.SignedIn += SetConnectionStatus;
            linkedState = (LinkedState) PlayerPrefs.GetInt(linkedStateRef, 0);
            
        }

        private void OnDisable()
        {
            SubIdentity_Authentication.SignedIn -= SetConnectionStatus;
        }

        public async void GoogleServiceConnectButton()
        //Effects the functionality of the service connection button depending if it is linked or not
        {
#if UNITY_ANDROID
            if (linkedState == LinkedState.linked)
            {
                try
                {
                    await AuthenticationService.Instance.UnlinkGooglePlayGamesAsync();
                    Debug.Log("Unlinked");
                    linkedState = LinkedState.unlinked;
                    PlayerPrefs.SetInt(linkedStateRef, 1);
                    SetConnectionStatus();
                }
                catch (AuthenticationException ex)
                {

                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
                catch (RequestFailedException ex)
                {
                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
                
            }
            else //Unlinked or FTUE
            {
                try
                {
                    GoogleSignIn.Instance.GoogleLinkComplete += GoogleLinkComplete;
                    GoogleSignIn.Instance.DoGoogleLogin(AuthMode.link);

                }
                catch (Exception ex)
                {

                    errorPanel.SetActive(true);
                    errorText.text = ("Error: " + ex.Message);
                }
            }
#endif
        }

        private void GoogleLinkComplete(bool success, string reason)
        {
#if UNITY_ANDROID
            if (success)
            {
                Debug.Log("Link is successful.");
                linkedState = LinkedState.linked;
                PlayerPrefs.SetInt(linkedStateRef, 2); //Set to Linked
                SetConnectionStatus();
            }
            else
            {
                errorPanel.SetActive(true);
                errorText.text = ("Link to Google Services Failed. Token:\n" + "\n Reason: " + reason);
            }
            GoogleSignIn.Instance.GoogleLinkComplete -= GoogleLinkComplete;
#endif
        }

 

        public async void AppleServiceConnectButton()
        {
#if UNITY_IOS
            if (linkedState==LinkedState.linked)
            {
                try
                {
                    await AuthenticationService.Instance.UnlinkAppleAsync();
                    Debug.Log("Unlinked");
                    SetConnectionStatus();
                }
                catch (AuthenticationException ex)
                {

                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
                catch (RequestFailedException ex)
                {
                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
            }
            else
            {
                try
                {
                   //Get Apple Token
                   //appleToken =

                }
                catch (Exception e)
                {
                    errorPanel.SetActive(true);
                    errorText.text = "Faled to do Apple Sign in" + e.Message;
                }

                try
                {
                    await AuthenticationService.Instance.LinkWithAppleAsync(appleToken);
                    Debug.Log("Link is successful.");
                    SetConnectionStatus();
                    
                }

                catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
                {
                    AuthenticationService.Instance.SignOut();
                    try
                    { // try to sign out anon and sign in using the existing account we are trying to link
                        await AuthenticationService.Instance.SignInWithAppleAsync(appleToken);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                        errorPanel.SetActive(true);
                        errorText.text = (": " + e.Message);
                    }
                }

                catch (AuthenticationException ex)
                {
                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
                catch (RequestFailedException ex)
                {
                    errorPanel.SetActive(true);
                    errorText.text = ("ErrorCode: " + ex.ErrorCode + " . Error: " + ex.Message);
                    Debug.LogException(ex);
                }
            }
#endif
        }

        public async void SetConnectionStatus()
        {
            PlayerInfo playerInfo = null;
            try
            {
                playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            }
            catch
            {
                errorPanel.SetActive(true);
                errorText.text = ("Error getting User data");

            }
            if (Application.platform == RuntimePlatform.Android || Application.isEditor)
            {
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(GoogleServiceConnectButton);
                googleToken = null;
                if (spriteRef != null && googleSprite != null)
                    spriteRef.sprite = googleSprite;
                defaultText = defaultText.Replace(defaultServiceType, googleServices);
                mainText.text = defaultText;
                if (playerInfo.GetGoogleId()!=null)
                {
                    defaultText = defaultText.Replace(defaultServiceState, connectedText);
                    mainText.text = defaultText;
                    connectionButtonText.text = "Unlink";
                    googleToken = playerInfo.GetGoogleId();
                }
                else
                {
                    linkedState = LinkedState.unlinked;
                    defaultText = defaultText.Replace(defaultServiceState, disconnectedText);
                    mainText.text = defaultText;
                    connectionButtonText.text = "Connect";
                }

            }
            else if (AppleAuthManager.IsPlatformSupported)
            {
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(AppleServiceConnectButton);
                string appleToken = null;
                if (spriteRef != null && appleSprite != null)
                    spriteRef.sprite = appleSprite;
                defaultText = defaultText.Replace(defaultServiceType, appleServices);
                if (playerInfo.GetAppleId() != null)
                {
                    linkedState = LinkedState.linked;
                    mainText.text.Replace(defaultServiceState, connectedText);
                    connectionButtonText.text = "Unlink";
                    appleToken = playerInfo.GetAppleId();
                }
                else
                {
                    mainText.text.Replace(defaultServiceState, disconnectedText);
                    connectionButtonText.text = "Connect";
                    
                    //Connect button triggers sign on process
                }
            }
            else
            {
                //this really only gets triggered in the Editor for now as all builds should be for Ios/Android
                if (spriteRef != null)
                {
                    spriteRef.sprite = notSupported;
                    mainText.text = "Not a supported Device";
                }

                try
                {
                    Debug.Log("should have user info");
                    
                    if (playerInfo == null)
                        Debug.Log("No user info");
                    if (playerInfo.Identities == null)
                        Debug.Log("No External IDs");
                   
                }
                catch (Exception e)
                {
                    Debug.Log("Failed");
                    Debug.Log("Problem:" + e.Message);
                }
            }
        }
    }
}
