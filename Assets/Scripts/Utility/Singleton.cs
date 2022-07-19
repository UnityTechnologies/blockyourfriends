using UnityEngine;

namespace BlockYourFriends.Utility
{

    /// Base Singleton Generic Class

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool isShuttingDown = false;
        private static object isLock = new object();
        private static T instance;

        private protected bool cleanInstance = false;

        public static T Instance
        {
            get
            {
                if (isShuttingDown)
                {
                    //Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    //    "' already destroyed. Returning null.");
                    return null;
                }

                lock (isLock)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));

                        // Create new instance if one doesn't already exist.
                        if (instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                    return instance;
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (cleanInstance)
                instance = null;
            else
                isShuttingDown = true;
        }

        private void OnDestroy()
        {
            if (cleanInstance)
                instance = null;
            else
                isShuttingDown = true;
        }
    }
}

