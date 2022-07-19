using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ForceScrollbarValue : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private float forcedValue = 1;
    private ScrollRect scrollRect;
    private int childCount;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
            Init();
    }

    private void Init()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void CheckChildCount()
    {
        if (childCount != scrollRect.content.childCount)
        {
            ForceValue();
            childCount = scrollRect.content.childCount;
        }
    }

    private void ForceValue()
    {
        scrollbar.value = forcedValue;
    }
}
