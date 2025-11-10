using TMPro;
using UnityEngine;

/// <summary>
/// 테트리스 게임 전용 UI 패널
/// 점수, 라인, 레벨, 다음 블록, 게임 오버 UI 관리
/// </summary>
public class TetrisUIPanel : UIPanel
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _linesText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _nextPieceText;
    [SerializeField] private TextMeshProUGUI _gameOverText;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        // 게임 오버 텍스트 숨기기
        if (_gameOverText != null)
        {
            _gameOverText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 점수 업데이트
    /// </summary>
    /// <param name="score">현재 점수</param>
    public void UpdateScore(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {score}";
        }
    }

    /// <summary>
    /// 라인 수 업데이트
    /// </summary>
    /// <param name="lines">제거한 라인 수</param>
    public void UpdateLines(int lines)
    {
        if (_linesText != null)
        {
            _linesText.text = $"Lines: {lines}";
        }
    }

    /// <summary>
    /// 레벨 업데이트
    /// </summary>
    /// <param name="level">현재 레벨</param>
    public void UpdateLevel(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"Level: {level}";
        }
    }

    /// <summary>
    /// 다음 블록 텍스트 업데이트
    /// </summary>
    /// <param name="text">표시할 텍스트</param>
    public void UpdateNextPieceText(string text)
    {
        if (_nextPieceText != null)
        {
            _nextPieceText.text = text;
        }
    }

    /// <summary>
    /// 게임 오버 표시
    /// </summary>
    /// <param name="finalScore">최종 점수</param>
    public void ShowGameOver(int finalScore)
    {
        if (_gameOverText != null)
        {
            _gameOverText.gameObject.SetActive(true);
            _gameOverText.text = $"Game Over!\nScore: {finalScore}\nPress R to Restart";
        }
    }

    /// <summary>
    /// 게임 오버 숨기기
    /// </summary>
    public void HideGameOver()
    {
        if (_gameOverText != null)
        {
            _gameOverText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 모든 UI 업데이트
    /// </summary>
    /// <param name="tetrisData">테트리스 게임 데이터</param>
    public void UpdateAll(TetrisData tetrisData)
    {
        if (tetrisData == null)
        {
            return;
        }

        UpdateScore(tetrisData.CurrentScore);
        UpdateLines(tetrisData.LinesCleared);
        UpdateLevel(tetrisData.Level);
        UpdateNextPieceText("Next");

        // 게임 오버 체크
        if (tetrisData.IsGameOver)
        {
            ShowGameOver(tetrisData.CurrentScore);
        }
    }

    /// <summary>
    /// UI 리셋 (게임 재시작 시 호출)
    /// </summary>
    public void ResetUI()
    {
        // 점수 초기화
        UpdateScore(0);
        UpdateLines(0);
        UpdateLevel(1);
        UpdateNextPieceText("Next");

        // 게임 오버 텍스트 숨기기
        HideGameOver();

        Debug.Log("[INFO] TetrisUIPanel::ResetUI - UI reset");
    }
}
