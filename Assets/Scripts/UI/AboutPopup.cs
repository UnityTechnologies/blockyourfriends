using UnityEngine;
using BlockYourFriends.Utility.SO;

namespace BlockYourFriends.UI
{
    [ExecuteAlways]
    public class AboutPopup : MonoBehaviour
    {
        public VersionsUsed versionsUsed;

        [SerializeField] private SetVersion engine;
        [SerializeField] private SetVersion app;
        [SerializeField] private SetVersion analytics;
        [SerializeField] private SetVersion authentication;
        [SerializeField] private SetVersion cloudcode;
        [SerializeField] private SetVersion cloudsave;
        [SerializeField] private SetVersion inAppPurchasing;
        [SerializeField] private SetVersion lobby;
        [SerializeField] private SetVersion mediation;
        [SerializeField] private SetVersion relay;
        [SerializeField] private SetVersion vivox;

        private void OnEnable()
        {
            SetVersionText();
        }

        private void SetVersionText()
        {
            engine.SetText(versionsUsed.engine);
            app.SetText(versionsUsed.app);
            analytics.SetText(versionsUsed.analytics);
            authentication.SetText(versionsUsed.authentication);
            cloudcode.SetText(versionsUsed.cloudcode);
            cloudsave.SetText(versionsUsed.cloudsave);
            inAppPurchasing.SetText(versionsUsed.inAppPurchasing);
            lobby.SetText(versionsUsed.lobby);
            mediation.SetText(versionsUsed.mediation);
            relay.SetText(versionsUsed.relay);
            vivox.SetText(versionsUsed.vivox);
        }
    }
}