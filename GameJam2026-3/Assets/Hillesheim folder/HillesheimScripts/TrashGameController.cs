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

    [Header("Garbage Bins")]
    public Transform plasticGarbage;
    public Transform paperGarbage;
    public Transform metalGarbage;
    public Transform glassGarbage;

    [Header("Arrow")]
    public GameObject arrow;

    [SerializeField] private Camera mainCamera;

    [Space(50)]
    [Header("Serialize Fields")]
    [SerializeField] private GameObject[] _trashGameObjects;
    [SerializeField] private UnityEvent _win;

    private Transform currentDestination;
    private bool showingArrow;
    private bool gameWon;

    public void AddTrash()
    {
        TrashThrownOutValue++;
    }

    private void Awake()
    {
        Instance = this;

        arrow.SetActive(false);
    }

    private void Update()
    {
        if (!gameWon && TrashThrownOutValue >= _trashGameObjects.Length)
        {
            gameWon = true;
            _win.Invoke();
        }

        UpdateArrow();
    }

    public void ShowTrashBin(TrashType type)
    {
        Transform destination = null;

        switch (type)
        {
            case TrashType.plastic:
                destination = plasticGarbage;
                break;

            case TrashType.paper:
                destination = paperGarbage;
                break;

            case TrashType.glass:
                destination = glassGarbage;
                break;

            case TrashType.metal:
                destination = metalGarbage;
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
