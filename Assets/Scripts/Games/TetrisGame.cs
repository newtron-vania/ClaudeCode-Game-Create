using UnityEngine;

/// <summary>
/// 테트리스 게임 구현
/// IMiniGame 인터페이스를 구현하여 미니게임 플랫폼과 통합됩니다.
/// </summary>
public class TetrisGame : IMiniGame
{
    private TetrisData _data;
    private CommonPlayerData _commonData;
    private bool _isInitialized;

    /// <summary>
    /// 게임 초기화
    /// </summary>
    /// <param name="commonData">공용 플레이어 데이터</param>
    public void Initialize(CommonPlayerData commonData)
    {
        _commonData = commonData;
        _data = new TetrisData();
        _data.Initialize();
        _isInitialized = true;

        Debug.Log("[INFO] TetrisGame::Initialize - Tetris game initialized");
    }

    /// <summary>
    /// 게임 시작
    /// InputManager 이벤트 구독 및 첫 블록 생성
    /// </summary>
    public void StartGame()
    {
        if (!_isInitialized)
        {
            Debug.LogError("[ERROR] TetrisGame::StartGame - Game not initialized");
            return;
        }

        // InputManager 이벤트 구독
        InputManager.Instance.OnInputEvent += HandleInput;

        // NextPieces 큐 초기화 (3개)
        _data.NextPieces.Clear();
        for (int i = 0; i < 3; i++)
        {
            _data.NextPieces.Add(TetrisPiece.CreateRandom(0, 0));
        }

        // 첫 블록 생성 - NextPieces의 첫 번째를 CurrentPiece로
        SpawnNewPiece();

        _data.LastMoveTime = Time.time;
        _data.PlayTime = 0f;

        Debug.Log("[INFO] TetrisGame::StartGame - Tetris game started");
    }

    /// <summary>
    /// 매 프레임 게임 로직 실행
    /// </summary>
    /// <param name="deltaTime">이전 프레임과의 시간 차이</param>
    public void Update(float deltaTime)
    {
        if (!_isInitialized || _data.IsGameOver)
        {
            return;
        }

        _data.PlayTime += deltaTime;

        // 자동 낙하
        if (Time.time - _data.LastMoveTime >= _data.FallInterval)
        {
            if (!MovePieceDown())
            {
                // 더 이상 내려갈 수 없으면 보드에 고정
                LockPiece();
                ClearLines();
                SpawnNewPiece();
            }
            _data.LastMoveTime = Time.time;
        }
    }

