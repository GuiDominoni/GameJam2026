using UnityEngine;

// Orquestra o sistema de gancho: lê o input, detecta o alvo no ponto
// onde o mouse está (limitado por maxDistance), controla a máquina de
// estados explícita e delega a física (HookPhysics) e o visual
// (HookVisual) para os componentes especializados.
//
// Não aplica força nem desenha a corda diretamente.
//
// Não depende de nenhuma implementação específica de personagem
// (ragdoll, sprite simples, character controller etc.).
// Funciona com qualquer Rigidbody2D atribuído manualmente
// no campo "Character Anchor".

[RequireComponent(typeof(HookPhysics))]
public class HookController : MonoBehaviour
{
    // =========================================================
    // MÁQUINA DE ESTADOS
    // =========================================================

    // Máquina de estados explícita usando enum em vez de várias
    // flags booleanas separadas.
    //
    // Isso evita estados inconsistentes.
    //
    // Todas as mudanças de estado passam por ChangeState().
    // currentState nunca deve ser alterado diretamente em outro lugar.

    public enum HookState
    {
        Idle,
        Aiming,
        Firing,
        Attached,
        Retracting
    }

    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("References")]

    [Tooltip("Rigidbody2D que serve como âncora do gancho (o personagem).")]
    [SerializeField] private Rigidbody2D characterAnchor;

    [Tooltip("Asset de configuração do gancho (ScriptableObject).")]
    [SerializeField] private HookConfig config;

    [Tooltip("Componente responsável pela física do gancho.")]
    [SerializeField] private HookPhysics hookPhysics;

    [Tooltip("Componente responsável por desenhar a corda.")]
    [SerializeField] private HookVisual hookVisual;

    [Tooltip("Eventos do gancho.")]
    [SerializeField] private HookEvents events;

    [Tooltip("Câmera usada para converter a posição do mouse para o mundo.")]
    [SerializeField] private Camera aimCamera;

    // =========================================================
    // SISTEMA DE LIXO
    // =========================================================

    [Header("Trash System")]

    [Tooltip("Controlador responsável pelo sistema de lixo e pela seta da lixeira correta.")]
    [SerializeField] private TrashGameController trashGameController;

    // =========================================================
    // ESTADO ATUAL
    // =========================================================

    [Header("Current State (Read Only)")]

    [SerializeField] private HookState currentState = HookState.Idle;

    // O modo do gancho é decidido automaticamente de acordo
    // com o componente encontrado no objeto.
    //
    // Grabbable -> Grudar
    // Pullable  -> Puxar

    [SerializeField] private HookMode currentMode = HookMode.Grudar;

    // Posição atual da ponta do gancho em coordenadas do mundo.
    // É utilizada pelo HookVisual.

    public Vector2 TipPosition { get; private set; }

    // Direção atual da mira (personagem -> mouse).
    // Não é utilizada para detectar o alvo, mas pode ser usada
    // futuramente para indicadores visuais.

    public Vector2 AimDirection { get; private set; } = Vector2.right;

    // Verdadeiro enquanto o gancho está realmente ativo:
    // Firing, Attached ou Retracting.

    public bool IsHookVisible =>
        currentState == HookState.Firing
        || currentState == HookState.Attached
        || currentState == HookState.Retracting;

    public HookState CurrentState => currentState;

    public HookMode CurrentMode => currentMode;

    // =========================================================
    // DADOS DO ALVO ATUAL
    // =========================================================

    // Posição atual da mira do mouse no mundo.
    private Vector2 aimWorldPoint;

    // Objeto atualmente detectado pelo gancho.
    private GameObject currentTarget;

    // Rigidbody2D do alvo atual, caso exista.
    // É usado para objetos móveis ou objetos Pullable.

    private Rigidbody2D currentTargetBody;

    // Ponto exato onde o gancho visualmente está preso.
    // É usado pela corda e pelos eventos.

    private Vector2 attachPoint;

    // Ponto utilizado pela física para prender o gancho.
    // Pode possuir uma margem de segurança quando estiver
    // preso em uma parede.

    private Vector2 physicsAttachPoint;

    // HookAnchorPoint do alvo atual, caso ele possua um.

    private HookAnchorPoint currentAnchorPoint;

    // Ponto final atual do gancho durante o lançamento.

    private Vector2 firingEndpoint;

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        if (characterAnchor == null)
        {
            Debug.LogError(
                "[HookController] Character Anchor (Rigidbody2D) não foi atribuído.",
                this
            );

            enabled = false;
            return;
        }

        if (config == null)
        {
            Debug.LogError(
                "[HookController] HookConfig não foi atribuído.",
                this
            );

            enabled = false;
            return;
        }

        // Evita que o personagem atravesse paredes quando
        // estiver sendo puxado rapidamente pelo gancho.
        //
        // O modo Continuous é utilizado porque o modo Discrete
        // pode permitir que objetos rápidos atravessem colliders finos.

        characterAnchor.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        characterAnchor.interpolation =
            RigidbodyInterpolation2D.Interpolate;

        // Se nenhuma câmera foi atribuída manualmente,
        // utiliza a câmera principal.

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        // Se o HookPhysics não foi atribuído manualmente,
        // procura o componente no mesmo GameObject.

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

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateAim();

        HandleFireInput();

        switch (currentState)
        {
            case HookState.Aiming:

                // Mantém a ponta do gancho presa ao personagem
                // enquanto ele está mirando.

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

    // =========================================================
    // INPUT
    // =========================================================

    private void UpdateAim()
    {
        if (aimCamera == null)
        {
            return;
        }

        Vector3 mouseScreen = Input.mousePosition;

        mouseScreen.z = Mathf.Abs(
            aimCamera.transform.position.z -
            characterAnchor.transform.position.z
        );

        aimWorldPoint =
            aimCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 toMouse =
            aimWorldPoint - characterAnchor.position;

        if (toMouse.sqrMagnitude > 0.0001f)
        {
            AimDirection = toMouse.normalized;
        }
    }

    // Retorna o ponto para onde o gancho deve ir.
    //
    // Se o mouse estiver além de maxDistance,
    // o gancho para na distância máxima permitida.

    private Vector2 GetClampedAimPoint()
    {
        Vector2 origin = characterAnchor.position;

        Vector2 toPoint =
            aimWorldPoint - origin;

        float distance = toPoint.magnitude;

        if (distance <= config.maxDistance ||
            distance < 0.0001f)
        {
            return aimWorldPoint;
        }

        return origin +
               (toPoint / distance) *
               config.maxDistance;
    }

    private void HandleFireInput()
    {
        bool fireDown = Input.GetMouseButtonDown(0);

        bool fireHeld = Input.GetMouseButton(0);

        if (fireDown)
        {
            bool hookAlreadyActive =
                currentState == HookState.Firing ||
                currentState == HookState.Attached;

            if (hookAlreadyActive)
            {
                // Caso o botão seja pressionado novamente enquanto
                // o gancho já estiver ativo, cancela e recolhe o gancho.

                if (config.refireBehaviour ==
                    HookConfig.RefireBehaviour.CancelAndRetract)
                {
                    ChangeState(HookState.Retracting);
                }

                return;
            }

            if (currentState == HookState.Idle ||
                currentState == HookState.Aiming)
            {
                ChangeState(HookState.Firing);
            }

            return;
        }

        // O jogador precisa manter o botão pressionado enquanto
        // o gancho estiver voando ou preso.
        //
        // Ao soltar o botão, o gancho começa a ser recolhido.

        if (!fireHeld &&
            (currentState == HookState.Firing ||
             currentState == HookState.Attached))
        {
            ChangeState(HookState.Retracting);
        }
    }

    // =========================================================
    // ESTADO FIRING
    // =========================================================

    private void TickFiring()
    {
        if (currentTarget == null)
        {
            // O gancho ainda não encontrou um alvo.
            // O ponto final acompanha o mouse respeitando maxDistance.

            firingEndpoint = GetClampedAimPoint();

            // Verifica se existe um alvo válido no ponto atual.

            DetectTargetAt(firingEndpoint);
        }

        // Move a ponta do gancho em direção ao alvo.

        TipPosition = Vector2.MoveTowards(
            TipPosition,
            firingEndpoint,
            config.launchSpeed * Time.deltaTime
        );

        // Quando o gancho chega ao alvo,
        // muda para o estado Attached.

        if (currentTarget != null &&
            Vector2.Distance(
                TipPosition,
                firingEndpoint
            ) <= 0.01f)
        {
            ChangeState(HookState.Attached);
        }
    }

    // =========================================================
    // ESTADO ATTACHED
    // =========================================================

    private void TickAttached()
    {
        // Caso o alvo seja destruído ou desativado enquanto
        // o gancho estiver preso, recolhe o gancho com segurança.

        if (currentTarget == null ||
            !currentTarget.activeInHierarchy)
        {
            ChangeState(HookState.Retracting);
            return;
        }

        // Se o alvo possuir um HookAnchorPoint,
        // acompanha esse ponto em tempo real.

        if (currentAnchorPoint != null)
        {
            TipPosition =
                currentAnchorPoint.WorldPosition;
        }
        else
        {
            // Caso não exista HookAnchorPoint,
            // acompanha o Rigidbody2D ou utiliza o ponto original.

            TipPosition =
                currentTargetBody != null
                    ? currentTargetBody.position
                    : attachPoint;
        }
    }

    // =========================================================
    // ESTADO RETRACTING
    // =========================================================

    private void TickRetracting()
    {
        // Move a ponta do gancho de volta para o personagem.

        TipPosition = Vector2.MoveTowards(
            TipPosition,
            characterAnchor.position,
            config.retractSpeed * Time.deltaTime
        );

        // Quando o gancho chega ao personagem,
        // volta para o estado Idle.

        if (Vector2.Distance(
                TipPosition,
                characterAnchor.position) < 0.05f)
        {
            ChangeState(HookState.Idle);
        }
    }

    // =========================================================
    // DETECÇÃO DO ALVO
    // =========================================================

    // Detecta objetos Grabbable e Pullable no ponto de destino.
    //
    // Grabbable -> Grudar
    // Pullable  -> Puxar
    //
    // O modo não é escolhido pelo nome ou pela tag do objeto.

    private void DetectTargetAt(Vector2 point)
    {
        currentTarget = null;

        currentTargetBody = null;

        currentAnchorPoint = null;

        // Combina as duas LayerMasks para detectar
        // tanto objetos Grabbable quanto Pullable.

        LayerMask combinedMask =
            config.grabbableLayerMask |
            config.pullableLayerMask;

        Collider2D hitCollider =
            Physics2D.OverlapCircle(
                point,
                config.aimDetectionRadius,
                combinedMask
            );

        // Caso não encontre nada exatamente no ponto,
        // tenta encontrar um alvo dentro do raio magnético.

        if (hitCollider == null &&
            config.magnetRadius > config.aimDetectionRadius)
        {
            hitCollider =
                FindClosestMagnetTarget(
                    point,
                    combinedMask
                );
        }

        if (hitCollider == null)
        {
            return;
        }

        Vector2 targetPoint = point;

        // Se o objeto possuir um HookAnchorPoint,
        // utiliza a posição dele como ponto de fixação.

        if (hitCollider.TryGetComponent<HookAnchorPoint>(
                out var anchor))
        {
            targetPoint =
                anchor.WorldPosition;
        }

        // =====================================================
        // GRABBABLE
        // =====================================================

        if (hitCollider.TryGetComponent<Grabbable>(
                out _))
        {
            currentMode = HookMode.Grudar;

            currentTarget =
                hitCollider.gameObject;

            hitCollider.TryGetComponent(
                out Rigidbody2D targetBody
            );

            currentTargetBody =
                targetBody;

            currentAnchorPoint =
                anchor;

            physicsAttachPoint =
                ComputeSafePhysicsPoint(
                    targetPoint,
                    hitCollider,
                    combinedMask
                );
        }

        // =====================================================
        // PULLABLE
        // =====================================================

        else if (hitCollider.TryGetComponent<Pullable>(
                     out var pullable))
        {
            currentMode = HookMode.Puxar;

            currentTarget =
                hitCollider.gameObject;

            currentTargetBody =
                pullable.Rigidbody2D;

            currentAnchorPoint =
                anchor;

            // Objetos Pullable não utilizam o ponto
            // de segurança utilizado pelo modo Grudar.

            physicsAttachPoint =
                targetPoint;
        }

        // Caso o objeto esteja na LayerMask, mas não possua
        // Grabbable ou Pullable, trata como uma detecção inválida.

        if (currentTarget == null)
        {
            return;
        }

        events?.OnModeChanged?.Invoke(currentMode);

        // O ponto visual permanece exatamente onde o alvo
        // foi detectado.

        attachPoint = targetPoint;
    }

    // =========================================================
    // DETECÇÃO MAGNÉTICA
    // =========================================================

    // Procura o objeto Grabbable/Pullable mais próximo
    // dentro do magnetRadius.
    //
    // O sistema magnético funciona somente com objetos
    // que possuem HookAnchorPoint.

    private Collider2D FindClosestMagnetTarget(
        Vector2 point,
        LayerMask mask)
    {
        Collider2D[] candidates =
            Physics2D.OverlapCircleAll(
                point,
                config.magnetRadius,
                mask
            );

        Collider2D closest = null;

        float closestDistance =
            float.MaxValue;

        foreach (Collider2D candidate in candidates)
        {
            // O sistema magnético só funciona em objetos
            // que possuem um HookAnchorPoint.

            if (!candidate.TryGetComponent<HookAnchorPoint>(
                    out _))
            {
                continue;
            }

            bool isValidTarget =
                candidate.TryGetComponent<Grabbable>(
                    out _
                ) ||
                candidate.TryGetComponent<Pullable>(
                    out _
                );

            if (!isValidTarget)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    point,
                    candidate.ClosestPoint(point)
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;

                closest = candidate;
            }
        }

        return closest;
    }

    // =========================================================
    // PONTO SEGURO DA FÍSICA
    // =========================================================

    // Calcula um ponto seguro para a física do modo Grudar.
    //
    // O ponto visual continua sendo o ponto exato da colisão.
    // Somente o ponto utilizado pela física recebe o offset
    // de segurança da superfície.

    private Vector2 ComputeSafePhysicsPoint(
        Vector2 desiredPoint,
        Collider2D hitCollider,
        LayerMask mask)
    {
        Vector2 origin =
            characterAnchor.position;

        RaycastHit2D surfaceHit =
            Physics2D.Linecast(
                origin,
                desiredPoint,
                mask
            );

        if (surfaceHit.collider == hitCollider)
        {
            return surfaceHit.point +
                   surfaceHit.normal *
                   config.grudarSurfaceOffset;
        }

        return desiredPoint;
    }

    // =========================================================
    // INDICADOR DA LIXEIRA
    // =========================================================

    // É chamado quando o jogador realmente agarra um objeto.
    //
    // Verifica se o objeto agarrado é um lixo.
    // Caso seja, pega o GarbageType e envia para o
    // TrashGameController.
    //
    // O TrashGameController é responsável por decidir
    // qual lixeira é correta através do switch.

    private void ShowTrashBinIndicator()
    {

        if (currentTarget == null)
            return;

        if (trashGameController == null)
            return;

        Trash trash = currentTarget.GetComponent<Trash>();
        if (trash == null)
            return;

        trashGameController.ShowTrashBin(trash.Type);
    }

    // =========================================================
    // MUDANÇA DE ESTADO
    // =========================================================

    // Este é o único método responsável por alterar currentState.
    //
    // Também faz a limpeza necessária ao sair do estado Attached.

    private void ChangeState(HookState newState)
    {
        if (currentState == HookState.Attached)
        {
            // Para toda a física do gancho.

            hookPhysics.StopAll();

            // Se o modo atual for Puxar,
            // informa que o objeto foi solto.

            if (currentMode == HookMode.Puxar &&
                currentTarget != null)
            {
                events?.OnObjectReleased?.Invoke(
                    currentTarget
                );
            }

            events?.OnHookReleased?.Invoke();
        }

        currentState = newState;

        switch (newState)
        {
            case HookState.Idle:

                // Limpa os dados do alvo atual.

                currentTarget = null;

                currentTargetBody = null;

                currentAnchorPoint = null;

                TipPosition =
                    characterAnchor.position;

                // Idle é um estado temporário.
                // Depois de resetar, volta para Aiming.

                ChangeState(HookState.Aiming);

                break;

            case HookState.Aiming:

                // Mantém a ponta do gancho no personagem.

                TipPosition =
                    characterAnchor.position;

                break;

            case HookState.Firing:

                // Começa o gancho na posição do personagem.

                TipPosition =
                    characterAnchor.position;

                // Limpa o alvo anterior.

                currentTarget = null;

                currentTargetBody = null;

                events?.OnHookFired?.Invoke();

                break;

            case HookState.Attached:

                // Informa aos outros sistemas que o gancho
                // conseguiu se prender ao objeto.

                events?.OnHookAttached?.Invoke(
                    currentTarget,
                    attachPoint
                );

                // =================================================
                // MODO GRUDAR
                // =================================================

                if (currentMode == HookMode.Grudar)
                {
                    hookPhysics.BeginGrudar(
                        characterAnchor,
                        physicsAttachPoint,
                        currentTargetBody
                    );
                }

                // =================================================
                // MODO PUXAR
                // =================================================

                else if (currentTargetBody != null)
                {
                    // Começa a puxar o objeto para o personagem.

                    hookPhysics.BeginPuxar(
                        characterAnchor,
                        currentTargetBody
                    );

                    // Informa aos outros sistemas que o objeto
                    // foi realmente agarrado.

                    events?.OnObjectGrabbed?.Invoke(
                        currentTarget
                    );

                    // =================================================
                    // INDICADOR DA LIXEIRA
                    // =================================================
                    //
                    // Se o objeto agarrado for um lixo,
                    // mostra a seta apontando para a lixeira correta.
                    //
                    // Se não for lixo, nada acontece.

                    ShowTrashBinIndicator();
                }
                else
                {
                    // Segurança:
                    // O modo Puxar precisa de um Rigidbody2D
                    // no objeto que será puxado.

                    ChangeState(
                        HookState.Retracting
                    );
                }

                break;

            case HookState.Retracting:

                // A corda começa a ser recolhida
                // a partir da posição atual da ponta.

                break;
        }
    }
}