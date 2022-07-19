using UnityEngine;
using UnityEngine.Android;

namespace BlockYourFriends.Utility
{
    public class AndroidRequestPermission : MonoBehaviour
    {
        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
            UIManager.Instance.HideAndroidMicPermissionPopup();
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
            UIManager.Instance.HideAndroidMicPermissionPopup();
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
            UIManager.Instance.ShowAndroidMicPermissionPopup();
        }

        public void AskPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                bool useCallbacks = true;
                if (!useCallbacks)
                {
                    // We do not have permission to use the microphone.
                    // Ask for permission or proceed without the functionality enabled.
                    Permission.RequestUserPermission(Permission.Microphone);
                }
                else
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                    callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                    callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                    Permission.RequestUserPermission(Permission.Microphone, callbacks);
                }
            }
        }

        void Start()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            AskPermission();
#endif
        }
    }
}
