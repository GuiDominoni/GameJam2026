using UnityEngine;

// Orquestra o sistema de gancho: lê input, detecta o alvo no ponto onde o mouse
// está (limitado por maxDistance), controla a máquina de estados explícita e
// delega física (HookPhysics) e visual (HookVisual) aos componentes especializados.
// Não aplica força nem desenha a corda diretamente.
//
// Não depende de nenhuma implementação específica de personagem (ragdoll, sprite
// simples, character controller etc): funciona com qualquer Rigidbody2D atribuído
// manualmente no campo "Character Anchor" abaixo.
[RequireComponent(typeof(HookPhysics))]
public class HookController : MonoBehaviour
{
    // Máquina de estados explícita (enum), em vez de flags booleanas soltas, para
    // evitar bugs de estado inconsistente. Todas as transições passam por
    // ChangeState — nunca são setadas diretamente em outro lugar.
    public enum HookState
    {
        Idle,
        Aiming,
        Firing,
        Attached,
        Retracting
    }

    [Header("Referências")]
    [Tooltip("Rigidbody2D que serve de 'âncora' do gancho (o personagem). Atribuído manualmente — o sistema nunca descobre isso por nome/tag/hierarquia.")]
    [SerializeField] private Rigidbody2D characterAnchor;

    [Tooltip("Asset de configuração do gancho (ScriptableObject). Troque o asset para ter variações (ex: 'Gancho Fraco'/'Gancho Forte') sem duplicar nenhum script.")]
    [SerializeField] private HookConfig config;

    [Tooltip("Componente responsável pela física do gancho (Joint/AddForce). Se vazio, usa o HookPhysics no mesmo GameObject.")]
    [SerializeField] private HookPhysics hookPhysics;

    [Tooltip("Componente responsável por desenhar a corda (LineRenderer). Opcional.")]
    [SerializeField] private HookVisual hookVisual;

    [Tooltip("Eventos de extensão (som, UI, câmera, feedback via DOTween, etc). Opcional.")]
    [SerializeField] private HookEvents events;

    [Tooltip("Câmera usada para converter a posição do mouse em ponto de mundo ao mirar. Se vazio, usa Camera.main.")]
    [SerializeField] private Camera aimCamera;

    [Header("Estado atual (somente leitura — útil para debug no Inspector)")]
    [SerializeField] private HookState currentState = HookState.Idle;
    // Não é mais escolhido pelo jogador: é decidido sozinho em DetectTargetAt,
    // de acordo com o componente (Grabbable/Pullable) do que o gancho encontrar.
    [SerializeField] private HookMode currentMode = HookMode.Grudar;

    // Posição atual da ponta do gancho, em coordenadas de mundo. Lida pelo HookVisual.
    public Vector2 TipPosition { get; private set; }

    // Direção atual da mira (personagem -> mouse). Não é usada para a detecção do
    // alvo (isso é por ponto, ver GetClampedAimPoint), mas fica exposta como
    // utilidade caso você queira um indicador visual de mira no futuro.
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    // Verdadeiro só enquanto o gancho está de fato em ação (voando, preso ou
    // recolhendo). Falso em Idle/Aiming — parado, não há corda pra mostrar.
    public bool IsHookVisible =>
        currentState == HookState.Firing
        || currentState == HookState.Attached
        || currentState == HookState.Retracting;

    public HookState CurrentState => currentState;
    public HookMode CurrentMode => currentMode;

    // --- Dados internos do lançamento/alvo atual ---
    private Vector2 aimWorldPoint;
    private GameObject currentTarget;
    private Rigidbody2D currentTargetBody; // Rigidbody2D do alvo, se houver (Grudar com plataforma móvel, ou Puxar)
    private Vector2 attachPoint;       // ponto exato (visual da corda, eventos) — sempre onde o mouse mirou
    private Vector2 physicsAttachPoint; // ponto que a física do Grudar mira — pode ter uma margem de segurança da parede
    private HookAnchorPoint currentAnchorPoint; // se o alvo tiver um, o ponto de fixação sempre é este, ao vivo
    private Vector2 firingEndpoint;

