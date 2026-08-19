using UnityEngine;

/// <summary>
/// Coloque este script em um GameObject vazio posicionado no local do mapa
/// onde o diálogo deve começar. Adicione um Collider2D nesse mesmo GameObject
/// marcado como "Is Trigger".
///
/// Use esta zona OU o fim da Timeline (PlayerIntroController) pra disparar o
/// diálogo — não precisa das duas coisas, mas se ambas estiverem ativas não
/// tem problema: o GameFlowManager ignora a segunda chamada.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTriggerZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        GameFlowManager.Instance.OnPlayerReachedDialoguePoint();
    }
}