using UnityEngine;

namespace BlockYourFriends.UI
{
    [ExecuteAlways]
    public class AdjustCameraForScreen : MonoBehaviour
    {
        [SerializeField] private AnimationCurve sizeCurve;
        private float aspectRatio;

        private void Awake()
        {
            AdjustCamera();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(!Application.isPlaying)
                AdjustCamera();
        }
#endif

        private void AdjustCamera()
        {
            aspectRatio = (float)Screen.width / (float)Screen.height; //curve time
            //Debug.Log("Aspect ratio: " + aspectRatio);

            if (sizeCurve != null)
                Camera.main.orthographicSize = sizeCurve.Evaluate(aspectRatio); //curve value
        }
    }
}