    private void Awake()
    {
        if (characterAnchor == null)
        {
            Debug.LogError("[HookController] Character Anchor (Rigidbody2D) não atribuído.", this);
            enabled = false;
            return;
        }

        if (config == null)
        {
            Debug.LogError("[HookController] HookConfig não atribuído.", this);
            enabled = false;
            return;
        }

        // Evita que o personagem atravesse paredes ao ser puxado rápido pelo gancho:
        // Discrete (padrão do Unity) pode deixar corpos rápidos "pularem" por cima
        // de colliders finos entre um FixedUpdate e outro.
        characterAnchor.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        characterAnchor.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (hookPhysics == null)
        {
            hookPhysics = GetComponent<HookPhysics>();
        }

        hookPhysics.Initialize(config);
    }

    private void Start()
    {
        ChangeState(HookState.Idle);
    }

    private void Update()
    {
        UpdateAim();
        HandleFireInput();

        switch (currentState)
        {
            case HookState.Aiming:
                // Mantém a ponta grudada no personagem enquanto ele se move em
                // repouso — sem isso, a ponta fica "congelada" onde o personagem
                // estava da última vez que entrou nesse estado.
                TipPosition = characterAnchor.position;
                break;

            case HookState.Firing:
                TickFiring();
                break;

            case HookState.Attached:
                TickAttached();
                break;

            case HookState.Retracting:
                TickRetracting();
                break;
        }
    }

    // --- Input ---
    // Usa o Input Manager legado (Input.*) por simplicidade; troque por uma leitura
    // via novo Input System aqui se o projeto passar a usá-lo — nenhum outro script
    // depende deste detalhe.

    private void UpdateAim()
    {
        if (aimCamera == null)
        {
            return;
        }

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(aimCamera.transform.position.z - characterAnchor.transform.position.z);
        aimWorldPoint = aimCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 toMouse = aimWorldPoint - characterAnchor.position;
        if (toMouse.sqrMagnitude > 0.0001f)
        {
            AimDirection = toMouse.normalized;
        }
    }

    // Ponto para onde o gancho deve ir: exatamente onde o mouse está, a menos que
    // isso ultrapasse maxDistance — nesse caso, vai até o mais longe que conseguir
    // na direção do mouse.
    private Vector2 GetClampedAimPoint()
    {
        Vector2 origin = characterAnchor.position;
        Vector2 toPoint = aimWorldPoint - origin;
        float distance = toPoint.magnitude;

        if (distance <= config.maxDistance || distance < 0.0001f)
        {
            return aimWorldPoint;
        }

        return origin + (toPoint / distance) * config.maxDistance;
    }

    private void HandleFireInput()
    {
        bool fireDown = Input.GetMouseButtonDown(0);
        bool fireHeld = Input.GetMouseButton(0);

        if (fireDown)
        {
            bool hookAlreadyActive = currentState == HookState.Firing || currentState == HookState.Attached;

            if (hookAlreadyActive)
            {
                // Caso-limite (seção 7): botão pressionado de novo com gancho já ativo.
                if (config.refireBehaviour == HookConfig.RefireBehaviour.CancelAndRetract)
                {
                    ChangeState(HookState.Retracting);
                }
                return;
            }

            if (currentState == HookState.Idle || currentState == HookState.Aiming)
            {
                ChangeState(HookState.Firing);
            }

            return;
        }

        // O jogador precisa MANTER o botão pressionado enquanto o gancho viaja e
        // enquanto fica preso (ou parado no ar, sem alvo — ver TickFiring). Soltar
        // a qualquer momento cancela/recolhe na hora. Checar o estado "segurando" a
        // cada frame (em vez de só o evento pontual de soltar) evita perder o
        // release quando ele acontece durante o estado Firing.
        if (!fireHeld && (currentState == HookState.Firing || currentState == HookState.Attached))
        {
            ChangeState(HookState.Retracting);
        }
    }

    // --- Ticks por estado ---

    private void TickFiring()
    {
        if (currentTarget == null)
        {
            // Ainda não achou nada: o ponto mirado segue o mouse ao vivo (limitado
            // por maxDistance), e a cada frame testamos de novo se ele passou por
            // cima de um Grabbable/Pullable. Isso cobre tanto o "voo" inicial
            // quanto o período parado no ar esperando o jogador mirar em algo —
            // é a mesma lógica, sem estado especial pra cada caso.
            firingEndpoint = GetClampedAimPoint();
            DetectTargetAt(firingEndpoint);
        }

        TipPosition = Vector2.MoveTowards(TipPosition, firingEndpoint, config.launchSpeed * Time.deltaTime);

        if (currentTarget != null && Vector2.Distance(TipPosition, firingEndpoint) <= 0.01f)
        {
            ChangeState(HookState.Attached);
        }

        // Se ainda não achou alvo, continua em Firing, seguindo o mouse — só sai
        // desse estado quando o jogador soltar o botão (ver HandleFireInput).
    }

