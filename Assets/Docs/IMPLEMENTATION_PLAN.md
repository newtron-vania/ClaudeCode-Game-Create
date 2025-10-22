# 모듈형 미니게임 플랫폼 구현 플랜

**작성일:** 2025-10-22
**기반 문서:** MINIGAME_PLATFORM_PRD.md
**목표:** OCP 원칙을 준수하는 확장 가능한 미니게임 플랫폼 구축

---

## 📋 전체 개요

본 플랜은 `IMiniGame` 인터페이스 기반의 모듈형 미니게임 플랫폼을 구축하는 과정을 6개 Phase로 나누어 진행합니다.

**핵심 원칙:**
- ✅ **OCP (개방-폐쇄 원칙):** 새 게임 추가 시 코어 시스템 수정 없음
- ✅ **SoC (관심사 분리):** 입력/로직/데이터 명확히 분리
- ✅ **인터페이스 기반 설계:** 모든 게임은 `IMiniGame` 규약 준수

---

## 🎯 Phase 1: 코어 인터페이스 및 데이터 구조 정의

**목표:** 모든 미니게임이 따라야 할 '플러그 규격' 정의

### 1.1. IMiniGame 인터페이스 작성

**파일:** `Assets/Scripts/Core/IMiniGame.cs`

```csharp
public interface IMiniGame
{
    /// <summary>
    /// 게임 초기화 - 공용 데이터 주입
    /// </summary>
    void Initialize(CommonPlayerData commonData);

    /// <summary>
    /// 게임 시작 - InputManager 구독 시작
    /// </summary>
    void StartGame();

    /// <summary>
    /// 매 프레임 게임 로직 실행
    /// </summary>
    void Update(float deltaTime);

    /// <summary>
    /// 게임 종료 - InputManager 구독 해제 및 정리
    /// </summary>
    void Cleanup();

    /// <summary>
    /// 게임 고유 데이터 반환
    /// </summary>
    IGameData GetData();
}
```

**검증 기준:**
- [ ] 5개 메서드 모두 정의됨
- [ ] XML 주석 완료
- [ ] 인터페이스 네이밍 규칙 준수 (`IPascalCase`)

### 1.2. IGameData 인터페이스 작성

**파일:** `Assets/Scripts/Core/IGameData.cs`

```csharp
public interface IGameData
{
    /// <summary>
    /// 현재 게임 데이터 저장
    /// </summary>
    void SaveState();

    /// <summary>
    /// 저장된 게임 데이터 로드
    /// </summary>
    void LoadState();

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    void Initialize();

    /// <summary>
    /// 데이터 리셋 (새 게임 시작)
    /// </summary>
    void Reset();

    /// <summary>
    /// 데이터 검증
    /// </summary>
    bool Validate();
}
```

**검증 기준:**
- [ ] 기존 `IGameData`와 호환성 유지
- [ ] 저장/로드 메서드 정의됨

### 1.3. CommonPlayerData 클래스 작성

**파일:** `Assets/Scripts/GameData/CommonPlayerData.cs`

```csharp
[System.Serializable]
public class CommonPlayerData
{
    public int PlayerLevel;
    public int Gold;
    public int TotalPlayTime; // 초 단위
    public Dictionary<string, int> GameHighScores; // 게임별 최고 점수

    public CommonPlayerData()
    {
        PlayerLevel = 1;
        Gold = 0;
        TotalPlayTime = 0;
        GameHighScores = new Dictionary<string, int>();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        if (Gold < 0) Gold = 0;
    }

    public void UpdateHighScore(string gameID, int score)
    {
        if (!GameHighScores.ContainsKey(gameID) || GameHighScores[gameID] < score)
        {
            GameHighScores[gameID] = score;
        }
    }
}
```

**검증 기준:**
- [ ] 공용 데이터 필드 정의 (Level, Gold)
- [ ] 직렬화 가능 (`[Serializable]`)
- [ ] 게임별 최고 점수 관리 기능

### 1.4. InputEventData 구조체 작성

**파일:** `Assets/Scripts/Core/InputEventData.cs`

```csharp
public enum InputType
{
    KeyDown,
    KeyUp,
    PointerDown,
    PointerUp,
    PointerMove
}

[System.Serializable]
public struct InputEventData
{
    public InputType Type;
    public KeyCode KeyCode;           // KeyDown/KeyUp 시 사용
    public Vector2 PointerPosition;   // Pointer 이벤트 시 사용
    public int PointerID;             // 터치 ID (멀티터치)

    public InputEventData(InputType type, KeyCode keyCode)
    {
        Type = type;
        KeyCode = keyCode;
        PointerPosition = Vector2.zero;
        PointerID = 0;
    }

    public InputEventData(InputType type, Vector2 position, int pointerID = 0)
    {
        Type = type;
        KeyCode = KeyCode.None;
        PointerPosition = position;
        PointerID = pointerID;
    }
}
```

