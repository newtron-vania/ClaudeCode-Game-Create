using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 게임용 데이터 관리 매니저
/// 모든 미니게임의 데이터 제공자를 중앙에서 관리합니다.
/// 지연 로딩 방식으로 게임 시작 시에만 해당 게임 데이터를 로드합니다.
/// </summary>
public class DataManager : Singleton<DataManager>
{
    // 등록된 데이터 제공자들 (key: GameID, value: Provider)
    private Dictionary<string, IGameDataProvider> _providers = new Dictionary<string, IGameDataProvider>();

    // 현재 로드된 게임 데이터 목록
    private HashSet<string> _loadedGames = new HashSet<string>();

    // 초기화 여부
    private bool _isInitialized = false;

    /// <summary>
    /// 초기화 여부
    /// </summary>
    public bool IsInitialized => _isInitialized;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[INFO] DataManager::Awake - DataManager initialized");

        // 모든 게임 데이터 프로바이더 자동 등록
        RegisterProvider(new SudokuDataProvider());

        _isInitialized = true;
        Debug.Log($"[INFO] DataManager::Awake - {_providers.Count} providers auto-registered");
    }

    /// <summary>
    /// 데이터 제공자 등록
    /// 게임 시작 시 또는 GameRegistry에서 호출합니다.
    /// </summary>
    /// <param name="provider">등록할 데이터 제공자</param>
    public void RegisterProvider(IGameDataProvider provider)
    {
        if (provider == null)
        {
            Debug.LogError("[ERROR] DataManager::RegisterProvider - Provider is null");
            return;
        }

        string gameID = provider.GameID;

        if (_providers.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] DataManager::RegisterProvider - Provider for {gameID} already registered. Replacing...");
            _providers[gameID] = provider;
        }
        else
        {
            _providers.Add(gameID, provider);
            Debug.Log($"[INFO] DataManager::RegisterProvider - Provider for {gameID} registered");
        }

        // 초기화
        provider.Initialize();
    }

    /// <summary>
    /// 데이터 제공자 등록 해제
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    public void UnregisterProvider(string gameID)
    {
        if (!_providers.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] DataManager::UnregisterProvider - Provider for {gameID} not found");
            return;
        }

        // 로드된 데이터가 있으면 먼저 언로드
        if (_loadedGames.Contains(gameID))
        {
            UnloadGameData(gameID);
        }

        _providers.Remove(gameID);
        Debug.Log($"[INFO] DataManager::UnregisterProvider - Provider for {gameID} unregistered");
    }

    /// <summary>
    /// 게임 데이터 로드 (지연 로딩)
    /// 게임 시작 시 호출하여 필요한 데이터만 로드합니다.
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    public void LoadGameData(string gameID)
    {
        if (!_providers.ContainsKey(gameID))
        {
            Debug.LogError($"[ERROR] DataManager::LoadGameData - Provider for {gameID} not registered");
            return;
        }

        if (_loadedGames.Contains(gameID))
        {
            Debug.LogWarning($"[WARNING] DataManager::LoadGameData - Data for {gameID} already loaded");
            return;
        }

        Debug.Log($"[INFO] DataManager::LoadGameData - Loading data for {gameID}");

        IGameDataProvider provider = _providers[gameID];
        provider.LoadData();

        if (provider.IsLoaded)
        {
            _loadedGames.Add(gameID);
            Debug.Log($"[INFO] DataManager::LoadGameData - Data for {gameID} loaded successfully");
        }
        else
        {
            Debug.LogError($"[ERROR] DataManager::LoadGameData - Failed to load data for {gameID}");
        }
    }

    /// <summary>
    /// 게임 데이터 언로드
    /// 게임 종료 시 호출하여 메모리를 해제합니다.
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    public void UnloadGameData(string gameID)
    {
        if (!_providers.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] DataManager::UnloadGameData - Provider for {gameID} not found");
            return;
        }

        if (!_loadedGames.Contains(gameID))
        {
            Debug.LogWarning($"[WARNING] DataManager::UnloadGameData - Data for {gameID} not loaded");
            return;
        }

        Debug.Log($"[INFO] DataManager::UnloadGameData - Unloading data for {gameID}");

        IGameDataProvider provider = _providers[gameID];
        provider.UnloadData();

        _loadedGames.Remove(gameID);
        Debug.Log($"[INFO] DataManager::UnloadGameData - Data for {gameID} unloaded");
    }

    /// <summary>
    /// 데이터 제공자 조회
    /// </summary>
    /// <typeparam name="T">데이터 제공자 타입</typeparam>
    /// <param name="gameID">게임 ID</param>
    /// <returns>데이터 제공자 (없으면 null)</returns>
    public T GetProvider<T>(string gameID) where T : class, IGameDataProvider
    {
        if (!_providers.ContainsKey(gameID))
        {
            Debug.LogError($"[ERROR] DataManager::GetProvider - Provider for {gameID} not registered");
            return null;
        }

        IGameDataProvider provider = _providers[gameID];

        if (provider is T typedProvider)
        {
            return typedProvider;
        }

        Debug.LogError($"[ERROR] DataManager::GetProvider - Provider for {gameID} is not of type {typeof(T).Name}");
        return null;
    }

    /// <summary>
    /// 데이터 제공자 조회 (타입 체크 없음)
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>데이터 제공자 (없으면 null)</returns>
    public IGameDataProvider GetProvider(string gameID)
    {
        if (!_providers.ContainsKey(gameID))
        {
            Debug.LogError($"[ERROR] DataManager::GetProvider - Provider for {gameID} not registered");
            return null;
        }

        return _providers[gameID];
    }

    /// <summary>
    /// 특정 게임의 데이터 로드 여부 확인
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>로드되었으면 true</returns>
    public bool IsGameDataLoaded(string gameID)
    {
        return _loadedGames.Contains(gameID);
    }

    /// <summary>
    /// 등록된 제공자 여부 확인
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <returns>등록되었으면 true</returns>
    public bool HasProvider(string gameID)
    {
        return _providers.ContainsKey(gameID);
    }

    /// <summary>
    /// 모든 게임 데이터 언로드
    /// </summary>
    public void UnloadAllGameData()
    {
        Debug.Log("[INFO] DataManager::UnloadAllGameData - Unloading all game data");

        List<string> loadedGamesCopy = new List<string>(_loadedGames);

        foreach (string gameID in loadedGamesCopy)
        {
            UnloadGameData(gameID);
        }

        Debug.Log($"[INFO] DataManager::UnloadAllGameData - Unloaded {loadedGamesCopy.Count} games");
    }

    /// <summary>
    /// 모든 제공자 해제 및 데이터 정리
    /// </summary>
    public void ClearAll()
    {
        Debug.Log("[INFO] DataManager::ClearAll - Clearing all providers and data");

        UnloadAllGameData();

        _providers.Clear();
        _loadedGames.Clear();

        Debug.Log("[INFO] DataManager::ClearAll - DataManager cleared");
    }

    /// <summary>
    /// 등록된 제공자 목록 반환
    /// </summary>
    /// <returns>게임 ID 목록</returns>
    public List<string> GetRegisteredProviders()
    {
        return new List<string>(_providers.Keys);
    }

    /// <summary>
    /// 로드된 게임 목록 반환
    /// </summary>
    /// <returns>로드된 게임 ID 목록</returns>
    public List<string> GetLoadedGames()
    {
        return new List<string>(_loadedGames);
    }

    private void OnApplicationQuit()
    {
        UnloadAllGameData();
    }
}
