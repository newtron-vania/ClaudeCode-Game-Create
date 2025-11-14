using UnityEngine;

/// <summary>
/// 스도쿠 씬 관리 스크립트
/// BaseScene을 상속받아 스도쿠 게임의 초기화, 실행, UI 업데이트를 담당합니다.
/// Unity 씬에 배치하여 사용합니다.
/// </summary>
public class SudokuScene : BaseScene
{
    /// <summary>
    /// 씬 ID
    /// </summary>
    public override SceneID SceneID => SceneID.Sudoku;

    private SudokuGameData _sudokuData;
    private SudokuUIPanel _uiPanel;

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        Debug.Log("[INFO] SudokuScene::OnSceneLoaded - Initializing Sudoku scene");

        // UI 패널 열기
        UIManager.Instance.OpenPanel<SudokuUIPanel>((panel) =>
        {
            if (panel != null)
            {
                _uiPanel = panel;
                Debug.Log("[INFO] SudokuScene::OnSceneLoaded - UI Panel opened");
            }
            else
            {
                Debug.LogError("[ERROR] SudokuScene::OnSceneLoaded - Failed to open UI Panel");
            }
        });

        // 게임 리셋 이벤트 구독
        MiniGameManager.Instance.OnGameReset += OnGameReset;

        // 스도쿠 게임 등록 (아직 등록되지 않았다면)
        if (!GameRegistry.Instance.IsGameRegistered("Sudoku"))
        {
            GameRegistry.Instance.RegisterGame("Sudoku", () => new SudokuGame());
        }

        // 스도쿠 데이터 프로바이더 등록 (아직 등록되지 않았다면)
        if (!DataManager.Instance.HasProvider("Sudoku"))
        {
            var provider = new SudokuDataProvider();
            DataManager.Instance.RegisterProvider(provider);
        }

        // 스도쿠 게임 로드 및 시작
        bool success = MiniGameManager.Instance.LoadGame("Sudoku");

        if (success)
        {
            Debug.Log("[INFO] SudokuScene::OnSceneLoaded - Sudoku game started successfully");
        }
        else
        {
            Debug.LogError("[ERROR] SudokuScene::OnSceneLoaded - Failed to start Sudoku game");
        }
    }

    /// <summary>
    /// 씬 업데이트
    /// UI 업데이트 및 게임 완료 체크
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // 데이터 참조 갱신
        if (_sudokuData == null && MiniGameManager.Instance.IsGameRunning)
        {
            _sudokuData = MiniGameManager.Instance.GetCurrentGameData<SudokuGameData>();
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
        if (_sudokuData == null || _uiPanel == null)
        {
            return;
        }

        // SudokuUIPanel이 자체적으로 상태를 관리하므로
        // 여기서는 데이터만 전달
        // _uiPanel은 SudokuGame으로부터 직접 업데이트 받음
    }

    /// <summary>
    /// 게임 완료 체크
    /// </summary>
    private void CheckGameComplete()
    {
        if (_sudokuData != null && _sudokuData.IsCompleted && _uiPanel != null)
        {
            Debug.Log($"[INFO] SudokuScene::CheckGameComplete - Game Complete! Time: {_sudokuData.PlayTime}s, Mistakes: {_sudokuData.Mistakes}");
        }
    }

    /// <summary>
    /// 게임 리셋 이벤트 핸들러
    /// </summary>
    private void OnGameReset()
    {
        if (_uiPanel != null)
        {
            Debug.Log("[INFO] SudokuScene::OnGameReset - Game reset via event");
        }
    }

    /// <summary>
    /// 씬 언로드 시 호출
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

        // 이벤트 구독 해제
        if (MiniGameManager.Instance != null)
        {
            MiniGameManager.Instance.OnGameReset -= OnGameReset;
        }

        // 현재 게임 언로드
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.CurrentGameID == "Sudoku")
        {
            MiniGameManager.Instance.UnloadCurrentGame();
            Debug.Log("[INFO] SudokuScene::OnSceneUnloaded - Sudoku game unloaded");
        }
    }

    /// <summary>
    /// 게임 재시작 (UI 버튼에서 호출 가능)
    /// </summary>
    public void RestartGame()
    {
        MiniGameManager.Instance.SwitchGame("Sudoku");

        Debug.Log("[INFO] SudokuScene::RestartGame - Game restarted");
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기 (UI 버튼에서 호출 가능)
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 현재 게임 저장
        MiniGameManager.Instance.SaveCurrentGame();

        // 메인 메뉴 씬 로드 (SceneID 기반)
        LoadScene(SceneID.MainMenu);

        Debug.Log("[INFO] SudokuScene::ReturnToMainMenu - Returning to main menu");
    }

    /// <summary>
    /// 다른 게임으로 전환 (UI 버튼에서 호출 가능)
    /// </summary>
    /// <param name="sceneID">전환할 씬 ID</param>
    public void SwitchToGame(SceneID sceneID)
    {
        // 현재 게임 저장
        MiniGameManager.Instance.SaveCurrentGame();

        // 다른 게임 씬으로 전환
        LoadSceneWithFade(sceneID, 0.5f);

        Debug.Log($"[INFO] SudokuScene::SwitchToGame - Switching to: {sceneID}");
    }
}
