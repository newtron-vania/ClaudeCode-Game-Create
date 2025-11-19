using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스도쿠 게임 UI 메인 패널
/// 단일 씬 아키텍처에서 모든 상태별 UI 패널을 관리
/// StartMenu → Generating → Playing → GameEnd 상태 전환
/// </summary>
public class SudokuUIPanel : UIPanel
{
    // ========================================
    // 4개 메인 패널 (Setup Guide 명세)
    // ========================================
    [Header("Main Panels")]
    [SerializeField] private GameObject _startMenuPanel;   // 시작 메뉴 패널
    [SerializeField] private GameObject _loadingPanel;     // 맵 생성 중 패널
    [SerializeField] private GameObject _playingPanel;     // 게임 플레이 패널
    [SerializeField] private GameObject _gameEndPanel;     // 게임 완료 패널

    // ========================================
    // StartMenuPanel UI 요소
    // ========================================
    [Header("StartMenuPanel Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;        // 타이틀 텍스트
    [SerializeField] private GameObject _difficultyPanel;       // 난이도 버튼 그룹 패널
    [SerializeField] private Button _easyButton;                // 쉬움 버튼
    [SerializeField] private Button _mediumButton;              // 중간 버튼
    [SerializeField] private Button _hardButton;                // 어려움 버튼
    [SerializeField] private Button _backButton;                // 뒤로가기 버튼

    // ========================================
    // LoadingPanel UI 요소
    // ========================================
    [Header("LoadingPanel Elements")]
    [SerializeField] private TextMeshProUGUI _loadingText;      // "Generating..." 텍스트
    [SerializeField] private Image _loadingSpinner;             // 회전 애니메이션 이미지

    // ========================================
    // PlayingPanel UI 요소
    // ========================================
    [Header("PlayingPanel Elements")]
    [SerializeField] private SudokuGridUI _gridUI;              // 그리드 UI 컴포넌트
    [SerializeField] private NumPadUI _numPadUI;                // NumPad UI 컴포넌트
    [SerializeField] private TimerUI _timerUI;                  // 타이머 UI 컴포넌트
    [SerializeField] private TextMeshProUGUI _mistakesText;     // 실수 횟수
    [SerializeField] private TextMeshProUGUI _hintsText;        // 남은 힌트
    [SerializeField] private Button _hintButton;                // 힌트 버튼
    [SerializeField] private Button _undoButton;                // 되돌리기 버튼
    [SerializeField] private Button _eraseButton;               // 지우기 버튼
    [SerializeField] private Button _pauseButton;               // 일시정지 버튼

    // ========================================
    // GameEndPanel UI 요소
    // ========================================
    [Header("GameEndPanel Elements")]
    [SerializeField] private TextMeshProUGUI _winText;          // "Congratulations!" 텍스트
    [SerializeField] private TextMeshProUGUI _timeText;         // 클리어 타임
    [SerializeField] private GameObject _statsPanel;            // 통계 정보 패널
    [SerializeField] private Button _newGameButton;             // 새 게임 버튼
    [SerializeField] private Button _mainMenuButton;            // 메인 메뉴 버튼

    // ========================================
    // 이벤트
    // ========================================
    public event Action<SudokuDifficulty> OnDifficultySelected;
    public event Action OnHintRequested;
    public event Action OnBackToMenu;

    private SudokuGame _game;
    private SudokuGameData _gameData;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 이벤트 등록
        RegisterButtonEvents();

        // 초기 상태: 모든 패널 비활성화
        HideAllPanels();
    }

    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        UnregisterButtonEvents();
    }

    /// <summary>
    /// UI 패널 초기화
    /// </summary>
    /// <param name="game">스도쿠 게임 인스턴스</param>
    public void Initialize(SudokuGame game)
    {
        _game = game;
        _gameData = game.GetData() as SudokuGameData;

        Debug.Log("[INFO] SudokuUIPanel::Initialize - UI initialized");

        // 그리드 UI 초기화
        if (_gridUI != null)
        {
            _gridUI.Initialize(game);
        }

        // NumPad UI 초기화
        if (_numPadUI != null)
        {
            _numPadUI.Initialize(game);
        }

        // 타이머 UI 초기화
        if (_timerUI != null)
        {
            _timerUI.Initialize();
        }

        // 시작 메뉴 표시
        ShowStartMenuPanel();
    }

    #region 상태별 UI 전환 (Setup Guide 명세에 맞춤)

    /// <summary>
    /// StartMenuPanel 표시 (난이도 선택)
    /// </summary>
    public void ShowStartMenuPanel()
    {
        HideAllPanels();

        if (_startMenuPanel != null)
        {
            _startMenuPanel.SetActive(true);
        }

        Debug.Log("[INFO] SudokuUIPanel::ShowStartMenuPanel - Start menu displayed");
    }

    /// <summary>
    /// LoadingPanel 표시 (맵 생성 중)
    /// </summary>
    public void ShowLoadingPanel()
    {
        HideAllPanels();

        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        Debug.Log("[INFO] SudokuUIPanel::ShowLoadingPanel - Loading screen displayed");
    }

    /// <summary>
    /// PlayingPanel 표시 (게임 플레이 중)
    /// </summary>
    public void ShowPlayingPanel()
    {
        HideAllPanels();

        if (_playingPanel != null)
        {
            _playingPanel.SetActive(true);
        }

        // 타이머 시작
        if (_timerUI != null)
        {
            _timerUI.StartTimer();
        }

        // 게임 정보 업데이트
        UpdateGameInfo();

        Debug.Log("[INFO] SudokuUIPanel::ShowPlayingPanel - Playing panel displayed");
    }