    private void TickAttached()
    {
        // Caso-limite (seção 7): alvo destruído/desativado enquanto o gancho está
        // preso a ele — solta sem lançar exceção e volta a Retracting.
        if (currentTarget == null || !currentTarget.activeInHierarchy)
        {
            ChangeState(HookState.Retracting);
            return;
        }

        // Ponto de ancoragem definido tem prioridade — recalculado ao vivo pra
        // acompanhar o objeto se ele girar/mover (ex: plataforma, objeto puxado).
        if (currentAnchorPoint != null)
        {
            TipPosition = currentAnchorPoint.WorldPosition;
        }
        else
        {
            TipPosition = currentTargetBody != null ? currentTargetBody.position : attachPoint;
        }
    }

    private void TickRetracting()
    {
        TipPosition = Vector2.MoveTowards(TipPosition, characterAnchor.position, config.retractSpeed * Time.deltaTime);

        if (Vector2.Distance(TipPosition, characterAnchor.position) < 0.05f)
        {
            ChangeState(HookState.Idle);
        }
    }

    // --- Detecção de alvo (OverlapCircle no ponto de destino) ---
    // Não há mais escolha manual de modo: checa os dois LayerMasks (grabbable e
    // pullable) de uma vez só, e o modo é decidido pelo componente que o objeto
    // encontrado realmente tem — Grabbable vira Grudar, Pullable vira Puxar.
    // Nunca por nome de objeto ou tag. Não considera obstáculos no caminho (fora
    // de escopo, seção "Fora de escopo" da spec).
    // Se um objeto tiver os dois componentes (caso incomum), Grabbable tem
    // prioridade.
    //
    // Se não achar nada exatamente no ponto mirado, tenta um "ímã": procura o
    // Grabbable/Pullable mais próximo dentro de um raio maior (magnetRadius) e
    // gruda nele direto, sem precisar de mira pixel-perfeita.
    //
    // Se o alvo encontrado tiver um HookAnchorPoint, o ponto de fixação é sempre
    // o dele (ao vivo) — não onde o gancho realmente bateu.

    private void DetectTargetAt(Vector2 point)
    {
        currentTarget = null;
        currentTargetBody = null;
        currentAnchorPoint = null;

        LayerMask combinedMask = config.grabbableLayerMask | config.pullableLayerMask;
        Collider2D hitCollider = Physics2D.OverlapCircle(point, config.aimDetectionRadius, combinedMask);

        if (hitCollider == null && config.magnetRadius > config.aimDetectionRadius)
        {
            hitCollider = FindClosestMagnetTarget(point, combinedMask);
        }

        if (hitCollider == null)
        {
            return;
        }

        Vector2 targetPoint = point;
        if (hitCollider.TryGetComponent<HookAnchorPoint>(out var anchor))
        {
            targetPoint = anchor.WorldPosition;
        }

        if (hitCollider.TryGetComponent<Grabbable>(out _))
        {
            currentMode = HookMode.Grudar;
            currentTarget = hitCollider.gameObject;
            hitCollider.TryGetComponent(out Rigidbody2D targetBody); // opcional: superfície pode ser móvel
            currentTargetBody = targetBody;
            currentAnchorPoint = anchor;
            physicsAttachPoint = ComputeSafePhysicsPoint(targetPoint, hitCollider, combinedMask);
        }
        else if (hitCollider.TryGetComponent<Pullable>(out var pullable))
        {
            currentMode = HookMode.Puxar;
            currentTarget = hitCollider.gameObject;
            currentTargetBody = pullable.Rigidbody2D;
            currentAnchorPoint = anchor;
            physicsAttachPoint = targetPoint; // Puxar não usa este valor, mantido só por consistência
        }

        if (currentTarget == null)
        {
            return; // bateu em algo do layer, mas sem Grabbable/Pullable — trata como miss
        }

        events?.OnModeChanged?.Invoke(currentMode);

        // O ponto VISUAL (corda, eventos) é o ponto de ancoragem se houver, ou
        // exatamente onde bateu, senão. Quem tem margem de segurança da parede é
        // só o physicsAttachPoint (Grudar), usado internamente pelo joint.
        attachPoint = targetPoint;
    }

