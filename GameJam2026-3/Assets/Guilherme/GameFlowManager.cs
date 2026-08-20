using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
/// <summary>
/// Orquestra a sequência inteira: clique em Jogar -> câmera segue o player ->
/// animação de acordar/andar -> textos da caminhada -> diálogo -> zoom out final.
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
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playerTuto;
    [SerializeField] private Canvas canvas;

    [Header("Scripts desativados durante o diálogo")]
    [SerializeField] private List<MonoBehaviour> scriptsToDisable;



    public UnityEvent changeAnim;

    

    private bool hasStartedWalking;
    private bool hasReachedDialoguePoint;

    private void Awake()
    {
        Instance = this;
        SetDialogueScriptsEnabled(false);
        //player.SetActive(false);
        canvas.gameObject.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);

        cameraController.StartFollowingPlayer();
        playerController.BeginIntroSequence();
        //Cursor.visible = false;
    }

    public void OnPlayerStartedWalking()
    {
        if (hasStartedWalking) return;
        hasStartedWalking = true;

        walkTextSequencer.BeginSequence();
    }

    public void OnPlayerReachedDialoguePoint()
    {
        if (hasReachedDialoguePoint) return;
        hasReachedDialoguePoint = true;

        walkTextSequencer.StopSequence();

        // Desativa os scripts escolhidos no Inspector
        SetDialogueScriptsEnabled(false);

        dialogueController.BeginDialogue();
       

        
    }

    public void OnDialogueFinished()
    {
        // Reativa os scripts depois que o diálogo terminar
        SetDialogueScriptsEnabled(true);

        cameraController.ZoomOut();
       
        //Cursor.visible = true;
        playerTuto.SetActive(false);
        player.SetActive(true);
        canvas.gameObject.SetActive(true);
        changeAnim.Invoke();
        
    }

    private void SetDialogueScriptsEnabled(bool enabled)
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}