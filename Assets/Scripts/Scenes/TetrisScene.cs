using TMPro;
using UnityEngine;

/// <summary>
/// 테트리스 씬 관리 스크립트
/// BaseScene을 상속받아 테트리스 게임의 초기화, 실행, UI 업데이트를 담당합니다.
/// Unity 씬에 배치하여 사용합니다.
/// </summary>
public class TetrisScene : BaseScene
{
    /// <summary>
    /// 씬 ID
    /// </summary>
    public override SceneID SceneID => SceneID.Tetris;

    private TetrisData _tetrisData;
    private TetrisUIPanel _uiPanel;

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        Debug.Log("[INFO] TetrisScene::OnSceneLoaded - Initializing Tetris scene");

        // UI 패널 열기
        UIManager.Instance.OpenPanel<TetrisUIPanel>((panel) =>
        {
            if (panel != null)
            {
                _uiPanel = panel;
                Debug.Log("[INFO] TetrisScene::OnSceneLoaded - UI Panel opened");
            }
            else
            {
                Debug.LogError("[ERROR] TetrisScene::OnSceneLoaded - Failed to open UI Panel");
            }
        });

        // 게임 리셋 이벤트 구독
        MiniGameManager.Instance.OnGameReset += OnGameReset;

        // 테트리스 게임 등록 (아직 등록되지 않았다면)
        if (!GameRegistry.Instance.IsGameRegistered("Tetris"))
        {
            GameRegistry.Instance.RegisterGame("Tetris", () => new TetrisGame());
        }

        // 테트리스 게임 로드 및 시작
        bool success = MiniGameManager.Instance.LoadGame("Tetris");

        if (success)
        {
            Debug.Log("[INFO] TetrisScene::OnSceneLoaded - Tetris game started successfully");
        }
        else
        {
            Debug.LogError("[ERROR] TetrisScene::OnSceneLoaded - Failed to start Tetris game");
        }
    }

    /// <summary>
    /// 씬 업데이트
    /// UI 업데이트 및 게임 오버 체크
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // 데이터 참조 갱신
        if (_tetrisData == null && MiniGameManager.Instance.IsGameRunning)
        {
            _tetrisData = MiniGameManager.Instance.GetCurrentGameData<TetrisData>();
        }

        // UI 업데이트
        UpdateUI();

        // 게임 오버 체크
        CheckGameOver();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (_tetrisData == null || _uiPanel == null)
        {
            return;
        }

        _uiPanel.UpdateAll(_tetrisData);
    }

    /// <summary>
    /// 게임 오버 체크
    /// </summary>
    private void CheckGameOver()
    {
        if (_tetrisData != null && _tetrisData.IsGameOver && _uiPanel != null)
        {
            _uiPanel.ShowGameOver(_tetrisData.CurrentScore);
            Debug.Log($"[INFO] TetrisScene::CheckGameOver - Game Over! Final Score: {_tetrisData.CurrentScore}");
        }
    }

    /// <summary>
    /// 게임 리셋 이벤트 핸들러
    /// </summary>
    private void OnGameReset()
    {
        if (_uiPanel != null)
        {
            _uiPanel.ResetUI();
            Debug.Log("[INFO] TetrisScene::OnGameReset - UI reset via event");
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
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.CurrentGameID == "Tetris")
        {
            MiniGameManager.Instance.UnloadCurrentGame();
            Debug.Log("[INFO] TetrisScene::OnSceneUnloaded - Tetris game unloaded");
        }
    }

    /// <summary>
    /// 게임 재시작 (UI 버튼에서 호출 가능)
    /// </summary>
    public void RestartGame()
    {
        MiniGameManager.Instance.SwitchGame("Tetris");

        // UI 리셋
        if (_uiPanel != null)
        {
            _uiPanel.ResetUI();
        }

        Debug.Log("[INFO] TetrisScene::RestartGame - Game restarted");
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

        Debug.Log("[INFO] TetrisScene::ReturnToMainMenu - Returning to main menu");
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

        Debug.Log($"[INFO] TetrisScene::SwitchToGame - Switching to: {sceneID}");
    }
}
