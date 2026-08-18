using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] RectTransform mainMenuPanel;
    [SerializeField] RectTransform optionsMenuPanel;

    [Header("Animation Settings")]
    [SerializeField] float duration = 0.3f;
    [SerializeField] Ease ease = Ease.OutBack;
    [SerializeField] Ease easeClose = Ease.InBack;

    [Header("Events")]
    public UnityEvent onOpenOptions;
    public UnityEvent onCloseOptions;

    void Start()
    {
      
        mainMenuPanel.localScale = Vector3.one;
        optionsMenuPanel.localScale = Vector3.zero;
    }

    public void OpenOptions()
    {
      
        mainMenuPanel.DOScale(Vector3.zero, duration).SetEase(easeClose);

        
        optionsMenuPanel.DOScale(Vector3.one, duration).SetEase(ease)
            .OnComplete(() => onOpenOptions.Invoke());
    }

    public void CloseOptions()
    {
       
        optionsMenuPanel.DOScale(Vector3.zero, duration).SetEase(easeClose)
            .OnComplete(() => onCloseOptions.Invoke());

       
        mainMenuPanel.DOScale(Vector3.one, duration).SetEase(ease);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
