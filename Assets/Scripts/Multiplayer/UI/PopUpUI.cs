using System.Text;
using TMPro;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Controls a pop-up message that lays over the rest of the UI, with a button to dismiss. Used for displaying player-facing error messages.
    /// </summary>
    public class PopUpUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField popupText;
        [SerializeField] private CanvasGroup buttonVisibility;

        private float buttonVisibilityTimeout = -1;
        private StringBuilder currentText = new StringBuilder();

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// If the pop-up is not currently visible, display it. If it is, append the incoming text to the existing pop-up.
        /// </summary>
        public void ShowPopup(string newText)
        {
            if (!gameObject.activeSelf)
            {   currentText.Clear();
                gameObject.SetActive(true);
            }
            currentText.AppendLine(newText);
            popupText.SetTextWithoutNotify(currentText.ToString());
            DisableButton();
        }

        private void DisableButton()
        {
            buttonVisibilityTimeout = 0.5f; // Briefly prevent the popup from being dismissed, to ensure the player doesn't accidentally click past it without seeing it.
            buttonVisibility.alpha = 0.5f;
            buttonVisibility.interactable = false;
        }
        private void ReenableButton()
        {
            buttonVisibility.alpha = 1;
            buttonVisibility.interactable = true;
        }

        private void Update()
        {
            if (buttonVisibilityTimeout >= 0 && buttonVisibilityTimeout - Time.deltaTime < 0)
                ReenableButton();
            buttonVisibilityTimeout -= Time.deltaTime;
        }

        public void ClearPopup()
        {
            gameObject.SetActive(false);
        }
    }
}
