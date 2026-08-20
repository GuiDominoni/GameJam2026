using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [Header("Textcoming")]
    [SerializeField] private TextMeshProUGUI loseText;
    [SerializeField] private float finalSize = 50f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float time = 0f;
    [Space(30)]
    [Header("Buttoncoming")]
    [SerializeField] private RectTransform button;
    [SerializeField] private float buttonDuration = 1f;
    [SerializeField] private float buttonProgress = 0f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float startTime = 90f;
    [Space(30)]
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float finalZoom = 3f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    private float timeRemaining;
    private bool isDone;

    private void Awake()
    {
        timeRemaining = startTime;
        isDone = false;
    }

    private void Update()
    {
        if (timeRemaining <= 0)
        {
            loseText.gameObject.SetActive(true);
            button.gameObject.SetActive(true);
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / duration);

            loseText.fontSize = Mathf.SmoothStep(
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

            Time.timeScale = Mathf.Lerp(
                Time.timeScale,
                0f,
                Time.unscaledDeltaTime / duration
            );

            if (buttonProgress >= 1f)
            {
                enabled = false;
            }
        }
        if (isDone)
            return;
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isDone = true;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        int centiseconds = Mathf.FloorToInt((timeRemaining * 100f) % 100f);

        timerText.text = string.Format(
            "{0:00}:{1:00}:{2:00}",
            minutes,
            seconds,
            centiseconds
        );
    }
    private void LateUpdate()
    {
        if (timeRemaining > 0)
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
    public void StopTimer()
    {
        isDone = true;
    }
    public void Restart()
    {
        StartCoroutine(FadeAndScene());
    }

    private IEnumerator FadeAndScene()
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}