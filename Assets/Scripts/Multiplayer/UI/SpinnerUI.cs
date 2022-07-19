using System.Text;
using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Controls a simple throbber that is displayed when the lobby list is being refreshed.
    /// </summary>
    public class SpinnerUI : ObserverPanel<LobbyServiceData>
    {
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private UIPanelBase spinnerImage;
        [SerializeField] private UIPanelBase noServerText;
        [SerializeField] private UIPanelBase errorTextVisibility;
        [Tooltip("This prevents selecting a lobby or Joining while the spinner is visible.")]
        [SerializeField] private UIPanelBase raycastBlocker;

        public override void ObservedUpdated(LobbyServiceData observed)
        {
            if (observed.State == LobbyQueryState.Fetching)
            {
                Show();
                spinnerImage.Show();
                raycastBlocker.Show();
                noServerText.Hide();
                errorTextVisibility.Hide();
            }
            else if (observed.State == LobbyQueryState.Error)
            {
                spinnerImage.Hide();
                raycastBlocker.Hide();
                errorTextVisibility.Show();
                errorText.SetText("Error. See Unity Console log for details.");
            }
            else if (observed.State == LobbyQueryState.Fetched)
            {
                if (observed.CurrentLobbies.Count < 1)
                {
                    noServerText.Show();
                }
                else
                {
                    noServerText.Hide();
                }

                spinnerImage.Hide();
                raycastBlocker.Hide();
            }
        }
    }
}
