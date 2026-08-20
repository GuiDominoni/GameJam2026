using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Coloque este script no GameObject do player (ou em um objeto de controle).
/// A sequência (dormindo -> acorda -> levanta -> anda) foi feita numa Timeline.
/// Este script dá Play, e quando a Timeline termina, avisa o GameFlowManager
/// que é hora de começar o diálogo.
///
/// IMPORTANTE sobre o fim da Timeline: o evento PlayableDirector.stopped só
/// dispara sozinho quando o Wrap Mode do PlayableAsset está configurado como
/// "None". Se estiver como "Hold" (bem comum), a Timeline fica parada no
/// último frame mas o evento "stopped" NUNCA é disparado — e o diálogo nunca
/// começa. Confira o Wrap Mode no Inspector da Timeline; se não for "None",
/// use o método OnIntroTimelineEnded() abaixo através de um Signal Emitter
/// colocado no fim da Timeline (mesmo esquema que você já usa pro sinal de
/// "começou a andar").
/// </summary>
public class PlayerIntroController : MonoBehaviour
{
    [SerializeField] private PlayableDirector introTimeline;

    private void OnEnable()
    {
        introTimeline.stopped += HandleTimelineStopped;
    }

    private void OnDisable()
    {
        introTimeline.stopped -= HandleTimelineStopped;
    }

    /// <summary>
    /// Chamado pelo GameFlowManager assim que o botão Jogar é pressionado.
    /// </summary>
    public void BeginIntroSequence()
    {
        introTimeline.Play();
    }

    /// <summary>
    /// NÃO chame isso por código — é disparado por um Signal Emitter colocado
    /// na Timeline, no ponto em que o personagem começa a andar de fato.
    /// </summary>
    public void OnWalkAnimationStart()
    {
        GameFlowManager.Instance.OnPlayerStartedWalking();
    }

    /// <summary>
    /// Alternativa ao evento "stopped" (ver comentário da classe). Conecte um
    /// Signal Emitter no último frame da Timeline a este método caso o Wrap
    /// Mode não seja "None". Pode conviver com o HandleTimelineStopped sem
    /// problema: o GameFlowManager já ignora chamadas duplicadas.
    /// </summary>
    public void OnIntroTimelineEnded()
    {
        GameFlowManager.Instance.OnPlayerReachedDialoguePoint();
    }

    // Chamado automaticamente pelo PlayableDirector quando a Timeline termina
    // (só dispara sozinho se o Wrap Mode for "None" — ver comentário acima).
    private void HandleTimelineStopped(PlayableDirector director)
    {
        GameFlowManager.Instance.OnPlayerReachedDialoguePoint();
    }

    public void DisableThisTimeline()
    {
        introTimeline.enabled = false;
    }
}