**검증 기준:**
- [ ] 입력 타입 enum 정의
- [ ] 키보드/마우스 데이터 모두 지원
- [ ] 생성자 오버로딩

---

## 🎯 Phase 2: 코어 시스템 구현

**목표:** 게임을 관리하고 입력을 분배하는 핵심 시스템 구축

### 2.1. InputManager 구현

**파일:** `Assets/Scripts/Managers/InputManager.cs`

**핵심 기능:**
- Singleton 패턴 (`Singleton<InputManager>` 상속)
- `OnInputEvent` 이벤트를 통한 입력 방송
- Unity Input System 또는 Legacy Input 사용

**구현 요구사항:**
```csharp
public class InputManager : Singleton<InputManager>
{
    public event Action<InputEventData> OnInputEvent;

    private void Update()
    {
        // 키보드 입력 감지
        if (Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    OnInputEvent?.Invoke(new InputEventData(InputType.KeyDown, key));
                }
            }
        }

        // 마우스/터치 입력 감지
        if (Input.GetMouseButtonDown(0))
        {
            OnInputEvent?.Invoke(new InputEventData(
                InputType.PointerDown,
                Input.mousePosition
            ));
        }

        // ... 추가 입력 처리
    }
}
```

**검증 기준:**
- [ ] Singleton 패턴 적용
- [ ] OnInputEvent 이벤트 정의
- [ ] 키보드 입력 감지 및 이벤트 발생
- [ ] 마우스/터치 입력 감지 및 이벤트 발생
- [ ] Update 루프에서 매 프레임 입력 체크

### 2.2. GameRegistry 구현

**파일:** `Assets/Scripts/Core/GameRegistry.cs`

**핵심 기능:**
- 게임 ID와 팩토리 함수 매핑
- 팩토리 패턴으로 게임 인스턴스 생성

**구현 요구사항:**
```csharp
public class GameRegistry
{
    private Dictionary<string, Func<IMiniGame>> _gameFactories;

    public GameRegistry()
    {
        _gameFactories = new Dictionary<string, Func<IMiniGame>>();
    }

    /// <summary>
    /// 게임을 레지스트리에 등록
    /// </summary>
    public void RegisterGame(string gameID, Func<IMiniGame> factory)
    {
        if (_gameFactories.ContainsKey(gameID))
        {
            Debug.LogWarning($"[WARNING] GameRegistry - Game already registered: {gameID}");
            return;
        }

        _gameFactories[gameID] = factory;
        Debug.Log($"[INFO] GameRegistry - Game registered: {gameID}");
    }

    /// <summary>
    /// 등록된 게임 인스턴스 생성
    /// </summary>
    public IMiniGame CreateGame(string gameID)
    {
        if (!_gameFactories.ContainsKey(gameID))
        {
            Debug.LogError($"[ERROR] GameRegistry - Game not found: {gameID}");
            return null;
        }

        var game = _gameFactories[gameID]();
        Debug.Log($"[INFO] GameRegistry - Game created: {gameID}");
        return game;
    }

    /// <summary>
    /// 등록된 모든 게임 ID 조회
    /// </summary>
    public List<string> GetRegisteredGameIDs()
    {
        return new List<string>(_gameFactories.Keys);
    }
}
```

**검증 기준:**
- [ ] Dictionary<string, Func<IMiniGame>> 사용
- [ ] RegisterGame 메서드 구현
- [ ] CreateGame 메서드 구현 (팩토리 실행)
- [ ] 중복 등록 방지 로직
- [ ] 에러 처리 및 로깅

### 2.3. GameManager 리팩토링

**파일:** `Assets/Scripts/Core/GameManager.cs`

**기존 `GameManager<T>` 수정:**
- 제네릭 제거
- `IMiniGame` 인터페이스 기반으로 변경
- `GameRegistry` 통합

