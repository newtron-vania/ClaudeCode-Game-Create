using UnityEngine;

/// <summary>
/// 스도쿠 게임의 런타임 데이터를 관리하는 클래스
/// IGameData 인터페이스를 구현하여 GameManager에서 사용 가능
/// </summary>
[System.Serializable]
public class SudokuGameData : IGameData
{
    // 게임 진행 상태
    private float _playTime;            // 플레이 시간 (초)
    private int _score;                 // 점수
    private int _hintsUsed;             // 사용한 힌트 수
    private int _mistakes;              // 실수 횟수
    private bool _isCompleted;          // 게임 완료 여부
    private SudokuDifficulty _difficulty;

    // 게임 설정
    private const int MAX_MISTAKES = 3;
    private const int MAX_HINTS = 3;        // 최대 힌트 사용 횟수

    // 통계
    private float _bestTimeEasy;
    private float _bestTimeMedium;
    private float _bestTimeHard;
    private int _totalGamesCompleted;

    /// <summary>
    /// 플레이 시간 (초)
    /// </summary>
    public float PlayTime => _playTime;

    /// <summary>
    /// 점수
    /// </summary>
    public int Score => _score;

    /// <summary>
    /// 사용한 힌트 수
    /// </summary>
    public int HintsUsed => _hintsUsed;

    /// <summary>
    /// 남은 힌트 수
    /// </summary>
    public int RemainingHints => MAX_HINTS - _hintsUsed;

    /// <summary>
    /// 최대 힌트 사용 횟수
    /// </summary>
    public int MaxHints => MAX_HINTS;

    /// <summary>
    /// 실수 횟수
    /// </summary>
    public int Mistakes => _mistakes;

    /// <summary>
    /// 최대 실수 횟수
    /// </summary>
    public int MaxMistakes => MAX_MISTAKES;

    /// <summary>
    /// 게임 완료 여부
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// 게임 오버 여부 (최대 실수 도달)
    /// </summary>
    public bool IsGameOver => _mistakes >= MAX_MISTAKES;

    /// <summary>
    /// 현재 난이도
    /// </summary>
    public SudokuDifficulty Difficulty => _difficulty;

