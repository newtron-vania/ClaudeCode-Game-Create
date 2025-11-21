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
    // 일시정지 버튼 제거됨 (Phase 6+에서 재구현 예정)

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

        // NumPad 이벤트 구독 해제
        if (_numPadUI != null)
        {
            _numPadUI.OnNumberInput -= OnNumPadNumberInput;
            _numPadUI.OnClearInput -= OnNumPadClearInput;
        }
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

        // NumPad UI 초기화 및 이벤트 구독
        if (_numPadUI != null)
        {
            _numPadUI.Initialize(game);

            // NumPad 이벤트 구독
            _numPadUI.OnNumberInput += OnNumPadNumberInput;
            _numPadUI.OnClearInput += OnNumPadClearInput;
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

        // 그리드 UI 업데이트 (보드 데이터 동기화)
        if (_gridUI != null)
        {
            _gridUI.UpdateBoard();
            Debug.Log("[INFO] SudokuUIPanel::ShowPlayingPanel - Grid board updated");
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
            _mistakesText.text = $"실수: {_gameData.Mistakes}/{_gameData.MaxMistakes}";
        }

        // 남은 힌트 횟수 업데이트
        if (_hintsText != null)
        {
            _hintsText.text = $"힌트: {_gameData.RemainingHints}/{_gameData.MaxHints}";
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

        // GameEndPanel 버튼
        if (_newGameButton != null)
            _newGameButton.onClick.RemoveAllListeners();
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 난이도 버튼 클릭 핸들러
    /// 선택한 난이도로 새 게임 시작
    /// </summary>
    private void OnDifficultyButtonClicked(SudokuDifficulty difficulty)
    {
        Debug.Log($"[INFO] SudokuUIPanel::OnDifficultyButtonClicked - Difficulty: {difficulty}");

        if (_game == null)
        {
            Debug.LogError("[ERROR] SudokuUIPanel::OnDifficultyButtonClicked - Game instance is null");
            return;
        }

        // DataProvider에서 난이도 정보 가져오기 (선택적)
        var provider = DataManager.Instance.GetProvider<SudokuDataProvider>("Sudoku");
        if (provider != null && provider.IsLoaded)
        {
            var config = provider.GetDifficultyConfig(difficulty);
            if (config != null)
            {
                Debug.Log($"[INFO] SudokuUIPanel::OnDifficultyButtonClicked - {config.DisplayName} selected (hints: {config.MinHints}-{config.MaxHints})");
            }
        }

        // 난이도 선택 이벤트 발생
        OnDifficultySelected?.Invoke(difficulty);

        // 새 게임 시작
        _game.StartNewGame(difficulty);

        // 로딩 패널 표시 (Activity Action을 통해 자동으로 전환됨)
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 핸들러
    /// 메인 메뉴로 돌아가기
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnBackButtonClicked - Returning to main menu");

        // 메인 메뉴 복귀 이벤트 발생
        OnBackToMenu?.Invoke();

        // 씬 전환 (SudokuScene에서 처리)
        // CustomSceneManager를 통해 MainMenuScene으로 이동
    }

    /// <summary>
    /// 힌트 버튼 클릭 핸들러
    /// 선택된 셀에 정답 힌트 표시
    /// </summary>
    private void OnHintButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnHintButtonClicked - Hint requested");

        if (_game == null)
        {
            Debug.LogError("[ERROR] SudokuUIPanel::OnHintButtonClicked - Game instance is null");
            return;
        }

        // 힌트 잔여 개수 확인
        if (_gameData == null || _gameData.RemainingHints <= 0)
        {
            Debug.LogWarning("[WARNING] SudokuUIPanel::OnHintButtonClicked - No hints remaining");
            return;
        }

        // 힌트 사용
        _game.UseHint();

        // 힌트 이벤트 발생
        OnHintRequested?.Invoke();

        // UI 업데이트
        UpdateGameInfo();
    }

    /// <summary>
    /// 되돌리기 버튼 클릭 핸들러
    /// 마지막 입력 취소
    /// </summary>
    private void OnUndoButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnUndoButtonClicked - Undo requested");

        if (_game == null)
        {
            Debug.LogError("[ERROR] SudokuUIPanel::OnUndoButtonClicked - Game instance is null");
            return;
        }

        // Undo 기능 구현 안내:
        // 1. SudokuGame에 Stack<(int row, int col, int oldValue, int newValue)> 추가
        // 2. InputNumber()에서 변경 전 값을 스택에 저장
        // 3. UndoLastMove() 메서드로 스택에서 꺼내어 복원
        // 4. 에러 체크 및 UI 업데이트

        // 현재는 버튼 비활성화 권장
        if (_undoButton != null)
        {
            _undoButton.interactable = false;
        }

        Debug.LogWarning("[WARNING] SudokuUIPanel::OnUndoButtonClicked - Undo feature not implemented (Phase 6+)");
    }

    /// <summary>
    /// 지우기 버튼 클릭 핸들러
    /// 선택된 셀의 값을 지움
    /// </summary>
    private void OnEraseButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnEraseButtonClicked - Erase requested");

        if (_game == null)
        {
            Debug.LogError("[ERROR] SudokuUIPanel::OnEraseButtonClicked - Game instance is null");
            return;
        }

        // 선택된 셀에 0 입력 (지우기)
        _game.InputNumber(0);
    }


    /// <summary>
    /// 새 게임 버튼 클릭 핸들러 (게임 종료 후)
    /// 시작 메뉴로 돌아가기
    /// </summary>
    private void OnNewGameButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnNewGameButtonClicked - New game requested");

        // 시작 메뉴 패널 표시 (난이도 재선택)
        ShowStartMenuPanel();
    }

    /// <summary>
    /// 메인 메뉴 버튼 클릭 핸들러 (게임 종료 후)
    /// 메인 메뉴로 돌아가기
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnMainMenuButtonClicked - Back to main menu");

        // 메인 메뉴 복귀 이벤트 발생
        OnBackToMenu?.Invoke();
    }

    /// <summary>
    /// NumPad 숫자 입력 이벤트 핸들러
    /// </summary>
    /// <param name="number">입력된 숫자 (1-9)</param>
    private void OnNumPadNumberInput(int number)
    {
        Debug.Log($"[INFO] SudokuUIPanel::OnNumPadNumberInput - Number {number} input from NumPad");

        // GridUI 업데이트 (게임에서 이미 InputNumber 호출됨)
        if (_gridUI != null && _game != null)
        {
            _gridUI.UpdateBoard();
        }

        // 게임 정보 업데이트
        UpdateGameInfo();
    }

    /// <summary>
    /// NumPad 지우기 입력 이벤트 핸들러
    /// </summary>
    private void OnNumPadClearInput()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnNumPadClearInput - Clear input from NumPad");

        // GridUI 업데이트 (게임에서 이미 InputNumber(0) 호출됨)
        if (_gridUI != null && _game != null)
        {
            _gridUI.UpdateBoard();
        }

        // 게임 정보 업데이트
        UpdateGameInfo();
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
