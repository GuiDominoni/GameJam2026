using UnityEngine;
using UnityEngine.Events;


public class TrashGameController : MonoBehaviour
{
    [Header("Game Controller Instance")]
    public static TrashGameController Instance { get; private set; }

    public int TrashThrownOutValue { get; private set; }

    public GameObject[] TrashGameObjects
    {
        get => _trashGameObjects;
    }

    public UnityEvent Win
    {
        get => _win;
    }
    public Sprite GreenCheck { get => _greenCheck;}
    public TrashUi TrashUi { get => _trashUi;}

    [Header("Garbage Bins")]
    [SerializeField] private Transform plasticGarbage;
    [SerializeField] private Transform paperGarbage;
    [SerializeField] private Transform metalGarbage;
    [SerializeField] private Transform glassGarbage;


    [Header("Arrow")]
    [SerializeField] private GameObject arrow;

    [SerializeField] private Camera mainCamera;

    [Header("Arrow Sprites")]
    [SerializeField] private Sprite plasticArrowSprite;
    [SerializeField] private Sprite paperArrowSprite;
    [SerializeField] private Sprite metalArrowSprite;
    [SerializeField] private Sprite glassArrowSprite;
    private SpriteRenderer arrowSpriteRenderer;
    [Space(35)]
    [Header("Serialize Fields")]
    [SerializeField] private GameObject[] _trashGameObjects;
    [SerializeField] private TrashUi _trashUi;
    [SerializeField] private Sprite _greenCheck;
    [SerializeField] private UnityEvent _win;

    private Transform currentDestination;
    private bool showingArrow;
    private bool gameWon = false;

    public void AddTrash()
    {
        TrashThrownOutValue++;
    }

    private void Awake()
    {
        Instance = this;
        arrowSpriteRenderer = arrow.GetComponentInChildren<SpriteRenderer>();

        arrow.SetActive(false);
    }

    private void Update()
    {
        if (!gameWon && TrashThrownOutValue >= _trashGameObjects.Length)
        {
            gameWon = true;
            Debug.Log("Game Won!");
            _win.Invoke();
        }

        UpdateArrow();
    }

    public void ShowGarbage(TrashType type)
    {
        Transform destination = null;

        switch (type)
        {
            case TrashType.plastic:
                destination = plasticGarbage;
                arrowSpriteRenderer.sprite = plasticArrowSprite;
                break;

            case TrashType.paper:
                destination = paperGarbage;
                arrowSpriteRenderer.sprite = paperArrowSprite;
                break;

            case TrashType.glass:
                destination = glassGarbage;
                arrowSpriteRenderer.sprite = glassArrowSprite;
                break;

            case TrashType.metal:
                destination = metalGarbage;
                arrowSpriteRenderer.sprite = metalArrowSprite;
                break;
        }

        if (destination == null)
            return;

        currentDestination = destination;

        showingArrow = true;

        arrow.SetActive(true);
    }

    private void UpdateArrow()
    {
        if (!showingArrow || currentDestination == null)
            return;

        Vector3 targetScreenPosition =
            mainCamera.WorldToScreenPoint(currentDestination.position);

        Vector2 screenCenter =
            new Vector2(Screen.width / 2f, Screen.height / 2f);

        Vector2 direction =
            (Vector2)targetScreenPosition - screenCenter;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        arrow.transform.rotation =
            Quaternion.Euler(0f, 0f, angle - 90f);
    }

    public void HideArrow()
    {
        showingArrow = false;
        currentDestination = null;

        arrow.SetActive(false);
    }
}
