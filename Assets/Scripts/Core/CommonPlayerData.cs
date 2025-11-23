using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 미니게임이 공유하는 공용 플레이어 데이터
/// 플레이어 레벨, 골드 등 게임 간 일관된 보상 및 성장 시스템을 위한 데이터
/// </summary>
[System.Serializable]
public class CommonPlayerData
{
    /// <summary>
    /// 플레이어 레벨 (모든 게임 플레이로 누적 경험치를 얻어 상승)
    /// </summary>
    public int PlayerLevel;

    /// <summary>
    /// 현재 경험치
    /// </summary>
    public int CurrentExp;

    /// <summary>
    /// 골드 (모든 게임에서 사용 가능한 공용 화폐)
    /// </summary>
    public int Gold;

    /// <summary>
    /// 총 플레이 시간 (초 단위)
    /// </summary>
    public int TotalPlayTime;

    /// <summary>
    /// 게임별 최고 점수
    /// Key: 게임 ID (예: "Tetris", "UndeadSurvivor")
    /// Value: 최고 점수
    /// </summary>
    public Dictionary<string, int> GameHighScores;

    /// <summary>
    /// 게임별 플레이 횟수
    /// Key: 게임 ID
    /// Value: 플레이 횟수
    /// </summary>
    public Dictionary<string, int> GamePlayCounts;

    /// <summary>
    /// 게임별 추가 통계 데이터
    /// Key: 게임 ID
    /// Value: 통계 데이터 Dictionary (예: "KillCount", "SurviveTime")
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> GameStatistics;

    /// <summary>
    /// 레벨당 필요 경험치 (레벨 * 100)
    /// </summary>
    public int ExpForNextLevel => PlayerLevel * 100;

    /// <summary>
    /// 경험치 진행률 (0.0 ~ 1.0)
    /// </summary>
    public float ExpProgress => ExpForNextLevel > 0 ? (float)CurrentExp / ExpForNextLevel : 0f;

    /// <summary>
    /// 기본 생성자 - 초기값 설정
    /// </summary>
    public CommonPlayerData()
    {
        PlayerLevel = 1;
        CurrentExp = 0;
        Gold = 0;
        TotalPlayTime = 0;
        GameHighScores = new Dictionary<string, int>();
        GamePlayCounts = new Dictionary<string, int>();
        GameStatistics = new Dictionary<string, Dictionary<string, int>>();
    }

    /// <summary>
    /// 골드 추가 (음수 가능, 단 총 골드는 0 이하로 내려가지 않음)
    /// </summary>
    /// <param name="amount">추가할 골드량 (음수 시 차감)</param>
    /// <returns>실제로 변경된 골드량</returns>
    public int AddGold(int amount)
    {
        int previousGold = Gold;
        Gold += amount;

        if (Gold < 0)
        {
            Gold = 0;
        }

        int actualChange = Gold - previousGold;
        Debug.Log($"[INFO] CommonPlayerData::AddGold - Gold changed: {previousGold} -> {Gold} (amount: {amount}, actual: {actualChange})");

        return actualChange;
    }

