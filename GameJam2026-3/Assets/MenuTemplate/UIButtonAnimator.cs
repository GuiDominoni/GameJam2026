using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Animation Settings")]
    [SerializeField] float duration = 0.25f;
    [SerializeField] Ease ease = Ease.OutBack;

    [Header("Scale Settings")]
    [SerializeField] bool animateScale = true;
    [SerializeField] Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);

    [Header("Rotation Settings")]
    [SerializeField] bool animateRotation = false;
    [SerializeField] Vector3 hoverRotation = new Vector3(0f, 0f, 10f);

    [Header("Position Settings")]
    [SerializeField] bool animatePosition = false;
    [SerializeField] Vector3 hoverPositionOffset = new Vector3(0f, 5f, 0f);

    [Header("Click Settings")]
    [SerializeField] bool animateClick = true;
    [SerializeField] Vector3 clickScale = new Vector3(0.9f, 0.9f, 1f);
    [SerializeField] float clickDuration = 0.15f;
    [SerializeField] Ease clickEase = Ease.InOutSine;

    RectTransform rect;
    Vector3 baseScale;
    Quaternion baseRotation;
    Vector3 basePosition;
    Tween activeTween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
        baseRotation = rect.localRotation;
        basePosition = rect.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateTo(hoverScale, hoverRotation, basePosition + hoverPositionOffset, duration, ease);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(baseScale, baseRotation.eulerAngles, basePosition, duration, ease);
    }
    public void PointerExit(){ AnimateTo(baseScale, baseRotation.eulerAngles, basePosition, duration, ease);
 }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!animateClick) return;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(clickScale, clickDuration).SetEase(clickEase));
        seq.Append(rect.DOScale(animateScale ? hoverScale : baseScale, clickDuration).SetEase(clickEase));
    }

    void AnimateTo(Vector3 targetScale, Vector3 targetRotation, Vector3 targetPosition, float time, Ease easing)
    {
        activeTween?.Kill();
        Sequence seq = DOTween.Sequence();

        if (animateScale)
            seq.Join(rect.DOScale(targetScale, time).SetEase(easing));

        if (animateRotation)
            seq.Join(rect.DOLocalRotate(targetRotation, time).SetEase(easing));

        if (animatePosition)
            seq.Join(rect.DOLocalMove(targetPosition, time).SetEase(easing));

        activeTween = seq;
    }
}
