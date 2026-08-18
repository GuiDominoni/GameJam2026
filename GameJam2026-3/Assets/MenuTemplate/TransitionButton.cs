using EasyTransition;
using UnityEngine;
using UnityEngine.Events;

public class TransitionButton : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] TransitionSettings tranSettings;
    
    [SerializeField] string nameScene;
    [SerializeField] public UnityEvent transition;

    [Header("Áudio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clickSound;

    void Awake()
    {
        // Se você esquecer de arrastar o AudioSource, ele tenta pegar o do próprio objeto
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TransitionScene()
    {
        // Toca o som imediatamente ao clicar
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // Inicia a transição com o delay configurado
        TransitionManager.Instance().Transition(nameScene, tranSettings, delay);
    }
}