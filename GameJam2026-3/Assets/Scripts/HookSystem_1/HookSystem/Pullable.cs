using UnityEngine;

// Marca um objeto como "puxável" pelo gancho (modo Puxar): objetos que são trazidos
// até o personagem em vez do personagem se mover até eles.
//
// Exige Rigidbody2D no mesmo objeto (RequireComponent) para eliminar qualquer
// NullReferenceException em runtime, já que o modo Puxar sempre aplica força sobre
// este corpo (ver HookPhysics.BeginPuxar).
//
// Detecção pelo HookController combina a presença deste componente com o
// pullableLayerMask do HookConfig — nunca por nome de objeto ou tag.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pullable : MonoBehaviour
{
    public Rigidbody2D Rigidbody2D { get; private set; }
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        Rigidbody2D = GetComponent<Rigidbody2D>();

        // Mesma prevenção de tunelamento usada no personagem: um objeto puxado com
        // força alta pode atravessar paredes/o próprio personagem em modo Discrete.
        Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == null)
            return;

        _source.Play();
    }
}