**구현 요구사항:**
```csharp
public class MiniGamePlatformManager : Singleton<MiniGamePlatformManager>
{
    [SerializeField] private bool _isGamePaused = false;

    private IMiniGame _currentGame;
    private IGameData _currentGameData;
    private CommonPlayerData _commonData;
    private GameRegistry _gameRegistry;

    public IMiniGame CurrentGame => _currentGame;
    public IGameData CurrentGameData => _currentGameData;
    public CommonPlayerData CommonData => _commonData;
    public bool IsGamePaused => _isGamePaused;

    protected override void Awake()
    {
        base.Awake();

        _commonData = new CommonPlayerData();
        _gameRegistry = new GameRegistry();

        RegisterAllGames();
    }

    private void Update()
    {
        if (_isGamePaused || _currentGame == null)
            return;

        _currentGame.Update(Time.deltaTime);
    }

    /// <summary>
    /// 모든 미니게임 등록 (초기화 시 1회 실행)
    /// </summary>
    private void RegisterAllGames()
    {
        // Phase 6에서 게임 등록 코드 추가
        // _gameRegistry.RegisterGame("Tetris", () => new TetrisGame());
        // _gameRegistry.RegisterGame("Sudoku", () => new SudokuGame());
        // _gameRegistry.RegisterGame("SlidingPuzzle", () => new SlidingPuzzleGame());
    }

    /// <summary>
    /// 게임 로드 및 시작
    /// </summary>
    public void LoadGame(string gameID)
    {
        // 기존 게임 정리
        if (_currentGame != null)
        {
            _currentGame.Cleanup();
            _currentGame = null;
            _currentGameData = null;
        }

        // 새 게임 생성
        var newGame = _gameRegistry.CreateGame(gameID);
        if (newGame == null)
        {
            Debug.LogError($"[ERROR] Failed to load game: {gameID}");
            return;
        }

        _currentGame = newGame;

        // 게임 초기화
        _currentGame.Initialize(_commonData);
        _currentGameData = _currentGame.GetData();
        _currentGame.StartGame();

        Debug.Log($"[INFO] Game loaded and started: {gameID}");
    }

    public void PauseGame()
    {
        _isGamePaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        _isGamePaused = false;
        Time.timeScale = 1f;
    }
}
```

**검증 기준:**
- [ ] 제네릭 제거, IMiniGame 기반으로 변경
- [ ] GameRegistry 통합
- [ ] LoadGame 메서드 구현 (생명주기 관리)
- [ ] Update 루프에서 currentGame.Update 호출
- [ ] 게임 전환 시 Cleanup 호출

---

## 🎯 Phase 3: 테트리스 미니게임 구현

**목표:** IMiniGame 인터페이스를 구현한 첫 번째 게임 완성

### 3.1. TetrisData 클래스 작성

**파일:** `Assets/Scripts/GameData/TetrisData.cs`

```csharp
public class TetrisData : IGameData
{
    public int[,] BoardState;          // 10x20 보드
    public TetrisPiece CurrentPiece;
    public int CurrentScore;
    public int LinesCleared;
    public float FallSpeed;             // 블록 낙하 속도

    public void Initialize()
    {
        BoardState = new int[10, 20];
        CurrentScore = 0;
        LinesCleared = 0;
        FallSpeed = 1f;
        CurrentPiece = null;
    }

    public void Reset()
    {
        Initialize();
    }

    public bool Validate()
    {
        return BoardState != null && BoardState.GetLength(0) == 10 && BoardState.GetLength(1) == 20;
    }

    public void SaveState()
    {
        // JSON 직렬화 또는 PlayerPrefs 저장
    }

    public void LoadState()
    {
        // JSON 역직렬화 또는 PlayerPrefs 로드
    }
}
```

**검증 기준:**
- [ ] IGameData 인터페이스 구현
- [ ] 보드 상태 배열 (10x20)
- [ ] 점수, 라인 클리어 수 관리

### 3.2. Tetris Piece 구조체 작성

**파일:** `Assets/Scripts/GameData/TetrisPiece.cs`

```csharp
public enum PieceType
{
    I, O, T, S, Z, J, L
}

[System.Serializable]
public class TetrisPiece
{
    public PieceType Type;
    public int[,] Shape;        // 4x4 배열
    public int RotationState;   // 0-3
    public Vector2Int Position; // 보드 상의 위치

    // I, O, T, S, Z, J, L 블록 Shape 정의
    private static readonly Dictionary<PieceType, int[,]> PieceShapes = new Dictionary<PieceType, int[,]>
    {
        // 예시: I 블록
        { PieceType.I, new int[,] {
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0},
            {0,0,0,0}
        }},
        // ... 나머지 블록 정의
    };

    public TetrisPiece(PieceType type)
    {
        Type = type;
        Shape = PieceShapes[type];
        RotationState = 0;
        Position = new Vector2Int(3, 0); // 시작 위치
    }

    public void Rotate()
    {
        // 90도 회전 로직
        RotationState = (RotationState + 1) % 4;
        Shape = RotateMatrix(Shape);
    }

    private int[,] RotateMatrix(int[,] matrix)
    {
        // 시계방향 90도 회전
        int n = matrix.GetLength(0);
        int[,] rotated = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                rotated[j, n - 1 - i] = matrix[i, j];
            }
        }
        return rotated;
    }
}
```

