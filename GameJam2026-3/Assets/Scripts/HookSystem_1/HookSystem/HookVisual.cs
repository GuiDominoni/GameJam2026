using UnityEngine;

// Desenha a corda do gancho com um LineRenderer de vários segmentos, simulados
// por um sistema massa-mola leve (sem Physics2D — é só matemática dentro deste
// script), dando sensação de corda mole/elástica em vez de uma linha reta
// rígida. Continua puramente cosmético: não tem Rigidbody2D nem Collider2D, não
// afeta a simulação física do gancho, e só lê TipPosition/CurrentState do
// HookController — nunca altera o estado dele.
//
// A frouxidão da corda é baseada no comprimento REAL registrado no instante em
// que ela gruda em algo (ver HookController.HookState.Attached): quanto mais o
// personagem se aproxima do ponto de fixação em relação a esse comprimento
// original, mais a corda cede no meio. Esticada no limite, fica reta.
[RequireComponent(typeof(LineRenderer))]
public class HookVisual : MonoBehaviour
{
    [Tooltip("Controller do gancho de onde a posição da ponta e o estado são lidos.")]
    [SerializeField] private HookController hookController;

    [Header("Origem da corda")]
    [Tooltip("Deslocamento (espaço local deste objeto) de onde a corda sai, a partir do transform. (0,0) = exatamente a posição deste objeto.")]
    [SerializeField] private Vector2 originOffset = Vector2.zero;

    [Header("Feel da corda (mola)")]
    [Tooltip("Quantos segmentos a corda tem. Mais segmentos = curva mais suave, mas mais caro. 8-12 costuma ser suficiente.")]
    [SerializeField] private int segmentCount = 10;

    [Tooltip("Rigidez da mola que puxa cada ponto de volta pra linha reta entre origem e ponta. Maior = corda mais 'dura'/responsiva; menor = mais 'bamba'.")]
    [SerializeField] private float springStiffness = 150f;

    [Tooltip("Amortecimento do movimento de cada ponto. Maior = a corda para de balançar mais rápido. Se a corda 'explodir'/vibrar sem parar, suba este valor ou abaixe springStiffness.")]
    [SerializeField] private float damping = 10f;

    [Tooltip("Quanto a corda cede no meio quando está TOTALMENTE frouxa (personagem bem perto do ponto de fixação), em unidades de mundo. A frouxidão real varia dinamicamente entre 0 (esticada) e este valor.")]
    [SerializeField] private float maxSag = 0.6f;

    [Tooltip("Quanto o movimento repentino da origem/ponta (personagem sendo puxado, gancho grudando de repente) faz a corda balançar. 0 desliga o efeito.")]
    [SerializeField] private float kickSensitivity = 0.4f;

    private LineRenderer lineRenderer;

    private Vector2[] pointPositions;
    private Vector2[] pointVelocities;
    private Vector2 previousOriginPos;
    private Vector2 previousTipPos;
    private bool wasVisible;

