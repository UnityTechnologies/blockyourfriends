using UnityEngine;
using UnityEngine.UI;

namespace BlockYourFriends.Multiplayer.UI
{
    public class LobbyUserVolumeUI : MonoBehaviour
    {
        [SerializeField]
        private UIPanelBase volumeSliderContainer;
        [SerializeField]
        private UIPanelBase muteToggleContainer;
        [SerializeField]
        [Tooltip("This is shown for other players, to mute them.")]
        private GameObject muteIcon;
        [SerializeField]
        [Tooltip("This is shown for the local player, to make it clearer that they are muting themselves.")]
        private GameObject micMuteIcon;
        [SerializeField]
        private Slider volumeSlider;
        [SerializeField]
        private Toggle muteToggle;

        public bool IsLocalPlayer { private get; set; }

        /// <param name="shouldResetUi">
        /// When the user is being added, we want the UI to reset to the default values.
        /// (We don't do this if the user is already in the lobby so that the previous values are retained. E.g. If they're too loud and volume was lowered, keep it lowered on reenable.)
        /// </param>
        public void EnableVoice(bool shouldResetUi)
        {
            if (shouldResetUi)
            {   volumeSlider.SetValueWithoutNotify(vivox.VivoxUserHandler.NormalizedVolumeDefault);
                muteToggle.SetIsOnWithoutNotify(false);
            }

            if (IsLocalPlayer)
            {
                volumeSliderContainer.Hide(0);
                muteToggleContainer.Show();
                muteIcon.SetActive(false);
                micMuteIcon.SetActive(true);
            }
            else
            {
                volumeSliderContainer.Show();
                muteToggleContainer.Show();
                muteIcon.SetActive(true);
                micMuteIcon.SetActive(false);
            }
        }

        /// <param name="shouldResetUi">
        /// When the user leaves the lobby (but not if they just lose voice access for some reason, e.g. device disconnect), reset state to the default values.
        /// (We can't just do this during Enable since it could cause Vivox to have a state conflict during participant add.)
        /// </param>
        public void DisableVoice(bool shouldResetUi)
        {
            if (shouldResetUi)
            {   volumeSlider.value = vivox.VivoxUserHandler.NormalizedVolumeDefault;
                muteToggle.isOn = false;
            }

            volumeSliderContainer.Hide(0.4f);
            muteToggleContainer.Hide(0.4f);
            muteIcon.SetActive(true);
            micMuteIcon.SetActive(false);
        }

        public void Mute()
        {
            muteToggle.isOn = true;
        }
    }
}