    /// <summary>
    /// 난이도별 최고 기록
    /// </summary>
    public float GetBestTime(SudokuDifficulty difficulty)
    {
        switch (difficulty)
        {
            case SudokuDifficulty.Easy:
                return _bestTimeEasy;
            case SudokuDifficulty.Medium:
                return _bestTimeMedium;
            case SudokuDifficulty.Hard:
                return _bestTimeHard;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 완료한 게임 수
    /// </summary>
    public int TotalGamesCompleted => _totalGamesCompleted;

    /// <summary>
    /// 게임 데이터 초기화
    /// </summary>
    public void Initialize()
    {
        Debug.Log("[INFO] SudokuGameData::Initialize - Initializing Sudoku game data");

        _playTime = 0f;
        _score = 0;
        _hintsUsed = 0;
        _mistakes = 0;
        _isCompleted = false;
        _difficulty = SudokuDifficulty.Medium;

        LoadBestTimes();
        LoadTotalGames();
    }

    /// <summary>
    /// 게임 데이터 리셋
    /// </summary>
    public void Reset()
    {
        Debug.Log("[INFO] SudokuGameData::Reset - Resetting Sudoku game data");

        // 완료된 게임이면 통계 업데이트
        if (_isCompleted)
        {
            UpdateBestTime();
            SaveTotalGames();
        }

        Initialize();
    }

    /// <summary>
    /// 게임 데이터 검증
    /// </summary>
    /// <returns>데이터가 유효하면 true</returns>
    public bool Validate()
    {
        bool isValid = true;

        if (_hintsUsed < 0)
        {
            Debug.LogError("[ERROR] SudokuGameData::Validate - Hints used is negative");
            isValid = false;
        }

        if (_mistakes < 0)
        {
            Debug.LogError("[ERROR] SudokuGameData::Validate - Mistakes is negative");
            isValid = false;
        }

        if (_playTime < 0)
        {
            Debug.LogError("[ERROR] SudokuGameData::Validate - Play time is negative");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 게임 상태 저장
    /// </summary>
    public void SaveState()
    {
        Debug.Log("[INFO] SudokuGameData::SaveState - Saving game state");
        // TODO: JSON 직렬화로 현재 게임 상태 저장
    }

    /// <summary>
    /// 게임 상태 로드
    /// </summary>
    public void LoadState()
    {
        Debug.Log("[INFO] SudokuGameData::LoadState - Loading game state");
        // TODO: JSON 역직렬화로 저장된 게임 상태 로드
    }

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    /// <param name="difficulty">난이도</param>
    public void StartNewGame(SudokuDifficulty difficulty)
    {
        _difficulty = difficulty;
        _playTime = 0f;
        _score = 0;
        _hintsUsed = 0;
        _mistakes = 0;
        _isCompleted = false;

        Debug.Log($"[INFO] SudokuGameData::StartNewGame - Starting new {difficulty} game");
    }

    /// <summary>
    /// 플레이 시간 업데이트
    /// </summary>
    /// <param name="deltaTime">경과 시간</param>
    public void UpdatePlayTime(float deltaTime)
    {
        if (!_isCompleted && !IsGameOver)
        {
            _playTime += deltaTime;
        }
    }

    /// <summary>
    /// 힌트 사용
    /// </summary>
    public void UseHint()
    {
        _hintsUsed++;
        Debug.Log($"[INFO] SudokuGameData::UseHint - Hints used: {_hintsUsed}");
    }

    /// <summary>
    /// 실수 추가
    /// </summary>
    public void AddMistake()
    {
        _mistakes++;
        Debug.Log($"[INFO] SudokuGameData::AddMistake - Mistakes: {_mistakes}/{MAX_MISTAKES}");

        if (IsGameOver)
        {
            Debug.Log("[INFO] SudokuGameData::AddMistake - Game Over!");
        }
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    /// <param name="points">추가할 점수</param>
    public void AddScore(int points)
    {
        _score += points;
        Debug.Log($"[INFO] SudokuGameData::AddScore - Score: {_score} (+{points})");
    }

    /// <summary>
    /// 게임 완료 처리
    /// </summary>
    public void CompleteGame()
    {
        _isCompleted = true;

        // 점수 계산 (시간 기반 + 힌트 페널티)
        float baseScore = 10000f;
        float timeBonus = Mathf.Max(0, 600f - _playTime); // 10분 기준
        float hintPenalty = _hintsUsed * 100f;

        _score = Mathf.RoundToInt(baseScore + timeBonus - hintPenalty);

        Debug.Log($"[INFO] SudokuGameData::CompleteGame - Completed! Time: {_playTime:F1}s, Score: {_score}");

        UpdateBestTime();
        SaveTotalGames();
    }

    /// <summary>
    /// 최고 기록 업데이트
    /// </summary>
    private void UpdateBestTime()
    {
        float currentBest = GetBestTime(_difficulty);

        if (currentBest == 0 || _playTime < currentBest)
        {
            switch (_difficulty)
            {
                case SudokuDifficulty.Easy:
                    _bestTimeEasy = _playTime;
                    PlayerPrefs.SetFloat("SudokuBestTimeEasy", _bestTimeEasy);
                    break;
                case SudokuDifficulty.Medium:
                    _bestTimeMedium = _playTime;
                    PlayerPrefs.SetFloat("SudokuBestTimeMedium", _bestTimeMedium);
                    break;
                case SudokuDifficulty.Hard:
                    _bestTimeHard = _playTime;
                    PlayerPrefs.SetFloat("SudokuBestTimeHard", _bestTimeHard);
                    break;
            }

            PlayerPrefs.Save();
            Debug.Log($"[INFO] SudokuGameData::UpdateBestTime - New best time for {_difficulty}: {_playTime:F1}s");
        }
    }

    /// <summary>
    /// 최고 기록 로드
    /// </summary>
    private void LoadBestTimes()
    {
        _bestTimeEasy = PlayerPrefs.GetFloat("SudokuBestTimeEasy", 0f);
        _bestTimeMedium = PlayerPrefs.GetFloat("SudokuBestTimeMedium", 0f);
        _bestTimeHard = PlayerPrefs.GetFloat("SudokuBestTimeHard", 0f);

        Debug.Log($"[INFO] SudokuGameData::LoadBestTimes - Easy: {_bestTimeEasy:F1}s, Medium: {_bestTimeMedium:F1}s, Hard: {_bestTimeHard:F1}s");
    }

    /// <summary>
    /// 완료한 게임 수 저장
    /// </summary>
    private void SaveTotalGames()
    {
        _totalGamesCompleted++;
        PlayerPrefs.SetInt("SudokuTotalGames", _totalGamesCompleted);
        PlayerPrefs.Save();

        Debug.Log($"[INFO] SudokuGameData::SaveTotalGames - Total games: {_totalGamesCompleted}");
    }

    /// <summary>
    /// 완료한 게임 수 로드
    /// </summary>
    private void LoadTotalGames()
    {
        _totalGamesCompleted = PlayerPrefs.GetInt("SudokuTotalGames", 0);
        Debug.Log($"[INFO] SudokuGameData::LoadTotalGames - Total games loaded: {_totalGamesCompleted}");
    }

    /// <summary>
    /// 게임 통계 출력
    /// </summary>
    public string GetGameStats()
    {
        float bestTime = GetBestTime(_difficulty);
        string bestTimeStr = bestTime > 0 ? $"{bestTime:F1}s" : "N/A";

        return $"Time: {_playTime:F1}s\n" +
               $"Score: {_score}\n" +
               $"Hints: {_hintsUsed}\n" +
               $"Mistakes: {_mistakes}/{MAX_MISTAKES}\n" +
               $"Best Time ({_difficulty}): {bestTimeStr}\n" +
               $"Total Games: {_totalGamesCompleted}";
    }
}

/// <summary>
/// 스도쿠 난이도
/// </summary>
public enum SudokuDifficulty
{
    Easy,       // 40개 힌트
    Medium,     // 35개 힌트
    Hard        // 30개 힌트
}
