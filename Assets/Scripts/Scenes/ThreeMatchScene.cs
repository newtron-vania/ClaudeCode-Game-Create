using UnityEngine;
using ThreeMatch;
using ThreeMatch.Board;
using ThreeMatch.Data;
using ThreeMatch.UI;

/// <summary>
/// 3-Match 게임 씬 관리 스크립트
/// BaseScene을 상속받아 3-Match 게임의 초기화, 실행, UI 업데이트를 담당합니다.
/// Unity 씬에 배치하여 사용합니다.
/// </summary>
public class ThreeMatchScene : BaseScene
{
    // ========== Inspector 참조 ==========
    [Header("Game Components")]
    [SerializeField] private ThreeMatchBoardView _boardView;
    [SerializeField] private InputController _inputController;

    // ========== 씬 ID ==========
    public override SceneID SceneID => SceneID.ThreeMatch;

    // ========== Private 필드 ==========
    private ThreeMatchGameData _gameData;
    private ThreeMatchUIPanel _uiPanel;

    // ========== 씬 생명주기 ==========

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        Debug.Log("[ThreeMatchScene] Initializing ThreeMatch scene");

        // 게임 리셋 이벤트 구독
        MiniGameManager.Instance.OnGameReset += OnGameReset;

        // 3-Match 게임 등록 (아직 등록되지 않았다면)
        if (!GameRegistry.Instance.IsGameRegistered("ThreeMatch"))
        {
            GameRegistry.Instance.RegisterGame("ThreeMatch", () => new ThreeMatchGame());
        }

        // 3-Match 데이터 프로바이더 등록 (아직 등록되지 않았다면)
        if (!DataManager.Instance.HasProvider("ThreeMatch"))
        {
            var provider = new ThreeMatchDataProvider();
            DataManager.Instance.RegisterProvider(provider);
        }

