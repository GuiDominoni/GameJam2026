using UnityEngine;

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
    public TrashType Type => _trash;

    //private Animator _garbageAnimator;
    private AudioSource _trashDescartedSound;
    private bool _pulled = false;

    private void Awake()
    {
        //_garbageAnimator = TrashGameController.Instance.GarbageAnimator;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Garbage>() != null)
        {
            _trashDescartedSound =
                collision.GetComponent<AudioSource>();

            Garbage garbage =
                collision.GetComponent<Garbage>();

            if (garbage.garbageType.Equals(GarbageType.plastic) &&
                _trash.Equals(TrashType.plastic))
            {
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.paper) &&
                     _trash.Equals(TrashType.paper))
            {
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.glass) &&
                     _trash.Equals(TrashType.glass))
            {
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.metal) &&
                     _trash.Equals(TrashType.metal))
            {
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                Destroy(gameObject);
            }
        }
    }
}