using UnityEngine;
using UnityEngine.Events;

public class TrashGameController : MonoBehaviour
{
    [Header("GamecontrollerInstace")]
    public static TrashGameController Instance { get; private set; }
    public int TrashTrowedOutValue { get; private set; }
    public GameObject[] TrashGameObjects { get => _trashGameObjects;}
    public UnityEvent Win { get => _win;}


    [Header("Garbages")]
    public Transform plasticGarbage;
    public Transform paperGarbage;
    public Transform metalGarbage;
    public Transform glassGarbage;

    [Header("Balão")]
    public GameObject balao;

    //public AudioSource TrashDescartedSound { get => _trashDescartedSound;}
    //public Animator GarbageAnimator { get => _garbageAnimator;}
    [Space(50)]
    [Header("SerializeFields")]
    [SerializeField] private GameObject[] _trashGameObjects;
    [SerializeField] private UnityEvent _win;
    //[SerializeField] private AudioSource _trashDescartedSound;
    //[SerializeField] private Animator _garbageAnimator;
    public void AddTrash()
    {
        TrashTrowedOutValue++;
    }
    void Awake()
    {
        Instance = this;
        balao.SetActive(false);
    }
    private void Update()
    {
        if (TrashTrowedOutValue >= _trashGameObjects.Length)
        {
            _win.Invoke();
            TrashTrowedOutValue++;
        }
    }
    public void MostrarLixeira(GarbageType type)
    {
        Transform destiny = null;

        switch (type)
        {
            case GarbageType.plastic:
                destiny = plasticGarbage;
                break;

            case GarbageType.paper:
                destiny = paperGarbage;
                break;

            case GarbageType.glass:
                destiny = glassGarbage;
                break;

            case GarbageType.metal:
                destiny = metalGarbage;
                break;        
        }

        if (destiny == null)
            return;

        balao.SetActive(true);

        // Posiciona o balão na lixeira
        balao.transform.position = destiny.position;
    }

    public void EsconderBalao()
    {
        balao.SetActive(false);
    }

}