        // UI 패널 열기
        UIManager.Instance.OpenPanel<ThreeMatchUIPanel>((panel) =>
        {
            if (panel != null)
            {
                _uiPanel = panel;
                Debug.Log("[ThreeMatchScene] UI Panel opened");

                // 3-Match 게임 로드 및 시작
                bool success = MiniGameManager.Instance.LoadGame("ThreeMatch");

                if (success)
                {
                    // 게임 인스턴스 가져오기
                    var game = MiniGameManager.Instance.GetCurrentGame() as ThreeMatchGame;

                    if (game != null)
                    {
                        // BoardView 및 InputController 설정 (Scene에서 참조)
                        if (_boardView == null)
                        {
                            Debug.LogError("[ThreeMatchScene] BoardView is not assigned in Inspector!");
                            return;
                        }

                        if (_inputController == null)
                        {
                            Debug.LogError("[ThreeMatchScene] InputController is not assigned in Inspector!");
                            return;
                        }

                        game.SetBoardView(_boardView);
                        game.SetInputController(_inputController);

                        // UI 패널 초기화 (게임 인스턴스 전달)
                        _uiPanel.Initialize(game);

                        // UI 이벤트 구독
                        SubscribeUIEvents();

                        Debug.Log("[ThreeMatchScene] ThreeMatch game started and UI initialized");
                    }
                    else
                    {
                        Debug.LogError("[ThreeMatchScene] Game instance is null");
                    }
                }
                else
                {
                    Debug.LogError("[ThreeMatchScene] Failed to start ThreeMatch game");
                }
            }
            else
            {
                Debug.LogError("[ThreeMatchScene] Failed to open UI Panel");
            }
        });
    }

    /// <summary>
    /// 씬 업데이트
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // 데이터 참조 갱신
        if (_gameData == null && MiniGameManager.Instance.IsGameRunning)
        {
            _gameData = MiniGameManager.Instance.GetCurrentGameData<ThreeMatchGameData>();
        }

        // UI 업데이트
        UpdateUI();

        // 게임 완료 체크
        CheckGameComplete();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (_gameData == null || _uiPanel == null)
        {
            return;
        }

        // ThreeMatchUIPanel이 자체적으로 상태를 관리하므로
        // 여기서는 필요한 데이터만 전달
        _uiPanel.UpdateGameInfo(_gameData);
    }

    /// <summary>
    /// 게임 완료 체크
    /// </summary>
    private void CheckGameComplete()
    {
        if (_gameData != null && _uiPanel != null)
        {
            // 목표 달성 또는 게임 오버 상태는 ThreeMatchGame에서 처리
            // 여기서는 추가 로직이 필요하면 구현
        }
    }

    /// <summary>
    /// 게임 리셋 이벤트 핸들러
    /// </summary>
    private void OnGameReset()
    {
        if (_uiPanel != null)
        {
            Debug.Log("[ThreeMatchScene] Game reset via event");
        }
    }

    // ========== UI 이벤트 구독/해제 ==========

    /// <summary>
    /// UI 이벤트 구독
    /// </summary>
    private void SubscribeUIEvents()
    {
        if (_uiPanel == null)
        {
            Debug.LogError("[ThreeMatchScene] UI Panel is null");
            return;
        }

        // UI → 게임 이벤트 구독
        _uiPanel.OnDifficultySelected += HandleDifficultySelected;
        _uiPanel.OnGameModeSelected += HandleGameModeSelected;
        _uiPanel.OnStartButtonClicked += HandleStartButtonClicked;
        _uiPanel.OnPauseButtonClicked += HandlePauseButtonClicked;
        _uiPanel.OnResumeButtonClicked += HandleResumeButtonClicked;
        _uiPanel.OnRestartButtonClicked += HandleRestartButtonClicked;
        _uiPanel.OnMainMenuButtonClicked += HandleMainMenuButtonClicked;

        Debug.Log("[ThreeMatchScene] UI events subscribed");
    }

    /// <summary>
    /// UI 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeUIEvents()
    {
        if (_uiPanel == null)
        {
            return;
        }

        _uiPanel.OnDifficultySelected -= HandleDifficultySelected;
        _uiPanel.OnGameModeSelected -= HandleGameModeSelected;
        _uiPanel.OnStartButtonClicked -= HandleStartButtonClicked;
        _uiPanel.OnPauseButtonClicked -= HandlePauseButtonClicked;
        _uiPanel.OnResumeButtonClicked -= HandleResumeButtonClicked;
        _uiPanel.OnRestartButtonClicked -= HandleRestartButtonClicked;
        _uiPanel.OnMainMenuButtonClicked -= HandleMainMenuButtonClicked;

        Debug.Log("[ThreeMatchScene] UI events unsubscribed");
    }

    // ========== UI 이벤트 핸들러 ==========

    /// <summary>
    /// 난이도 선택 이벤트 핸들러
    /// </summary>
    private void HandleDifficultySelected(DifficultyLevel difficulty)
    {
        Debug.Log($"[ThreeMatchScene] Difficulty selected: {difficulty}");

        if (_gameData != null)
        {
            _gameData.CurrentDifficulty = difficulty;
        }
    }

    /// <summary>
    /// 게임 모드 선택 이벤트 핸들러
    /// </summary>
    private void HandleGameModeSelected(GameMode mode)
    {
        Debug.Log($"[ThreeMatchScene] Game mode selected: {mode}");

        if (_gameData != null)
        {
            _gameData.CurrentMode = mode;
        }
    }

    /// <summary>
    /// 게임 시작 버튼 클릭 핸들러
    /// </summary>
    private void HandleStartButtonClicked()
    {
        Debug.Log("[ThreeMatchScene] Start button clicked");

        var game = MiniGameManager.Instance.GetCurrentGame() as ThreeMatchGame;
        if (game != null)
        {
            game.StartGame();
        }
    }

    /// <summary>
    /// 일시정지 버튼 클릭 핸들러
    /// </summary>
    private void HandlePauseButtonClicked()
    {
        Debug.Log("[ThreeMatchScene] Pause button clicked");

        // 일시정지 로직 (필요 시 구현)
        if (_uiPanel != null)
        {
            _uiPanel.ShowPausedPanel();
        }
    }

    /// <summary>
    /// 재개 버튼 클릭 핸들러
    /// </summary>
    private void HandleResumeButtonClicked()
    {
        Debug.Log("[ThreeMatchScene] Resume button clicked");

        if (_uiPanel != null)
        {
            _uiPanel.ShowPlayingPanel();
        }
    }

    /// <summary>
    /// 재시작 버튼 클릭 핸들러
    /// </summary>
    private void HandleRestartButtonClicked()
    {
        Debug.Log("[ThreeMatchScene] Restart button clicked");

        RestartGame();
    }

    /// <summary>
    /// 메인 메뉴 버튼 클릭 핸들러
    /// </summary>
    private void HandleMainMenuButtonClicked()
    {
        Debug.Log("[ThreeMatchScene] Main menu button clicked");

        ReturnToMainMenu();
    }

    // ========== 씬 언로드 ==========

    /// <summary>
    /// 씬 언로드 시 호출
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

        // UI 이벤트 구독 해제
        UnsubscribeUIEvents();

        // 이벤트 구독 해제
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.OnGameReset -= OnGameReset;
        }

        // 현재 게임 언로드
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.CurrentGameID == "ThreeMatch")
        {
            MiniGameManager.Instance.UnloadCurrentGame();
            Debug.Log("[ThreeMatchScene] ThreeMatch game unloaded");
        }
    }

    // ========== Public 메서드 (UI 버튼에서 호출 가능) ==========

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        MiniGameManager.Instance.SwitchGame("ThreeMatch");
        Debug.Log("[ThreeMatchScene] Game restarted");
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 현재 게임 저장
        MiniGameManager.Instance.SaveCurrentGame();

        // 메인 메뉴 씬 로드
        LoadScene(SceneID.MainMenu);

        Debug.Log("[ThreeMatchScene] Returning to main menu");
    }

    /// <summary>
    /// 다른 게임으로 전환
    /// </summary>
    public void SwitchToGame(SceneID sceneID)
    {
        // 현재 게임 저장
        MiniGameManager.Instance.SaveCurrentGame();

        // 다른 게임 씬으로 전환
        LoadSceneWithFade(sceneID, 0.5f);

        Debug.Log($"[ThreeMatchScene] Switching to: {sceneID}");
    }
}
