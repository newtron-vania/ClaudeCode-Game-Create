# 3-Match 게임 아키텍처 설계

**작성일**: 2025-11-27
**버전**: 1.0
**프로젝트**: ClaudeCode-Game-Create

---

## 📋 목차

1. [아키텍처 개요](#아키텍처-개요)
2. [데이터-View 분리 패턴](#데이터-view-분리-패턴)
3. [핵심 컴포넌트 설계](#핵심-컴포넌트-설계)
4. [이벤트 시스템](#이벤트-시스템)
5. [게임 플로우](#게임-플로우)
6. [클래스 다이어그램](#클래스-다이어그램)
7. [참고 구현](#참고-구현)

---

## 아키텍처 개요

### 설계 원칙

3-Match 게임은 **데이터-View 완전 분리** 아키텍처를 채택합니다. 이는 Sudoku 게임에서 검증된 패턴을 기반으로 합니다.

**핵심 원칙**:
1. **순수 데이터 모델**: MonoBehaviour를 상속하지 않는 순수 C# 클래스
2. **단방향 데이터 흐름**: Data → Event → View (후처리 방식)
3. **테스트 가능성**: UI 없이 게임 로직 단위 테스트 가능
4. **재사용성**: 데이터 모델을 다른 View에서 재사용 가능

### 전체 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                    ThreeMatchGame (IMiniGame)                │
│                  게임 라이프사이클 관리자                      │
└─────────────────────────────────────────────────────────────┘
                            │
                ┌───────────┴───────────┐
                │                       │
    ┌───────────▼──────────┐   ┌───────▼────────────┐
    │   Data Layer         │   │   View Layer       │
    │   (순수 로직)         │   │   (Unity UI)       │
    └──────────────────────┘   └────────────────────┘
                │                       │
        ┌───────┴───────┐       ┌───────┴────────┐
        │               │       │                │
    ┌───▼────┐    ┌────▼────┐  ┌▼──────────┐ ┌──▼──────┐
    │ Board  │    │ Match   │  │ BoardView │ │ Piece   │
    │ Data   │    │Detector │  │           │ │ View    │
    └────────┘    └─────────┘  └───────────┘ └─────────┘
        │              │              ▲            ▲
        └──────┬───────┘              │            │
               │                      │            │
        ┌──────▼──────┐               │            │
        │   Events    ├───────────────┴────────────┘
        │   System    │   (이벤트 구독)
        └─────────────┘
```

---

## 데이터-View 분리 패턴

### Sudoku 패턴 적용 (참고)

Sudoku 게임에서 검증된 데이터-View 분리 패턴:

```csharp
// Sudoku 데이터 모델 (순수 C# 클래스)
public class SudokuBoard
{
    private int[,] _board;           // 순수 데이터
    private int[,] _solution;        // 정답
    private bool[,] _isFixed;        // 고정 셀

    // 메서드: 데이터만 변경
    public void SetNumber(int row, int col, int number) { }
    public bool IsSolved() { }
}

// Sudoku View (MonoBehaviour)
public class SudokuGridUI : MonoBehaviour
{
    private SudokuCellButton[,] _cellButtons;  // UI 컴포넌트

    // 데이터 변경 후 UI 업데이트 (후처리)
    public void UpdateCell(int row, int col, int number) { }
    public void HighlightErrors(bool[,] errors) { }
}
```

### 3-Match 적용 설계

동일한 패턴을 3-Match에 적용:

```csharp
// 3-Match 데이터 모델 (순수 C# 클래스)
public class ThreeMatchBoard
{
    private int[,] _board;           // 7x7 퍼즐 배열

    // 이벤트 (View가 구독)
    public event Action<int, int, int> OnPieceChanged;
    public event Action<List<Match>> OnMatchesFound;
    public event Action OnBoardShuffled;

    // 순수 데이터 조작
    public void SwapPieces(int x1, int y1, int x2, int y2) { }
    public List<Match> FindAllMatches() { }
    public bool IsDeadlocked() { }
}

// 3-Match View (MonoBehaviour)
public class ThreeMatchBoardView : MonoBehaviour
{
    private PuzzlePiece[,] _pieceViews;  // UI 컴포넌트

    // 이벤트 구독 및 UI 업데이트 (후처리)
    private void HandlePieceChanged(int x, int y, int pieceId) { }
    private void HandleMatchesFound(List<Match> matches) { }
    private void HandleBoardShuffled() { }
}
```

---

## 핵심 컴포넌트 설계

### 1. ThreeMatchBoard (데이터 모델)

**책임**:
- 보드 상태 관리 (7x7 배열)
- 퍼즐 교체 로직
- 매치 감지
- Deadlock 감지 및 Shuffle
- 이벤트 발생 (View 통지)

**주요 메서드**:
```csharp
public class ThreeMatchBoard
{
    // ========== 필드 ==========
    private int[,] _board;                    // 보드 상태 (0: 빈칸, 1~N: 퍼즐 ID)
    private int _width;                       // 보드 너비
    private int _height;                      // 보드 높이
    private int _pieceTypeCount;              // 퍼즐 종류 수

    // ========== 이벤트 ==========
    public event Action<int, int, int> OnPieceChanged;              // (x, y, newPieceId)
    public event Action<int, int, int, int> OnPiecesSwapped;        // (x1, y1, x2, y2)
    public event Action<List<Match>> OnMatchesFound;                // 매치 발견
    public event Action<List<Vector2Int>> OnPiecesDestroyed;        // 퍼즐 파괴
    public event Action<List<PieceMove>> OnPiecesFalling;           // 중력 낙하
    public event Action OnBoardShuffled;                            // Shuffle 발생
    public event Action<bool> OnDeadlockDetected;                   // Deadlock 감지

    // ========== 초기화 ==========
    public void Initialize(int width, int height, int pieceTypeCount);
    public void GenerateInitialBoard();

    // ========== 보드 조작 ==========
    public void SwapPieces(int x1, int y1, int x2, int y2);
    public bool IsValidSwap(int x1, int y1, int x2, int y2);
    public void SetPieceAt(int x, int y, int pieceId);
    public int GetPieceAt(int x, int y);

    // ========== 매치 감지 ==========
    public List<Match> FindAllMatches();
    public bool HasMatchAt(int x, int y);
    public void DestroyMatches(List<Match> matches);

    // ========== 중력 및 채우기 ==========
    public List<PieceMove> ApplyGravity();
    public List<Vector2Int> FillEmptyCells();

    // ========== Deadlock 관리 ==========
    public bool IsDeadlocked();
    public void ShuffleBoard();

    // ========== 유틸리티 ==========
    public void ClearBoard();
    public int[,] GetBoardCopy();
}

// 데이터 구조
public struct Match
{
    public MatchType Type;                    // Basic3/4/5, Cross33/43/53
    public List<Vector2Int> Positions;        // 매치된 위치들
    public int PieceId;                       // 매치된 퍼즐 ID
    public int Score;                         // 점수
}

public struct PieceMove
{
    public Vector2Int From;                   // 시작 위치
    public Vector2Int To;                     // 목표 위치
    public int PieceId;                       // 퍼즐 ID
}

public enum MatchType
{
    Basic3,      // 3개 일렬 (100점)
    Basic4,      // 4개 일렬 (500점)
    Basic5,      // 5개 일렬 (1,000점)
    Cross33,     // 3+3 크로스 (1,000점)
    Cross43,     // 4+3 크로스 (1,500점)
    Cross53      // 5+3 크로스 (2,000점)
}
```

### 2. MatchDetector (매치 감지 로직)

**책임**:
- 가로/세로 매치 찾기
- 크로스 매치 감지
- 점수 계산

**주요 메서드**:
```csharp
public class MatchDetector
{
    // ========== 매치 감지 ==========
    public static List<Match> FindAllMatches(int[,] board, int width, int height);
    public static Match FindMatchAt(int[,] board, int x, int y);

    // ========== 개별 방향 매치 ==========
    private static List<Vector2Int> FindHorizontalMatch(int[,] board, int x, int y);
    private static List<Vector2Int> FindVerticalMatch(int[,] board, int x, int y);

    // ========== 크로스 매치 ==========
    private static Match DetectCrossMatch(List<Vector2Int> horizontal, List<Vector2Int> vertical);

    // ========== 점수 계산 ==========
    public static int CalculateScore(Match match, int comboMultiplier);
    private static MatchType DetermineMatchType(int horizontalCount, int verticalCount);
}
```

### 3. ThreeMatchBoardView (View 레이어)

**책임**:
- PuzzlePiece 인스턴스화 및 배치
- 보드 이벤트 구독
- 애니메이션 처리 (교체, 파괴, 낙하)
- 시각 피드백 (이펙트, 사운드)

**주요 메서드**:
```csharp
public class ThreeMatchBoardView : MonoBehaviour
{
    // ========== 필드 ==========
    [SerializeField] private Transform _boardContainer;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _spacing = 0.1f;

    private PuzzlePiece[,] _pieceViews;
    private ThreeMatchBoard _boardData;
    private bool _isAnimating;

    // ========== 초기화 ==========
    public void Initialize(ThreeMatchBoard boardData, int width, int height);
    private void CreatePieceViews(int width, int height);
    private void SubscribeToEvents();
    private void UnsubscribeFromEvents();

    // ========== 이벤트 핸들러 (후처리) ==========
    private void HandlePieceChanged(int x, int y, int newPieceId);
    private void HandlePiecesSwapped(int x1, int y1, int x2, int y2);
    private void HandleMatchesFound(List<Match> matches);
    private void HandlePiecesDestroyed(List<Vector2Int> positions);
    private void HandlePiecesFalling(List<PieceMove> moves);
    private void HandleBoardShuffled();

    // ========== 애니메이션 ==========
    private IEnumerator SwapAnimation(Vector2Int pos1, Vector2Int pos2);
    private IEnumerator DestroyAnimation(List<Vector2Int> positions);
    private IEnumerator FallAnimation(List<PieceMove> moves);
    private IEnumerator ShuffleAnimation();

    // ========== 시각 피드백 ==========
    private void PlayMatchEffect(Match match);
    private void PlayComboEffect(int comboCount);
    private void PlayDeadlockWarning();

    // ========== 유틸리티 ==========
    private Vector3 GridToWorldPosition(int x, int y);
    private PuzzlePiece GetPieceViewAt(int x, int y);
    public bool IsAnimating => _isAnimating;
}
```

### 4. PuzzlePiece (개별 퍼즐 View)

**책임**:
- 개별 퍼즐 시각화
- 스프라이트 설정
- 애니메이션 재생
- 풀링 지원 (IPoolable)

**주요 메서드**:
```csharp
public class PuzzlePiece : MonoBehaviour, IPoolable
{
    // ========== 필드 ==========
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _matchEffect;

    private int _pieceId;
    private Vector2Int _gridPosition;

    // ========== 초기화 ==========
    public void SetPieceType(int pieceId, Sprite sprite);
    public void SetGridPosition(int x, int y);

    // ========== 애니메이션 ==========
    public IEnumerator MoveToPosition(Vector3 targetPosition, float duration);
    public void PlayMatchEffect();
    public void PlaySpawnEffect();

    // ========== IPoolable 구현 ==========
    public void OnSpawnFromPool();
    public void OnReturnToPool();

    // ========== 유틸리티 ==========
    public int PieceId => _pieceId;
    public Vector2Int GridPosition => _gridPosition;
}
```

### 5. InputController (입력 처리)

**책임**:
- 퍼즐 선택 및 드래그
- 인접 확인
- 교체 요청 이벤트 발생

**주요 메서드**:
```csharp
public class InputController : MonoBehaviour
{
    // ========== 필드 ==========
    private Vector2Int? _selectedPiece;
    private ThreeMatchBoard _board;
    private ThreeMatchBoardView _boardView;
    private bool _isProcessing;

    // ========== 이벤트 ==========
    public event Action<Vector2Int, Vector2Int> OnSwapRequested;

    // ========== 초기화 ==========
    public void Initialize(ThreeMatchBoard board, ThreeMatchBoardView boardView);

    // ========== 입력 처리 ==========
    private void Update();
    private void HandleMouseInput();
    private void HandleTouchInput();

    // ========== 선택 로직 ==========
    private void SelectPiece(Vector2Int gridPos);
    private void TrySwap(Vector2Int gridPos);
    private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2);

    // ========== 상태 관리 ==========
    public void SetProcessing(bool processing);
    public void ClearSelection();
}
```

### 6. ComboSystem (콤보 관리)

**책임**:
- 콤보 카운터 관리
- 콤보 배율 계산
- 콤보 UI 이벤트

**주요 메서드**:
```csharp
public class ComboSystem
{
    // ========== 필드 ==========
    private int _currentCombo;
    private int _maxCombo;
    private float _comboTimer;
    private const float COMBO_TIMEOUT = 2f;

    // ========== 이벤트 ==========
    public event Action<int, int> OnComboChanged;  // (currentCombo, multiplier)
    public event Action OnComboReset;

    // ========== 콤보 관리 ==========
    public void IncrementCombo();
    public void ResetCombo();
    public void Update(float deltaTime);

    // ========== 배율 계산 ==========
    public int GetMultiplier();  // 1, 2, 3, 4, 5 (콤보 5 이상은 5배)

    // ========== 프로퍼티 ==========
    public int CurrentCombo => _currentCombo;
    public int MaxCombo => _maxCombo;
}
```

---

## 이벤트 시스템

### 이벤트 흐름도

```
[사용자 입력]
      │
      ▼
[InputController]
      │ OnSwapRequested
      ▼
[ThreeMatchGame] ──────► 유효성 검증
      │
      ▼
[ThreeMatchBoard.SwapPieces()]
      │
      ├─► OnPiecesSwapped 발생
      │       │
      │       ▼
      │   [ThreeMatchBoardView.HandlePiecesSwapped()]
      │       │
      │       └─► 교체 애니메이션 재생
      │
      ├─► FindAllMatches()
      │       │
      │       ├─► OnMatchesFound 발생
      │       │       │
      │       │       ▼
      │       │   [ThreeMatchBoardView.HandleMatchesFound()]
      │       │       │
      │       │       └─► 매치 이펙트 재생
      │       │
      │       └─► OnPiecesDestroyed 발생
      │               │
      │               ▼
      │           [ThreeMatchBoardView.HandlePiecesDestroyed()]
      │               │
      │               └─► 파괴 애니메이션 재생
      │
      └─► ApplyGravity()
              │
              └─► OnPiecesFalling 발생
                      │
                      ▼
                  [ThreeMatchBoardView.HandlePiecesFalling()]
                      │
                      └─► 낙하 애니메이션 재생
```

### 이벤트 종류 및 파라미터

| 이벤트 이름 | 파라미터 | 발생 시점 | 용도 |
|------------|---------|----------|------|
| `OnPieceChanged` | `(int x, int y, int pieceId)` | 개별 퍼즐 변경 | 셀 업데이트 |
| `OnPiecesSwapped` | `(int x1, int y1, int x2, int y2)` | 퍼즐 교체 | 교체 애니메이션 |
| `OnMatchesFound` | `(List<Match> matches)` | 매치 발견 | 점수 계산, 이펙트 |
| `OnPiecesDestroyed` | `(List<Vector2Int> positions)` | 퍼즐 파괴 | 파괴 애니메이션 |
| `OnPiecesFalling` | `(List<PieceMove> moves)` | 중력 적용 | 낙하 애니메이션 |
| `OnBoardShuffled` | 없음 | Shuffle 실행 | 전체 보드 애니메이션 |
| `OnDeadlockDetected` | `(bool isDeadlocked)` | Deadlock 감지 | 경고 UI 표시 |

---

## 게임 플로우

### 전체 게임 루프

```
1. 게임 시작
   └─► ThreeMatchBoard.GenerateInitialBoard()
       └─► ThreeMatchBoardView에서 초기 보드 시각화

2. 사용자 입력 대기
   └─► InputController가 마우스/터치 입력 감지

3. 퍼즐 교체 요청
   └─► ThreeMatchGame.OnSwapRequested(pos1, pos2)
       │
       ├─► 유효성 검증 (인접 확인)
       │
       └─► ThreeMatchBoard.SwapPieces(x1, y1, x2, y2)
           │
           ├─► OnPiecesSwapped 이벤트 발생
           │   └─► BoardView: 교체 애니메이션
           │
           └─► 매치 확인 루프 시작

4. 매치 확인 루프 (연쇄 매치 처리)
   ┌───────────────────────────────────────┐
   │ FindAllMatches()                       │
   │   │                                    │
   │   ├─ 매치 있음?                        │
   │   │   ├─ Yes                           │
   │   │   │   ├─► OnMatchesFound 발생      │
   │   │   │   │   └─► BoardView: 매치 이펙트
   │   │   │   │                            │
   │   │   │   ├─► DestroyMatches()         │
   │   │   │   │   └─► OnPiecesDestroyed 발생
   │   │   │   │       └─► BoardView: 파괴 애니메이션
   │   │   │   │                            │
   │   │   │   ├─► ComboSystem.IncrementCombo()
   │   │   │   │                            │
   │   │   │   ├─► ApplyGravity()           │
   │   │   │   │   └─► OnPiecesFalling 발생  │
   │   │   │   │       └─► BoardView: 낙하 애니메이션
   │   │   │   │                            │
   │   │   │   ├─► FillEmptyCells()         │
   │   │   │   │   └─► OnPieceChanged 발생   │
   │   │   │   │       └─► BoardView: 새 퍼즐 생성
   │   │   │   │                            │
   │   │   │   └─► 다시 FindAllMatches()로  │ ◄─┐
   │   │   │       (연쇄 매치 확인)          │   │
   │   │   │                                │   │
   │   │   └─ No                            │   │
   │   │       └─► ComboSystem.ResetCombo() │   │
   │   │           └─► 입력 대기 상태로      │   │
   └───┼───────────────────────────────────┘   │
       └─────────────────────────────────────────┘
       (연쇄 매치 루프)

5. Deadlock 체크 (매 턴 후)
   └─► ThreeMatchBoard.IsDeadlocked()
       │
       ├─ Deadlocked?
       │   └─► OnDeadlockDetected(true) 발생
       │       └─► BoardView: 경고 UI 표시
       │       └─► ShuffleBoard()
       │           └─► OnBoardShuffled 발생
       │               └─► BoardView: Shuffle 애니메이션
       │
       └─ Not Deadlocked
           └─► 계속 플레이

6. 게임 종료 조건 체크
   ├─► 시간 종료 (Classic 모드)
   ├─► 이동 횟수 소진 (MovesLimited 모드)
   └─► Shuffle 실패 (3회 연속)
       └─► ThreeMatchGame.OnGameOver()
```

### 교체 애니메이션 시퀀스 (상세)

```
사용자: 퍼즐 A, B 선택
    │
    ▼
InputController.OnSwapRequested(A, B)
    │
    ▼
ThreeMatchGame: 유효성 검증
    │
    ├─► 인접하지 않음 → 무시
    │
    └─► 인접함
        │
        ▼
    ThreeMatchBoard.SwapPieces(A, B)
        │
        ├─► 데이터: A ↔ B 교체
        │
        ├─► OnPiecesSwapped(A, B) 발생
        │       │
        │       ▼
        │   BoardView.HandlePiecesSwapped()
        │       │
        │       └─► SwapAnimation 코루틴 시작
        │           (0.3초 애니메이션)
        │
        └─► 매치 확인
            │
            ├─► 매치 없음
            │   │
            │   └─► 원위치 SwapPieces(B, A)
            │       └─► OnPiecesSwapped(B, A) 발생
            │           └─► BoardView: 되돌리기 애니메이션
            │
            └─► 매치 있음
                └─► 매치 처리 루프 진입
```

---

## 클래스 다이어그램

### 데이터 레이어

```
┌─────────────────────────────────────┐
│      ThreeMatchBoard                │
│  (순수 C# 클래스, 데이터 모델)        │
├─────────────────────────────────────┤
│ - _board: int[,]                    │
│ - _width: int                       │
│ - _height: int                      │
│ - _pieceTypeCount: int              │
├─────────────────────────────────────┤
│ + OnPieceChanged: Event             │
│ + OnPiecesSwapped: Event            │
│ + OnMatchesFound: Event             │
│ + OnPiecesDestroyed: Event          │
│ + OnPiecesFalling: Event            │
│ + OnBoardShuffled: Event            │
│ + OnDeadlockDetected: Event         │
├─────────────────────────────────────┤
│ + Initialize()                      │
│ + GenerateInitialBoard()            │
│ + SwapPieces()                      │
│ + FindAllMatches()                  │
│ + DestroyMatches()                  │
│ + ApplyGravity()                    │
│ + FillEmptyCells()                  │
│ + IsDeadlocked()                    │
│ + ShuffleBoard()                    │
└─────────────────────────────────────┘
            │
            │ uses
            ▼
┌─────────────────────────────────────┐
│      MatchDetector                  │
│  (정적 유틸리티 클래스)              │
├─────────────────────────────────────┤
│ + FindAllMatches()                  │
│ + FindMatchAt()                     │
│ + CalculateScore()                  │
└─────────────────────────────────────┘
```

### View 레이어

```
┌─────────────────────────────────────┐
│   ThreeMatchBoardView               │
│  (MonoBehaviour, UI 관리)            │
├─────────────────────────────────────┤
│ - _pieceViews: PuzzlePiece[,]       │
│ - _boardData: ThreeMatchBoard       │
│ - _isAnimating: bool                │
├─────────────────────────────────────┤
│ + Initialize()                      │
│ - HandlePieceChanged()              │
│ - HandlePiecesSwapped()             │
│ - HandleMatchesFound()              │
│ - HandlePiecesDestroyed()           │
│ - HandlePiecesFalling()             │
│ - HandleBoardShuffled()             │
├─────────────────────────────────────┤
│ - SwapAnimation()                   │
│ - DestroyAnimation()                │
│ - FallAnimation()                   │
│ - ShuffleAnimation()                │
└─────────────────────────────────────┘
            │
            │ manages
            ▼
┌─────────────────────────────────────┐
│      PuzzlePiece                    │
│  (MonoBehaviour, IPoolable)          │
├─────────────────────────────────────┤
│ - _pieceId: int                     │
│ - _gridPosition: Vector2Int         │
│ - _spriteRenderer: SpriteRenderer   │
├─────────────────────────────────────┤
│ + SetPieceType()                    │
│ + MoveToPosition()                  │
│ + PlayMatchEffect()                 │
│ + OnSpawnFromPool()                 │
│ + OnReturnToPool()                  │
└─────────────────────────────────────┘
```

### 게임 로직 통합

```
┌─────────────────────────────────────┐
│      ThreeMatchGame                 │
│  (IMiniGame 구현)                    │
├─────────────────────────────────────┤
│ - _board: ThreeMatchBoard           │
│ - _boardView: ThreeMatchBoardView   │
│ - _inputController: InputController │
│ - _comboSystem: ComboSystem         │
│ - _gameData: ThreeMatchGameData     │
├─────────────────────────────────────┤
│ + Initialize()                      │
│ + StartGame()                       │
│ + Update()                          │
│ + Cleanup()                         │
│ + GetData()                         │
├─────────────────────────────────────┤
│ - OnSwapRequested()                 │
│ - ProcessMatches()                  │
│ - CheckGameOver()                   │
└─────────────────────────────────────┘
```

---

## 참고 구현

### Sudoku 게임에서 검증된 패턴

#### 1. 데이터-View 분리

**Sudoku 구현**:
```csharp
// 데이터 (SudokuBoard.cs)
public class SudokuBoard
{
    private int[,] _board = new int[9, 9];

    public void SetNumber(int row, int col, int number)
    {
        _board[row, col] = number;
        // 이벤트 발생하지 않음 - SudokuGame이 관리
    }
}

// View (SudokuGridUI.cs)
public class SudokuGridUI : MonoBehaviour
{
    public void UpdateCell(int row, int col, int number)
    {
        var cell = _cells[row, col];
        cell.SetNumber(number);
    }
}

// Game (SudokuGame.cs)
public class SudokuGame : IMiniGame
{
    public void SetNumber(int row, int col, int number)
    {
        _board.SetNumber(row, col, number);  // 데이터 변경
        _gridUI.UpdateCell(row, col, number); // View 업데이트 (후처리)
    }
}
```

**3-Match 적용**:
```csharp
// 데이터 (ThreeMatchBoard.cs)
public class ThreeMatchBoard
{
    private int[,] _board;
    public event Action<int, int, int> OnPieceChanged;

    public void SetPieceAt(int x, int y, int pieceId)
    {
        _board[x, y] = pieceId;
        OnPieceChanged?.Invoke(x, y, pieceId);  // 이벤트 발생
    }
}

// View (ThreeMatchBoardView.cs)
public class ThreeMatchBoardView : MonoBehaviour
{
    private void Initialize(ThreeMatchBoard board)
    {
        board.OnPieceChanged += HandlePieceChanged;  // 이벤트 구독
    }

    private void HandlePieceChanged(int x, int y, int pieceId)
    {
        var piece = _pieceViews[x, y];
        piece.SetPieceType(pieceId);  // View만 업데이트
    }
}
```

#### 2. 이벤트 기반 아키텍처

**Sudoku의 Activity Action 패턴**:
```csharp
public class SudokuGame
{
    public Action StartMenuActivityAction;
    public Action PlayingActivityAction;

    private void ChangeState(GameState newState)
    {
        _currentState = newState;
        switch (newState)
        {
            case GameState.Playing:
                PlayingActivityAction?.Invoke();  // UI 업데이트 트리거
                break;
        }
    }
}

public class SudokuScene
{
    private void SubscribeUIEvents()
    {
        var game = MiniGameManager.Instance.GetCurrentGame() as SudokuGame;
        game.PlayingActivityAction = () => _uiPanel.ShowPlayingPanel();
    }
}
```

**3-Match의 이벤트 시스템**:
```csharp
public class ThreeMatchBoard
{
    public event Action<List<Match>> OnMatchesFound;

    public List<Match> FindAllMatches()
    {
        var matches = MatchDetector.FindAllMatches(_board, _width, _height);
        if (matches.Count > 0)
        {
            OnMatchesFound?.Invoke(matches);  // UI 업데이트 트리거
        }
        return matches;
    }
}

public class ThreeMatchBoardView
{
    private void Initialize(ThreeMatchBoard board)
    {
        board.OnMatchesFound += HandleMatchesFound;
    }

    private void HandleMatchesFound(List<Match> matches)
    {
        foreach (var match in matches)
        {
            PlayMatchEffect(match);  // UI 시각 효과
        }
    }
}
```

#### 3. 테스트 가능성

**Sudoku 테스트 예시**:
```csharp
[Test]
public void SudokuBoard_SetNumber_UpdatesBoard()
{
    // UI 없이 순수 로직 테스트
    var board = new SudokuBoard();
    board.Initialize();
    board.SetNumber(0, 0, 5);

    Assert.AreEqual(5, board.GetNumber(0, 0));
}
```

**3-Match 테스트 설계**:
```csharp
[Test]
public void ThreeMatchBoard_SwapPieces_CreatesMatch()
{
    // UI 없이 순수 로직 테스트
    var board = new ThreeMatchBoard(null);
    board.Initialize(7, 7, 6);

    // 수동으로 보드 설정 (테스트용)
    board.SetPieceAt(0, 0, 1);
    board.SetPieceAt(1, 0, 1);
    board.SetPieceAt(2, 0, 2);
    board.SetPieceAt(3, 0, 1);

    // 교체
    board.SwapPieces(2, 0, 3, 0);

    // 매치 확인
    var matches = board.FindAllMatches();
    Assert.AreEqual(1, matches.Count);
    Assert.AreEqual(MatchType.Basic3, matches[0].Type);
}
```

---

## 구현 우선순위

### Phase 2-A: 데이터 레이어 (Week 1)

1. **ThreeMatchBoard** (데이터 모델)
   - 보드 초기화 및 생성
   - 퍼즐 교체 로직
   - 이벤트 시스템 구현

2. **MatchDetector** (매치 감지)
   - 가로/세로 매치 찾기
   - 크로스 매치 감지
   - 점수 계산

3. **단위 테스트**
   - 보드 생성 테스트
   - 매치 감지 정확도 테스트
   - Deadlock 감지 테스트

### Phase 2-B: View 레이어 (Week 2)

4. **PuzzlePiece** (개별 퍼즐 View)
   - 스프라이트 설정
   - 기본 애니메이션
   - 풀링 구현

5. **ThreeMatchBoardView** (보드 View)
   - PuzzlePiece 인스턴스화
   - 이벤트 구독
   - 기본 애니메이션 (교체, 파괴, 낙하)

6. **InputController** (입력 처리)
   - 마우스/터치 입력
   - 퍼즐 선택 및 교체 요청

### Phase 2-C: 게임 로직 통합 (Week 3)

7. **ComboSystem** (콤보 관리)
   - 콤보 카운터
   - 배율 계산

8. **ThreeMatchGame** (게임 통합)
   - IMiniGame 구현
   - 매치 처리 루프
   - Deadlock 관리

---

## 체크리스트

### 데이터-View 분리 검증

- [ ] ThreeMatchBoard는 MonoBehaviour를 상속하지 않음
- [ ] ThreeMatchBoard는 Unity 관련 타입 사용하지 않음 (Vector3, Transform 등)
- [ ] ThreeMatchBoardView는 데이터를 직접 수정하지 않음
- [ ] 모든 데이터 변경은 이벤트를 통해 View에 전달됨
- [ ] UI 없이 게임 로직 단위 테스트 가능

### 이벤트 시스템 검증

- [ ] 모든 중요 데이터 변경에 대한 이벤트 정의됨
- [ ] 이벤트 구독 시점 명확 (Initialize)
- [ ] 이벤트 해제 시점 명확 (Cleanup, OnDestroy)
- [ ] 이벤트 파라미터가 충분한 정보 제공
- [ ] 순환 참조 없음

### 아키텍처 일관성

- [ ] Sudoku 패턴과 일관성 유지
- [ ] 네이밍 컨벤션 준수
- [ ] 폴더 구조 일관성
- [ ] 주석 및 문서화 완료

---

**문서 버전**: 1.0
**마지막 업데이트**: 2025-11-27
**다음 리뷰**: Phase 2 구현 완료 후
