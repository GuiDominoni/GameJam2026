using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    public Transform Player { get => player;}

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
    [Space(30)]
    [Header("Textcoming")]
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private float finalSize = 50f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float time = 0f;
    [Space(30)]
    [Header("CamerazoomPlayer")]
    [SerializeField] private Transform player;
    [SerializeField] private float finalZoom = 3f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [Space(30)]
    [Header("Buttoncoming")]
    [SerializeField] private RectTransform button;
    [SerializeField] private float buttonDuration = 1f;
    [SerializeField] private float buttonProgress = 0f;
    [Space(30)]
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private Transform currentDestination;
    private bool showingArrow;
    private bool gameWon = false;

    private Trash currentTrash;

    public bool IsHoldingTrash => currentTrash != null;

    public void SetTrash(Trash trash)
    {
        currentTrash = trash;
    }

    public void ClearTrash()
    {
        currentTrash = null;
    }

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

            victoryText.fontSize = 0;
            _win.Invoke();
        }

        if (gameWon)
        {
            victoryText.gameObject.SetActive(true);
            button.gameObject.SetActive(true);
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / duration);

            victoryText.fontSize = Mathf.SmoothStep(
                0f,
                finalSize,
                progress
            );

            buttonProgress += Time.unscaledDeltaTime / buttonDuration;
            buttonProgress = Mathf.Clamp01(buttonProgress);

            float buttonScale = Mathf.SmoothStep(
                0f,
                1f,
                buttonProgress
            );

            button.localScale = Vector3.one * buttonScale;

            if (buttonProgress >= 1f)
            {
                enabled = false;
            }
        }

        UpdateArrow();
    }
    private void LateUpdate()
    {
        if (!gameWon)
            return;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            1f - Mathf.Exp(-speed * Time.unscaledDeltaTime)
        );

        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            finalZoom,
            1f - Mathf.Exp(-speed * Time.unscaledDeltaTime)
        );
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
    public void next(string scene)
    {
        StartCoroutine(FadeAndScene(scene));
    }

    private IEnumerator FadeAndScene(string scene)
    {
        float fadeTime = 0f;
        Color _color = fadeImage.color;

        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.unscaledDeltaTime;

            float fadeProgress = Mathf.Clamp01(fadeTime / fadeDuration);
            _color.a = Mathf.Lerp(0f, 1f, fadeProgress);

            fadeImage.color = _color;

            yield return null;
        }
        Time.timeScale = 0f;
        SceneManager.LoadScene(scene);
    }
}
