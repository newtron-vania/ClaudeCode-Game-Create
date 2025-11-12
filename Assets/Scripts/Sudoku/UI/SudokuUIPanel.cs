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
    [Header("Sub Panels")]
    [SerializeField] private GameObject _startMenuPanel;  // 난이도 선택 패널
    [SerializeField] private GameObject _gameHUDPanel;    // 게임 플레이 패널
    [SerializeField] private GameObject _winPopupPanel;   // 승리 팝업 패널
    [SerializeField] private GameObject _loadingPanel;    // 로딩 패널 (맵 생성 중)

    [Header("Start Menu UI")]
    [SerializeField] private Button _easyButton;
    [SerializeField] private Button _mediumButton;
    [SerializeField] private Button _hardButton;
    [SerializeField] private Button _backButton;

    [Header("Game HUD UI")]
    [SerializeField] private SudokuGridUI _gridUI;
    [SerializeField] private NumPadUI _numPadUI;
    [SerializeField] private TimerUI _timerUI;
    [SerializeField] private TextMeshProUGUI _difficultyText;
    [SerializeField] private TextMeshProUGUI _hintsUsedText;
    [SerializeField] private TextMeshProUGUI _mistakesText;
    [SerializeField] private Button _hintButton;
    [SerializeField] private Button _newGameButton;

    [Header("Win Popup UI")]
    [SerializeField] private TextMeshProUGUI _winTitleText;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Settings")]
    [SerializeField] private Color _easyColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color _mediumColor = new Color(0.9f, 0.7f, 0.2f);
    [SerializeField] private Color _hardColor = new Color(0.9f, 0.3f, 0.3f);

    // 이벤트
    public event Action<SudokuDifficulty> OnDifficultySelected;
    public event Action OnHintRequested;
    public event Action OnNewGameRequested;
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
        ShowStartMenu();
    }

    #region 상태별 UI 전환

    /// <summary>
    /// 시작 메뉴 표시 (난이도 선택)
    /// </summary>
    public void ShowStartMenu()
    {
        HideAllPanels();

        if (_startMenuPanel != null)
        {
            _startMenuPanel.SetActive(true);
        }

        Debug.Log("[INFO] SudokuUIPanel::ShowStartMenu - Start menu displayed");
    }

    /// <summary>
    /// 로딩 화면 표시 (맵 생성 중)
    /// </summary>
    public void ShowLoading()
    {
        HideAllPanels();

        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        Debug.Log("[INFO] SudokuUIPanel::ShowLoading - Loading screen displayed");
    }

    /// <summary>
    /// 게임 HUD 표시 (플레이 중)
    /// </summary>
    public void ShowGameHUD()
    {
        HideAllPanels();

        if (_gameHUDPanel != null)
        {
            _gameHUDPanel.SetActive(true);
        }

        // 타이머 시작
        if (_timerUI != null)
        {
            _timerUI.StartTimer();
        }

        // 게임 정보 업데이트
        UpdateGameInfo();

        Debug.Log("[INFO] SudokuUIPanel::ShowGameHUD - Game HUD displayed");
    }

    /// <summary>
    /// 승리 팝업 표시
    /// </summary>
    public void ShowWinPopup()
    {
        if (_winPopupPanel != null)
        {
            _winPopupPanel.SetActive(true);
        }

        // 타이머 정지
        if (_timerUI != null)
        {
            _timerUI.StopTimer();
        }

        // 승리 정보 업데이트
        UpdateWinPopup();

        Debug.Log("[INFO] SudokuUIPanel::ShowWinPopup - Win popup displayed");
    }

    /// <summary>
    /// 모든 패널 숨기기
    /// </summary>
    private void HideAllPanels()
    {
        if (_startMenuPanel != null) _startMenuPanel.SetActive(false);
        if (_gameHUDPanel != null) _gameHUDPanel.SetActive(false);
        if (_winPopupPanel != null) _winPopupPanel.SetActive(false);
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
    }

    #endregion

    #region UI 업데이트

    /// <summary>
    /// 게임 정보 업데이트 (난이도, 힌트, 실수 등)
    /// </summary>
    public void UpdateGameInfo()
    {
        if (_gameData == null) return;

        // 난이도 표시
        if (_difficultyText != null)
        {
            _difficultyText.text = GetDifficultyDisplayName(_gameData.Difficulty);
            _difficultyText.color = GetDifficultyColor(_gameData.Difficulty);
        }

        // 힌트 사용 횟수
        if (_hintsUsedText != null)
        {
            _hintsUsedText.text = $"힌트: {_gameData.HintsUsed}";
        }

        // 실수 횟수
        if (_mistakesText != null)
        {
            _mistakesText.text = $"실수: {_gameData.Mistakes}";
        }
    }

    /// <summary>
    /// 승리 팝업 정보 업데이트
    /// </summary>
    private void UpdateWinPopup()
    {
        if (_gameData == null) return;

        // 승리 타이틀
        if (_winTitleText != null)
        {
            _winTitleText.text = "축하합니다!";
        }

        // 클리어 타임
        if (_clearTimeText != null)
        {
            int minutes = Mathf.FloorToInt(_gameData.PlayTime / 60f);
            int seconds = Mathf.FloorToInt(_gameData.PlayTime % 60f);
            _clearTimeText.text = $"클리어 타임: {minutes:00}:{seconds:00}";
        }

        // 최종 점수
        if (_finalScoreText != null)
        {
            _finalScoreText.text = $"점수: {_gameData.Score}\n힌트: {_gameData.HintsUsed} / 실수: {_gameData.Mistakes}";
        }
    }

    /// <summary>
    /// 난이도별 표시 이름 반환
    /// </summary>
    private string GetDifficultyDisplayName(SudokuDifficulty difficulty)
    {
        return difficulty switch
        {
            SudokuDifficulty.Easy => "쉬움",
            SudokuDifficulty.Medium => "중간",
            SudokuDifficulty.Hard => "어려움",
            _ => "알 수 없음"
        };
    }

    /// <summary>
    /// 난이도별 색상 반환
    /// </summary>
    private Color GetDifficultyColor(SudokuDifficulty difficulty)
    {
        return difficulty switch
        {
            SudokuDifficulty.Easy => _easyColor,
            SudokuDifficulty.Medium => _mediumColor,
            SudokuDifficulty.Hard => _hardColor,
            _ => Color.white
        };
    }

    #endregion

    #region 버튼 이벤트

    private void RegisterButtonEvents()
    {
        // 시작 메뉴 버튼
        if (_easyButton != null)
            _easyButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Easy));
        if (_mediumButton != null)
            _mediumButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Medium));
        if (_hardButton != null)
            _hardButton.onClick.AddListener(() => OnDifficultyButtonClicked(SudokuDifficulty.Hard));
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackButtonClicked);

        // 게임 HUD 버튼
        if (_hintButton != null)
            _hintButton.onClick.AddListener(OnHintButtonClicked);
        if (_newGameButton != null)
            _newGameButton.onClick.AddListener(OnNewGameButtonClicked);

        // 승리 팝업 버튼
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void UnregisterButtonEvents()
    {
        // 시작 메뉴 버튼
        if (_easyButton != null)
            _easyButton.onClick.RemoveAllListeners();
        if (_mediumButton != null)
            _mediumButton.onClick.RemoveAllListeners();
        if (_hardButton != null)
            _hardButton.onClick.RemoveAllListeners();
        if (_backButton != null)
            _backButton.onClick.RemoveAllListeners();

        // 게임 HUD 버튼
        if (_hintButton != null)
            _hintButton.onClick.RemoveAllListeners();
        if (_newGameButton != null)
            _newGameButton.onClick.RemoveAllListeners();

        // 승리 팝업 버튼
        if (_playAgainButton != null)
            _playAgainButton.onClick.RemoveAllListeners();
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

    private void OnNewGameButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnNewGameButtonClicked - New game requested");
        OnNewGameRequested?.Invoke();
    }

    private void OnPlayAgainButtonClicked()
    {
        Debug.Log("[INFO] SudokuUIPanel::OnPlayAgainButtonClicked - Play again");
        ShowStartMenu();
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
