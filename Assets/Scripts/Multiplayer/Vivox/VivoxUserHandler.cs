using UnityEngine;
using Unity.Services.Vivox;
using VivoxUnity;
using BlockYourFriends.Multiplayer.UI;

namespace BlockYourFriends.Multiplayer.vivox
{
    /// <summary>
    /// Listens for changes to Vivox state for one user in the lobby.
    /// Instead of going through Relay, this will listen to the Vivox service since it will already transmit state changes for all clients.
    /// </summary>
    public class VivoxUserHandler : MonoBehaviour
    {
        [SerializeField] private LobbyUserVolumeUI lobbyUserVolumeUI;

        private IChannelSession channelSession;
        private string id;
        private string vivoxId;

        private const int k_volumeMin = -50, k_volumeMax = 20; // From the Vivox docs, the valid range is [-50, 50] but anything above 25 risks being painfully loud.
        public static float NormalizedVolumeDefault { get { return (0f - k_volumeMin) / (k_volumeMax - k_volumeMin); } }

        public void Start()
        {
            lobbyUserVolumeUI.DisableVoice(true);
        }

        public void SetId(string id)
        {
            this.id = id;
            // Vivox appends additional info to the ID we provide, in order to associate it with a specific channel. We'll construct m_vivoxId to match the ID used by Vivox.
            // FUTURE: This isn't yet available. When using Auth, the Vivox ID will match this format:
            // Account account = new Account(id);
            // m_vivoxId = $"sip:.{account.Issuer}.{m_id}.{environmentId}.@{account.Domain}";
            // However, the environment ID from Auth is not exposed anywhere, and Vivox doesn't provide a way to retrieve the ID, either.
            // Instead, when needed, we'll search for the Vivox ID containing this user's Auth ID, which is a GUID so collisions are extremely unlikely.
            // In the future, remove FindVivoxId and pass the environment ID here instead.
            vivoxId = null;

            // SetID might be called after we've received the IChannelSession for remote players, which would mean after OnParticipantAdded. So, duplicate the VivoxID work here.
            if (channelSession != null)
            {
                foreach (var participant in channelSession.Participants)
                {
                    if (this.id == participant.Account.DisplayName)
                    {
                        vivoxId = participant.Key;
                        lobbyUserVolumeUI.IsLocalPlayer = participant.IsSelf;
                        lobbyUserVolumeUI.EnableVoice(true);
                        break;
                    }
                }
            }
        }

        public void OnChannelJoined(IChannelSession channelSession) // Called after a connection is established, which begins once a lobby is joined.
        {
            //Check if we are muted or not

            this.channelSession = channelSession;
            this.channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
            this.channelSession.Participants.BeforeKeyRemoved += BeforeParticipantRemoved;
            this.channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
        }

        public void OnChannelLeft() // Called when we leave the lobby.
        {
            if (channelSession != null) // It's possible we'll attempt to leave a channel that isn't joined, if we leave the lobby while Vivox is connecting.
            {
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= BeforeParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                channelSession = null;
            }
        }

        /// <summary>
        /// To be called whenever a new Participant is added to the channel, using the events from Vivox's custom dictionary.
        /// </summary>
        private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
            var participant = source[keyEventArg.Key];
            var username = participant.Account.DisplayName;

            bool isThisUser = username == id;
            if (isThisUser)
            {   vivoxId = keyEventArg.Key; // Since we couldn't construct the Vivox ID earlier, retrieve it here.
                lobbyUserVolumeUI.IsLocalPlayer = participant.IsSelf;

                if(!participant.IsMutedForAll)
                    lobbyUserVolumeUI.EnableVoice(false);//Should check if user is muted or not.
                else
                    lobbyUserVolumeUI.DisableVoice(false);
            }
            else
            {
                if(!participant.LocalMute)
                    lobbyUserVolumeUI.EnableVoice(false);//Should check if user is muted or not.
                else
                    lobbyUserVolumeUI.DisableVoice(false);
            }

            lobbyUserVolumeUI.Mute();
            OnMuteToggle(true);
        }

        private void BeforeParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
            var participant = source[keyEventArg.Key];
            var username = participant.Account.DisplayName;

            bool isThisUser = username == id;
            if (isThisUser)
            {   lobbyUserVolumeUI.DisableVoice(true);
            }
        }

        private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
            var participant = source[valueEventArg.Key];
            var username = participant.Account.DisplayName;
            string property = valueEventArg.PropertyName;

            if (username == id)
            {
                if (property == "UnavailableCaptureDevice")
                {
                    if (participant.UnavailableCaptureDevice)
                    {   lobbyUserVolumeUI.DisableVoice(false);
                        participant.SetIsMuteForAll(true, null); // Note: If you add more places where a player might be globally muted, a state machine might be required for accurate logic.
                    }
                    else
                    {   lobbyUserVolumeUI.EnableVoice(false);
                        participant.SetIsMuteForAll(false, null); // Also note: This call is asynchronous, so it's possible to exit the lobby before this completes, resulting in a Vivox error.
                    }
                }
                else if (property == "IsMutedForAll")
                {
                    if (participant.IsMutedForAll)
                        lobbyUserVolumeUI.DisableVoice(false);
                    else
                        lobbyUserVolumeUI.EnableVoice(false);
                }
            }
        }

        public void OnVolumeSlide(float volumeNormalized)
        {
            if (channelSession == null || vivoxId == null) // Verify initialization, since SetId and OnChannelJoined are called at different times for local vs. remote clients.
                return;

            int vol = (int)Mathf.Clamp(k_volumeMin + (k_volumeMax - k_volumeMin) * volumeNormalized, k_volumeMin, k_volumeMax); // Clamping as a precaution; if UserVolume somehow got above 1, listeners could be harmed.
            bool isSelf = channelSession.Participants[vivoxId].IsSelf;
            if (isSelf)
            {
                VivoxService.Instance.Client.AudioInputDevices.VolumeAdjustment = vol;
            }
            else
            {
                channelSession.Participants[vivoxId].LocalVolumeAdjustment = vol;
            }
        }

        public void OnMuteToggle(bool isMuted)
        {
            if (channelSession == null || vivoxId == null)
                return;

            bool isSelf = channelSession.Participants[vivoxId].IsSelf;
            if (isSelf)
            {
                VivoxService.Instance.Client.AudioInputDevices.Muted = isMuted;
            }
            else
            {
                channelSession.Participants[vivoxId].LocalMute = isMuted;
            }
        }
    }
}
