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
    [SerializeField] private TrashUi _trashUi;
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
        print("passou");
        if (collision.GetComponent<Garbage>() != null)
        {
            _trashDescartedSound =
                collision.GetComponent<AudioSource>();

            Garbage garbage =
                collision.GetComponent<Garbage>();

            if (garbage.garbageType.Equals(GarbageType.plastic) &&
                _trash.Equals(TrashType.plastic))
            {
                _trashUi.TrashUIUpdate(TrashType.plastic);
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                gameObject.SetActive(false);
            }
            else if (garbage.garbageType.Equals(GarbageType.paper) &&
                     _trash.Equals(TrashType.paper))
            {
                _trashUi.TrashUIUpdate(TrashType.paper);
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                gameObject.SetActive(false);
            }
            else if (garbage.garbageType.Equals(GarbageType.glass) &&
                     _trash.Equals(TrashType.glass))
            {
                _trashUi.TrashUIUpdate(TrashType.glass);
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                gameObject.SetActive(false);
            }
            else if (garbage.garbageType.Equals(GarbageType.metal) &&
                     _trash.Equals(TrashType.metal))
            {
                _trashUi.TrashUIUpdate(TrashType.metal);
                TrashGameController.Instance.AddTrash();
                _trashDescartedSound.Play();
                gameObject.SetActive(false);
            }
            TrashGameController.Instance.HideArrow();
        }
    }
}