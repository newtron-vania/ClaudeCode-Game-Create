using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThreeMatch.Data;

namespace ThreeMatch.UI
{
    /// <summary>
    /// 3-Match 게임 UI 메인 패널
    /// 5-상태 UI 패널 관리: StartMenu → Playing → Paused/GameClear/GameOver
    /// </summary>
    public class ThreeMatchUIPanel : UIPanel
    {
        // ========================================
        // 5개 메인 패널
        // ========================================
        [Header("Main Panels")]
        [SerializeField] private GameObject _startMenuPanel;    // 시작 메뉴 (난이도/모드 선택)
        [SerializeField] private GameObject _playingPanel;      // 게임 플레이 중
        [SerializeField] private GameObject _pausedPanel;       // 일시정지
        [SerializeField] private GameObject _gameClearPanel;    // 목표 달성
        [SerializeField] private GameObject _gameOverPanel;     // 실패/시간초과

        // ========================================
        // StartMenuPanel UI 요소
        // ========================================
        [Header("StartMenuPanel Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TMP_Dropdown _difficultyDropdown;  // 난이도 선택
        [SerializeField] private TMP_Dropdown _gameModeDropdown;    // 게임 모드 선택
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;

        // ========================================
        // PlayingPanel UI 요소
        // ========================================
        [Header("PlayingPanel Elements")]
        [SerializeField] private TextMeshProUGUI _scoreText;        // 현재 점수
        [SerializeField] private TextMeshProUGUI _targetScoreText;  // 목표 점수
        [SerializeField] private TextMeshProUGUI _comboText;        // 현재 콤보
        [SerializeField] private TextMeshProUGUI _timerText;        // 타이머 (Classic, MovesLimited 모드)
        [SerializeField] private TextMeshProUGUI _movesText;        // 남은 이동 횟수 (MovesLimited 모드)
        [SerializeField] private Button _pauseButton;               // 일시정지 버튼
        [SerializeField] private Slider _progressBar;               // 진행도 바 (목표 점수 대비)

        // ========================================
        // PausedPanel UI 요소
        // ========================================
        [Header("PausedPanel Elements")]
        [SerializeField] private TextMeshProUGUI _pausedTitleText;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        // ========================================
        // GameClearPanel UI 요소
        // ========================================
        [Header("GameClearPanel Elements")]
        [SerializeField] private TextMeshProUGUI _clearTitleText;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _maxComboText;
        [SerializeField] private TextMeshProUGUI _clearTimeText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _clearMainMenuButton;

        // ========================================
        // GameOverPanel UI 요소
        // ========================================
        [Header("GameOverPanel Elements")]
        [SerializeField] private TextMeshProUGUI _gameOverTitleText;
        [SerializeField] private TextMeshProUGUI _gameOverReasonText;  // 실패 이유
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _gameOverMainMenuButton;

        // ========================================
        // 이벤트
        // ========================================
        public event Action<DifficultyLevel> OnDifficultySelected;
        public event Action<GameMode> OnGameModeSelected;
        public event Action OnStartButtonClicked;
        public event Action OnPauseButtonClicked;
        public event Action OnResumeButtonClicked;
        public event Action OnRestartButtonClicked;
        public event Action OnMainMenuButtonClicked;

        // ========================================
        // Private 필드
        // ========================================
        private ThreeMatchGame _game;
        private ThreeMatchGameData _gameData;
        private DifficultyLevel _selectedDifficulty = DifficultyLevel.Normal;
        private GameMode _selectedMode = GameMode.Classic;

        // ========================================
        // 패널 타입 enum
        // ========================================
        private enum PanelType
        {
            StartMenu,
            Playing,
            Paused,
            GameClear,
            GameOver
        }

        // ========================================
        // Unity 생명주기
        // ========================================

        protected override void Awake()
        {
            base.Awake();

            // 버튼 이벤트 등록
            RegisterButtonEvents();

            // 드롭다운 이벤트 등록
            RegisterDropdownEvents();

            // 초기 상태: 모든 패널 비활성화
            HideAllPanels();
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            UnregisterButtonEvents();

            // 드롭다운 이벤트 해제
            UnregisterDropdownEvents();
        }

        // ========================================
        // 초기화
        // ========================================

        /// <summary>
        /// UI 패널 초기화
        /// </summary>
        public void Initialize(ThreeMatchGame game)
        {
            _game = game;
            _gameData = game.GetData() as ThreeMatchGameData;

            Debug.Log("[ThreeMatchUIPanel] UI initialized");

            // 드롭다운 초기화
            InitializeDropdowns();

            // 시작 메뉴 표시
            ShowStartMenuPanel();
        }

        /// <summary>
        /// 드롭다운 초기화
        /// </summary>
        private void InitializeDropdowns()
        {
            if (_difficultyDropdown != null)
            {
                _difficultyDropdown.ClearOptions();
                _difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Easy",
                    "Normal",
                    "Hard"
                });
                _difficultyDropdown.value = 1; // Normal이 기본값
            }

            if (_gameModeDropdown != null)
            {
                _gameModeDropdown.ClearOptions();
                _gameModeDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Classic",
                    "MovesLimited",
                    "Endless"
                });
                _gameModeDropdown.value = 0; // Classic이 기본값
            }
        }

        // ========================================
        // 상태별 UI 전환
        // ========================================

        /// <summary>
        /// 시작 메뉴 패널 표시
        /// </summary>
        public void ShowStartMenuPanel()
        {
            ShowPanel(PanelType.StartMenu);
        }

        /// <summary>
        /// 플레이 중 패널 표시
        /// </summary>
        public void ShowPlayingPanel()
        {
            ShowPanel(PanelType.Playing);
            UpdateGameInfo(_gameData);
        }

        /// <summary>
        /// 일시정지 패널 표시
        /// </summary>
        public void ShowPausedPanel()
        {
            ShowPanel(PanelType.Paused);
        }

        /// <summary>
        /// 게임 클리어 패널 표시
        /// </summary>
        public void ShowGameClearPanel()
        {
            ShowPanel(PanelType.GameClear);
            UpdateGameClearPanel();
        }

        /// <summary>
        /// 게임 오버 패널 표시
        /// </summary>
        public void ShowGameOverPanel()
        {
            ShowPanel(PanelType.GameOver);
            UpdateGameOverPanel();
        }

        /// <summary>
        /// 지정된 패널 표시
        /// </summary>
        private void ShowPanel(PanelType panelType)
        {
            HideAllPanels();

            switch (panelType)
            {
                case PanelType.StartMenu:
                    if (_startMenuPanel != null) _startMenuPanel.SetActive(true);
                    break;
                case PanelType.Playing:
                    if (_playingPanel != null) _playingPanel.SetActive(true);
                    break;
                case PanelType.Paused:
                    if (_pausedPanel != null) _pausedPanel.SetActive(true);
                    break;
                case PanelType.GameClear:
                    if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
                    break;
                case PanelType.GameOver:
                    if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
                    break;
            }

            Debug.Log($"[ThreeMatchUIPanel] Showing panel: {panelType}");
        }

        /// <summary>
        /// 모든 패널 숨김
        /// </summary>
        private void HideAllPanels()
        {
            if (_startMenuPanel != null) _startMenuPanel.SetActive(false);
            if (_playingPanel != null) _playingPanel.SetActive(false);
            if (_pausedPanel != null) _pausedPanel.SetActive(false);
            if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        // ========================================
        // UI 업데이트
        // ========================================

        /// <summary>
        /// 게임 정보 업데이트 (Playing 상태)
        /// </summary>
        public void UpdateGameInfo(ThreeMatchGameData gameData)
        {
            if (gameData == null) return;

            // 점수 업데이트
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {gameData.Score}";
            }

            if (_targetScoreText != null)
            {
                _targetScoreText.text = $"Target: {gameData.TargetScore}";
            }

            // 콤보 업데이트
            if (_comboText != null)
            {
                if (gameData.CurrentCombo > 1)
                {
                    _comboText.text = $"Combo: x{gameData.CurrentCombo}";
                    _comboText.gameObject.SetActive(true);
                }
                else
                {
                    _comboText.gameObject.SetActive(false);
                }
            }

            // 진행도 바 업데이트
            if (_progressBar != null)
            {
                float progress = Mathf.Clamp01((float)gameData.Score / gameData.TargetScore);
                _progressBar.value = progress;
            }

            // 게임 모드별 UI 업데이트
            UpdateModeSpecificUI(gameData);
        }

        /// <summary>
        /// 게임 모드별 UI 업데이트
        /// </summary>
        private void UpdateModeSpecificUI(ThreeMatchGameData gameData)
        {
            switch (gameData.CurrentMode)
            {
                case GameMode.Classic:
                    // 타이머 표시
                    if (_timerText != null)
                    {
                        _timerText.gameObject.SetActive(true);
                        float remainingTime = Mathf.Max(0, gameData.TimeLimit - gameData.ElapsedTime);
                        int minutes = Mathf.FloorToInt(remainingTime / 60f);
                        int seconds = Mathf.FloorToInt(remainingTime % 60f);
                        _timerText.text = $"Time: {minutes:00}:{seconds:00}";
                    }
                    if (_movesText != null) _movesText.gameObject.SetActive(false);
                    break;

                case GameMode.MovesLimited:
                    // 남은 이동 횟수 표시
                    if (_movesText != null)
                    {
                        _movesText.gameObject.SetActive(true);
                        _movesText.text = $"Moves: {gameData.RemainingMoves}";
                    }
                    if (_timerText != null) _timerText.gameObject.SetActive(false);
                    break;

                case GameMode.Endless:
                    // 경과 시간만 표시
                    if (_timerText != null)
                    {
                        _timerText.gameObject.SetActive(true);
                        int minutes = Mathf.FloorToInt(gameData.ElapsedTime / 60f);
                        int seconds = Mathf.FloorToInt(gameData.ElapsedTime % 60f);
                        _timerText.text = $"Time: {minutes:00}:{seconds:00}";
                    }
                    if (_movesText != null) _movesText.gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// 게임 클리어 패널 업데이트
        /// </summary>
        private void UpdateGameClearPanel()
        {
            if (_gameData == null) return;

            if (_clearTitleText != null)
            {
                _clearTitleText.text = "STAGE CLEAR!";
            }

            if (_finalScoreText != null)
            {
                _finalScoreText.text = $"Final Score: {_gameData.Score}";
            }

            if (_maxComboText != null)
            {
                _maxComboText.text = $"Max Combo: x{_gameData.MaxCombo}";
            }

            if (_clearTimeText != null)
            {
                int minutes = Mathf.FloorToInt(_gameData.ElapsedTime / 60f);
                int seconds = Mathf.FloorToInt(_gameData.ElapsedTime % 60f);
                _clearTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }

        /// <summary>
        /// 게임 오버 패널 업데이트
        /// </summary>
        private void UpdateGameOverPanel()
        {
            if (_gameData == null) return;

            if (_gameOverTitleText != null)
            {
                _gameOverTitleText.text = "GAME OVER";
            }

            if (_gameOverReasonText != null)
            {
                string reason = "Failed to reach target score";
                if (_gameData.CurrentMode == GameMode.Classic && _gameData.IsTimeUp())
                {
                    reason = "Time's Up!";
                }
                else if (_gameData.CurrentMode == GameMode.MovesLimited && _gameData.IsMovesExhausted())
                {
                    reason = "No More Moves!";
                }
                _gameOverReasonText.text = reason;
            }

            if (_gameOverScoreText != null)
            {
                _gameOverScoreText.text = $"Score: {_gameData.Score} / {_gameData.TargetScore}";
            }
        }

        // ========================================
        // 이벤트 등록/해제
        // ========================================

        /// <summary>
        /// 버튼 이벤트 등록
        /// </summary>
        private void RegisterButtonEvents()
        {
            // StartMenuPanel
            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartButtonClick);
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClick);

            // PlayingPanel
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseButtonClick);

            // PausedPanel
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeButtonClick);
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartButtonClick);
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);

            // GameClearPanel
            if (_playAgainButton != null)
                _playAgainButton.onClick.AddListener(OnPlayAgainButtonClick);
            if (_clearMainMenuButton != null)
                _clearMainMenuButton.onClick.AddListener(OnMainMenuButtonClick);

            // GameOverPanel
            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetryButtonClick);
            if (_gameOverMainMenuButton != null)
                _gameOverMainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        /// <summary>
        /// 버튼 이벤트 해제
        /// </summary>
        private void UnregisterButtonEvents()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(OnStartButtonClick);
            if (_backButton != null)
                _backButton.onClick.RemoveListener(OnBackButtonClick);
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(OnPauseButtonClick);
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(OnResumeButtonClick);
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartButtonClick);
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
            if (_playAgainButton != null)
                _playAgainButton.onClick.RemoveListener(OnPlayAgainButtonClick);
            if (_clearMainMenuButton != null)
                _clearMainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
            if (_retryButton != null)
                _retryButton.onClick.RemoveListener(OnRetryButtonClick);
            if (_gameOverMainMenuButton != null)
                _gameOverMainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
        }

        /// <summary>
        /// 드롭다운 이벤트 등록
        /// </summary>
        private void RegisterDropdownEvents()
        {
            if (_difficultyDropdown != null)
                _difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
            if (_gameModeDropdown != null)
                _gameModeDropdown.onValueChanged.AddListener(OnGameModeDropdownChanged);
        }

        /// <summary>
        /// 드롭다운 이벤트 해제
        /// </summary>
        private void UnregisterDropdownEvents()
        {
            if (_difficultyDropdown != null)
                _difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyDropdownChanged);
            if (_gameModeDropdown != null)
                _gameModeDropdown.onValueChanged.RemoveListener(OnGameModeDropdownChanged);
        }

        // ========================================
        // 버튼 클릭 핸들러
        // ========================================

        private void OnStartButtonClick()
        {
            Debug.Log($"[ThreeMatchUIPanel] Start button clicked - Difficulty: {_selectedDifficulty}, Mode: {_selectedMode}");

            OnDifficultySelected?.Invoke(_selectedDifficulty);
            OnGameModeSelected?.Invoke(_selectedMode);
            OnStartButtonClicked?.Invoke();
        }

        private void OnBackButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Back button clicked");
            OnMainMenuButtonClicked?.Invoke();
        }

        private void OnPauseButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Pause button clicked");
            OnPauseButtonClicked?.Invoke();
        }

        private void OnResumeButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Resume button clicked");
            OnResumeButtonClicked?.Invoke();
        }

        private void OnRestartButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Restart button clicked");
            OnRestartButtonClicked?.Invoke();
        }

        private void OnMainMenuButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Main menu button clicked");
            OnMainMenuButtonClicked?.Invoke();
        }

        private void OnPlayAgainButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Play again button clicked");
            OnRestartButtonClicked?.Invoke();
        }

        private void OnRetryButtonClick()
        {
            Debug.Log("[ThreeMatchUIPanel] Retry button clicked");
            OnRestartButtonClicked?.Invoke();
        }

        // ========================================
        // 드롭다운 변경 핸들러
        // ========================================

        private void OnDifficultyDropdownChanged(int index)
        {
            _selectedDifficulty = (DifficultyLevel)index;
            Debug.Log($"[ThreeMatchUIPanel] Difficulty changed: {_selectedDifficulty}");
        }

        private void OnGameModeDropdownChanged(int index)
        {
            _selectedMode = (GameMode)index;
            Debug.Log($"[ThreeMatchUIPanel] Game mode changed: {_selectedMode}");
        }
    }
}
