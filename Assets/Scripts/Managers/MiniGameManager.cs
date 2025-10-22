using UnityEngine;

/// <summary>
/// 미니게임 플랫폼 관리자 (싱글톤)
/// IMiniGame 기반의 미니게임들을 생명주기 관리합니다.
/// GameRegistry와 협력하여 게임 로드, 전환, 업데이트를 처리합니다.
/// </summary>
public class MiniGameManager : Singleton<MiniGameManager>
{
    /// <summary>
    /// 현재 실행 중인 미니게임 인스턴스
    /// </summary>
    private IMiniGame _currentGame;

    /// <summary>
    /// 모든 미니게임이 공유하는 플레이어 데이터
    /// </summary>
    private CommonPlayerData _commonPlayerData;

    /// <summary>
    /// 현재 로드된 게임 ID
    /// </summary>
    public string CurrentGameID { get; private set; }

    /// <summary>
    /// 게임이 실행 중인지 여부
    /// </summary>
    public bool IsGameRunning { get; private set; }

    /// <summary>
    /// 공용 플레이어 데이터 접근 (읽기 전용)
    /// </summary>
    public CommonPlayerData CommonData => _commonPlayerData;

    protected override void Awake()
    {
        base.Awake();

        // 공용 플레이어 데이터 초기화
        _commonPlayerData = new CommonPlayerData();
        Debug.Log("[INFO] MiniGameManager::Awake - Common player data initialized");
    }

    private void Update()
    {
        // 현재 게임이 실행 중이면 Update 호출
        if (IsGameRunning && _currentGame != null)
        {
            _currentGame.Update(Time.deltaTime);
        }
    }

