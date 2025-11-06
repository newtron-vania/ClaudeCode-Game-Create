using UnityEngine;
using UndeadSurvivor;

/// <summary>
/// Undead Survivor 게임 플레이 씬 컨트롤러
/// BaseScene을 상속받아 씬 생명주기를 관리하고 IMiniGame과 MiniGameManager를 연결합니다.
/// 실제 게임 플레이 (전투, 생존)가 진행되는 씬입니다.
/// </summary>
public class UndeadSurvivorGameScene : BaseScene
{
    /// <summary>
    /// 씬 ID
    /// </summary>
    public override SceneID SceneID => SceneID.UndeadSurvivorGameScene;

    private bool _isGameRunning;

    #region BaseScene Overrides

    private int _selectedCharacterId = 1; // PlayerPrefs에서 로드됨

    /// <summary>
    /// 씬 초기화
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // 선택한 캐릭터 ID 로드
        _selectedCharacterId = PlayerPrefs.GetInt("SelectedCharacterId", 1);
        Debug.Log($"[INFO] UndeadSurvivorGameScene::Awake - Selected character ID: {_selectedCharacterId}");

        // DataManager에 UndeadSurvivorDataProvider 등록 및 로드
        InitializeDataProvider();
    }

    /// <summary>
    /// 씬 로드 완료
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        // UndeadSurvivorGame 생성, 초기화 및 시작 (MiniGameManager.LoadGame이 모두 처리)
        InitializeGame();
    }

    /// <summary>
    /// 씬 업데이트
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // MiniGameManager가 게임 Update를 호출하므로 여기서는 추가 작업 없음
        // 필요한 씬별 업데이트 로직이 있다면 여기에 추가
    }

    /// <summary>
    /// 씬 언로드
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

        // 게임 정리
        CleanupGame();

        // DataProvider 언로드
        if (DataManager.Instance != null)
        {
            DataManager.Instance.UnloadGameData("UndeadSurvivor");
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// DataProvider 초기화 및 등록
    /// </summary>
    private void InitializeDataProvider()
    {
        // 이미 등록되어 있는지 확인
        var existingProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

        if (existingProvider == null)
        {
            // 등록되지 않았으면 생성 및 등록
            var dataProvider = new UndeadSurvivorDataProvider();
            dataProvider.Initialize();
            DataManager.Instance.RegisterProvider(dataProvider);

            Debug.Log("[INFO] UndeadSurvivorGameScene::InitializeDataProvider - DataProvider registered");
        }

        // 데이터 로드
        DataManager.Instance.LoadGameData("UndeadSurvivor");

        Debug.Log("[INFO] UndeadSurvivorGameScene::InitializeDataProvider - Game data loaded");
    }

    /// <summary>
    /// 게임 초기화 및 MiniGameManager 등록
    /// </summary>
    private void InitializeGame()
    {
        // MiniGameManager의 LoadGame을 통해 게임 생성 및 초기화
        // LoadGame이 GameRegistry → 게임 생성 → Initialize → StartGame 순서로 처리
        bool success = MiniGameManager.Instance.LoadGame("UndeadSurvivor");

        if (!success)
        {
            Debug.LogError("[ERROR] UndeadSurvivorGameScene::InitializeGame - Failed to load game from MiniGameManager");
            return;
        }

        // 현재 게임 인스턴스 가져오기 (MiniGameManager가 관리하는 인스턴스)
        var gameData = MiniGameManager.Instance.GetCurrentGameData<UndeadSurvivorGameData>();
        if (gameData != null)
        {
            // 선택한 캐릭터 ID 설정
            gameData.SelectedCharacterId = _selectedCharacterId;
            _isGameRunning = true;
            Debug.Log($"[INFO] UndeadSurvivorGameScene::InitializeGame - Game initialized with character ID: {_selectedCharacterId}");
        }
        else
        {
            Debug.LogError("[ERROR] UndeadSurvivorGameScene::InitializeGame - Game data is null after LoadGame");
        }
    }


    /// <summary>
    /// 게임 정리
    /// </summary>
    private void CleanupGame()
    {
        if (!_isGameRunning)
        {
            return;
        }

        // MiniGameManager를 통해 게임 정리
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.UnloadCurrentGame();
        }

        _isGameRunning = false;

        Debug.Log("[INFO] UndeadSurvivorGameScene::CleanupGame - Game cleaned up");
    }

    #endregion

    #region Public API

    /// <summary>
    /// 현재 게임 데이터 가져오기
    /// </summary>
    public UndeadSurvivorGameData GetGameData()
    {
        return MiniGameManager.Instance.GetCurrentGameData<UndeadSurvivorGameData>();
    }

    /// <summary>
    /// 게임 오버 후 캐릭터 선택 씬으로 돌아가기
    /// </summary>
    public void ReturnToCharacterSelect()
    {
        Debug.Log("[INFO] UndeadSurvivorGameScene::ReturnToCharacterSelect - Returning to character selection");
        LoadSceneWithFade(SceneID.UndeadSurvivorCharacterSelectionScene);
    }

    /// <summary>
    /// 게임 오버 후 초기 화면으로 돌아가기
    /// </summary>
    public void ReturnToInitialScene()
    {
        Debug.Log("[INFO] UndeadSurvivorGameScene::ReturnToInitialScene - Returning to initial scene");
        LoadSceneWithFade(SceneID.UndeadSurvivor);
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기 (플랫폼 메인)
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[INFO] UndeadSurvivorGameScene::ReturnToMainMenu - Returning to main menu");
        LoadSceneWithFade(SceneID.MainMenu);
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        // 현재 씬 재로드
        CustomSceneManager.Instance.ReloadCurrentScene();
    }

    #endregion

    #region Unity Callbacks (Optional)

    /// <summary>
    /// 애플리케이션 종료 또는 포커스 잃을 때
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && _isGameRunning)
        {
            // 게임 상태 저장
            MiniGameManager.Instance.SaveCurrentGame();
        }
    }

    /// <summary>
    /// 애플리케이션 종료 시
    /// </summary>
    private void OnApplicationQuit()
    {
        if (_isGameRunning)
        {
            // 게임 상태 저장
            MiniGameManager.Instance.SaveCurrentGame();
        }
    }

    #endregion
}
