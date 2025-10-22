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

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _linesText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _nextPieceText;
    [SerializeField] private TextMeshProUGUI _gameOverText;

    private TetrisData _tetrisData;

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        Debug.Log("[INFO] TetrisScene::OnSceneLoaded - Initializing Tetris scene");

        // 테트리스 게임 등록 (아직 등록되지 않았다면)
        if (!GameRegistry.Instance.IsGameRegistered("Tetris"))
        {
            GameRegistry.Instance.RegisterGame("Tetris", () => new TetrisGame());
        }

        // 테트리스 게임 로드 및 시작
        bool success = MiniGameManager.Instance.LoadGame("Tetris");

        if (success)
        {
            // 데이터 참조 가져오기 (게임 로직은 MiniGameManager의 Update에서 실행됨)
            // TetrisScene은 UI 업데이트만 담당
            Debug.Log("[INFO] TetrisScene::OnSceneLoaded - Tetris game started successfully");

            // 게임 오버 UI 숨기기
            if (_gameOverText != null)
            {
                _gameOverText.gameObject.SetActive(false);
            }
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
        if (_tetrisData == null)
        {
            return;
        }

        // 점수 업데이트
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {_tetrisData.CurrentScore}";
        }

        // 라인 업데이트
        if (_linesText != null)
        {
            _linesText.text = $"Lines: {_tetrisData.LinesCleared}";
        }

        // 레벨 업데이트
        if (_levelText != null)
        {
            _levelText.text = $"Level: {_tetrisData.Level}";
        }

        // 다음 블록 업데이트
        if (_nextPieceText != null)
        {
            _nextPieceText.text = $"Next";
        }
    }

    /// <summary>
    /// 게임 오버 체크
    /// </summary>
    private void CheckGameOver()
    {
        if (_tetrisData != null && _tetrisData.IsGameOver)
        {
            if (_gameOverText != null && !_gameOverText.gameObject.activeSelf)
            {
                _gameOverText.gameObject.SetActive(true);
                _gameOverText.text = $"Game Over!\nScore: {_tetrisData.CurrentScore}\nPress R to Restart";

                Debug.Log($"[INFO] TetrisScene::CheckGameOver - Game Over! Final Score: {_tetrisData.CurrentScore}");
            }
        }
    }

    /// <summary>
    /// 씬 언로드 시 호출
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

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

        if (_gameOverText != null)
        {
            _gameOverText.gameObject.SetActive(false);
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
