using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 레지스트리 (싱글톤)
/// 모든 미니게임의 팩토리 함수를 등록하고 관리합니다.
/// OCP(개방-폐쇄 원칙): 새로운 게임 추가 시 RegisterGame만 호출하면 되며, 기존 코드 수정이 불필요합니다.
/// </summary>
public class GameRegistry : Singleton<GameRegistry>
{
    /// <summary>
    /// 게임 ID와 팩토리 함수 매핑
    /// Key: 게임 ID (예: "Tetris", "Sudoku", "SlidingPuzzle")
    /// Value: 해당 게임의 인스턴스를 생성하는 팩토리 함수
    /// </summary>
    private Dictionary<string, Func<IMiniGame>> _gameFactories = new Dictionary<string, Func<IMiniGame>>();

    /// <summary>
    /// 등록된 모든 게임 ID 목록 (읽기 전용)
    /// </summary>
    public IReadOnlyCollection<string> RegisteredGameIDs => _gameFactories.Keys;

    /// <summary>
    /// 등록된 게임 수
    /// </summary>
    public int GameCount => _gameFactories.Count;

    /// <summary>
    /// 게임 팩토리 함수 등록
    /// 동일한 gameID로 여러 번 호출 시 마지막 팩토리로 덮어씁니다.
    /// </summary>
    /// <param name="gameID">게임 고유 ID (대소문자 구분)</param>
    /// <param name="factory">게임 인스턴스를 생성하는 팩토리 함수</param>
    /// <returns>등록 성공 여부</returns>
    public bool RegisterGame(string gameID, Func<IMiniGame> factory)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GameRegistry::RegisterGame - gameID cannot be null or empty");
            return false;
        }

        if (factory == null)
        {
            Debug.LogError($"[ERROR] GameRegistry::RegisterGame - factory for '{gameID}' cannot be null");
            return false;
        }

        if (_gameFactories.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] GameRegistry::RegisterGame - '{gameID}' already registered. Overwriting factory.");
        }

        _gameFactories[gameID] = factory;
        Debug.Log($"[INFO] GameRegistry::RegisterGame - '{gameID}' registered successfully");
        return true;
    }

    /// <summary>
    /// 게임 인스턴스 생성
    /// 등록된 팩토리 함수를 사용하여 새 게임 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="gameID">생성할 게임의 ID</param>
    /// <returns>생성된 게임 인스턴스 (실패 시 null)</returns>
    public IMiniGame CreateGame(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GameRegistry::CreateGame - gameID cannot be null or empty");
            return null;
        }

        if (!_gameFactories.ContainsKey(gameID))
        {
            Debug.LogError($"[ERROR] GameRegistry::CreateGame - '{gameID}' is not registered");
            Debug.Log($"[INFO] Registered games: {string.Join(", ", _gameFactories.Keys)}");
            return null;
        }

        try
        {
            IMiniGame game = _gameFactories[gameID]();

            if (game == null)
            {
                Debug.LogError($"[ERROR] GameRegistry::CreateGame - Factory for '{gameID}' returned null");
                return null;
            }

            Debug.Log($"[INFO] GameRegistry::CreateGame - '{gameID}' instance created successfully");
            return game;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] GameRegistry::CreateGame - Exception while creating '{gameID}': {ex.Message}");
            Debug.LogException(ex);
            return null;
        }
    }

    /// <summary>
    /// 게임 등록 해제
    /// </summary>
    /// <param name="gameID">등록 해제할 게임 ID</param>
    /// <returns>해제 성공 여부</returns>
    public bool UnregisterGame(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GameRegistry::UnregisterGame - gameID cannot be null or empty");
            return false;
        }

        if (!_gameFactories.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] GameRegistry::UnregisterGame - '{gameID}' is not registered");
            return false;
        }

        _gameFactories.Remove(gameID);
        Debug.Log($"[INFO] GameRegistry::UnregisterGame - '{gameID}' unregistered successfully");
        return true;
    }

    /// <summary>
    /// 특정 게임이 등록되어 있는지 확인
    /// </summary>
    /// <param name="gameID">확인할 게임 ID</param>
    /// <returns>등록 여부</returns>
    public bool IsGameRegistered(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            return false;
        }

        return _gameFactories.ContainsKey(gameID);
    }

    /// <summary>
    /// 모든 게임 등록 해제
    /// 게임 종료 시나 레지스트리 초기화 시 호출
    /// </summary>
    public void ClearAll()
    {
        int count = _gameFactories.Count;
        _gameFactories.Clear();
        Debug.Log($"[INFO] GameRegistry::ClearAll - {count} games unregistered");
    }

    /// <summary>
    /// 등록된 모든 게임 ID를 문자열로 반환 (디버깅용)
    /// </summary>
    /// <returns>게임 ID 목록 문자열</returns>
    public string GetRegisteredGamesInfo()
    {
        if (_gameFactories.Count == 0)
        {
            return "No games registered";
        }

        return $"Registered games ({_gameFactories.Count}): {string.Join(", ", _gameFactories.Keys)}";
    }

    /// <summary>
    /// 싱글톤 초기화 시 모든 게임 자동 등록
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // UndeadSurvivor 게임 등록
        RegisterGame("UndeadSurvivor", () => new UndeadSurvivor.UndeadSurvivorGame());

        // 향후 다른 게임들도 여기에 추가
        // RegisterGame("Tetris", () => new Tetris.TetrisGame());

        Debug.Log($"[INFO] GameRegistry::Awake - {_gameFactories.Count} games auto-registered");
    }
}
