using System;
using UnityEngine;

/// <summary>
/// 테트리스 게임 데이터
/// IGameData를 구현하여 게임 상태 저장/로드를 지원합니다.
/// </summary>
[Serializable]
public class TetrisData : IGameData
{
    /// <summary>
    /// 게임 보드 크기
    /// </summary>
    public const int BOARD_WIDTH = 10;
    public const int BOARD_HEIGHT = 20;

    /// <summary>
    /// 게임 보드 상태 (2차원 배열)
    /// 0: 빈 칸, 1~7: 블록 타입
    /// </summary>
    public int[,] BoardState;

    /// <summary>
    /// 현재 떨어지는 블록
    /// </summary>
    public TetrisPiece CurrentPiece;

    /// <summary>
    /// 다음 블록 (미리보기)
    /// </summary>
    public TetrisPiece NextPiece;

    /// <summary>
    /// 현재 점수
    /// </summary>
    public int CurrentScore;

    /// <summary>
    /// 제거한 라인 수
    /// </summary>
    public int LinesCleared;

    /// <summary>
    /// 현재 레벨
    /// </summary>
    public int Level;

    /// <summary>
    /// 게임 오버 여부
    /// </summary>
    public bool IsGameOver;

    /// <summary>
    /// 마지막 블록 이동 시간 (자동 낙하용)
    /// </summary>
    public float LastMoveTime;

    /// <summary>
    /// 낙하 간격 (초 단위, 레벨에 따라 감소)
    /// </summary>
    public float FallInterval;

