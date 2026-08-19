using UnityEngine;

/// <summary>
/// Orquestra a sequência inteira: clique em Jogar -> câmera segue o player ->
/// animação de acordar/andar -> textos da caminhada -> diálogo -> zoom out final.
/// Coloque este script em um GameObject vazio (ex: "GameFlowManager") e arraste
/// as referências abaixo pelo Inspector.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private CameraIntroController cameraController;
    [SerializeField] private PlayerIntroController playerController;
    [SerializeField] private WalkTextSequencer walkTextSequencer;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private GameObject menuCanvas;

    // Guards centralizados: evitam que o mesmo passo do fluxo seja disparado
    // duas vezes. Ex: PlayerIntroController pode chamar OnPlayerReachedDialoguePoint
    // tanto pelo evento "stopped" da Timeline quanto por um Signal Emitter, e
    // DialogueTriggerZone também pode chamar o mesmo método via trigger 2D — com
    // o guard aqui, não importa quantas fontes disparem, só a primeira conta.
    private bool hasStartedWalking;
    private bool hasReachedDialoguePoint;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Ligue este método no OnClick() do botão "Jogar" do menu, pelo Inspector.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (menuCanvas != null) menuCanvas.SetActive(false);

        cameraController.StartFollowingPlayer();
        playerController.BeginIntroSequence();
    }

    /// <summary>
    /// Chamado pelo PlayerIntroController via Animation Event ou Signal Emitter,
    /// no frame em que o personagem realmente começa a caminhar (depois de levantar).
    /// </summary>
    public void OnPlayerStartedWalking()
    {
        if (hasStartedWalking) return;
        hasStartedWalking = true;

        walkTextSequencer.BeginSequence();
    }

    /// <summary>
    /// Chamado quando o player chega no ponto onde o diálogo deve começar —
    /// seja pelo fim da Timeline (PlayerIntroController) ou por uma
    /// DialogueTriggerZone. Protegido contra chamada duplicada.
    /// </summary>
    public void OnPlayerReachedDialoguePoint()
    {
        if (hasReachedDialoguePoint) return;
        hasReachedDialoguePoint = true;

        walkTextSequencer.StopSequence();
        dialogueController.BeginDialogue();
    }

    /// <summary>
    /// Chamado pelo DialogueController quando a última linha do diálogo termina.
    /// </summary>
    public void OnDialogueFinished()
    {
        cameraController.ZoomOut();
    }
}