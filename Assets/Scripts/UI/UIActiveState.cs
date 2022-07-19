using UnityEngine;

namespace BlockYourFriends.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIActiveState : MonoBehaviour
    {
        [SerializeField] private bool activeOnAwake;

        private CanvasGroup canvasGroup;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup != null) return canvasGroup;
                return canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void ResetUI()
        {
            if (activeOnAwake)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }
    }
}
