using UnityEngine;

// Desenha a corda do gancho como uma linha reta simples entre o personagem e a
// ponta do gancho (LineRenderer de 2 pontos). A física real (HookPhysics, via
// PointEffector2D no modo Grudar) já produz o balanço/trajetória de verdade —
// uma linha reta ligando as posições reais mostra esse movimento sozinha, sem
// precisar de nenhuma simulação visual separada que pudesse ficar "descolada"
// do que a física real está fazendo.
//
// Continua puramente cosmético: não tem Rigidbody2D nem Collider2D, não afeta a
// simulação física do gancho, e só lê TipPosition/IsHookVisible do
// HookController — nunca altera o estado dele.
[RequireComponent(typeof(LineRenderer))]
public class HookVisual : MonoBehaviour
{
    [Tooltip("Controller do gancho de onde a posição da ponta e a visibilidade são lidas.")]
    [SerializeField] private HookController hookController;

    [Tooltip("Rigidbody2D do personagem, usado como ponto de origem da corda.")]
    [SerializeField] private Rigidbody2D characterAnchor;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    private void LateUpdate()
    {
        if (hookController == null || characterAnchor == null)
        {
            return;
        }

        bool visible = hookController.IsHookVisible;
        lineRenderer.enabled = visible;

        if (!visible)
        {
            return;
        }

        lineRenderer.SetPosition(0, characterAnchor.position);
        lineRenderer.SetPosition(1, hookController.TipPosition);
    }
}