    /// <summary>
    /// GameEndPanel 표시 (게임 완료)
    /// </summary>
    public void ShowGameEndPanel()
    {
        HideAllPanels();

        if (_gameEndPanel != null)
        {
            _gameEndPanel.SetActive(true);
        }

        // 타이머 정지
        if (_timerUI != null)
        {
            _timerUI.StopTimer();
        }

        // 게임 완료 정보 업데이트
        UpdateGameEndPanel();

        Debug.Log("[INFO] SudokuUIPanel::ShowGameEndPanel - Game end panel displayed");
    }

    /// <summary>
    /// 모든 패널 숨기기
    /// </summary>
    private void HideAllPanels()
    {
        if (_startMenuPanel != null) _startMenuPanel.SetActive(false);
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        if (_playingPanel != null) _playingPanel.SetActive(false);
        if (_gameEndPanel != null) _gameEndPanel.SetActive(false);
    }

    #endregion

    #region UI 업데이트

    /// <summary>
    /// PlayingPanel 게임 정보 업데이트 (타이머, 힌트, 실수)
    /// </summary>
    public void UpdateGameInfo()
    {
        if (_gameData == null) return;

        // 실수 횟수 업데이트
        if (_mistakesText != null)
        {
            _mistakesText.text = $"실수: {_gameData.Mistakes}";
        }

        // 힌트 사용 횟수 업데이트
        if (_hintsText != null)
        {
            _hintsText.text = $"힌트: {_gameData.HintsUsed}";
        }

        // 타이머는 TimerUI 컴포넌트가 자동으로 업데이트
    }

    /// <summary>
    /// GameEndPanel 정보 업데이트
    /// </summary>
    private void UpdateGameEndPanel()
    {
        if (_gameData == null) return;

        // 승리 텍스트
        if (_winText != null)
        {
            _winText.text = "Congratulations!";
        }

        // 클리어 타임
        if (_timeText != null)
        {
            int minutes = Mathf.FloorToInt(_gameData.PlayTime / 60f);
            int seconds = Mathf.FloorToInt(_gameData.PlayTime % 60f);
            _timeText.text = $"클리어 타임: {minutes:00}:{seconds:00}";
        }

        // 통계 정보는 StatsPanel 내부에서 별도 처리
        // (향후 확장: StatsPanel에 점수, 힌트, 실수 등 표시)
    }

    #endregion

    #region 버튼 이벤트

    private void RegisterButtonEvents()
    {
        // StartMenuPanel 버튼
        if (_easyButton != null)
            _easyButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Easy));
        if (_mediumButton != null)
            _mediumButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Medium));
        if (_hardButton != null)
            _hardButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Hard));
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackButtonClicked);

        // PlayingPanel 버튼
        if (_hintButton != null)
            _hintButton.onClick.AddListener(OnHintButtonClicked);
        if (_undoButton != null)
            _undoButton.onClick.AddListener(OnUndoButtonClicked);
        if (_eraseButton != null)
            _eraseButton.onClick.AddListener(OnEraseButtonClicked);
        if (_pauseButton != null)
            _pauseButton.onClick.AddListener(OnPauseButtonClicked);

        // GameEndPanel 버튼
        if (_newGameButton != null)
            _newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void UnregisterButtonEvents()
    {
        // StartMenuPanel 버튼
        if (_easyButton != null)
            _easyButton.onClick.RemoveAllListeners();
        if (_mediumButton != null)
            _mediumButton.onClick.RemoveAllListeners();
        if (_hardButton != null)
            _hardButton.onClick.RemoveAllListeners();
        if (_backButton != null)
            _backButton.onClick.RemoveAllListeners();

        // PlayingPanel 버튼
        if (_hintButton != null)
            _hintButton.onClick.RemoveAllListeners();
        if (_undoButton != null)
            _undoButton.onClick.RemoveAllListeners();
        if (_eraseButton != null)
            _eraseButton.onClick.RemoveAllListeners();
        if (_pauseButton != null)
            _pauseButton.onClick.RemoveAllListeners();

        // GameEndPanel 버튼
        if (_newGameButton != null)
            _newGameButton.onClick.RemoveAllListeners();
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveAllListeners();
    }

    private void OnDifficultyButtonClicked(SudokuDifficulty difficulty)
    {
        Debug.Log($"[INFO] SudokuUIPanel::OnDifficultyButtonClicked - Difficulty: {difficulty}");
        OnDifficultySelected?.Invoke(difficulty);
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnBackButtonClicked - Returning to main menu");
        OnBackToMenu?.Invoke();
    }

    private void OnHintButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnHintButtonClicked - Hint requested");
        OnHintRequested?.Invoke();
        UpdateGameInfo();
    }

    private void OnUndoButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnUndoButtonClicked - Undo requested");
        // 향후 구현: 되돌리기 기능
    }

    private void OnEraseButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnEraseButtonClicked - Erase requested");
        // 향후 구현: 선택된 셀 지우기
    }

    private void OnPauseButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnPauseButtonClicked - Pause requested");
        // 향후 구현: 일시정지 기능
    }

    private void OnNewGameButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnNewGameButtonClicked - New game requested");
        ShowStartMenuPanel();
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnMainMenuButtonClicked - Back to main menu");
        OnBackToMenu?.Invoke();
    }

    #endregion

    #region Public API

    /// <summary>
    /// 그리드 UI 가져오기
    /// </summary>
    public SudokuGridUI GridUI => _gridUI;

    /// <summary>
    /// NumPad UI 가져오기
    /// </summary>
    public NumPadUI NumPadUI => _numPadUI;

    /// <summary>
    /// 타이머 UI 가져오기
    /// </summary>
    public TimerUI TimerUI => _timerUI;

    #endregion
}
