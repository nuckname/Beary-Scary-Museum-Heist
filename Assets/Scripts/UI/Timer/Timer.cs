using System;
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

    private TextMeshProUGUI timerText;

    [Header("UI Reference")]
    public float _currentTime;
    public bool isPaused;

    private void Awake()
    {
//        timerText = GameObject.FindGameObjectWithTag("Timer").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        _currentTime = startTimeInSeconds;
        isPaused = !runOnStart;
    }

    void Update()
    {
        if (isPaused) return;

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
                isPaused = true;
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