    /// <summary>
    /// 게임 플레이 시간 (초 단위)
    /// </summary>
    public float PlayTime;

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    public void Initialize()
    {
        BoardState = new int[BOARD_HEIGHT, BOARD_WIDTH];
        CurrentPiece = new TetrisPiece();
        NextPiece = new TetrisPiece();
        CurrentScore = 0;
        LinesCleared = 0;
        Level = 1;
        IsGameOver = false;
        LastMoveTime = 0f;
        FallInterval = 1.0f; // 초기 1초 간격
        PlayTime = 0f;

        // 보드 초기화 (모든 칸을 빈 칸으로)
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                BoardState[y, x] = 0;
            }
        }

        Debug.Log("[INFO] TetrisData::Initialize - Tetris data initialized");
    }

    /// <summary>
    /// 데이터 리셋 (재시작)
    /// </summary>
    public void Reset()
    {
        Initialize();
        Debug.Log("[INFO] TetrisData::Reset - Tetris data reset");
    }

    /// <summary>
    /// 데이터 검증
    /// </summary>
    /// <returns>데이터가 유효하면 true</returns>
    public bool Validate()
    {
        if (BoardState == null)
        {
            Debug.LogError("[ERROR] TetrisData::Validate - BoardState is null");
            return false;
        }

        if (BoardState.GetLength(0) != BOARD_HEIGHT || BoardState.GetLength(1) != BOARD_WIDTH)
        {
            Debug.LogError($"[ERROR] TetrisData::Validate - Invalid board size: {BoardState.GetLength(0)}x{BoardState.GetLength(1)}");
            return false;
        }

        if (CurrentScore < 0)
        {
            Debug.LogError($"[ERROR] TetrisData::Validate - Invalid score: {CurrentScore}");
            return false;
        }

        if (LinesCleared < 0)
        {
            Debug.LogError($"[ERROR] TetrisData::Validate - Invalid lines cleared: {LinesCleared}");
            return false;
        }

        if (Level < 1)
        {
            Debug.LogError($"[ERROR] TetrisData::Validate - Invalid level: {Level}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 게임 상태 저장
    /// PlayerPrefs를 사용한 간단한 저장 (추후 JSON으로 확장 가능)
    /// </summary>
    public void SaveState()
    {
        try
        {
            PlayerPrefs.SetInt("Tetris_Score", CurrentScore);
            PlayerPrefs.SetInt("Tetris_Lines", LinesCleared);
            PlayerPrefs.SetInt("Tetris_Level", Level);
            PlayerPrefs.SetFloat("Tetris_PlayTime", PlayTime);
            PlayerPrefs.Save();

            Debug.Log($"[INFO] TetrisData::SaveState - Saved: Score={CurrentScore}, Lines={LinesCleared}, Level={Level}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] TetrisData::SaveState - Exception: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// 게임 상태 로드
    /// PlayerPrefs에서 저장된 데이터 불러오기
    /// </summary>
    public void LoadState()
    {
        try
        {
            if (PlayerPrefs.HasKey("Tetris_Score"))
            {
                CurrentScore = PlayerPrefs.GetInt("Tetris_Score", 0);
                LinesCleared = PlayerPrefs.GetInt("Tetris_Lines", 0);
                Level = PlayerPrefs.GetInt("Tetris_Level", 1);
                PlayTime = PlayerPrefs.GetFloat("Tetris_PlayTime", 0f);

                Debug.Log($"[INFO] TetrisData::LoadState - Loaded: Score={CurrentScore}, Lines={LinesCleared}, Level={Level}");
            }
            else
            {
                Debug.LogWarning("[WARNING] TetrisData::LoadState - No saved data found, using defaults");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] TetrisData::LoadState - Exception: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    /// <param name="points">추가할 점수</param>
    public void AddScore(int points)
    {
        CurrentScore += points;
        Debug.Log($"[INFO] TetrisData::AddScore - Score: {CurrentScore} (+{points})");
    }

    /// <summary>
    /// 라인 제거 처리
    /// </summary>
    /// <param name="lineCount">제거된 라인 수</param>
    public void AddClearedLines(int lineCount)
    {
        LinesCleared += lineCount;

        // 라인 수에 따른 점수 계산 (테트리스 표준 점수)
        int[] scoreTable = { 0, 100, 300, 500, 800 }; // 0줄, 1줄, 2줄, 3줄, 4줄
        int lineScore = lineCount > 0 && lineCount <= 4 ? scoreTable[lineCount] : 0;
        AddScore(lineScore * Level);

        // 10줄마다 레벨 업
        int newLevel = (LinesCleared / 10) + 1;
        if (newLevel > Level)
        {
            Level = newLevel;
            UpdateFallInterval();
            Debug.Log($"[INFO] TetrisData::AddClearedLines - Level up! New level: {Level}");
        }

        Debug.Log($"[INFO] TetrisData::AddClearedLines - Lines: {LinesCleared} (+{lineCount})");
    }

    /// <summary>
    /// 레벨에 따른 낙하 속도 조정
    /// </summary>
    private void UpdateFallInterval()
    {
        // 레벨이 올라갈수록 낙하 속도 증가
        FallInterval = Mathf.Max(0.1f, 1.0f - (Level - 1) * 0.1f);
        Debug.Log($"[INFO] TetrisData::UpdateFallInterval - Fall interval: {FallInterval}s");
    }

    /// <summary>
    /// 특정 위치의 블록 타입 가져오기
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <returns>블록 타입 (0: 빈 칸, 1~7: 블록)</returns>
    public int GetBlock(int x, int y)
    {
        if (x < 0 || x >= BOARD_WIDTH || y < 0 || y >= BOARD_HEIGHT)
        {
            return -1; // 범위 밖
        }
        return BoardState[y, x];
    }

    /// <summary>
    /// 특정 위치에 블록 설정
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <param name="blockType">블록 타입</param>
    public void SetBlock(int x, int y, int blockType)
    {
        if (x < 0 || x >= BOARD_WIDTH || y < 0 || y >= BOARD_HEIGHT)
        {
            Debug.LogWarning($"[WARNING] TetrisData::SetBlock - Out of bounds: ({x}, {y})");
            return;
        }
        BoardState[y, x] = blockType;
    }

    /// <summary>
    /// 보드 상태를 문자열로 출력 (디버깅용)
    /// </summary>
    public override string ToString()
    {
        return $"TetrisData [Score: {CurrentScore}, Lines: {LinesCleared}, Level: {Level}, GameOver: {IsGameOver}]";
    }
}
