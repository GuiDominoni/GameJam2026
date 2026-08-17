using UnityEngine;
using DG.Tweening;

// Biblioteca de efeitos de "feel"/juice prontos pra usar, com DOTween. Cole este
// componente em QUALQUER objeto que deva reagir a alguma interação (o
// personagem, um Pullable, a câmera, uma parede...) e chame os métodos — direto
// por código, ou arrastando este componente num campo de UnityEvent (ex: os
// eventos do HookEvents) e escolhendo o método na lista, sem código a mais.
//
// Cada objeto que precisar de feedback ganha sua própria instância deste
// componente, então o mesmo script serve pra qualquer coisa do jogo sem
// precisar duplicar nada. Métodos sem parâmetro (ou só com valores com padrão)
// aparecem certinho no seletor de método do UnityEvent no Inspector.
public class FeelFX : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private Camera cam;

    private Vector3 baseScale;
    private float baseLineWidth;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        baseScale = transform.localScale;
        if (lineRenderer != null)
        {
            baseLineWidth = lineRenderer.startWidth;
        }
    }

    // ------------------------------------------------------------------
    // ESCALA
    // ------------------------------------------------------------------

    // "Pop" genérico — pegar item, gancho grudando, qualquer confirmação positiva.
    public void PunchScale(float strength = 0.3f, float duration = 0.3f)
    {
        transform.DOKill();
        transform.localScale = baseScale;
        transform.DOPunchScale(Vector3.one * strength, duration, 8, 0.8f);
    }

    // Achata na direção de um impacto (bater na parede, ser puxado com força).
    public void SquashStretch(Vector2 impactDirection, float intensity = 0.3f, float duration = 0.2f)
    {
        transform.DOKill();
        Vector2 dir = impactDirection.sqrMagnitude > 0.01f ? impactDirection.normalized : Vector2.up;
        Vector3 squashed = new Vector3(
            baseScale.x * (1f + Mathf.Abs(dir.x) * intensity - Mathf.Abs(dir.y) * intensity * 0.5f),
            baseScale.y * (1f + Mathf.Abs(dir.y) * intensity - Mathf.Abs(dir.x) * intensity * 0.5f),
            baseScale.z);
        transform.localScale = squashed;
        transform.DOScale(baseScale, duration).SetEase(Ease.OutElastic);
    }

    // Entrada elástica (objeto "nascendo"/aparecendo) — de escala 0 até o normal.
    public void PopIn(float duration = 0.4f)
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(baseScale, duration).SetEase(Ease.OutBack);
    }

    // Pulsação contínua em loop — destaca um objeto (ex: "dá pra grudar aqui")
    // enquanto o jogador mira perto dele. Lembre de chamar StopPulse depois.
    public void StartPulse(float scaleAmount = 0.1f, float duration = 0.6f)
    {
        transform.DOKill();
        transform.DOScale(baseScale * (1f + scaleAmount), duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void StopPulse()
    {
        transform.DOKill();
        transform.localScale = baseScale;
    }

    // Encolhe rapidinho antes de agir — dá antecipação (ex: um instante antes do
    // gancho ser lançado).
    public void Anticipation(float intensity = 0.15f, float duration = 0.1f)
    {
        transform.DOKill();
        transform.DOScale(baseScale * (1f - intensity), duration).SetLoops(2, LoopType.Yoyo);
    }

    // Encolhe, gira e desaparece — objeto sendo "consumido" (ex: Pullable
    // chegando no personagem e sendo destruído logo em seguida).
    public void DestroyPop(float duration = 0.3f, System.Action onComplete = null)
    {
        transform.DOKill();
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
        seq.Join(transform.DORotate(new Vector3(0, 0, 45f), duration, RotateMode.LocalAxisAdd));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    // ------------------------------------------------------------------
    // POSIÇÃO / ROTAÇÃO
    // ------------------------------------------------------------------

    // Sacudida rápida de posição — impacto leve.
    public void PunchPosition(Vector2 direction, float strength = 0.2f, float duration = 0.25f)
    {
        transform.DOKill();
        transform.DOPunchPosition(direction.normalized * strength, duration, 10, 1f);
    }

    // Sacudida de rotação — objeto "abalado" (ex: Pullable sendo agarrado).
    public void PunchRotation(float angle = 15f, float duration = 0.4f)
    {
        transform.DOKill();
        transform.DOPunchRotation(new Vector3(0, 0, angle), duration, 10, 1f);
    }

    // Estica na direção da velocidade — sensação de velocidade (ex: personagem
    // em pleno swing).
    public void VelocityStretch(Vector2 velocity, float referenceSpeed = 20f, float intensity = 0.15f, float duration = 0.2f)
    {
        transform.DOKill();
        float speedFactor = Mathf.Clamp01(velocity.magnitude / referenceSpeed);
        Vector3 stretched = new Vector3(
            baseScale.x * (1f + speedFactor * intensity),
            baseScale.y * (1f - speedFactor * intensity * 0.5f),
            baseScale.z);
        transform.DOScale(stretched, duration * 0.3f).SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DOScale(baseScale, duration * 0.7f).SetEase(Ease.OutElastic));
    }

    // ------------------------------------------------------------------
    // COR / SPRITE (precisa de SpriteRenderer no mesmo objeto)
    // ------------------------------------------------------------------

    // Pisca uma cor rapidamente e volta — "hit"/dano/grab.
    public void ColorFlash(Color flashColor, float duration = 0.15f)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.DOKill();
        Color original = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        spriteRenderer.DOColor(original, duration);
    }

    public void FadeIn(float duration = 0.3f)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.DOKill();
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;
        spriteRenderer.DOFade(1f, duration);
    }

    public void FadeOut(float duration = 0.3f)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0f, duration);
    }

    // ------------------------------------------------------------------
    // CÂMERA (funciona se este componente estiver na própria Camera)
    // ------------------------------------------------------------------

    // Sacode a câmera — impactos grandes (bater na parede, gancho grudando forte).
    public void ScreenShake(float strength = 0.3f, float duration = 0.2f, int vibrato = 20)
    {
        if (cam == null)
        {
            return;
        }

        cam.transform.DOKill();
        cam.transform.DOShakePosition(duration, strength, vibrato, 90, false, true);
    }

    // Punch no zoom — aperta e solta o tamanho ortográfico, dá peso a um impacto.
    public void CameraZoomPunch(float strength = 1f, float duration = 0.3f)
    {
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        cam.DOKill();
        float original = cam.orthographicSize;
        cam.DOOrthoSize(original - strength, duration * 0.3f).SetEase(Ease.OutQuad)
            .OnComplete(() => cam.DOOrthoSize(original, duration * 0.7f).SetEase(Ease.OutElastic));
    }

    // ------------------------------------------------------------------
    // CORDA (funciona se este componente estiver no mesmo objeto do LineRenderer)
    // ------------------------------------------------------------------

    // "Punch" na largura da linha — gancho grudando/tensionando.
    public void LineWidthPunch(float punchAmount = 0.05f, float duration = 0.2f)
    {
        if (lineRenderer == null)
        {
            return;
        }

        DOTween.Kill(lineRenderer);
        DOVirtual.Float(baseLineWidth + punchAmount, baseLineWidth, duration, w =>
        {
            lineRenderer.startWidth = w;
            lineRenderer.endWidth = w;
            //abobora
        }).SetId(lineRenderer);
    }

    // ------------------------------------------------------------------
    // TEMPO (global — não depende de nenhum componente específico)
    // ------------------------------------------------------------------

    // Congela o jogo por um instante — dá peso a impactos fortes. Use com
    // moderação, poucos milissegundos já bastam (0.03-0.08 costuma ser o ponto doce).
    public void HitStop(float duration = 0.05f)
    {
        Time.timeScale = 0f;
        DOVirtual.DelayedCall(duration, () => Time.timeScale = 1f, true);
    }
}