**검증 기준:**
- [ ] 7가지 블록 타입 정의
- [ ] 4x4 Shape 배열
- [ ] 회전 로직 구현

### 3.3. TetrisGame 클래스 작성

**파일:** `Assets/Scripts/MiniGames/TetrisGame.cs`

**구현 요구사항:**
```csharp
public class TetrisGame : IMiniGame
{
    private TetrisData _data;
    private CommonPlayerData _commonDataRef;
    private float _timeSinceLastFall;

    public void Initialize(CommonPlayerData commonData)
    {
        _commonDataRef = commonData;
        _data = new TetrisData();
        _data.Initialize();
    }

    public void StartGame()
    {
        InputManager.Instance.OnInputEvent += HandleTetrisInput;
        SpawnNewPiece();
    }

    public void Update(float deltaTime)
    {
        _timeSinceLastFall += deltaTime;

        // 자동 낙하
        if (_timeSinceLastFall >= _data.FallSpeed)
        {
            _timeSinceLastFall = 0f;
            MovePieceDown();
        }
    }

    public void Cleanup()
    {
        InputManager.Instance.OnInputEvent -= HandleTetrisInput;
    }

    public IGameData GetData()
    {
        return _data;
    }

    private void HandleTetrisInput(InputEventData inputData)
    {
        if (inputData.Type != InputType.KeyDown)
            return;

        switch (inputData.KeyCode)
        {
            case KeyCode.LeftArrow:
                MovePiece(-1, 0);
                break;
            case KeyCode.RightArrow:
                MovePiece(1, 0);
                break;
            case KeyCode.UpArrow:
                RotatePiece();
                break;
            case KeyCode.DownArrow:
                MovePieceDown();
                break;
        }
    }

    private void SpawnNewPiece()
    {
        // 랜덤 블록 생성
        var randomType = (PieceType)UnityEngine.Random.Range(0, 7);
        _data.CurrentPiece = new TetrisPiece(randomType);
    }

    private bool MovePiece(int dx, int dy)
    {
        // 이동 가능 여부 체크 후 이동
        // 충돌 검사 로직
        return true; // 구현 필요
    }

    private void MovePieceDown()
    {
        if (!MovePiece(0, 1))
        {
            // 바닥에 닿음 - 블록 고정
            LockPiece();
            CheckLines();
            SpawnNewPiece();
        }
    }

    private void RotatePiece()
    {
        _data.CurrentPiece.Rotate();
        // 회전 후 충돌 검사
    }

    private void LockPiece()
    {
        // 현재 블록을 보드에 고정
    }

    private void CheckLines()
    {
        // 완성된 라인 검사 및 제거
        // 점수 증가
    }
}
```

**검증 기준:**
- [ ] IMiniGame 인터페이스 완전 구현
- [ ] InputManager 이벤트 구독/해제
- [ ] 블록 생성, 이동, 회전 로직
- [ ] 라인 완성 검사 및 제거
- [ ] 점수 계산

### 3.4. 테트리스 블록 생성 및 회전 로직

**구현 체크리스트:**
- [ ] 7가지 블록 타입별 Shape 정의
- [ ] 블록 회전 시 보드 경계 검사
- [ ] 블록 회전 시 다른 블록과 충돌 검사

### 3.5. 테트리스 라인 완성 검사 및 제거 로직

**구현 체크리스트:**
- [ ] 보드 전체 행 스캔
- [ ] 완성된 라인 제거
- [ ] 상위 블록들 한 칸씩 아래로 이동
- [ ] 점수 증가 (1줄: 100점, 2줄: 300점, 3줄: 600점, 4줄: 1000점)
- [ ] CommonPlayerData에 점수 반영

---

## 🎯 Phase 4: 스도쿠 미니게임 구현

**목표:** 두 번째 게임으로 OCP 검증

### 4.1. SudokuData 클래스 작성

