using UnityEngine;
using DG.Tweening;
public class ParticleSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem collisionParticles;
    [SerializeField] private float cooldown = 2f;
    private Vector3 tamanhoOriginal;
    private float ultimoEfeito = -2f;
    private void Start()
    {
        tamanhoOriginal = transform.localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica o cooldown
        if (Time.time < ultimoEfeito + cooldown)
            return;

        ultimoEfeito = Time.time;

        // Pega o ponto da colisão
        ContactPoint2D contato = collision.contacts[0];

        // Cria a partícula no ponto atingido
        ParticleSystem efeito = Instantiate(
            collisionParticles,
            contato.point,
            Quaternion.identity
        );

        efeito.Play();
        Destroy(efeito.gameObject, 2f);

        // Anima o tamanho do Player
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
