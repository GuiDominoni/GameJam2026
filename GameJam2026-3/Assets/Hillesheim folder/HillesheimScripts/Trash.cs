using UnityEngine;
using UnityEngine.Events;

public enum TrashType
{
    plastic,
    paper,
    glass,
    metal
}

public class Trash : MonoBehaviour
{
    [SerializeField] private TrashType _trash;
    [SerializeField] private TrashUi _trashUi;
    public TrashType Type => _trash;

    private TrashType _objectType;
    private AudioSource _trashDescartedSound;
    private bool _trashCooldown = false;
    [SerializeField] private Transform trashPosition;
    [SerializeField] private UnityEvent OnDiscard;
    private void Awake()
    {
        _objectType = _trash;
    }
    private void DestroyTrash(TrashType type)
    {
        if (_trashCooldown == true)
            return;
        _trashCooldown = true;
        OnDiscard?.Invoke();
        transform.SetParent(null, true);
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        TrashGameController.Instance.ClearTrash();
        TrashGameController.Instance.HideArrow();
        if (_trashUi != null)
            _trashUi.TrashUIUpdate(type);
        TrashGameController.Instance.AddTrash();
        _trashDescartedSound.Play();
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Garbage>() != null)
        {
            _trashDescartedSound = collision.GetComponent<AudioSource>();

            Garbage garbage = collision.GetComponent<Garbage>();

            if (garbage.garbageType.Equals(GarbageType.plastic) && _trash.Equals(TrashType.plastic))
            {
                DestroyTrash(TrashType.plastic);
            }

            else if (garbage.garbageType.Equals(GarbageType.paper) && _trash.Equals(TrashType.paper))
            {
                DestroyTrash(TrashType.paper);
            }
            else if (garbage.garbageType.Equals(GarbageType.glass) && _trash.Equals(TrashType.glass))
            {
                DestroyTrash(TrashType.glass);
            }
            else if (garbage.garbageType.Equals(GarbageType.metal) && _trash.Equals(TrashType.metal))
            {
                DestroyTrash(TrashType.metal);
            }

        }
        Debug.Log(collision.GetComponent<PlayerAnimController>());
        if (collision.GetComponent<PlayerAnimController>() != null)
        {
            if(TrashGameController.Instance.IsHoldingTrash)
                return;

            TrashGameController.Instance.SetTrash(this);
            TrashGameController.Instance.ShowGarbage(_objectType);
            transform.SetParent(trashPosition, false);
            transform.localPosition = Vector3.zero;
            GetComponent<Collider2D>().enabled = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}