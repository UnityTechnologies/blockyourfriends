using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Analytics;
using System.Collections.Generic;
using UnityEngine;
using BlockYourFriends.Gameplay;
using BlockYourFriends.Utility;
using Unity.Services.Vivox;
using AppleAuth;

namespace BlockYourFriends.Multiplayer.Auth
{
    /// <summary>
    /// The Authentication package will sign in asynchronously and anonymously. When complete, we will need to store the generated ID.
    /// </summary>
    public class SubIdentity_Authentication : SubIdentity, IDisposable
    {
        private const string ServiceEnvironmentProduction = "production";
        private const string ServiceEnvironmentDevelopment = "gameplaytesting";

        public static Action Initialized;
        public static Action SignedIn;

        private LinkedState linkedState = LinkedState.FTUE; //default linked state is first time user
        private const string linkedStateRef = "linkedState"; //hold the player pref reference for linked state

        private string serviceEnvironment = ServiceEnvironmentDevelopment;

        private bool hasDisposed = false;

        /// <summary>
        /// This will kick off a login.
        /// </summary>
        public SubIdentity_Authentication(Action onSigninComplete = null)
        {
            DoSignIn();
            SignedIn += onSigninComplete;
        }

        ~SubIdentity_Authentication()
        {
            Dispose();
        }

        public void SetServiceEnvironment(string newEnvironment)
        {
            serviceEnvironment = newEnvironment;
        }

        public void Dispose()
        {
            if (!hasDisposed)
            {
                AuthenticationService.Instance.SignedIn -= OnSignInChange;
                AuthenticationService.Instance.SignedOut -= OnSignInChange;
                hasDisposed = true;
            }
        }

        private async void DoSignIn()
        {
            //set environment (set on build) for analytics/services
#if Dev
            serviceEnvironment = ServiceEnvironmentDevelopment;
#elif Prod
            serviceEnvironment = ServiceEnvironmentProduction;
#endif
            var options = new InitializationOptions();
            options.SetEnvironmentName(serviceEnvironment);

            //check if linked to Google/Apple and appropriate device
            await UnityServices.InitializeAsync(options);
            if (Initialized != null)
                Initialized();
            AuthenticationService.Instance.SignedIn += OnSignInChange;
            AuthenticationService.Instance.SignedOut += OnSignInChange;

            linkedState = (LinkedState) PlayerPrefs.GetInt(linkedStateRef, 0); //default First Time User if no value is set

            try
            {
                List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong with Analytics Initialization: " + e.Message);
            }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    //If we are a First time user or our account is not linked to a third party service (ex: GPGS/Apple/FB), we can login using the session token or create a new account
                    if (linkedState == LinkedState.FTUE || linkedState == LinkedState.unlinked)
                    {
                        try
                        {
                            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Don't sign out later, since that changes the anonymous token, which would prevent the player from exiting lobbies they're already in.
                            //Debug.Log("Sign in anonymously succeeded!");

                        }
                        catch (AuthenticationException ex)
                        {
                            //TODO: Add some type of UI popup for Auth failed scenarios (short popup to display error).
                            Debug.LogException(ex);
                        }
                        catch (RequestFailedException ex)
                        {

                            Debug.LogException(ex);
                        }
                        SignedIn?.Invoke();
                    }
                    else //linked
                    {
                        //If we are linked with a Platform we will try to log in with the appropriate service and Authenticate that way or fall back on the Session token which should exist. Messaging should be provided
                        //in this case.
                        if (Application.platform == RuntimePlatform.Android)
                        {
#if UNITY_ANDROID
                            GoogleSignIn.Instance.GoogleSignOnComplete += OnGoogleSignOnComplete;
                            GoogleSignIn.Instance.DoGoogleLogin(AuthMode.login); //Authenticate with GPGS
#endif
                        }
                        else if (AppleAuthManager.IsCurrentPlatformSupported) //Can use Apple Auth (IOS)
                        {
#if UNITY_IOS
                            AppleSignIn.Instance.AppleSignOnComplete += OnAppleSignOnComplete;
                            AppleSignIn.Instance.DoAppleLogin(AuthMode.login);
#endif
                        }
                        else
                        {
                            await AuthenticationService.Instance.SignInAnonymouslyAsync();
                            //TODO:need to give some feedback about platform not supported here
                        }
                    }

                }
            
        }

        private async void OnGoogleSignOnComplete(bool success, string message)
        {
#if UNITY_ANDROID
            if (success) //sign on success
            {
                //TODO: Message popup?
            }
            else
            {
                //TODO: Add message?
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    try
                    {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Don't sign out later, since that changes the anonymous token, which would prevent the player from exiting lobbies they're already in.
                        Debug.Log("Sign in anonymously succeeded!");

                    }
                    catch (AuthenticationException ex)
                    {
                        //TODO: Add some type of UI popup for Auth failed scenarios (short popup to display error).
                        Debug.LogException(ex);
                    }
                    catch (RequestFailedException ex)
                    {

                        Debug.LogException(ex);
                    }
                }
            }
            SignedIn?.Invoke(); //Callback for completion of Auth Sign In process
            GoogleSignIn.Instance.GoogleSignOnComplete -= OnGoogleSignOnComplete;
#endif
        }

        private async void OnAppleSignOnComplete(bool success, string message)
        {
#if UNITY_IOS
            if (success) //sign on success
            {
                //TODO: Messaging
            }
            else
            {
                //TODO: Messaging
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    try
                    {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Don't sign out later, since that changes the anonymous token, which would prevent the player from exiting lobbies they're already in.
                        Debug.Log("Sign in anonymously succeeded!");

                    }
                    catch (AuthenticationException ex)
                    {
                        //TODO: Add some type of UI popup for Auth failed scenarios (short popup to display error).
                        Debug.LogException(ex);
                    }
                    catch (RequestFailedException ex)
                    {

                        Debug.LogException(ex);
                    }
                }
            }
            SignedIn?.Invoke(); //Callback for completion of Auth Sign In process
            AppleSignIn.Instance.AppleSignOnComplete -= OnAppleSignOnComplete;
#endif
        }


        private void OnSignInChange()
        {
            SetContent("id", AuthenticationService.Instance.PlayerId);
        }
    }
}
