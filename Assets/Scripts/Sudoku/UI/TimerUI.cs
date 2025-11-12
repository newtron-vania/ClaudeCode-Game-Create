using UnityEngine;
using TMPro;

/// <summary>
/// 스도쿠 타이머 UI
/// 게임 경과 시간을 MM:SS 형식으로 표시
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("Format Settings")]
    [SerializeField] private string _timeFormat = "{0:00}:{1:00}";  // MM:SS
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _warningColor = new Color(1f, 0.7f, 0f);  // 경고 색상 (선택)
    [SerializeField] private float _warningTime = 600f;  // 10분 (선택)

    // 타이머 상태
    private float _elapsedTime;
    private bool _isRunning;

    private void Awake()
    {
        // 컴포넌트 자동 찾기
        if (_timeText == null)
        {
            _timeText = GetComponent<TextMeshProUGUI>();
        }

        // 초기화
        _elapsedTime = 0f;
        _isRunning = false;

        UpdateTimeDisplay();
    }

    private void Update()
    {
        if (_isRunning)
        {
            // 경과 시간 증가
            _elapsedTime += Time.deltaTime;

            // UI 업데이트
            UpdateTimeDisplay();
        }
    }

    /// <summary>
    /// 타이머 초기화
    /// </summary>
    public void Initialize()
    {
        _elapsedTime = 0f;
        _isRunning = false;
        UpdateTimeDisplay();

        Debug.Log("[INFO] TimerUI::Initialize - Timer initialized");
    }

    /// <summary>
    /// 타이머 시작
    /// </summary>
    public void StartTimer()
    {
        _elapsedTime = 0f;
        _isRunning = true;

        Debug.Log("[INFO] TimerUI::StartTimer - Timer started");
    }

    /// <summary>
    /// 타이머 정지
    /// </summary>
    public void StopTimer()
    {
        _isRunning = false;

        Debug.Log($"[INFO] TimerUI::StopTimer - Timer stopped at {_elapsedTime:F2}s");
    }

    /// <summary>
    /// 타이머 재개
    /// </summary>
    public void ResumeTimer()
    {
        _isRunning = true;

        Debug.Log("[INFO] TimerUI::ResumeTimer - Timer resumed");
    }

    /// <summary>
    /// 타이머 리셋
    /// </summary>
    public void ResetTimer()
    {
        _elapsedTime = 0f;
        _isRunning = false;
        UpdateTimeDisplay();

        Debug.Log("[INFO] TimerUI::ResetTimer - Timer reset");
    }

    /// <summary>
    /// 경과 시간 설정 (외부에서 호출)
    /// </summary>
    /// <param name="time">설정할 시간 (초)</param>
    public void SetTime(float time)
    {
        _elapsedTime = Mathf.Max(0f, time);
        UpdateTimeDisplay();
    }

    /// <summary>
    /// 시간 표시 업데이트
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (_timeText == null) return;

        // 분과 초 계산
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);

        // 텍스트 업데이트
        _timeText.text = string.Format(_timeFormat, minutes, seconds);

        // 색상 업데이트 (선택: 경고 시간 초과 시)
        if (_elapsedTime >= _warningTime)
        {
            _timeText.color = _warningColor;
        }
        else
        {
            _timeText.color = _normalColor;
        }
    }

    #region Public Properties

    /// <summary>
    /// 현재 경과 시간 (초)
    /// </summary>
    public float ElapsedTime => _elapsedTime;

    /// <summary>
    /// 타이머 실행 중 여부
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 포맷된 시간 문자열 (MM:SS)
    /// </summary>
    public string FormattedTime
    {
        get
        {
            int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
            return string.Format(_timeFormat, minutes, seconds);
        }
    }

    #endregion
}
