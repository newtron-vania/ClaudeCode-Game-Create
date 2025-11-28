using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeMatch.Board;
using ThreeMatch.Systems;
using ThreeMatch.Data;
using Vector2Int = ThreeMatch.Board.Vector2Int;

namespace ThreeMatch
{
    /// <summary>
    /// 3-Match 게임 메인 클래스 (IMiniGame 구현)
    /// 모든 컴포넌트를 통합하고 게임 플로우 관리
    /// </summary>
    public class ThreeMatchGame : IMiniGame
    {
        // ========== 핵심 컴포넌트 ==========
        private ThreeMatchBoard _board;
        private ThreeMatchBoardView _boardView;
        private InputController _inputController;
        private ComboSystem _comboSystem;
        private ThreeMatchGameData _gameData;
        private CommonPlayerData _commonData;

        // ========== 게임 상태 ==========
        private bool _isProcessingMatches;
        private int _shuffleAttempts;
        private const int MAX_SHUFFLE_ATTEMPTS = 3;

        // ========== DataProvider ==========
        private Data.ThreeMatchDataProvider _dataProvider;

        // ========== 게임 설정 (DataProvider에서 로드) ==========
        private int _boardWidth;
        private int _boardHeight;
        private int _pieceTypeCount;

        // ========== IMiniGame 구현 ==========

        /// <summary>
        /// 게임 초기화
        /// </summary>
        public void Initialize(CommonPlayerData commonData)
        {
            _commonData = commonData;
            _gameData = new ThreeMatchGameData();
            _gameData.Initialize();

            // DataManager를 통해 DataProvider 등록 및 로드
            _dataProvider = new Data.ThreeMatchDataProvider();
            _dataProvider.Initialize();
            DataManager.Instance.RegisterProvider(_dataProvider);
            DataManager.Instance.LoadGameData("ThreeMatch");

            // 난이도 설정 로드 (기본값: Normal)
            var difficultyConfig = _dataProvider.GetDifficultyConfig(DifficultyLevel.Normal);
            _boardWidth = difficultyConfig.BoardWidth;
            _boardHeight = difficultyConfig.BoardHeight;
            _pieceTypeCount = difficultyConfig.PieceTypeCount;

            // 게임 모드 설정 로드 (기본값: Classic)
            var gameModeConfig = _dataProvider.GetGameModeConfig(GameMode.Classic);
            _gameData.TimeLimit = gameModeConfig.TimeLimit;
            _gameData.TargetScore = difficultyConfig.TargetScore;

            // 콤보 시스템 초기화
            _comboSystem = new ComboSystem(comboTimeout: 2f, maxComboMultiplier: 5);
            _comboSystem.OnComboChanged += HandleComboChanged;
            _comboSystem.OnComboReset += HandleComboReset;

            // 보드 데이터 초기화
            _board = new ThreeMatchBoard();
            _board.Initialize(_boardWidth, _boardHeight, _pieceTypeCount);

            Debug.Log($"[ThreeMatchGame] Initialized: {_boardWidth}x{_boardHeight}, {_pieceTypeCount} piece types");
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            // 보드 생성
            _board.GenerateInitialBoard();

            // BoardView 및 InputController는 Scene에서 설정됨 (SetBoardView, SetInputController 호출 필요)

            // 게임 데이터 초기화
            _gameData.Reset();
            _gameData.CurrentDifficulty = DifficultyLevel.Normal;
            _gameData.CurrentMode = GameMode.Classic;

            _shuffleAttempts = 0;
            _isProcessingMatches = false;

            Debug.Log("[ThreeMatchGame] Game started");
        }

        /// <summary>
        /// 게임 업데이트 (매 프레임)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_gameData == null)
                return;

            // 콤보 타이머 업데이트
            _comboSystem?.Update(deltaTime);

            // 게임 모드별 업데이트
            switch (_gameData.CurrentMode)
            {
                case GameMode.Classic:
                    UpdateClassicMode(deltaTime);
                    break;

                case GameMode.MovesLimited:
                    UpdateMovesLimitedMode(deltaTime);
                    break;

                case GameMode.Endless:
                    UpdateEndlessMode(deltaTime);
                    break;
            }

            // Deadlock 체크 (주기적으로)
            if (!_isProcessingMatches && Time.frameCount % 60 == 0)
            {
                CheckDeadlock();
            }
        }

        /// <summary>
        /// 게임 정리 (종료 시)
        /// </summary>
        public void Cleanup()
        {
            // InputController 이벤트 구독 해제
            if (_inputController != null)
            {
                _inputController.OnSwapRequested -= HandleSwapRequested;
            }

            // 콤보 시스템 이벤트 구독 해제
            if (_comboSystem != null)
            {
                _comboSystem.OnComboChanged -= HandleComboChanged;
                _comboSystem.OnComboReset -= HandleComboReset;
            }

            // DataManager를 통해 게임 데이터 언로드
            DataManager.Instance.UnloadGameData("ThreeMatch");

            Debug.Log("[ThreeMatchGame] Cleaned up");
        }

        /// <summary>
        /// 게임 데이터 반환
        /// </summary>
        public IGameData GetData()
        {
            return _gameData;
        }

        // ========== 외부 설정 메서드 (Scene에서 호출) ==========

        /// <summary>
        /// BoardView 설정
        /// </summary>
        public void SetBoardView(ThreeMatchBoardView boardView)
        {
            _boardView = boardView;
            _boardView.Initialize(_board, _boardWidth, _boardHeight);
            _boardView.CreatePieceViews();
        }

        /// <summary>
        /// InputController 설정
        /// </summary>
        public void SetInputController(InputController inputController)
        {
            _inputController = inputController;
            _inputController.Initialize(_board, _boardView);
            _inputController.OnSwapRequested += HandleSwapRequested;
        }

