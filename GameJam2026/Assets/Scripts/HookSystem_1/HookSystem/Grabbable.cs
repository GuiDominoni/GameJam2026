using UnityEngine;

// Marca um objeto como "grudável" pelo gancho (modo Grudar): superfícies e paredes
// onde o personagem pode se prender e ser puxado/balançar.
//
// Não tem lógica própria — a detecção do HookController combina a presença deste
// componente com o grabbableLayerMask configurado no HookConfig, nunca por nome
// de objeto ou tag hardcoded no código.
//
// Se o objeto grudável também tiver um Rigidbody2D (ex: uma plataforma móvel), o
// gancho se conecta a esse Rigidbody2D em vez de a um ponto fixo no mundo.
[RequireComponent(typeof(Collider2D))]
public class Grabbable : MonoBehaviour
{
}
