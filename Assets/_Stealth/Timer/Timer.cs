using TMPro;
using UnityEngine;

//AI
// https://gemini.google.com/share/5cd3fc888608
public class Timer : MonoBehaviour
{
    public enum TimerMode { CountUp, CountDown }

    [Header("Settings")]
    public TimerMode currentMode = TimerMode.CountDown;
    public float startTimeInSeconds = 60f;
    public bool runOnStart = true;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    private float _currentTime;
    private bool _isPaused;

    void Start()
    {
        _currentTime = startTimeInSeconds;
        _isPaused = !runOnStart;
    }

    void Update()
    {
        if (_isPaused) return;

        if (currentMode == TimerMode.CountUp)
        {
            _currentTime += Time.deltaTime;
        }
        else
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime <= 0)
            {
                _currentTime = 0;
                _isPaused = true;
                OnTimerComplete();
            }
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (timerText == null) return;

        // Formats to Minutes:Seconds (00:00)
        int minutes = Mathf.FloorToInt(_currentTime / 60);
        int seconds = Mathf.FloorToInt(_currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void SwitchMode(TimerMode newMode)
    {
        currentMode = newMode;
    }

    public void ResetTimer(float newStartTime)
    {
        _currentTime = newStartTime;
    }

    private void OnTimerComplete()
    {
        Debug.Log("Timer reached zero!");
    }
}
