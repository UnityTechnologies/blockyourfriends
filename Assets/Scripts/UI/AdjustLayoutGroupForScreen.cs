using UnityEngine;
using UnityEngine.UI;

namespace BlockYourFriends.UI
{
    [ExecuteAlways]
    public class AdjustLayoutGroupForScreen : MonoBehaviour
    {
        [SerializeField] private bool adjustSides;
        [SerializeField] private bool adjustTop;
        [SerializeField] private bool adjustBottom;
        [SerializeField] private bool adjustSpacing;
        [SerializeField] private AnimationCurve sideCurve;
        [SerializeField] private AnimationCurve topCurve;
        [SerializeField] private AnimationCurve bottomCurve;
        [SerializeField] private AnimationCurve spacingCurve;

        private HorizontalOrVerticalLayoutGroup layoutGroup;
        private float aspectRatio;

        private void Awake()
        {
            GetLayoutGroup();
            AdjustLayoutGroup();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (layoutGroup == null)
                    GetLayoutGroup();
                AdjustLayoutGroup();
            }
        }
#endif

        private void GetLayoutGroup()
        {
            layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }

        private void AdjustLayoutGroup()
        {
            aspectRatio = (float)Screen.width / (float)Screen.height; //curve time
            //Debug.Log("Aspect ratio: " + aspectRatio);

            if (layoutGroup != null)
            {
                if (adjustSides && sideCurve != null)
                {
                    layoutGroup.padding.left = (int)sideCurve.Evaluate(aspectRatio); //curve values
                    layoutGroup.padding.right = (int)sideCurve.Evaluate(aspectRatio);
                }
                if (adjustTop && topCurve != null)
                    layoutGroup.padding.top = (int)topCurve.Evaluate(aspectRatio);
                if (adjustBottom && bottomCurve != null)
                    layoutGroup.padding.bottom = (int)bottomCurve.Evaluate(aspectRatio);
                if (adjustSpacing && spacingCurve != null)
                    layoutGroup.spacing = (int)spacingCurve.Evaluate(aspectRatio);
            }
        }
    }
}