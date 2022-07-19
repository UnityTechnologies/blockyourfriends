using UnityEngine;

namespace BlockYourFriends.Multiplayer.UI
{
    /// <summary>
    /// Observes the Lobby request rate limits and changes the visibility of a UIPanelBase to suit.
    /// E.g. the refresh button on the Join menu should be inactive after a refresh for long enough to avoid the lobby query rate limit.
    /// </summary>
    public class RateLimitVisibility : MonoBehaviour
    {
        [SerializeField] private UIPanelBase target;
        [SerializeField] private float alphaWhenHidden = 0.5f;
        [SerializeField] private LobbyAsyncRequests.RequestType requestType;

        private void Start()
        {
            LobbyAsyncRequests.Instance.GetRateLimit(requestType).onChanged += UpdateVisibility;
        }
        private void OnDestroy()
        {
            LobbyAsyncRequests.Instance.GetRateLimit(requestType).onChanged -= UpdateVisibility;
        }

        private void UpdateVisibility(LobbyAsyncRequests.RateLimitCooldown rateLimit)
        {
            if (rateLimit.IsInCooldown)
                target.Hide(alphaWhenHidden);
            else
                target.Show();
        }
    }
}
