using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Acts as a buffer between receiving requests to display error messages to the player and running the pop-up UI to do so.
    /// </summary>
    public class LogHandlerSettings : MonoBehaviour, IReceiveMessages
    {
        [SerializeField]
        [Tooltip("Only logs of this level or higher will appear in the console.")]
        private LogMode editorLogVerbosity = LogMode.Critical;

        [SerializeField]
        private PopUpUI popUp;

        private void Awake()
        {
            LogHandler.Get().mode = editorLogVerbosity;
            Locator.Get.Messenger.Subscribe(this);
        }
        private void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }

        /// <summary>
        /// For convenience while in the Editor, update the log verbosity when its value is changed in the Inspector.
        /// </summary>
        public void OnValidate()
        {
            LogHandler.Get().mode = editorLogVerbosity;
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.DisplayErrorPopup && msg != null)
                SpawnErrorPopup((string)msg);
        }

        private void SpawnErrorPopup(string errorMessage)
        {
            popUp.ShowPopup(errorMessage);
        }
    }
}
