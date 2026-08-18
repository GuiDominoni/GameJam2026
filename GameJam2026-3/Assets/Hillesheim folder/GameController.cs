using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    [Header("GamecontrollerInstace")]
    public static GameController Instance { get; private set; }
    public GameObject[] GarbageGameObjects { get => _garbageGameObjects;}
    public UnityEvent Win { get => _win;}
    public int TrashTrowedOutValue { get; private set; }

    [SerializeField] private GameObject[] _garbageGameObjects;
    [SerializeField] private UnityEvent _win;
    [SerializeField] private AudioSource _trashDescarted;
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
