using UnityEngine;
using UnityEngine.Events;

// Modos de operação do gancho.
// Grudar: o PERSONAGEM é puxado/balança em direção ao ponto de fixação.
// Puxar: o OBJETO alvo é puxado em direção ao personagem.
public enum HookMode
{
    Grudar,
    Puxar
}

// Variações de UnityEvent com argumentos, para eventos que precisam enviar dados
// (o alvo atingido, o ponto de fixação, o novo modo, etc).
[System.Serializable]
public class HookAttachEvent : UnityEvent<GameObject, Vector2> { }

[System.Serializable]
public class HookTargetEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class HookModeEvent : UnityEvent<HookMode> { }

// Centraliza todos os eventos de extensão do gancho num único componente, para que
// som/UI/câmera/feedback (ex: squash e flash via DOTween) se conectem via Inspector
// sem que cada script declare seus próprios eventos de forma inconsistente.
//
// Este componente não tem lógica própria: apenas expõe os eventos para o
// HookController disparar e para outros sistemas escutarem.
public class HookEvents : MonoBehaviour
{
    [Header("Eventos do Gancho")]
    [Tooltip("Disparado no início do lançamento do gancho (entrada no estado Firing).")]
    public UnityEvent OnHookFired;

    [Tooltip("Disparado quando o gancho se prende a um alvo válido. Envia o GameObject alvo e o ponto de fixação em coordenadas de mundo.")]
    public HookAttachEvent OnHookAttached;

    [Tooltip("Disparado quando o gancho se solta do alvo e começa a recolher (início do estado Retracting a partir de Attached).")]
    public UnityEvent OnHookReleased;

    [Tooltip("Disparado quando um objeto puxável é agarrado (modo Puxar efetivo). Envia o GameObject agarrado.")]
    public HookTargetEvent OnObjectGrabbed;

    [Tooltip("Disparado quando um objeto puxável é liberado e volta a ficar sujeito apenas à física normal. Envia o GameObject liberado.")]
    public HookTargetEvent OnObjectReleased;

    [Tooltip("Disparado quando o modo do gancho é alternado (Grudar/Puxar). Envia o novo modo.")]
    public HookModeEvent OnModeChanged;
}
