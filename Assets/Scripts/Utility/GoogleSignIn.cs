#if UNITY_ANDROID
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using BlockYourFriends.Gameplay;
using System;

namespace BlockYourFriends.Utility
{
    //Google related functions for Authentication
    //NOTE As of April 19, 2022 not yet functional (Google Auth returns Developer Error)

    public class GoogleSignIn : Singleton<GoogleSignIn>
    { 

        public Action<bool, string> GoogleSignOnComplete;
        public Action<bool, string> GoogleLinkComplete;
        private string authCode = null;
        private AuthMode authMode = AuthMode.none;

        public void InitializePlayGamesLogin()
        {
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
        }

        public void DoGoogleLogin(AuthMode authModeToUse)
        {
            authMode = authModeToUse;
            InitializePlayGamesLogin();
            LoginGooglePlayGames();
        }

        public void LoginGooglePlayGames()
        {
            Social.localUser.Authenticate(OnGooglePlayGamesLogin);
        }

        void OnGooglePlayGamesLogin(bool success, string reason)
        {
            if (success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, SetGoogleAuthCode);
            }
            else
            {
                GoogleSignOnComplete?.Invoke(false, "GPGS Sign on failed: " + reason);
            }
                    
        }

        async internal void SetGoogleAuthCode(string newAuthCode)
        {
            authCode = newAuthCode;
            if (authMode == AuthMode.login)
            {
                try
                {
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(newAuthCode);
                    GoogleSignOnComplete?.Invoke(true, "Successful Auth Sign in with GPGS");
                }
                catch (Exception ex)
                {
                    string errorMessage = "Could not sign in with GPGS:" + ex.Message;
                    GoogleSignOnComplete?.Invoke(false, errorMessage);
                    Debug.LogError(errorMessage);
                }
            }
            else if (authMode == AuthMode.link)
            {
                try
                {
                    await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(newAuthCode);
                    GoogleLinkComplete?.Invoke(true, "Succesffully linked Auth to GPGS");
                }
                catch (Exception ex)
                {
                    string errorMessage = "Could not link Auth to GPGS" + ex.Message;
                    GoogleLinkComplete?.Invoke(false, errorMessage);
                    Debug.LogError(errorMessage);
                }
            }
            else
            {
                Debug.Log("Google Sign on reuqested without Authenticate");
            }
        }
    }
}

#endif