    // Busca, dentro de magnetRadius, o Grabbable/Pullable mais próximo do ponto
    // mirado que também tenha um HookAnchorPoint (o ímã só vale pra esses — sem
    // ele, é preciso mirar exatamente em cima, ex: paredes). Retorna null se
    // nada elegível for encontrado dentro do raio.
    private Collider2D FindClosestMagnetTarget(Vector2 point, LayerMask mask)
    {
        Collider2D[] candidates = Physics2D.OverlapCircleAll(point, config.magnetRadius, mask);

        Collider2D closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D candidate in candidates)
        {
            // O ímã só vale pra quem tem um ponto de ancoragem definido — objetos
            // sem HookAnchorPoint (ex: paredes) continuam exigindo mira exata.
            if (!candidate.TryGetComponent<HookAnchorPoint>(out _))
            {
                continue;
            }

            bool isValidTarget = candidate.TryGetComponent<Grabbable>(out _) || candidate.TryGetComponent<Pullable>(out _);
            if (!isValidTarget)
            {
                continue;
            }

            float distance = Vector2.Distance(point, candidate.ClosestPoint(point));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    // Calcula um ponto seguro para a FÍSICA do Grudar, afastado da superfície da
    // parede pela normal do impacto — sem alterar o ponto visual (attachPoint
    // continua exato). Sem essa margem: o personagem pendula com o CENTRO dele a
    // uma distância fixa do ponto de fixação, mas o collider dele tem raio — nas
    // partes mais baixas do balanço, a borda do collider pode roçar/atravessar a
    // própria parede em que está grudado.
    private Vector2 ComputeSafePhysicsPoint(Vector2 desiredPoint, Collider2D hitCollider, LayerMask mask)
    {
        Vector2 origin = characterAnchor.position;
        RaycastHit2D surfaceHit = Physics2D.Linecast(origin, desiredPoint, mask);

        if (surfaceHit.collider == hitCollider)
        {
            return surfaceHit.point + surfaceHit.normal * config.grudarSurfaceOffset;
        }

        return desiredPoint;
    }

    // --- Transição central de estados ---
    // Único ponto de mutação de currentState: limpa o estado anterior e configura
    // o novo, para que nenhum outro método precise setar currentState diretamente.

    private void ChangeState(HookState newState)
    {
        if (currentState == HookState.Attached)
        {
            hookPhysics.StopAll();

            if (currentMode == HookMode.Puxar && currentTarget != null)
            {
                events?.OnObjectReleased?.Invoke(currentTarget);
            }

            events?.OnHookReleased?.Invoke();
        }

        currentState = newState;

        switch (newState)
        {
            case HookState.Idle:
                currentTarget = null;
                currentTargetBody = null;
                currentAnchorPoint = null;
                TipPosition = characterAnchor.position;
                // Idle é transitório: assim que reseta, volta a ficar pronto para mirar.
                ChangeState(HookState.Aiming);
                break;

            case HookState.Aiming:
                TipPosition = characterAnchor.position;
                break;

            case HookState.Firing:
                TipPosition = characterAnchor.position;
                currentTarget = null;
                currentTargetBody = null;
                events?.OnHookFired?.Invoke();
                break;

            case HookState.Attached:
                events?.OnHookAttached?.Invoke(currentTarget, attachPoint);

                if (currentMode == HookMode.Grudar)
                {
                    hookPhysics.BeginGrudar(characterAnchor, physicsAttachPoint, currentTargetBody);
                }
                else if (currentTargetBody != null)
                {
                    hookPhysics.BeginPuxar(characterAnchor, currentTargetBody);
                    events?.OnObjectGrabbed?.Invoke(currentTarget);
                }
                else
                {
                    // Segurança: modo Puxar sem Rigidbody2D no alvo — não há o que puxar.
                    ChangeState(HookState.Retracting);
                }
                break;

            case HookState.Retracting:
                // A corda começa a recolher a partir da posição atual da ponta.
                break;
        }
    }
}