**파일:** `Assets/Scripts/GameData/SudokuData.cs`

```csharp
public class SudokuData : IGameData
{
    public int[,] BoardState;         // 9x9, 유저가 채운 현재 상태
    public int[,] PuzzleDefinition;   // 9x9, 초기 문제 (0은 빈칸)
    public int[,] Solution;           // 9x9, 정답
    public Vector2Int SelectedCell;
    public float PlayTime;

    public void Initialize()
    {
        BoardState = new int[9, 9];
        PuzzleDefinition = new int[9, 9];
        Solution = new int[9, 9];
        SelectedCell = new Vector2Int(-1, -1);
        PlayTime = 0f;
    }

    // ... IGameData 구현
}
```

### 4.2. SudokuGame 클래스 작성

**파일:** `Assets/Scripts/MiniGames/SudokuGame.cs`

**구현 요구사항:**
- IMiniGame 인터페이스 구현
- 셀 선택 입력 처리
- 숫자 입력 처리 (1-9)
- 실시간 검증 (선택 사항)
- 타이머 업데이트

**검증 기준:**
- [ ] IMiniGame 인터페이스 구현
- [ ] InputManager 이벤트 구독/해제
- [ ] 셀 선택 로직
- [ ] 숫자 입력 및 검증

### 4.3. 스도쿠 퍼즐 생성 로직

**구현 체크리스트:**
- [ ] 유효한 9x9 스도쿠 보드 생성
- [ ] 난이도별 빈칸 개수 조절 (쉬움: 30, 보통: 40, 어려움: 50)
- [ ] 유일한 해를 가지는 퍼즐 보장

### 4.4. 스도쿠 입력 핸들러 및 검증 로직

**구현 체크리스트:**
- [ ] 마우스 클릭으로 셀 선택
- [ ] 키보드 1-9로 숫자 입력
- [ ] 가로/세로/3x3 박스 중복 검사
- [ ] 완성 여부 체크

---

## 🎯 Phase 5: 슬라이딩 퍼즐 미니게임 구현

**목표:** 세 번째 게임으로 플랫폼 안정성 검증

### 5.1. SlidingPuzzleData 클래스 작성

**파일:** `Assets/Scripts/GameData/SlidingPuzzleData.cs`

```csharp
public class SlidingPuzzleData : IGameData
{
    public int[,] TilePositions;  // 3x3, 0은 빈칸
    public int MoveCount;
    public bool IsCompleted;

    public void Initialize()
    {
        TilePositions = new int[3, 3];
        MoveCount = 0;
        IsCompleted = false;
        InitializeSolvedState();
        ShuffleTiles();
    }

    private void InitializeSolvedState()
    {
        int num = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                TilePositions[i, j] = (i == 2 && j == 2) ? 0 : num++;
            }
        }
    }

    private void ShuffleTiles()
    {
        // 해결 가능한 상태로 셔플
    }

    // ... IGameData 구현
}
```

### 5.2. SlidingPuzzleGame 클래스 작성

**파일:** `Assets/Scripts/MiniGames/SlidingPuzzleGame.cs`

**구현 요구사항:**
- IMiniGame 인터페이스 구현
- 타일 클릭 입력 처리
- 빈칸 주변 타일 이동 로직
- 완성 여부 체크

**검증 기준:**
- [ ] IMiniGame 인터페이스 구현
- [ ] InputManager 이벤트 구독/해제
- [ ] 타일 이동 로직
- [ ] 완성 검사

### 5.3. 슬라이딩 퍼즐 셔플 로직

**구현 체크리스트:**
- [ ] 항상 해결 가능한 상태로 셔플 (짝수 inversion count)
- [ ] 최소 N번 이상 이동하여 섞기

### 5.4. 슬라이딩 퍼즐 이동 및 완성 검사 로직

**구현 체크리스트:**
- [ ] 빈칸 주변 타일만 이동 가능
- [ ] 이동 카운트 증가
- [ ] 완성 상태 체크 (1-8 순서대로, 0은 마지막)

---

## 🎯 Phase 6: 게임 등록 및 통합 테스트

**목표:** 모든 게임을 플랫폼에 등록하고 OCP 검증

### 6.1. 게임 초기화 시 3개 게임 GameRegistry에 등록

**파일:** `Assets/Scripts/Core/GameManager.cs` (MiniGamePlatformManager)

