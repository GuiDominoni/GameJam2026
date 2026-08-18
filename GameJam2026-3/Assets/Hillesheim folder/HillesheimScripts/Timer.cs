using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private float timePassing;
    void Update()
    {
        timePassing += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timePassing / 60f);
        int seconds = Mathf.FloorToInt(timePassing % 60f);
        int milisseconds = Mathf.FloorToInt((timePassing * 100f) % 100f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milisseconds);
    }
}