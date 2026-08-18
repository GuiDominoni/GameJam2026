using DG.Tweening;
using EasyTransition;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

public class UIFade : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Image targetImage; // se não tiver, usa canvasGroup
    [SerializeField] CanvasGroup targetGroup;

    [Header("Settings")]
    [SerializeField] float duration = 1f;
    [SerializeField] float delay = 0f;
    [SerializeField] Ease ease = Ease.Linear;
    [SerializeField] bool fadeOnStart = false;
    [SerializeField] bool startVisible = false;

    [Header("Events")]
    public UnityEvent onFadeInComplete;
    public UnityEvent onFadeOutComplete;

    [SerializeField] TransitionSettings tranSettings;

    void Awake()
    {
        if (targetImage == null && targetGroup == null)
        {
            targetGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (targetImage != null)
            targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, startVisible ? 1 : 0);

        if (targetGroup != null)
            targetGroup.alpha = startVisible ? 1 : 0;

        if (fadeOnStart)
            FadeIn();
    }

    public void FadeIn(string sceneToLoad = "")
    {
        if (targetImage != null)
        {
            targetImage.DOFade(1f, duration).SetEase(ease).SetDelay(delay).OnComplete(() =>
            {
                onFadeInComplete.Invoke();
                if (!string.IsNullOrEmpty(sceneToLoad))
                    SceneManager.LoadScene(sceneToLoad);
            });
        }
        else if (targetGroup != null)
        {
            targetGroup.DOFade(1f, duration).SetEase(ease).SetDelay(delay).OnComplete(() =>
            {
                onFadeInComplete.Invoke();
                if (!string.IsNullOrEmpty(sceneToLoad))
                    SceneManager.LoadScene(sceneToLoad);
            });
        }
    }

    public void FadeOut(string sceneToLoad = "")
    {
        if (targetImage != null)
        {
            targetImage.DOFade(0f, duration).SetEase(ease).SetDelay(delay).OnComplete(() =>
            {
                onFadeOutComplete.Invoke();
                if (!string.IsNullOrEmpty(sceneToLoad))
                    SceneManager.LoadScene(sceneToLoad);
            });
        }
        else if (targetGroup != null)
        {
            targetGroup.DOFade(0f, duration).SetEase(ease).SetDelay(delay).OnComplete(() =>
            {
                onFadeOutComplete.Invoke();
                if (!string.IsNullOrEmpty(sceneToLoad))
                    SceneManager.LoadScene(sceneToLoad);
            });
        }
    }
}