```csharp
private void RegisterAllGames()
{
    Debug.Log("[INFO] Registering all mini games...");

    _gameRegistry.RegisterGame("Tetris", () => new TetrisGame());
    _gameRegistry.RegisterGame("Sudoku", () => new SudokuGame());
    _gameRegistry.RegisterGame("SlidingPuzzle", () => new SlidingPuzzleGame());

    Debug.Log($"[INFO] Total games registered: {_gameRegistry.GetRegisteredGameIDs().Count}");
}
```

**검증 기준:**
- [ ] 3개 게임 모두 등록됨
- [ ] 등록 로그 출력
- [ ] GetRegisteredGameIDs로 확인 가능

### 6.2. 게임 전환 테스트

**테스트 스크립트:** `Assets/Scripts/Tests/MiniGamePlatformTest.cs`

```csharp
public class MiniGamePlatformTest : MonoBehaviour
{
    private void Start()
    {
        TestGameSwitching();
    }

    private void TestGameSwitching()
    {
        var manager = MiniGamePlatformManager.Instance;

        // 테트리스 로드
        manager.LoadGame("Tetris");
        Assert.IsNotNull(manager.CurrentGame);
        Assert.IsTrue(manager.CurrentGame is TetrisGame);

        // 스도쿠로 전환
        manager.LoadGame("Sudoku");
        Assert.IsTrue(manager.CurrentGame is SudokuGame);

        // 슬라이딩 퍼즐로 전환
        manager.LoadGame("SlidingPuzzle");
        Assert.IsTrue(manager.CurrentGame is SlidingPuzzleGame);

        Debug.Log("[TEST] Game switching test passed!");
    }
}
```

**검증 기준:**
- [ ] Tetris → Sudoku → SlidingPuzzle 전환 성공
- [ ] 이전 게임의 Cleanup 호출 확인
- [ ] InputManager 구독/해제 정상 동작

### 6.3. CommonPlayerData 공유 검증

**테스트 시나리오:**
1. 테트리스에서 100점 획득 → CommonData.Gold += 10
2. 스도쿠로 전환 → Gold가 10으로 유지되는지 확인
3. 슬라이딩 퍼즐 완성 → Gold += 5 → 총 15 확인

**검증 기준:**
- [ ] 게임 전환 시 CommonData 유지
- [ ] 모든 게임에서 동일한 CommonData 참조
- [ ] Gold, Level 등이 누적됨

### 6.4. InputManager 이벤트 구독/해제 테스트

**테스트 시나리오:**
1. 테트리스 시작 → InputManager.OnInputEvent 구독자 수 = 1
2. 스도쿠로 전환 → 구독자 수 = 1 (테트리스 해제, 스도쿠 구독)
3. 게임 종료 → 구독자 수 = 0

**검증 기준:**
- [ ] Cleanup 시 이벤트 해제 확인
- [ ] 메모리 누수 없음 (구독자 수 검증)
- [ ] 입력 이벤트가 현재 게임에만 전달됨

---

## ✅ 전체 완료 기준

### 코어 시스템
- [ ] IMiniGame, IGameData 인터페이스 완성
- [ ] InputManager 정상 동작
- [ ] GameRegistry 정상 동작
- [ ] MiniGamePlatformManager 정상 동작

### 3개 미니게임
- [ ] TetrisGame 완전 구현 및 플레이 가능
- [ ] SudokuGame 완전 구현 및 플레이 가능
- [ ] SlidingPuzzleGame 완전 구현 및 플레이 가능

### OCP 검증
- [ ] 새 게임 추가 시 코어 시스템 수정 없음
- [ ] RegisterGame 한 줄만으로 게임 추가 가능
- [ ] 모든 게임이 IMiniGame 규약 준수

### 통합 테스트
- [ ] 게임 전환 시 Cleanup/Initialize/StartGame 정상 호출
- [ ] CommonPlayerData 공유 및 유지
- [ ] InputManager 구독/해제 정상 동작
- [ ] 메모리 누수 없음

---

## 📝 다음 단계 (Post-MVP)

1. **UI 시스템 통합**
   - 게임 선택 메뉴 UI
   - 점수 표시 UI
   - 공용 데이터 표시 (Level, Gold)

2. **데이터 저장/로드**
   - JSON 직렬화
   - PlayerPrefs 또는 파일 시스템
   - 게임별 진행 상황 저장

3. **추가 미니게임**
   - 4번째 게임 추가로 OCP 재검증
   - 다양한 장르 추가 (액션, 타이머 기반 등)

4. **최적화**
   - 오브젝트 풀링 (PoolManager 활용)
   - Addressables 리소스 로딩
   - 씬 전환 최적화
