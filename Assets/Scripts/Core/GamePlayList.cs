using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 플레이 목록 데이터
/// 어떤 게임들이 플레이 가능한지 관리하는 컴포넌트
/// </summary>
[Serializable]
public class GameInfo
{
    [Tooltip("게임 고유 ID (GameRegistry에 등록된 ID와 일치해야 함)")]
    public string GameID;

    [Tooltip("플레이 가능 여부")]
    public bool IsPlayable;

    public GameInfo(string gameID, bool isPlayable = true)
    {
        GameID = gameID;
        IsPlayable = isPlayable;
    }
}

/// <summary>
/// 게임 플레이 목록 관리 컴포넌트
/// Inspector에서 게임 목록과 플레이 가능 여부를 설정할 수 있습니다.
/// </summary>
public class GamePlayList : MonoBehaviour
{
    [Header("게임 목록 설정")]
    [SerializeField]
    [Tooltip("플레이 가능한 게임 목록 (Inspector에서 설정)")]
    private List<GameInfo> _gameList = new List<GameInfo>();

    /// <summary>
    /// 게임 목록 (읽기 전용)
    /// </summary>
    public IReadOnlyList<GameInfo> GameList => _gameList;

    /// <summary>
    /// 플레이 가능한 게임 목록만 반환
    /// </summary>
    public List<GameInfo> GetPlayableGames()
    {
        List<GameInfo> playableGames = new List<GameInfo>();

        foreach (var gameInfo in _gameList)
        {
            if (gameInfo.IsPlayable && !string.IsNullOrEmpty(gameInfo.GameID))
            {
                playableGames.Add(gameInfo);
            }
        }

        Debug.Log($"[INFO] GamePlayList::GetPlayableGames - Found {playableGames.Count} playable games");
        return playableGames;
    }

    /// <summary>
    /// 특정 게임이 플레이 가능한지 확인
    /// </summary>
    /// <param name="gameID">확인할 게임 ID</param>
    /// <returns>플레이 가능 여부</returns>
    public bool IsGamePlayable(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            return false;
        }

        foreach (var gameInfo in _gameList)
        {
            if (gameInfo.GameID == gameID)
            {
                return gameInfo.IsPlayable;
            }
        }

        return false;
    }

    /// <summary>
    /// 게임의 플레이 가능 상태 변경
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="isPlayable">플레이 가능 여부</param>
    /// <returns>변경 성공 여부</returns>
    public bool SetGamePlayable(string gameID, bool isPlayable)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GamePlayList::SetGamePlayable - gameID cannot be null or empty");
            return false;
        }

        foreach (var gameInfo in _gameList)
        {
            if (gameInfo.GameID == gameID)
            {
                gameInfo.IsPlayable = isPlayable;
                Debug.Log($"[INFO] GamePlayList::SetGamePlayable - '{gameID}' playable status set to {isPlayable}");
                return true;
            }
        }

        Debug.LogWarning($"[WARNING] GamePlayList::SetGamePlayable - Game '{gameID}' not found in list");
        return false;
    }

    /// <summary>
    /// 게임을 목록에 추가
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="isPlayable">플레이 가능 여부</param>
    public void AddGame(string gameID, bool isPlayable = true)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GamePlayList::AddGame - gameID cannot be null or empty");
            return;
        }

        // 이미 존재하는지 확인
        foreach (var gameInfo in _gameList)
        {
            if (gameInfo.GameID == gameID)
            {
                Debug.LogWarning($"[WARNING] GamePlayList::AddGame - Game '{gameID}' already exists");
                return;
            }
        }

        _gameList.Add(new GameInfo(gameID, isPlayable));
        Debug.Log($"[INFO] GamePlayList::AddGame - Added '{gameID}' to game list");
    }

    /// <summary>
    /// 게임을 목록에서 제거
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveGame(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GamePlayList::RemoveGame - gameID cannot be null or empty");
            return false;
        }

        for (int i = 0; i < _gameList.Count; i++)
        {
            if (_gameList[i].GameID == gameID)
            {
                _gameList.RemoveAt(i);
                Debug.Log($"[INFO] GamePlayList::RemoveGame - Removed '{gameID}' from game list");
                return true;
            }
        }

        Debug.LogWarning($"[WARNING] GamePlayList::RemoveGame - Game '{gameID}' not found");
        return false;
    }

    /// <summary>
    /// 게임 수 반환
    /// </summary>
    public int GetGameCount()
    {
        return _gameList.Count;
    }

    /// <summary>
    /// 플레이 가능한 게임 수 반환
    /// </summary>
    public int GetPlayableGameCount()
    {
        return GetPlayableGames().Count;
    }

    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log("=== GamePlayList Debug Info ===");
        Debug.Log($"Total Games: {_gameList.Count}");
        Debug.Log($"Playable Games: {GetPlayableGameCount()}");

        foreach (var gameInfo in _gameList)
        {
            Debug.Log($"  - {gameInfo.GameID}: {(gameInfo.IsPlayable ? "Playable" : "Locked")}");
        }

        Debug.Log("==============================");
    }

    private void OnValidate()
    {
        // Inspector에서 값 변경 시 자동으로 null/빈 문자열 체크
        for (int i = _gameList.Count - 1; i >= 0; i--)
        {
            if (_gameList[i] == null || string.IsNullOrEmpty(_gameList[i].GameID))
            {
                Debug.LogWarning($"[WARNING] GamePlayList::OnValidate - Invalid game entry at index {i}");
            }
        }
    }
}
