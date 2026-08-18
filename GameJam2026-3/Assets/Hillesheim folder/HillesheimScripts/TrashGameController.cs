using UnityEngine;
using UnityEngine.Events;

public class TrashGameController : MonoBehaviour
{
    [Header("GamecontrollerInstace")]
    public static TrashGameController Instance { get; private set; }
    public int TrashTrowedOutValue { get; private set; }

    public GameObject[] GarbageGameObjects { get => _garbageGameObjects;}
    public UnityEvent Win { get => _win;}
    public AudioSource TrashDescarted { get => _trashDescarted;}
    public Animator GarbageAnimator { get => _garbageAnimator;}

    [Space(50)]
    [Header("SerializeFields")]
    [SerializeField] private GameObject[] _garbageGameObjects;
    [SerializeField] private UnityEvent _win;
    [SerializeField] private AudioSource _trashDescarted;
    [SerializeField] private Animator _garbageAnimator;

    public void AddTrash()
    {
        TrashTrowedOutValue++;
    }
    void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (TrashTrowedOutValue >= _garbageGameObjects.Length)
        {
            _win.Invoke();
        }
    }
}