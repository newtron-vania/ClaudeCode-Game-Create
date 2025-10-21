using UnityEngine;

/// <summary>
/// 테트리스 게임의 데이터를 관리하는 클래스
/// IGameData 인터페이스를 구현하여 GameManager에서 사용 가능
/// </summary>
[System.Serializable]
public class TetrisGameData : IGameData
{
    // 게임 상태
    private int _score;
    private int _level;
    private int _linesCleared;
    private int _totalBlocksPlaced;

    // 게임 설정
    private const int INITIAL_LEVEL = 1;
    private const int LINES_PER_LEVEL = 10;
    private const int SCORE_PER_LINE = 100;
    private const int SCORE_MULTIPLIER_TETRIS = 4; // 4줄 동시 제거 보너스

    // 게임 통계
    private float _playTime;
    private int _highScore;

    /// <summary>
    /// 현재 점수
    /// </summary>
    public int Score => _score;

    /// <summary>
    /// 현재 레벨
    /// </summary>
    public int Level => _level;

    /// <summary>
    /// 제거한 총 라인 수
    /// </summary>
    public int LinesCleared => _linesCleared;

    /// <summary>
    /// 배치한 총 블록 수
    /// </summary>
    public int TotalBlocksPlaced => _totalBlocksPlaced;

    /// <summary>
    /// 플레이 시간 (초)
    /// </summary>
    public float PlayTime => _playTime;

    /// <summary>
    /// 최고 점수
    /// </summary>
    public int HighScore => _highScore;

    /// <summary>
    /// 현재 레벨의 블록 낙하 속도 (초당 칸 수)
    /// </summary>
    public float FallSpeed => Mathf.Max(0.1f, 1f - (_level - 1) * 0.05f);

    /// <summary>
    /// 다음 레벨까지 남은 라인 수
    /// </summary>
    public int LinesToNextLevel => LINES_PER_LEVEL - (_linesCleared % LINES_PER_LEVEL);

    /// <summary>
    /// 게임 데이터 초기화
    /// </summary>
    public void Initialize()
    {
        Debug.Log("[INFO] TetrisGameData::Initialize - Initializing Tetris game data");

        _score = 0;
        _level = INITIAL_LEVEL;
        _linesCleared = 0;
        _totalBlocksPlaced = 0;
        _playTime = 0f;

        // 저장된 최고 점수 로드
        LoadHighScore();
    }

    /// <summary>
    /// 게임 데이터 리셋
    /// </summary>
    public void Reset()
    {
        Debug.Log("[INFO] TetrisGameData::Reset - Resetting Tetris game data");

        // 최고 점수 갱신 확인
        if (_score > _highScore)
        {
            _highScore = _score;
            SaveHighScore();
        }

        // 게임 데이터 초기화
        Initialize();
    }

    /// <summary>
    /// 게임 데이터 검증
    /// </summary>
    /// <returns>데이터가 유효하면 true</returns>
    public bool Validate()
    {
        bool isValid = true;

        if (_score < 0)
        {
            Debug.LogError("[ERROR] TetrisGameData::Validate - Score is negative");
            isValid = false;
        }

        if (_level < 1)
        {
            Debug.LogError("[ERROR] TetrisGameData::Validate - Level is less than 1");
            isValid = false;
        }

        if (_linesCleared < 0)
        {
            Debug.LogError("[ERROR] TetrisGameData::Validate - Lines cleared is negative");
            isValid = false;
        }

        if (_totalBlocksPlaced < 0)
        {
            Debug.LogError("[ERROR] TetrisGameData::Validate - Total blocks placed is negative");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 블록 배치 시 호출
    /// </summary>
    public void OnBlockPlaced()
    {
        _totalBlocksPlaced++;
        Debug.Log($"[INFO] TetrisGameData::OnBlockPlaced - Total blocks: {_totalBlocksPlaced}");
    }

    /// <summary>
    /// 라인 제거 시 호출
    /// </summary>
    /// <param name="linesCount">제거된 라인 수 (1-4)</param>
    public void OnLinesCleared(int linesCount)
    {
        if (linesCount < 1 || linesCount > 4)
        {
            Debug.LogWarning($"[WARNING] TetrisGameData::OnLinesCleared - Invalid lines count: {linesCount}");
            return;
        }

        _linesCleared += linesCount;

        // 점수 계산 (4줄 동시 제거 시 보너스)
        int baseScore = SCORE_PER_LINE * linesCount;
        if (linesCount == 4)
        {
            baseScore *= SCORE_MULTIPLIER_TETRIS;
        }

        int scoreGained = baseScore * _level;
        _score += scoreGained;

        Debug.Log($"[INFO] TetrisGameData::OnLinesCleared - Lines: {linesCount}, Score gained: {scoreGained}, Total score: {_score}");

        // 레벨 업 체크
        CheckLevelUp();
    }

    /// <summary>
    /// 레벨 업 체크
    /// </summary>
    private void CheckLevelUp()
    {
        int newLevel = (_linesCleared / LINES_PER_LEVEL) + 1;

        if (newLevel > _level)
        {
            _level = newLevel;
            Debug.Log($"[INFO] TetrisGameData::CheckLevelUp - Level up! New level: {_level}, Fall speed: {FallSpeed}");
        }
    }

    /// <summary>
    /// 플레이 시간 업데이트
    /// </summary>
    /// <param name="deltaTime">경과 시간</param>
    public void UpdatePlayTime(float deltaTime)
    {
        _playTime += deltaTime;
    }

    /// <summary>
    /// 최고 점수 저장
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("TetrisHighScore", _highScore);
        PlayerPrefs.Save();
        Debug.Log($"[INFO] TetrisGameData::SaveHighScore - High score saved: {_highScore}");
    }

    /// <summary>
    /// 최고 점수 로드
    /// </summary>
    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("TetrisHighScore", 0);
        Debug.Log($"[INFO] TetrisGameData::LoadHighScore - High score loaded: {_highScore}");
    }

    /// <summary>
    /// 게임 통계 출력
    /// </summary>
    /// <returns>게임 통계 문자열</returns>
    public string GetGameStats()
    {
        return $"Score: {_score}\n" +
               $"Level: {_level}\n" +
               $"Lines Cleared: {_linesCleared}\n" +
               $"Blocks Placed: {_totalBlocksPlaced}\n" +
               $"Play Time: {_playTime:F1}s\n" +
               $"High Score: {_highScore}";
    }
}
