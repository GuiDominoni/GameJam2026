using UnityEngine;

// Componente OPCIONAL. Quando presente num objeto (junto com Grabbable ou
// Pullable), o gancho sempre gruda exatamente neste ponto — em vez de onde o
// jogador mirou dentro da área do objeto. Sem este componente, o gancho gruda
// exatamente onde mirou (comportamento normal, é o que você quer pra paredes:
// não adicione este componente nelas).
//
// O ponto é definido como um deslocamento em espaço LOCAL do objeto — segue
// posição, rotação e escala dele automaticamente. (0,0) = exatamente o
// centro/pivot do objeto.
public class HookAnchorPoint : MonoBehaviour
{
    [Tooltip("Deslocamento (espaço local do objeto) do ponto de ancoragem a partir do centro/pivot dele. (0,0) = exatamente o centro.")]
    [SerializeField] private Vector2 localOffset = Vector2.zero;

    // Ponto de ancoragem em coordenadas de mundo, já considerando a posição,
    // rotação e escala atuais do objeto — recalcule a cada frame se o objeto se
    // mover (não é armazenado em cache).
    public Vector2 WorldPosition => transform.TransformPoint(localOffset);

    // Desenha o ponto de ancoragem na Scene view pra facilitar ajustar o
    // deslocamento no Inspector sem precisar dar Play.
    private void OnDrawGizmosSelected()
    {
        Vector2 worldPos = WorldPosition;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(worldPos, 0.15f);
        Gizmos.DrawLine(transform.position, worldPos);
    }
}