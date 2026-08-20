using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class GrabParticle : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    [SerializeField] private float cooldown = 2f;
    private Vector3 tamanhoOriginal;
    private void Awake()
    {
        tamanhoOriginal = transform.localScale;
        _particleSystem = GetComponent<ParticleSystem>();
    }
    public void InvokeParticle()
    {

        print("sdads");
        _particleSystem.Play();

        transform.DOKill();

        transform.DOScale(tamanhoOriginal * 0.8f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(tamanhoOriginal, 0.15f)
                    .SetEase(Ease.OutBack);
            });
    }
}