    private HookController.HookState previousState = HookController.HookState.Idle;
    private float ropeLengthAtAttach; // 0 = ainda não grudou nesse ciclo (corda fica reta)

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = segmentCount + 1;
        pointPositions = new Vector2[segmentCount + 1];
        pointVelocities = new Vector2[segmentCount + 1];
    }

    // Ponto de onde a corda sai, em coordenadas de mundo — sempre a partir do
    // transform deste objeto (nunca de uma referência externa), deslocado por
    // originOffset. (0,0) = exatamente a posição deste objeto.
    private Vector2 GetOriginPoint()
    {
        return transform.TransformPoint(originOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 origin = transform.TransformPoint(originOffset);
        Gizmos.DrawWireSphere(origin, 0.12f);
    }

    private void LateUpdate()
    {
        if (hookController == null)
        {
            return;
        }

        bool visible = hookController.IsHookVisible;
        lineRenderer.enabled = visible;

        if (!visible)
        {
            wasVisible = false;
            previousState = hookController.CurrentState;
            return;
        }

        Vector2 originPos = GetOriginPoint();
        Vector2 tipPos = hookController.TipPosition;
        HookController.HookState currentState = hookController.CurrentState;
        bool isAttached = currentState == HookController.HookState.Attached;

        if (!wasVisible)
        {
            // Gancho acabou de aparecer: encosta todos os pontos na linha reta
            // atual, pra não "esticar" a partir de uma posição antiga/zerada.
            ResetPoints(originPos, tipPos);
            wasVisible = true;
        }

        if (isAttached && previousState != HookController.HookState.Attached)
        {
            // Acabou de grudar: registra a distância real nesse instante (essa
            // referência define "corda esticada" dali pra frente) e começa a
            // simulação mole a partir de uma linha reta, sem "pulo" visual.
            ropeLengthAtAttach = Vector2.Distance(originPos, tipPos);
            ResetPoints(originPos, tipPos);
        }

        previousState = currentState;

        if (isAttached)
        {
            SimulateRope(originPos, tipPos);
        }
        else
        {
            // Ainda voando (seguindo o mouse) ou já recolhendo: corda sempre
            // reta e exata, sem NENHUMA simulação de mola — essencial enquanto
            // a ponta está seguindo o mouse ao vivo, pra não parecer atrasada
            // ou fora do lugar.
            SnapStraight(originPos, tipPos);
        }

        for (int i = 0; i <= segmentCount; i++)
        {
            lineRenderer.SetPosition(i, pointPositions[i]);
        }

        previousOriginPos = originPos;
        previousTipPos = tipPos;
    }

    private void ResetPoints(Vector2 originPos, Vector2 tipPos)
    {
        SnapStraight(originPos, tipPos);
        previousOriginPos = originPos;
        previousTipPos = tipPos;
    }

    // Linha reta exata, sem física nenhuma — usada sempre que o gancho não
    // estiver preso a algo (voando ou recolhendo).
    private void SnapStraight(Vector2 originPos, Vector2 tipPos)
    {
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            pointPositions[i] = Vector2.Lerp(originPos, tipPos, t);
            pointVelocities[i] = Vector2.zero;
        }
    }

    private void SimulateRope(Vector2 originPos, Vector2 tipPos)
    {
        // Limita o dt pra não instabilizar a simulação num pico de lag.
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
        if (dt <= 0f)
        {
            return;
        }

        float currentDistance = Vector2.Distance(originPos, tipPos);

        // Frouxidão real: 0 = esticada no comprimento registrado ao grudar,
        // 1 = totalmente frouxa. Enquanto não grudou ainda (ropeLengthAtAttach
        // == 0, ex: durante o voo), fica sempre reta — não há "comprimento
        // total" ainda pra comparar.
        float slack = ropeLengthAtAttach > 0.01f
            ? Mathf.Clamp01((ropeLengthAtAttach - currentDistance) / ropeLengthAtAttach)
            : 0f;

        float effectiveSag = maxSag * slack;

        Vector2 rope = tipPos - originPos;
        Vector2 perpendicular = rope.sqrMagnitude > 0.0001f
            ? new Vector2(-rope.y, rope.x).normalized
            : Vector2.up;

        // "Chute" na corda quando a origem ou a ponta se movem rápido
        // (personagem sendo puxado, objeto sendo arrastado, gancho grudando de
        // repente) — reforça a sensação de mola/elástico reagindo ao movimento.
        Vector2 originKick = (originPos - previousOriginPos) * kickSensitivity;
        Vector2 tipKick = (tipPos - previousTipPos) * kickSensitivity;

        // As pontas ficam sempre exatamente na origem/ponta reais.
        pointPositions[0] = originPos;
        pointPositions[segmentCount] = tipPos;
        pointVelocities[0] = Vector2.zero;
        pointVelocities[segmentCount] = Vector2.zero;

        for (int i = 1; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;

            Vector2 idealPos = Vector2.Lerp(originPos, tipPos, t);
            idealPos += perpendicular * effectiveSag * Mathf.Sin(t * Mathf.PI);
            idealPos += Vector2.Lerp(originKick, tipKick, t);

            Vector2 toIdeal = idealPos - pointPositions[i];
            Vector2 springForce = toIdeal * springStiffness;
            Vector2 dampingForce = -pointVelocities[i] * damping;

            pointVelocities[i] += (springForce + dampingForce) * dt;
            pointPositions[i] += pointVelocities[i] * dt;
        }
    }
}