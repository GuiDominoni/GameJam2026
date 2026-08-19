using UnityEngine;
using Febucci.UI.Core;

/// <summary>
/// Coloque este script em um GameObject de controle do diálogo (pode ser o
/// mesmo Canvas do diálogo). Preencha "lines" com as falas, "objectsToActivate"
/// com os objetos que devem ligar quando o diálogo começa, e arraste o
/// Typewriter do texto de diálogo (pode ser o mesmo do menu ou um separado).
/// Clique do mouse: se o texto ainda está sendo revelado, o clique completa
/// ele na hora; se já está totalmente revelado, o clique avança pra próxima fala.
/// </summary>
public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea] public string text;
    }

    [Header("Referências")]
    [SerializeField] private TypewriterCore typewriter;
    [SerializeField] private GameObject dialogueBox;

    [Header("Conteúdo do diálogo")]
    [SerializeField] private DialogueLine[] lines;

    [Header("Objetos ativados quando o diálogo começa")]
    [SerializeField] private GameObject[] objectsToActivate;

    private int currentIndex;
    private bool dialogueActive;

    /// <summary>Chamado pelo GameFlowManager quando o player chega no local do diálogo.</summary>
    public void BeginDialogue()
    {
        // Protege contra reentrada (ex: se algo chamar BeginDialogue duas vezes,
        // o diálogo não reinicia do zero no meio de uma fala).
        if (dialogueActive) return;
        dialogueActive = true;
        currentIndex = -1;

        if (dialogueBox != null) dialogueBox.SetActive(true);

        foreach (var obj in objectsToActivate)
            if (obj != null) obj.SetActive(true);

        ShowNextLine();
    }

    private void Update()
    {
        if (!dialogueActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (typewriter.isShowingText)
            typewriter.SkipTypewriter();
        else
            ShowNextLine();
    }

    private void ShowNextLine()
    {
        currentIndex++;

        if (currentIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        typewriter.ShowText(lines[currentIndex].text);
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);

        GameFlowManager.Instance.OnDialogueFinished();
    }
}