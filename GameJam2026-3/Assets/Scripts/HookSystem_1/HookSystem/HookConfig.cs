using UnityEngine;

// Todos os valores ajustáveis do gancho, num asset separado do código.
// Permite ter múltiplas variações (ex: "Gancho Fraco", "Gancho Forte") sem duplicar
// nenhum script — basta criar outro asset e trocar a referência no HookController.
[CreateAssetMenu(fileName = "HookConfig", menuName = "Gancho/Hook Config")]
public class HookConfig : ScriptableObject
{
    // O que fazer se o jogador apertar o botão de lançar com um gancho já ativo
    // (estado Firing ou Attached). Ver seção 7 (casos-limite) da especificação.
    public enum RefireBehaviour
    {
        Ignore,           // Ignora o novo input; o gancho atual continua normalmente.
        CancelAndRetract  // Cancela o gancho atual e começa a recolher imediatamente.
    }

    [Header("Alcance e Velocidade")]
    [Tooltip("Distância máxima (unidades de mundo) que o gancho pode percorrer. Se o mouse estiver além disso, o gancho vai só até onde alcançar na direção do mouse.")]
    public float maxDistance = 10f;

    [Tooltip("Velocidade (unidades/segundo) com que a ponta do gancho viaja até o ponto mirado durante o lançamento (estado Firing).")]
    public float launchSpeed = 25f;

    [Tooltip("Velocidade (unidades/segundo) com que o gancho recolhe de volta ao personagem (estado Retracting). Suba esse valor se quiser um retorno bem rápido ao soltar o botão.")]
    public float retractSpeed = 20f;

    [Header("Física — Modo Puxar (AddForce)")]
    [Tooltip("Magnitude da força (via Rigidbody2D.AddForce) aplicada ao objeto puxável em direção ao personagem, a cada FixedUpdate.")]
    public float pullForce = 15f;

    [Tooltip("Distância mínima (unidades) abaixo da qual a força de puxão para de ser aplicada. Evita travar/vibrar a física quando o objeto encosta no personagem.")]
    public float pullMinDistance = 0.3f;

    [Header("Física — Modo Grudar (PointEffector2D)")]
    [Tooltip("Força (magnitude) que a zona de atração aplica sobre o personagem enquanto ele estiver dentro do raio dela.")]
    public float grudarEffectorForce = 25f;

    [Tooltip("Variação aleatória (+/-) somada à força a cada passo de física. É o que dá aquela sensação de força 'pulsando', nunca 100% constante.")]
    public float grudarEffectorForceVariation = 5f;

    [Tooltip("Multiplicador sobre Max Distance pra definir o raio da zona de atração. Precisa ser bem maior que 1 — se o personagem sair da zona no meio do balanço, a força some de repente. 1.5-2 costuma ser seguro.")]
    public float grudarEffectorZoneRadiusMultiplier = 1.75f;

    [Tooltip("Arrasto (drag) aplicado ao personagem enquanto estiver dentro da zona de atração. Uma força constante sem nenhum arrasto pode ir somando velocidade sem limite ao longo de vários balanços seguidos — um pouco de arrasto evita isso sem matar o impulso. 0 = nenhuma perda de energia.")]
    public float grudarEffectorDrag = 0.08f;

    [Tooltip("Distância (unidades) que o CENTRO da zona de atração é afastado da superfície da parede, na direção da normal do impacto. Reduz o quanto o personagem se aproxima da parede sólida no ponto de maior aproximação do balanço. Ajuste pelo raio do collider do personagem.")]
    public float grudarSurfaceOffset = 0.3f;

    [Tooltip("Multiplicador sobre a distância real (no momento em que gruda) que define o teto de quão longe o personagem pode ficar do ponto de fixação. 1.0 = nunca mais longe do que estava ao grudar (corda 'de verdade', sem esticar) — é isso que garante o balanço cruzando limpo pro lado oposto, em vez de espiralar pra fora do eixo. Valores um pouco acima de 1 (ex: 1.1) dão uma pequena folga extra.")]
    public float grudarMaxDistanceMultiplier = 1f;

    [Tooltip("Velocidade máxima (unidades/segundo) do personagem enquanto grudado. Trava de segurança contra picos de física — sem ela, a força constante do effector poderia, em tese, acumular velocidade sem limite ao longo de vários balanços.")]
    public float grudarMaxSpeed = 40f;

    [Header("Detecção de Mira")]
    [Tooltip("Camadas (layers) consideradas 'grudáveis' para o modo Grudar. Configurável aqui — nunca hardcoded no código.")]
    public LayerMask grabbableLayerMask;

    [Tooltip("Camadas (layers) consideradas 'puxáveis' para o modo Puxar. Configurável aqui — nunca hardcoded no código.")]
    public LayerMask pullableLayerMask;

    [Tooltip("Raio (unidades) do OverlapCircle usado para detectar um alvo válido bem no ponto onde o gancho para (posição do mouse, já limitada pelo Max Distance). Dá tolerância a pequenos desvios de precisão do jogador.")]
    public float aimDetectionRadius = 0.15f;

    [Tooltip("Raio (unidades) de 'ímã': se o ponto mirado não acertar nada em cima, procura o Grabbable/Pullable mais próximo dentro desse raio maior e gruda nele direto — não precisa de mira pixel-perfeita. Deixe igual ou menor que Aim Detection Radius pra desligar o ímã.")]
    public float magnetRadius = 0.6f;

    [Header("Regras de Estado")]
    [Tooltip("Define o comportamento ao apertar o botão de lançar com um gancho já ativo (Firing ou Attached).")]
    public RefireBehaviour refireBehaviour = RefireBehaviour.Ignore;
}