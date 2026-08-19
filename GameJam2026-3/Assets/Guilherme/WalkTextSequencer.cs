using System.Collections;
using UnityEngine;
using Febucci.UI.Core;

/// <summary>
/// Coloque este script no mesmo GameObject de UI que tem o componente Typewriter
/// do Text Animator (ex: o TMP_Text usado para os textos da caminhada).
/// Preencha a lista "texts" no Inspector com as falas/textos, na ordem em que
/// devem aparecer, com quanto tempo cada um fica visível, quanto tempo leva
/// pra sumir e a pausa antes do próximo.
/// </summary>
public class WalkTextSequencer : MonoBehaviour
{
    [System.Serializable]
    public class WalkText
    {
        [TextArea] public string text;
        public float displayDuration = 2f; // tempo visível, já totalmente revelado
        public float hideDuration = 0.4f;  // tempo estimado até o efeito de sumir terminar
        public float gapAfter = 0.5f;      // pausa antes do próximo texto
    }

    [SerializeField] private TypewriterCore typewriter;
    [SerializeField] private WalkText[] texts;

    private Coroutine routine;
    private bool textFullyShown;

    private void OnEnable()
    {
        typewriter.onTextShowed.AddListener(HandleTextShowed);
    }

    private void OnDisable()
    {
        typewriter.onTextShowed.RemoveListener(HandleTextShowed);
    }

    private void HandleTextShowed() => textFullyShown = true;

    /// <summary>Chamado pelo GameFlowManager quando o player começa a andar.</summary>
    public void BeginSequence()
    {
        routine = StartCoroutine(PlaySequence());
    }

    /// <summary>Chamado pelo GameFlowManager quando o player chega no local do diálogo.</summary>
    public void StopSequence()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        // Se o texto ainda estava sendo digitado, completa ele antes de mandar
        // sumir — chamar StartDisappearingText no meio da digitação pode deixar
        // o efeito do Text Animator num estado inconsistente.
        if (typewriter.isShowingText)
            typewriter.SkipTypewriter();

        typewriter.StartDisappearingText();
    }

    private IEnumerator PlaySequence()
    {
        foreach (var entry in texts)
        {
            textFullyShown = false;
            typewriter.ShowText(entry.text);
            yield return new WaitUntil(() => textFullyShown);

            yield return new WaitForSeconds(entry.displayDuration);

            // Não esperamos mais o evento onTextDisappeared: pedimos pro texto sumir
            // e seguimos em frente depois de um tempo fixo, pra corrotina nunca travar.
            typewriter.StartDisappearingText();
            yield return new WaitForSeconds(entry.hideDuration);

            yield return new WaitForSeconds(entry.gapAfter);
        }
    }
}