    /// <summary>
    /// 게임 로드 및 시작
    /// GameRegistry에서 게임을 생성하고 Initialize → StartGame 순서로 호출
    /// </summary>
    /// <param name="gameID">로드할 게임 ID</param>
    /// <returns>로드 성공 여부</returns>
    public bool LoadGame(string gameID)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] MiniGameManager::LoadGame - gameID cannot be null or empty");
            return false;
        }

        // 기존 게임이 실행 중이면 정리
        if (_currentGame != null)
        {
            Debug.Log($"[INFO] MiniGameManager::LoadGame - Cleaning up current game: {CurrentGameID}");
            UnloadCurrentGame();
        }

        // GameRegistry에서 게임 인스턴스 생성
        IMiniGame newGame = GameRegistry.Instance.CreateGame(gameID);
        if (newGame == null)
        {
            Debug.LogError($"[ERROR] MiniGameManager::LoadGame - Failed to create game: {gameID}");
            return false;
        }

        // 게임 초기화
        try
        {
            newGame.Initialize(_commonPlayerData);
            Debug.Log($"[INFO] MiniGameManager::LoadGame - Game '{gameID}' initialized");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] MiniGameManager::LoadGame - Exception during Initialize: {ex.Message}");
            Debug.LogException(ex);
            return false;
        }

        // 게임 시작
        try
        {
            newGame.StartGame();
            Debug.Log($"[INFO] MiniGameManager::LoadGame - Game '{gameID}' started");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] MiniGameManager::LoadGame - Exception during StartGame: {ex.Message}");
            Debug.LogException(ex);
            newGame.Cleanup();
            return false;
        }

        // 현재 게임 설정
        _currentGame = newGame;
        CurrentGameID = gameID;
        IsGameRunning = true;

        // 플레이 횟수 증가
        _commonPlayerData.IncrementPlayCount(gameID);

        Debug.Log($"[INFO] MiniGameManager::LoadGame - Successfully loaded game: {gameID}");
        return true;
    }

    /// <summary>
    /// 현재 게임 언로드
    /// Cleanup 호출 및 리소스 정리
    /// </summary>
    public void UnloadCurrentGame()
    {
        if (_currentGame == null)
        {
            Debug.LogWarning("[WARNING] MiniGameManager::UnloadCurrentGame - No game to unload");
            return;
        }

        Debug.Log($"[INFO] MiniGameManager::UnloadCurrentGame - Unloading game: {CurrentGameID}");

        try
        {
            _currentGame.Cleanup();
            Debug.Log($"[INFO] MiniGameManager::UnloadCurrentGame - Game '{CurrentGameID}' cleaned up");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] MiniGameManager::UnloadCurrentGame - Exception during Cleanup: {ex.Message}");
            Debug.LogException(ex);
        }

        _currentGame = null;
        CurrentGameID = null;
        IsGameRunning = false;
    }

    /// <summary>
    /// 다른 게임으로 전환
    /// 현재 게임을 언로드하고 새 게임을 로드합니다.
    /// </summary>
    /// <param name="newGameID">전환할 게임 ID</param>
    /// <returns>전환 성공 여부</returns>
    public bool SwitchGame(string newGameID)
    {
        if (string.IsNullOrEmpty(newGameID))
        {
            Debug.LogError("[ERROR] MiniGameManager::SwitchGame - newGameID cannot be null or empty");
            return false;
        }

        if (CurrentGameID == newGameID)
        {
            Debug.LogWarning($"[WARNING] MiniGameManager::SwitchGame - Already running game: {newGameID}");
            return false;
        }

        Debug.Log($"[INFO] MiniGameManager::SwitchGame - Switching from '{CurrentGameID}' to '{newGameID}'");

        return LoadGame(newGameID);
    }

    /// <summary>
    /// 현재 게임 데이터 저장
    /// IGameData.SaveState() 호출
    /// </summary>
    public void SaveCurrentGame()
    {
        if (_currentGame == null)
        {
            Debug.LogWarning("[WARNING] MiniGameManager::SaveCurrentGame - No game to save");
            return;
        }

        IGameData gameData = _currentGame.GetData();
        if (gameData == null)
        {
            Debug.LogError($"[ERROR] MiniGameManager::SaveCurrentGame - Game '{CurrentGameID}' returned null data");
            return;
        }

        try
        {
            gameData.SaveState();
            Debug.Log($"[INFO] MiniGameManager::SaveCurrentGame - Game '{CurrentGameID}' data saved");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] MiniGameManager::SaveCurrentGame - Exception during SaveState: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// 현재 게임 데이터 로드
    /// IGameData.LoadState() 호출
    /// </summary>
    public void LoadCurrentGame()
    {
        if (_currentGame == null)
        {
            Debug.LogWarning("[WARNING] MiniGameManager::LoadCurrentGame - No game to load");
            return;
        }

        IGameData gameData = _currentGame.GetData();
        if (gameData == null)
        {
            Debug.LogError($"[ERROR] MiniGameManager::LoadCurrentGame - Game '{CurrentGameID}' returned null data");
            return;
        }

        try
        {
            gameData.LoadState();
            Debug.Log($"[INFO] MiniGameManager::LoadCurrentGame - Game '{CurrentGameID}' data loaded");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] MiniGameManager::LoadCurrentGame - Exception during LoadState: {ex.Message}");
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// 공용 플레이어 데이터에 골드 추가
    /// </summary>
    /// <param name="amount">추가할 골드량 (음수 가능)</param>
    /// <returns>실제로 변경된 골드량</returns>
    public int AddGold(int amount)
    {
        int actualChange = _commonPlayerData.AddGold(amount);
        Debug.Log($"[INFO] MiniGameManager::AddGold - Gold changed by {actualChange}");
        return actualChange;
    }

    /// <summary>
    /// 현재 게임의 최고 점수 업데이트
    /// </summary>
    /// <param name="score">획득한 점수</param>
    /// <returns>최고 점수가 갱신되었으면 true</returns>
    public bool UpdateHighScore(int score)
    {
        if (string.IsNullOrEmpty(CurrentGameID))
        {
            Debug.LogWarning("[WARNING] MiniGameManager::UpdateHighScore - No game running");
            return false;
        }

        return _commonPlayerData.UpdateHighScore(CurrentGameID, score);
    }

    /// <summary>
    /// 현재 게임의 최고 점수 조회
    /// </summary>
    /// <returns>최고 점수 (기록이 없으면 0)</returns>
    public int GetCurrentGameHighScore()
    {
        if (string.IsNullOrEmpty(CurrentGameID))
        {
            Debug.LogWarning("[WARNING] MiniGameManager::GetCurrentGameHighScore - No game running");
            return 0;
        }

        return _commonPlayerData.GetHighScore(CurrentGameID);
    }

    /// <summary>
    /// 애플리케이션 종료 시 정리
    /// </summary>
    private void OnApplicationQuit()
    {
        if (_currentGame != null)
        {
            Debug.Log("[INFO] MiniGameManager::OnApplicationQuit - Cleaning up before quit");
            UnloadCurrentGame();
        }
    }

    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log("=== MiniGameManager Debug Info ===");
        Debug.Log($"Current Game: {CurrentGameID ?? "None"}");
        Debug.Log($"Is Running: {IsGameRunning}");
        Debug.Log($"Common Data: {_commonPlayerData}");
        Debug.Log($"Registry: {GameRegistry.Instance.GetRegisteredGamesInfo()}");
        Debug.Log("==================================");
    }
}
