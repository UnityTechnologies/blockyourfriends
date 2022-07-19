using System;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using VivoxUnity;

namespace BlockYourFriends.Multiplayer.vivox
{
    /// <summary>
    /// Handles setting up a voice channel once inside a lobby.
    /// </summary>
    public class VivoxSetup
    {
        private const string vivoxTokenKey = "ajDUH3o3oJ479tZqFtjif5dGqvfZM41y";
        private bool hasInitialized = false;
        private bool isMidInitialize = false;
        private ILoginSession loginSession = null;
        private IChannelSession channelSession = null;
        private List<VivoxUserHandler> userHandlers;

        /// <summary>
        /// Initialize the Vivox service, before actually joining any audio channels.
        /// </summary>
        /// <param name="onComplete">Called whether the login succeeds or not.</param>
        public void Initialize(List<VivoxUserHandler> userHandlers, Action<bool> onComplete)
        {
            
            if (isMidInitialize)
                return;
            isMidInitialize = true;

            this.userHandlers = userHandlers;

            if(VivoxService.Instance.Client != null)
                VivoxService.Instance.Client.Uninitialize();

            VivoxService.Instance.Initialize();
            string userId = Locator.Get.Identity.GetSubIdentity(Auth.IIdentityType.Auth).GetContent("id");
            Account account = new Account(userId);
            loginSession = VivoxService.Instance.Client.GetLoginSession(account);

            string token = loginSession.GetLoginToken();
            loginSession.BeginLogin(token, SubscriptionMode.Accept, null, null, null, result =>
            {
                try
                {
                    loginSession.EndLogin(result);
                    hasInitialized = true;
                    onComplete?.Invoke(true);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning("Vivox failed to login: " + ex.Message);
                    onComplete?.Invoke(false);
                }
                finally
                {
                    isMidInitialize = false;
                }
            });
            
        }

        /// <summary>
        /// Once in a lobby, start joining a voice channel for that lobby. Be sure to complete Initialize first.
        /// </summary>
        /// <param name="onComplete">Called whether the channel is successfully joined or not.</param>
        public void JoinLobbyChannel(string lobbyId, Action<bool> onComplete)
        {
            Debug.Log("Trying to join channel");
            if (!hasInitialized || loginSession.State != LoginState.LoggedIn)
            {
                UnityEngine.Debug.LogWarning("Can't join a Vivox audio channel, as Vivox login hasn't completed yet.");
                onComplete?.Invoke(false);
                return;
            }
            TimeSpan expirationTimeSpan = TimeSpan.FromSeconds(90);
            ChannelType channelType = ChannelType.NonPositional;
            Channel channel = new Channel(lobbyId + "_voice", channelType, null);
            channelSession = loginSession.GetChannelSession(channel);
            //string token = channelSession.GetConnectToken(vivoxTokenKey, expirationTimeSpan);
            string token = channelSession.GetConnectToken();
            Debug.Log("Token:" + token);
            channelSession.BeginConnect(true, false, true, token, result =>
            {
                try
                {
                    // Special case: It's possible for the player to leave the lobby between the time we called BeginConnect and the time we hit this callback.
                    // If that's the case, we should abort the rest of the connection process.
                    if (channelSession.ChannelState == ConnectionState.Disconnecting || channelSession.ChannelState == ConnectionState.Disconnected)
                    {
                        UnityEngine.Debug.LogWarning("Vivox channel is already disconnecting. Terminating the channel connect sequence.");
                        HandleEarlyDisconnect();
                        return;
                    }

                    channelSession.EndConnect(result);
                    onComplete?.Invoke(true);
                    foreach (VivoxUserHandler userHandler in userHandlers)
                        userHandler.OnChannelJoined(channelSession);
                }
                catch (Exception ex)
                {   UnityEngine.Debug.LogWarning("Vivox failed to connect: " + ex.Message);
                    onComplete?.Invoke(false);
                    channelSession?.Disconnect();
                }
            });
            
        }

        /// <summary>
        /// To be called when leaving a lobby.
        /// </summary>
        public void LeaveLobbyChannel()
        {
            
            if (channelSession != null)
            {
                // Special case: The EndConnect call requires a little bit of time before the connection actually completes, but the player might
                // disconnect before then. If so, sending the Disconnect now will fail, and the played would stay connected to voice while no longer
                // in the lobby. So, wait until the connection is completed before disconnecting in that case.
                if (channelSession.ChannelState == ConnectionState.Connecting)
                {
                    UnityEngine.Debug.LogWarning("Vivox channel is trying to disconnect while trying to complete its connection. Will wait until connection completes.");
                    HandleEarlyDisconnect();
                    return;
                }

                ChannelId id = channelSession.Channel;
                channelSession?.Disconnect(
                    (result) => { loginSession.DeleteChannelSession(id); channelSession = null; });
            }
            foreach (VivoxUserHandler userHandler in userHandlers)
                userHandler.OnChannelLeft();
            
        }

        private void HandleEarlyDisconnect()
        {
            Locator.Get.UpdateSlow.Subscribe(DisconnectOnceConnected, 0.2f);
        }

        private void DisconnectOnceConnected(float unused)
        {
            
            if (channelSession?.ChannelState == ConnectionState.Connecting)
                return;
            Locator.Get.UpdateSlow.Unsubscribe(DisconnectOnceConnected);
            LeaveLobbyChannel();
            
        }

        /// <summary>
        /// To be called on quit, this will disconnect the player from Vivox entirely instead of just leaving any open lobby channels.
        /// </summary>
        public void Uninitialize()
        {
            
            if (!hasInitialized)
                return;
            loginSession.Logout();
            VivoxService.Instance.Client.Uninitialize();
            
        }
    }
}
