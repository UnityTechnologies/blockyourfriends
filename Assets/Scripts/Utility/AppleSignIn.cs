using System;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BlockYourFriends.Gameplay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


#if UNITY_IOS

namespace BlockYourFriends.Utility
{
    //Apple related functions for authenticaiton.
    //github.com/lupidan/apple-signin-unity for more information on AppleAuth library and functionality for the Apple Auth plugin

    public class AppleSignIn : Singleton<AppleSignIn>
    {
        private const string AppleUserIdKeyRef = "AppleUserIdKey";

        private IAppleAuthManager appleAuthManager;

        //Callback to give requesting function information about success/failure of sign on or linking
        public Action<bool, string> AppleSignOnComplete;

        public AppleSignIn()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                this.appleAuthManager = new AppleAuthManager(deserializer);
            }
        }

        private void DoAppleSignIn()
        {
            var loginArgs = new AppleAuthLoginArgs();
            this.appleAuthManager.LoginWithAppleId(loginArgs, credential =>
            {
                var appleIDCredential = credential as IAppleIDCredential;
                if (appleIDCredential != null)
                {
                    var userID = appleIDCredential.User;
                    PlayerPrefs.SetString(AppleUserIdKeyRef, userID);
                    var identityToken = Encoding.UTF8.GetString(appleIDCredential.AuthorizationCode, 0, appleIDCredential.AuthorizationCode.Length);
                    var authorizationCode = Encoding.UTF8.GetString(appleIDCredential.AuthorizationCode, 0, appleIDCredential.AuthorizationCode.Length);
                }
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
            }
            );
        }

        public void DoAppleLogin(AuthMode authMode)
        {
            DoAppleSignIn();
        }

        public async Task LinkWithAppleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.LinkWithAppleAsync(idToken);
                Debug.Log("Link is successful.");
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // Prompt the player with an error message.
                Debug.LogError("This user is already linked with another account. Log in instead.");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        public async Task SignInWithAppleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(idToken);
                Debug.Log("SignIn is successful.");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        private void Update()
        {
            if (this.appleAuthManager != null)
            {
                this.appleAuthManager.Update();
            }
        }
    }
}

#endif