    /// <summary>
    /// 게임 종료 및 정리
    /// InputManager 이벤트 구독 해제
    /// </summary>
    public void Cleanup()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInputEvent -= HandleInput;
        }

        Debug.Log("[INFO] TetrisGame::Cleanup - Tetris game cleaned up");
    }

    /// <summary>
    /// 게임 데이터 반환
    /// </summary>
    /// <returns>TetrisData 인스턴스</returns>
    public IGameData GetData()
    {
        return _data;
    }

    /// <summary>
    /// 입력 이벤트 처리
    /// </summary>
    /// <param name="inputData">입력 이벤트 데이터</param>
    private void HandleInput(InputEventData inputData)
    {
        if (!_isInitialized)
        {
            return;
        }

        // 키보드 입력만 처리
        if (inputData.Type == InputType.KeyDown)
        {
            // R키는 게임 오버 상태에서도 처리
            if (inputData.KeyCode == KeyCode.R)
            {
                RestartGame();
                return;
            }

            // 게임 오버 상태에서는 R키 외 다른 입력 무시
            if (_data.IsGameOver)
            {
                return;
            }

            switch (inputData.KeyCode)
            {
                case KeyCode.LeftArrow:
                case KeyCode.A:
                    MovePieceLeft();
                    break;

                case KeyCode.RightArrow:
                case KeyCode.D:
                    MovePieceRight();
                    break;

                case KeyCode.DownArrow:
                case KeyCode.S:
                    if (!MovePieceDown())
                    {
                        // 바닥에 도달했으면 즉시 고정
                        LockPiece();
                        ClearLines();
                        SpawnNewPiece();
                    }
                    break;

                case KeyCode.UpArrow:
                case KeyCode.W:
                    RotatePiece();
                    break;

                case KeyCode.Space:
                    HardDrop();
                    break;
            }
        }
    }

    /// <summary>
    /// 새 블록 생성
    /// </summary>
    private void SpawnNewPiece()
    {
        // NextPieces 큐에서 첫 번째를 CurrentPiece로 이동
        if (_data.NextPieces.Count > 0)
        {
            _data.CurrentPiece = _data.NextPieces[0];
            _data.CurrentPiece.X = TetrisData.BOARD_WIDTH / 2;
            _data.CurrentPiece.Y = 0;

            // 큐에서 제거
            _data.NextPieces.RemoveAt(0);

            // 새 블록을 큐 끝에 추가
            _data.NextPieces.Add(TetrisPiece.CreateRandom(0, 0));
        }

        // 게임 오버 체크 (생성 위치에 이미 블록이 있으면)
        if (IsCollision(_data.CurrentPiece))
        {
            _data.IsGameOver = true;
            Debug.Log($"[INFO] TetrisGame::SpawnNewPiece - Game Over! Score: {_data.CurrentScore}");

            // 최고 점수 업데이트
            MiniGameManager.Instance.UpdateHighScore(_data.CurrentScore);

            // 골드 보상 (점수 / 100)
            int goldReward = _data.CurrentScore / 100;
            _commonData.AddGold(goldReward);
        }
    }

    /// <summary>
    /// 블록을 왼쪽으로 이동
    /// </summary>
    private void MovePieceLeft()
    {
        _data.CurrentPiece.MoveLeft();
        if (IsCollision(_data.CurrentPiece))
        {
            _data.CurrentPiece.MoveRight(); // 충돌 시 원위치
        }
    }

    /// <summary>
    /// 블록을 오른쪽으로 이동
    /// </summary>
    private void MovePieceRight()
    {
        _data.CurrentPiece.MoveRight();
        if (IsCollision(_data.CurrentPiece))
        {
            _data.CurrentPiece.MoveLeft(); // 충돌 시 원위치
        }
    }

    /// <summary>
    /// 블록을 아래로 이동
    /// </summary>
    /// <returns>이동 성공 여부</returns>
    private bool MovePieceDown()
    {
        _data.CurrentPiece.MoveDown();
        if (IsCollision(_data.CurrentPiece))
        {
            _data.CurrentPiece.MoveUp(); // 충돌 시 원위치
            return false;
        }
        return true;
    }

    /// <summary>
    /// 블록 회전
    /// </summary>
    private void RotatePiece()
    {
        _data.CurrentPiece.RotateClockwise();
        if (IsCollision(_data.CurrentPiece))
        {
            _data.CurrentPiece.RotateCounterClockwise(); // 충돌 시 원위치
        }
    }

    /// <summary>
    /// 하드 드롭 (바로 떨어뜨리기)
    /// </summary>
    private void HardDrop()
    {
        int dropDistance = 0;
        while (MovePieceDown())
        {
            dropDistance++;
        }

        // 드롭 거리만큼 보너스 점수
        _data.AddScore(dropDistance * 2);

        // 블록 고정
        LockPiece();
        ClearLines();
        SpawnNewPiece();
    }

    /// <summary>
    /// 현재 블록을 보드에 고정
    /// </summary>
    private void LockPiece()
    {
        int[][] shape = _data.CurrentPiece.GetShape();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (shape[y][x] != 0)
                {
                    int boardX = _data.CurrentPiece.X + x - 1;
                    int boardY = _data.CurrentPiece.Y + y - 1;

                    if (boardX >= 0 && boardX < TetrisData.BOARD_WIDTH &&
                        boardY >= 0 && boardY < TetrisData.BOARD_HEIGHT)
                    {
                        _data.SetBlock(boardX, boardY, (int)_data.CurrentPiece.Type);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 완성된 라인 제거
    /// </summary>
    private void ClearLines()
    {
        int linesCleared = 0;

        // 아래부터 위로 검사
        for (int y = TetrisData.BOARD_HEIGHT - 1; y >= 0; y--)
        {
            if (IsLineFull(y))
            {
                RemoveLine(y);
                linesCleared++;
                y++; // 같은 줄을 다시 검사
            }
        }

        if (linesCleared > 0)
        {
            _data.AddClearedLines(linesCleared);
            Debug.Log($"[INFO] TetrisGame::ClearLines - Cleared {linesCleared} lines");
        }
    }

    /// <summary>
    /// 특정 라인이 가득 찼는지 확인
    /// </summary>
    /// <param name="y">라인 번호</param>
    /// <returns>라인이 가득 찼으면 true</returns>
    private bool IsLineFull(int y)
    {
        for (int x = 0; x < TetrisData.BOARD_WIDTH; x++)
        {
            if (_data.GetBlock(x, y) == 0)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 특정 라인 제거 및 위 라인들 아래로 이동
    /// </summary>
    /// <param name="lineY">제거할 라인 번호</param>
    private void RemoveLine(int lineY)
    {
        // 제거된 라인 위의 모든 라인을 한 칸씩 아래로
        for (int y = lineY; y > 0; y--)
        {
            for (int x = 0; x < TetrisData.BOARD_WIDTH; x++)
            {
                _data.SetBlock(x, y, _data.GetBlock(x, y - 1));
            }
        }

        // 최상단 라인은 빈 칸으로
        for (int x = 0; x < TetrisData.BOARD_WIDTH; x++)
        {
            _data.SetBlock(x, 0, 0);
        }
    }

    /// <summary>
    /// 블록과 보드의 충돌 검사
    /// </summary>
    /// <param name="piece">검사할 블록</param>
    /// <returns>충돌하면 true</returns>
    private bool IsCollision(TetrisPiece piece)
    {
        int[][] shape = piece.GetShape();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (shape[y][x] != 0)
                {
                    int boardX = piece.X + x - 1;
                    int boardY = piece.Y + y - 1;

                    // 보드 범위 밖
                    if (boardX < 0 || boardX >= TetrisData.BOARD_WIDTH ||
                        boardY < 0 || boardY >= TetrisData.BOARD_HEIGHT)
                    {
                        return true;
                    }

                    // 이미 블록이 있음
                    if (_data.GetBlock(boardX, boardY) != 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    private void RestartGame()
    {
        _data.Reset();

        // NextPieces 큐 재초기화 (3개)
        _data.NextPieces.Clear();
        for (int i = 0; i < 3; i++)
        {
            _data.NextPieces.Add(TetrisPiece.CreateRandom(0, 0));
        }

        SpawnNewPiece();
        _data.LastMoveTime = Time.time;

        // UI 리셋 이벤트 발생
        MiniGameManager.Instance.NotifyGameReset();

        Debug.Log("[INFO] TetrisGame::RestartGame - Game restarted");
    }
}