    /// <summary>
    /// 특정 게임의 최고 점수 업데이트
    /// 기존 최고 점수보다 높을 경우에만 갱신
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="score">획득한 점수</param>
    /// <returns>최고 점수가 갱신되었으면 true</returns>
    public bool UpdateHighScore(string gameID, int score)
    {
        if (!GameHighScores.ContainsKey(gameID))
        {
            GameHighScores[gameID] = score;
            Debug.Log($"[INFO] CommonPlayerData::UpdateHighScore - New high score for {gameID}: {score}");
            return true;
        }

        if (GameHighScores[gameID] < score)
        {
            int previousScore = GameHighScores[gameID];
            GameHighScores[gameID] = score;
            Debug.Log($"[INFO] CommonPlayerData::UpdateHighScore - High score updated for {gameID}: {previousScore} -> {score}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 특정 게임의 최고 점수 조회
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>최고 점수 (기록이 없으면 0)</returns>
    public int GetHighScore(string gameID)
    {
        if (GameHighScores.ContainsKey(gameID))
        {
            return GameHighScores[gameID];
        }
        return 0;
    }

    /// <summary>
    /// 게임 플레이 횟수 증가
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    public void IncrementPlayCount(string gameID)
    {
        if (!GamePlayCounts.ContainsKey(gameID))
        {
            GamePlayCounts[gameID] = 0;
        }

        GamePlayCounts[gameID]++;
        Debug.Log($"[INFO] CommonPlayerData::IncrementPlayCount - {gameID} play count: {GamePlayCounts[gameID]}");
    }

    /// <summary>
    /// 특정 게임의 플레이 횟수 조회
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>플레이 횟수 (기록이 없으면 0)</returns>
    public int GetPlayCount(string gameID)
    {
        if (GamePlayCounts.ContainsKey(gameID))
        {
            return GamePlayCounts[gameID];
        }
        return 0;
    }

    /// <summary>
    /// 플레이 시간 추가 (초 단위)
    /// </summary>
    /// <param name="seconds">추가할 플레이 시간 (초)</param>
    public void AddPlayTime(int seconds)
    {
        TotalPlayTime += seconds;
    }

    /// <summary>
    /// 경험치 추가 (자동 레벨업 처리)
    /// </summary>
    /// <param name="exp">추가할 경험치</param>
    /// <returns>레벨업 발생 횟수</returns>
    public int AddExp(int exp)
    {
        if (exp <= 0)
        {
            return 0;
        }

        CurrentExp += exp;
        int levelUpCount = 0;

        while (CurrentExp >= ExpForNextLevel)
        {
            CurrentExp -= ExpForNextLevel;
            PlayerLevel++;
            levelUpCount++;
            Debug.Log($"[INFO] CommonPlayerData::AddExp - Level up! New level: {PlayerLevel}");
        }

        if (levelUpCount > 0)
        {
            Debug.Log($"[INFO] CommonPlayerData::AddExp - Total level ups: {levelUpCount}, Current level: {PlayerLevel}, Remaining exp: {CurrentExp}/{ExpForNextLevel}");
        }
        else
        {
            Debug.Log($"[INFO] CommonPlayerData::AddExp - Exp gained: {exp}, Current exp: {CurrentExp}/{ExpForNextLevel}");
        }

        return levelUpCount;
    }

    /// <summary>
    /// 레벨업 (수동 레벨업, 경험치는 소모되지 않음)
    /// </summary>
    /// <returns>새로운 레벨</returns>
    public int LevelUp()
    {
        PlayerLevel++;
        Debug.Log($"[INFO] CommonPlayerData::LevelUp - Manual level up! New level: {PlayerLevel}");
        return PlayerLevel;
    }

    /// <summary>
    /// 게임별 통계 데이터 업데이트
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="statKey">통계 키 (예: "KillCount", "SurviveTime")</param>
    /// <param name="value">값</param>
    public void UpdateGameStatistic(string gameID, string statKey, int value)
    {
        if (!GameStatistics.ContainsKey(gameID))
        {
            GameStatistics[gameID] = new Dictionary<string, int>();
        }

        GameStatistics[gameID][statKey] = value;
        Debug.Log($"[INFO] CommonPlayerData::UpdateGameStatistic - {gameID}.{statKey} = {value}");
    }

    /// <summary>
    /// 게임별 통계 데이터 조회
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="statKey">통계 키</param>
    /// <returns>통계 값 (없으면 0)</returns>
    public int GetGameStatistic(string gameID, string statKey)
    {
        if (GameStatistics.ContainsKey(gameID) && GameStatistics[gameID].ContainsKey(statKey))
        {
            return GameStatistics[gameID][statKey];
        }
        return 0;
    }

    /// <summary>
    /// 게임별 통계 데이터 증가
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="statKey">통계 키</param>
    /// <param name="amount">증가량</param>
    public void IncrementGameStatistic(string gameID, string statKey, int amount = 1)
    {
        int currentValue = GetGameStatistic(gameID, statKey);
        UpdateGameStatistic(gameID, statKey, currentValue + amount);
    }

    /// <summary>
    /// 데이터 검증
    /// </summary>
    /// <returns>유효하면 true</returns>
    public bool Validate()
    {
        bool isValid = true;

        if (PlayerLevel < 1)
        {
            Debug.LogError("[ERROR] CommonPlayerData::Validate - PlayerLevel must be at least 1");
            isValid = false;
        }

        if (CurrentExp < 0)
        {
            Debug.LogError("[ERROR] CommonPlayerData::Validate - CurrentExp cannot be negative");
            isValid = false;
        }

        if (Gold < 0)
        {
            Debug.LogError("[ERROR] CommonPlayerData::Validate - Gold cannot be negative");
            isValid = false;
        }

        if (TotalPlayTime < 0)
        {
            Debug.LogError("[ERROR] CommonPlayerData::Validate - TotalPlayTime cannot be negative");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 데이터를 문자열로 출력 (디버깅용)
    /// </summary>
    public override string ToString()
    {
        return $"CommonPlayerData [Level: {PlayerLevel}, Exp: {CurrentExp}/{ExpForNextLevel}, Gold: {Gold}, PlayTime: {TotalPlayTime}s, Games Played: {GamePlayCounts.Count}]";
    }
}