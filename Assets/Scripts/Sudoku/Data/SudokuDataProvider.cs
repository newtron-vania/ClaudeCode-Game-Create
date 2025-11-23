using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스도쿠 게임의 데이터 제공자
/// IGameDataProvider를 구현하여 DataManager와 통합
/// </summary>
public class SudokuDataProvider : IGameDataProvider
{
    public string GameID => "Sudoku";
    public bool IsLoaded { get; private set; }

    // 난이도별 설정
    private Dictionary<SudokuDifficulty, SudokuDifficultyConfig> _difficultyConfigs;

    /// <summary>
    /// 난이도 설정 데이터 구조
    /// </summary>
    public class SudokuDifficultyConfig
    {
        public int MinHints;        // 최소 힌트 개수
        public int MaxHints;        // 최대 힌트 개수
        public string DisplayName;  // 표시 이름
        public Color ThemeColor;    // 테마 색상

        public SudokuDifficultyConfig(int minHints, int maxHints, string displayName, Color themeColor)
        {
            MinHints = minHints;
            MaxHints = maxHints;
            DisplayName = displayName;
            ThemeColor = themeColor;
        }
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize()
    {
        _difficultyConfigs = new Dictionary<SudokuDifficulty, SudokuDifficultyConfig>();
        IsLoaded = false;

        Debug.Log("[INFO] SudokuDataProvider::Initialize - Data provider initialized");
    }

    /// <summary>
    /// 데이터 로드
    /// </summary>
    public void LoadData()
    {
        if (IsLoaded)
        {
            Debug.LogWarning("[WARNING] SudokuDataProvider::LoadData - Data already loaded");
            return;
        }

        Debug.Log("[INFO] SudokuDataProvider::LoadData - Loading Sudoku data");

        LoadDifficultyConfigs();

        IsLoaded = true;
        Debug.Log("[INFO] SudokuDataProvider::LoadData - Data loaded successfully");
    }

    /// <summary>
    /// 데이터 언로드
    /// </summary>
    public void UnloadData()
    {
        if (!IsLoaded)
        {
            Debug.LogWarning("[WARNING] SudokuDataProvider::UnloadData - Data not loaded");
            return;
        }

        Debug.Log("[INFO] SudokuDataProvider::UnloadData - Unloading Sudoku data");

        _difficultyConfigs.Clear();

        IsLoaded = false;
        Debug.Log("[INFO] SudokuDataProvider::UnloadData - Data unloaded");
    }

    /// <summary>
    /// 데이터 조회 (제네릭)
    /// </summary>
    public T GetData<T>(string key) where T : class
    {
        Debug.LogWarning($"[WARNING] SudokuDataProvider::GetData - Generic GetData not implemented for key: {key}");
        return null;
    }

    /// <summary>
    /// 데이터 존재 여부 확인
    /// </summary>
    public bool HasData(string key)
    {
        return false;
    }

    #region 데이터 로딩

    /// <summary>
    /// 난이도 설정 로드
    /// </summary>
    private void LoadDifficultyConfigs()
    {
        // Easy: 36-40 힌트 (PRD: 35-40)
        _difficultyConfigs[SudokuDifficulty.Easy] = new SudokuDifficultyConfig(
            minHints: 36,
            maxHints: 40,
            displayName: "쉬움",
            themeColor: new Color(0.3f, 0.8f, 0.3f) // 녹색
        );

        // Medium: 30-34 힌트 (PRD: 28-34)
        _difficultyConfigs[SudokuDifficulty.Medium] = new SudokuDifficultyConfig(
            minHints: 30,
            maxHints: 34,
            displayName: "중간",
            themeColor: new Color(1f, 0.7f, 0f) // 주황색
        );

        // Hard: 24-27 힌트 (PRD: 22-27)
        _difficultyConfigs[SudokuDifficulty.Hard] = new SudokuDifficultyConfig(
            minHints: 24,
            maxHints: 27,
            displayName: "어려움",
            themeColor: new Color(0.9f, 0.2f, 0.2f) // 빨간색
        );

        Debug.Log("[INFO] SudokuDataProvider::LoadDifficultyConfigs - Difficulty configs loaded");
    }

    #endregion

    #region Public API

    /// <summary>
    /// 난이도 설정 조회
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <returns>난이도 설정</returns>
    public SudokuDifficultyConfig GetDifficultyConfig(SudokuDifficulty difficulty)
    {
        if (!IsLoaded)
        {
            Debug.LogError("[ERROR] SudokuDataProvider::GetDifficultyConfig - Data not loaded");
            return null;
        }

        if (_difficultyConfigs.TryGetValue(difficulty, out SudokuDifficultyConfig config))
        {
            return config;
        }

        Debug.LogError($"[ERROR] SudokuDataProvider::GetDifficultyConfig - Difficulty {difficulty} not found");
        return null;
    }

    /// <summary>
    /// 난이도별 힌트 개수 가져오기
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <returns>힌트 개수 (랜덤 범위 내)</returns>
    public int GetHintCount(SudokuDifficulty difficulty)
    {
        SudokuDifficultyConfig config = GetDifficultyConfig(difficulty);

        if (config == null)
        {
            Debug.LogWarning($"[WARNING] SudokuDataProvider::GetHintCount - Using default hint count for {difficulty}");
            return 30; // 기본값
        }

        int hintCount = Random.Range(config.MinHints, config.MaxHints + 1);
        Debug.Log($"[INFO] SudokuDataProvider::GetHintCount - {difficulty}: {hintCount} hints ({config.MinHints}-{config.MaxHints})");

        return hintCount;
    }

    /// <summary>
    /// 난이도 표시 이름 가져오기
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <returns>표시 이름</returns>
    public string GetDifficultyDisplayName(SudokuDifficulty difficulty)
    {
        SudokuDifficultyConfig config = GetDifficultyConfig(difficulty);

        if (config == null)
        {
            return difficulty.ToString();
        }

        return config.DisplayName;
    }

    /// <summary>
    /// 난이도 테마 색상 가져오기
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <returns>테마 색상</returns>
    public Color GetDifficultyColor(SudokuDifficulty difficulty)
    {
        SudokuDifficultyConfig config = GetDifficultyConfig(difficulty);

        if (config == null)
        {
            return Color.white;
        }

        return config.ThemeColor;
    }

    /// <summary>
    /// 모든 난이도 목록 반환
    /// </summary>
    /// <returns>난이도 목록</returns>
    public List<SudokuDifficulty> GetAllDifficulties()
    {
        return new List<SudokuDifficulty>
        {
            SudokuDifficulty.Easy,
            SudokuDifficulty.Medium,
            SudokuDifficulty.Hard
        };
    }

    #endregion
}
