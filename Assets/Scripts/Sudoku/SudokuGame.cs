using UnityEngine;

/// <summary>
/// 스도쿠 게임 구현
/// IMiniGame 인터페이스를 구현하여 미니게임 플랫폼과 통합
/// </summary>
public class SudokuGame : IMiniGame
{
    private SudokuGameData _data;
    private CommonPlayerData _commonData;
    private SudokuBoard _board;
    private SudokuGenerator _generator;
    private bool _isInitialized;

    // 게임 상태
    private GameState _currentState;
    private int _selectedRow = -1;
    private int _selectedCol = -1;

    /// <summary>
    /// 현재 보드
    /// </summary>
    public SudokuBoard Board => _board;

    /// <summary>
    /// 게임 상태
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// 선택된 셀 위치
    /// </summary>
    public (int row, int col) SelectedCell => (_selectedRow, _selectedCol);

    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        StartMenu,      // 난이도 선택
        Generating,     // 맵 생성 중
        Playing,        // 게임 플레이
        GameEnd         // 게임 종료 (승리/패배)
    }

    /// <summary>
    /// 게임 초기화
    /// </summary>
    /// <param name="commonData">공용 플레이어 데이터</param>
    public void Initialize(CommonPlayerData commonData)
    {
        _commonData = commonData;
        _data = new SudokuGameData();
        _data.Initialize();

        _board = new SudokuBoard();
        _generator = new SudokuGenerator();

        _currentState = GameState.StartMenu;
        _isInitialized = true;

        Debug.Log("[INFO] SudokuGame::Initialize - Sudoku game initialized");
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        if (!_isInitialized)
        {
            Debug.LogError("[ERROR] SudokuGame::StartGame - Game not initialized");
            return;
        }

        // InputManager 이벤트 구독
        InputManager.Instance.OnInputEvent += HandleInput;

        // 시작 메뉴 상태로 전환
        ChangeState(GameState.StartMenu);

        Debug.Log("[INFO] SudokuGame::StartGame - Sudoku game started");
    }

    /// <summary>
    /// 매 프레임 게임 로직 실행
    /// </summary>
    /// <param name="deltaTime">이전 프레임과의 시간 차이</param>
    public void Update(float deltaTime)
    {
        if (!_isInitialized)
        {
            return;
        }

        // Playing 상태에서만 시간 업데이트
        if (_currentState == GameState.Playing)
        {
            _data.UpdatePlayTime(deltaTime);
        }
    }

    /// <summary>
    /// 게임 종료 및 정리
    /// </summary>
    public void Cleanup()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInputEvent -= HandleInput;
        }

        Debug.Log("[INFO] SudokuGame::Cleanup - Sudoku game cleaned up");
    }

    /// <summary>
    /// 게임 데이터 반환
    /// </summary>
    /// <returns>SudokuGameData 인스턴스</returns>
    public IGameData GetData()
    {
        return _data;
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    /// <param name="newState">새 상태</param>
    public void ChangeState(GameState newState)
    {
        Debug.Log($"[INFO] SudokuGame::ChangeState - {_currentState} → {newState}");

        _currentState = newState;

        // 상태별 처리
        switch (newState)
        {
            case GameState.StartMenu:
                OnEnterStartMenu();
                break;
            case GameState.Generating:
                OnEnterGenerating();
                break;
            case GameState.Playing:
                OnEnterPlaying();
                break;
            case GameState.GameEnd:
                OnEnterGameEnd();
                break;
        }
    }

    /// <summary>
    /// 새 게임 시작 (난이도 선택)
    /// </summary>
    /// <param name="difficulty">난이도</param>
    public void StartNewGame(SudokuDifficulty difficulty)
    {
        Debug.Log($"[INFO] SudokuGame::StartNewGame - Starting new {difficulty} game");

        _data.StartNewGame(difficulty);

        // 맵 생성 상태로 전환
        ChangeState(GameState.Generating);
    }

    /// <summary>
    /// 셀 선택
    /// </summary>
    /// <param name="row">행</param>
    /// <param name="col">열</param>
    public void SelectCell(int row, int col)
    {
        if (_currentState != GameState.Playing)
        {
            return;
        }

        if (!_board.IsValidPosition(row, col))
        {
            Debug.LogWarning($"[WARNING] SudokuGame::SelectCell - Invalid position ({row}, {col})");
            return;
        }

        _selectedRow = row;
        _selectedCol = col;

        Debug.Log($"[INFO] SudokuGame::SelectCell - Selected cell ({row}, {col})");
    }

    /// <summary>
    /// 선택된 셀에 숫자 입력
    /// </summary>
    /// <param name="value">입력 값 (1-9, 0은 지우기)</param>
    public void InputNumber(int value)
    {
        if (_currentState != GameState.Playing)
        {
            return;
        }

        if (_selectedRow == -1 || _selectedCol == -1)
        {
            Debug.LogWarning("[WARNING] SudokuGame::InputNumber - No cell selected");
            return;
        }

        if (_board.IsCellFixed(_selectedRow, _selectedCol))
        {
            Debug.LogWarning("[WARNING] SudokuGame::InputNumber - Cannot modify fixed cell");
            return;
        }

        // 이전 값 저장
        int prevValue = _board.GetCell(_selectedRow, _selectedCol);

        // 값 설정
        if (_board.SetCell(_selectedRow, _selectedCol, value))
        {
            Debug.Log($"[INFO] SudokuGame::InputNumber - Input {value} at ({_selectedRow}, {_selectedCol})");

            // 정답과 비교 (틀렸으면 실수 카운트)
            if (value != 0 && value != _board.GetSolution(_selectedRow, _selectedCol))
            {
                _data.AddMistake();

                // 게임 오버 체크
                if (_data.IsGameOver)
                {
                    ChangeState(GameState.GameEnd);
                    return;
                }
            }

            // 에러 체크 및 업데이트
            bool[,] errors = SudokuValidator.FindErrors(_board.Board);
            _board.UpdateErrors(errors);

            // 완성 체크
            if (_board.IsAllCellsFilled() && _board.IsSolved())
            {
                OnPuzzleCompleted();
            }
        }
    }

    /// <summary>
    /// 힌트 사용
    /// </summary>
    public void UseHint()
    {
        if (_currentState != GameState.Playing)
        {
            return;
        }

        if (_selectedRow == -1 || _selectedCol == -1)
        {
            Debug.LogWarning("[WARNING] SudokuGame::UseHint - No cell selected");
            return;
        }

        if (_board.IsCellFixed(_selectedRow, _selectedCol))
        {
            Debug.LogWarning("[WARNING] SudokuGame::UseHint - Cell is already fixed");
            return;
        }

        // 정답 값으로 채우기
        int correctValue = _board.GetSolution(_selectedRow, _selectedCol);

        if (_board.SetCell(_selectedRow, _selectedCol, correctValue))
        {
            _data.UseHint();

            // 에러 체크 및 업데이트
            bool[,] errors = SudokuValidator.FindErrors(_board.Board);
            _board.UpdateErrors(errors);

            Debug.Log($"[INFO] SudokuGame::UseHint - Hint used: {correctValue} at ({_selectedRow}, {_selectedCol})");

            // 완성 체크
            if (_board.IsAllCellsFilled() && _board.IsSolved())
            {
                OnPuzzleCompleted();
            }
        }
    }

    /// <summary>
    /// 입력 이벤트 처리
    /// </summary>
    /// <param name="inputData">입력 이벤트 데이터</param>
    private void HandleInput(InputEventData inputData)
    {
        if (!_isInitialized || _currentState != GameState.Playing)
        {
            return;
        }

        // 키보드 입력 처리
        if (inputData.Type == InputType.KeyDown)
        {
            // 숫자 키 (1-9)
            if (inputData.KeyCode >= KeyCode.Alpha1 && inputData.KeyCode <= KeyCode.Alpha9)
            {
                int number = inputData.KeyCode - KeyCode.Alpha0;
                InputNumber(number);
            }
            // 숫자패드 키 (1-9)
            else if (inputData.KeyCode >= KeyCode.Keypad1 && inputData.KeyCode <= KeyCode.Keypad9)
            {
                int number = inputData.KeyCode - KeyCode.Keypad0;
                InputNumber(number);
            }
            // Backspace 또는 Delete (지우기)
            else if (inputData.KeyCode == KeyCode.Backspace || inputData.KeyCode == KeyCode.Delete)
            {
                InputNumber(0);
            }
            // H 키 (힌트)
            else if (inputData.KeyCode == KeyCode.H)
            {
                UseHint();
            }
        }
    }

    #region 상태 진입 처리

    private void OnEnterStartMenu()
    {
        // UI에서 난이도 선택 대기
        Debug.Log("[INFO] SudokuGame::OnEnterStartMenu - Waiting for difficulty selection");
    }

    private void OnEnterGenerating()
    {
        Debug.Log("[INFO] SudokuGame::OnEnterGenerating - Generating puzzle");

        // 퍼즐 생성
        var puzzleResult = _generator.GeneratePuzzle(_data.Difficulty);

        if (puzzleResult == null)
        {
            Debug.LogError("[ERROR] SudokuGame::OnEnterGenerating - Failed to generate puzzle");
            ChangeState(GameState.StartMenu);
            return;
        }

        // 보드 설정
        _board.SetSolution(puzzleResult.Solution);
        _board.SetInitialHints(puzzleResult.Hints);

        Debug.Log($"[INFO] SudokuGame::OnEnterGenerating - Puzzle generated: {puzzleResult}");

        // 플레이 상태로 전환
        ChangeState(GameState.Playing);
    }

    private void OnEnterPlaying()
    {
        Debug.Log("[INFO] SudokuGame::OnEnterPlaying - Start playing");

        // 선택 초기화
        _selectedRow = -1;
        _selectedCol = -1;
    }

    private void OnEnterGameEnd()
    {
        Debug.Log("[INFO] SudokuGame::OnEnterGameEnd - Game ended");

        if (_data.IsCompleted)
        {
            Debug.Log($"[INFO] SudokuGame::OnEnterGameEnd - Puzzle completed! {_data.GetGameStats()}");

            // 골드 보상 (점수 / 100)
            int goldReward = _data.Score / 100;
            _commonData.AddGold(goldReward);
        }
        else if (_data.IsGameOver)
        {
            Debug.Log("[INFO] SudokuGame::OnEnterGameEnd - Game over (too many mistakes)");
        }
    }

    #endregion

    /// <summary>
    /// 퍼즐 완성 처리
    /// </summary>
    private void OnPuzzleCompleted()
    {
        _data.CompleteGame();
        ChangeState(GameState.GameEnd);

        Debug.Log($"[INFO] SudokuGame::OnPuzzleCompleted - Puzzle completed! Time: {_data.PlayTime:F1}s, Score: {_data.Score}");
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[INFO] SudokuGame::RestartGame - Restarting game");

        _data.Reset();
        _board.ClearBoard();

        ChangeState(GameState.StartMenu);
    }
}
