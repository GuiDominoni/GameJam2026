using UnityEngine;

// Responsável apenas pela física do gancho. Não lê input nem faz raycast — só
// reage a comandos explícitos do HookController através dos métodos públicos
// abaixo.
//
// Grudar usa PointEffector2D em vez de Joint: ao grudar, cria uma zona de
// atração temporária (um GameObject com CircleCollider2D trigger +
// PointEffector2D) no ponto de fixação, que atrai continuamente o personagem
// enquanto ele estiver dentro do raio dessa zona — não trava distância nenhuma,
// então o personagem "orbita"/balança ao redor do ponto (raramente parando
// exatamente em cima dele, a menos que chegue quase sem velocidade tangencial),
// com a força variando levemente ao longo do tempo. O raio da zona precisa ser
// bem maior que o alcance do gancho, senão o personagem "sai" da zona no meio do
// balanço e a força some de repente.
public class HookPhysics : MonoBehaviour
{
    private HookConfig config;

    private GameObject effectorZoneObject;
    private Rigidbody2D anchorBody;

    private Rigidbody2D pullableBody;
    private bool isPulling;

    // Deve ser chamado uma vez (ex: no Awake do HookController) antes de qualquer
    // outro método público.
    public void Initialize(HookConfig hookConfig)
    {
        config = hookConfig;
    }

    // Inicia o modo Grudar: cria a zona de atração (PointEffector2D) no ponto de
    // fixação.
    public void BeginGrudar(Rigidbody2D anchor, Vector2 attachPoint, Rigidbody2D attachedBody)
    {
        StopAll(); // garante que não sobra zona/força de um estado anterior

        anchorBody = anchor;

        effectorZoneObject = new GameObject("HookEffectorZone");
        effectorZoneObject.transform.position = attachPoint;

        if (attachedBody != null)
        {
            // Acompanha a superfície móvel, se houver (ex: plataforma).
            effectorZoneObject.transform.SetParent(attachedBody.transform, true);
        }

        CircleCollider2D zoneCollider = effectorZoneObject.AddComponent<CircleCollider2D>();
        zoneCollider.isTrigger = true;
        zoneCollider.usedByEffector = true;
        zoneCollider.radius = config.maxDistance * config.grudarEffectorZoneRadiusMultiplier;

        PointEffector2D effector = effectorZoneObject.AddComponent<PointEffector2D>();
        effector.colliderMask = LayerMask.GetMask("Player");
        effector.forceMagnitude = config.grudarEffectorForce;
        effector.forceVariation = config.grudarEffectorForceVariation;
        effector.forceMode = EffectorForceMode2D.Constant;
        effector.forceSource = EffectorSelection2D.Rigidbody;
        effector.forceTarget = EffectorSelection2D.Rigidbody;
        effector.linearDamping = config.grudarEffectorDrag;
    }

    // Inicia o modo Puxar: passa a aplicar força sobre o Rigidbody2D do objeto
    // puxável, em direção ao personagem, a cada FixedUpdate.
    public void BeginPuxar(Rigidbody2D anchor, Rigidbody2D pullable)
    {
        StopAll();

        anchorBody = anchor;
        pullableBody = pullable;
        isPulling = true;
    }

    // Encerra qualquer física ativa do gancho (zona de atração e/ou força).
    // Destrói o GameObject da zona explicitamente antes de zerar as referências,
    // pra não sobrar zonas "fantasmas" ativas na cena.
    public void StopAll()
    {
        if (effectorZoneObject != null)
        {
            Destroy(effectorZoneObject);
        }
        effectorZoneObject = null;

        isPulling = false;
        pullableBody = null;
        anchorBody = null;
    }

    private void FixedUpdate()
    {
        // Trava de segurança contra picos de velocidade — mais importante ainda
        // aqui: uma força constante sem essa trava (e sem drag suficiente)
        // poderia, em teoria, somar energia a cada balanço sem limite nenhum.
        if (effectorZoneObject != null && anchorBody != null)
        {
            anchorBody.linearVelocity = Vector2.ClampMagnitude(anchorBody.linearVelocity, config.grudarMaxSpeed);
        }

        if (!isPulling || pullableBody == null || anchorBody == null)
        {
            return;
        }

        Vector2 toAnchor = anchorBody.position - pullableBody.position;
        float distance = toAnchor.magnitude;

        // Evita travar a física quando o objeto encosta no personagem: abaixo da
        // distância mínima, a direção normalizada fica instável e a força "vibra"
        // o objeto contra o personagem em vez de simplesmente pará-lo.
        if (distance <= config.pullMinDistance)
        {
            return;
        }

        Vector2 force = (toAnchor / distance) * config.pullForce;
        pullableBody.AddForce(force);
    }
}