        // ========== 게임 모드별 업데이트 ==========

        private void UpdateClassicMode(float deltaTime)
        {
            _gameData.ElapsedTime += deltaTime;

            // 시간 종료 체크
            if (_gameData.IsTimeUp())
            {
                OnGameOver();
            }
        }

        private void UpdateMovesLimitedMode(float deltaTime)
        {
            // 이동 횟수는 교체 시 감소 (HandleSwapRequested에서 처리)

            // 이동 횟수 소진 체크
            if (_gameData.IsMovesExhausted())
            {
                OnGameOver();
            }
        }

        private void UpdateEndlessMode(float deltaTime)
        {
            // 무한 모드는 특별한 업데이트 없음
            // Deadlock만 체크
        }

        // ========== 입력 이벤트 처리 ==========

        /// <summary>
        /// 퍼즐 교체 요청 이벤트 핸들러
        /// </summary>
        private void HandleSwapRequested(Vector2Int pos1, Vector2Int pos2)
        {
            if (_isProcessingMatches || _boardView.IsAnimating)
                return;

            // 유효한 교체인지 확인
            if (!_board.IsValidSwap(pos1.x, pos1.y, pos2.x, pos2.y))
            {
                _inputController.ClearSelection();
                return;
            }

            // 교체 실행
            _board.SwapPieces(pos1.x, pos1.y, pos2.x, pos2.y);

            // 매치 확인 (코루틴으로 처리)
            if (_boardView != null)
            {
                CoroutineRunner.Instance.StartCoroutine(ProcessSwapAndMatches(pos1, pos2));
            }

            // 이동 횟수 감소 (MovesLimited 모드)
            if (_gameData.CurrentMode == GameMode.MovesLimited)
            {
                _gameData.RemainingMoves--;
            }

            _inputController.ClearSelection();
        }

        /// <summary>
        /// 교체 및 매치 처리 코루틴
        /// </summary>
        private IEnumerator ProcessSwapAndMatches(Vector2Int pos1, Vector2Int pos2)
        {
            _isProcessingMatches = true;

            // 교체 애니메이션 대기
            yield return new WaitWhile(() => _boardView.IsAnimating);

            // 매치 확인
            List<Match> matches = _board.FindAllMatches();

            if (matches.Count == 0)
            {
                // 매치 없음 → 원위치
                _board.SwapPieces(pos2.x, pos2.y, pos1.x, pos1.y);
                yield return new WaitWhile(() => _boardView.IsAnimating);
            }
            else
            {
                // 매치 있음 → 연쇄 매치 처리
                yield return CoroutineRunner.Instance.StartCoroutine(ProcessMatchLoop());
            }

            _isProcessingMatches = false;
        }

        /// <summary>
        /// 연쇄 매치 처리 루프
        /// </summary>
        private IEnumerator ProcessMatchLoop()
        {
            bool hasMatches = true;

            while (hasMatches)
            {
                // 매치 찾기
                List<Match> matches = _board.FindAllMatches();

                if (matches.Count > 0)
                {
                    // 콤보 증가
                    _comboSystem.IncrementCombo();

                    // 점수 계산 및 추가
                    int totalScore = 0;
                    foreach (var match in matches)
                    {
                        int score = MatchDetector.CalculateScore(match, _comboSystem.GetMultiplier());
                        totalScore += score;
                    }
                    _gameData.AddScore(totalScore);

                    // 매치된 퍼즐 파괴
                    _board.DestroyMatches(matches);
                    yield return new WaitWhile(() => _boardView.IsAnimating);

                    // 중력 적용
                    _board.ApplyGravity();
                    yield return new WaitWhile(() => _boardView.IsAnimating);

                    // 빈칸 채우기
                    _board.FillEmptyCells();
                    yield return new WaitWhile(() => _boardView.IsAnimating);

                    // 다음 매치 확인 (연쇄)
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    hasMatches = false;
                }
            }

            // 목표 달성 체크
            if (_gameData.IsGoalAchieved())
            {
                OnGameClear();
            }
        }

        // ========== 콤보 이벤트 처리 ==========

        private void HandleComboChanged(int currentCombo, int multiplier)
        {
            Debug.Log($"[ThreeMatchGame] Combo: {currentCombo} (x{multiplier})");

            // BoardView에서 콤보 이펙트 재생
            if (_boardView != null && currentCombo >= 3)
            {
                _boardView.PlayComboEffect(currentCombo);
            }
        }

        private void HandleComboReset()
        {
            Debug.Log("[ThreeMatchGame] Combo reset");
        }

        // ========== Deadlock 관리 ==========

        private void CheckDeadlock()
        {
            if (_board.IsDeadlocked())
            {
                _shuffleAttempts++;

                if (_shuffleAttempts >= MAX_SHUFFLE_ATTEMPTS)
                {
                    // Shuffle 실패 → 게임 오버
                    OnGameOver();
                }
                else
                {
                    // Shuffle 실행
                    _board.ShuffleBoard();
                }
            }
        }

        // ========== 게임 종료 처리 ==========

        private void OnGameClear()
        {
            Debug.Log("[ThreeMatchGame] Game Clear!");
            _gameData.SaveState();
        }

        private void OnGameOver()
        {
            Debug.Log("[ThreeMatchGame] Game Over");
            _gameData.SaveState();
        }
    }

    /// <summary>
    /// 코루틴 실행을 위한 싱글톤 MonoBehaviour
    /// </summary>
    public class CoroutineRunner : Singleton<CoroutineRunner>
    {
        // Singleton 패턴으로 자동 생성됨
    }
}
