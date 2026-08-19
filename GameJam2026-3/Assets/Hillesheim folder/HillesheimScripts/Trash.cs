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

    //private Animator _garbageAnimator;
    private AudioSource _trashDescartedSound;
    [SerializeField] private UnityEvent OnDiscard;

    private void Awake()
    {
        //_garbageAnimator = TrashGameController.Instance.GarbageAnimator;
    }
    private void DestroyTrash(TrashType type)
    {
        OnDiscard?.Invoke();
        TrashGameController.Instance.HideArrow();
        if (_trashUi != null)
            _trashUi.TrashUIUpdate(type);
        TrashGameController.Instance.AddTrash();
        _trashDescartedSound.Play();
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("passou");
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
    }
}