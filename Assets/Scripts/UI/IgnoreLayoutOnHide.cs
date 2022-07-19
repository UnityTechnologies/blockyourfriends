using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutElement), typeof(CanvasGroup))]
public class IgnoreLayoutOnHide : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;

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
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        CheckCanvasGroupAlpha();
    }

    private void OnCanvasGroupChanged()
    {
        if (canvasGroup != null)
            CheckCanvasGroupAlpha();
    }

    private void CheckCanvasGroupAlpha()
    {
        if (canvasGroup.alpha == 0)
            IgnoreLayout();
        else if (canvasGroup.alpha == 1)
            ObserveLayout();
    }

    private void IgnoreLayout()
    {
        layoutElement.ignoreLayout = true;
    }

    private void ObserveLayout()
    {
        layoutElement.ignoreLayout = false;
    }
}
