# 3-Match Puzzle Game PRD (ClaudeCode-Game-Create 통합)

## Part 1. 제품 요구사항 정의서 (PRD)

### 1. 개요 (Overview)

#### 1.1 기본 정보
* **게임 ID**: `ThreeMatch`
* **장르**: 3-Match Puzzle
* **플랫폼**: PC (Unity 빌드)
* **Unity 버전**: Unity 6 (6000.0.58f2)
* **렌더 파이프라인**: Universal Render Pipeline (URP) 2D
* **목표**: 규칙에 따라 퍼즐을 매치하여 점수를 획득하고, 교착 상태(Deadlock) 없이 게임을 지속하거나 종료 조건에 도달

#### 1.2 아키텍처 통합
이 게임은 **IMiniGame 인터페이스** 기반의 플러그인 아키텍처를 따릅니다:

```
ThreeMatchDataProvider (IGameDataProvider)
    ↓ 게임 데이터 제공 (난이도, 퍼즐 타입)
ThreeMatchGame (IMiniGame)
    ↓ 게임 로직 실행 (보드, 매치, 콤보)
ThreeMatchScene (Scene Controller)
    ↓ UI 이벤트 연결
ThreeMatchUIPanel (UIPanel)
    ↓ UI 렌더링 (4-상태 패널)
```

### 2. 게임 상태 (Game States)

#### 2.1 상태 정의
```csharp
public enum GameState
{
    StartMenu,  // 시작 메뉴: 난이도/모드 선택
    Playing,    // 플레이 중: 게임 진행
    Paused,     // 일시정지
    GameOver    // 게임 종료: 결과 표시
}
```

#### 2.2 상태 전환 규칙
1. **StartMenu → Playing**: [게임 시작] 버튼 클릭 시 (난이도/모드 선택 후)
2. **Playing → Paused**: [일시정지] 버튼 클릭 시
3. **Paused → Playing**: [재개] 버튼 클릭 시
4. **Playing → GameOver**: 목표 달성, 시간 종료, 또는 이동 횟수 소진 시
5. **GameOver → StartMenu**: [재시작] 버튼 클릭 시

#### 2.3 상태별 UI 표시
| 상태 | UI 요소 |
|------|---------|
| StartMenu | [난이도 선택], [게임 모드 선택], [게임 시작], [메인으로] |
| Playing | 게임 보드, 점수, 목표, 남은 시간/이동, 콤보, [일시정지] |
| Paused | 일시정지 오버레이, [재개], [재시작], [메인으로] |
| GameOver | 최종 점수, 목표 달성 여부, [재시작], [메인으로] |

### 3. 핵심 규칙 (Core Rules)

#### 3.1 매치 판정 (Match Logic)
* **매치 조건**: 가로(행) 또는 세로(열)로 동일한 모양의 퍼즐이 **3개 이상** 연속될 경우
* **크로스 매치**: 한 퍼즐을 기준으로 행과 열이 동시에 매치 조건을 만족하는 경우
* **L자/T자 매치**: 교차점에서 발생하는 복합 매치 패턴

#### 3.2 점수 계산 (Scoring)

매치된 형태에 따라 아래 우선순위(내림차순)로 점수를 부여합니다.

| 우선순위 | 매치 형태 | 점수 | 비고 |
| :--- | :--- | :--- | :--- |
| 1 | **5개 + 3개** (행+열) | **2,000** | 복합 매치 최상위 |
| 2 | **4개 + 3개** (행+열) | **1,500** | |
| 3 | **3개 + 3개** (행+열) | **1,000** | 크로스 매치 기본 |
| 4 | **5개** (일렬) | **1,000** | |
| 5 | **4개** (일렬) | **500** | |
| 6 | **3개** (일렬) | **100** | 기본 점수 |

#### 3.3 콤보 시스템 (Combo System)
* 플레이어 조작 후 슬라이딩(빈칸 채우기)으로 발생한 추가 매치는 '콤보'로 정의
* **계산식**: `최종 점수 = 매치 점수 × 현재 콤보 수`
* **예시**: 3매치(100점)가 5콤보째 터질 경우: 100 × 5 = 500점 획득
* **콤보 UI**: 콤보 카운터 표시 및 이펙트

#### 3.4 물리 및 예외 처리
* **슬라이딩**: 퍼즐 파괴 시 상단의 퍼즐이 아래로 이동. 이동 중에는 매치 판정 중단
* **교착 상태(Deadlock)**: 더 이상 매치를 만들 수 없는 상태 감지 시, 보드를 **재생성(Shuffle)**
* **유효하지 않은 교체**: 매치가 생성되지 않는 교체는 원위치로 되돌림 (애니메이션 + SFX)

### 4. 게임 모드 (Game Modes)

#### 4.1 클래식 모드 (Classic)
* **목표**: 제한 시간 내 목표 점수 달성
* **제한**: 시간 제한 (60초, 90초, 120초 - 난이도별)
* **종료 조건**: 시간 종료 (목표 미달 시 실패) 또는 목표 점수 달성 (성공)

#### 4.2 이동 횟수 모드 (Moves Limited)
* **목표**: 제한된 이동 횟수 내 목표 점수 달성
* **제한**: 이동 횟수 (20회, 30회, 40회 - 난이도별)
* **종료 조건**: 이동 횟수 소진 (목표 미달 시 실패) 또는 목표 점수 달성 (성공)

#### 4.3 무한 모드 (Endless)
* **목표**: 최고 점수 갱신
* **제한**: 없음
* **종료 조건**: Deadlock 발생 시 게임 오버 (Shuffle 재시도 3회 실패)

### 5. 난이도 설정 (Difficulty)

| 난이도 | 보드 크기 | 퍼즐 종류 | 목표 점수 (클래식) | 제한 시간 | 제한 이동 |
|--------|-----------|-----------|-------------------|-----------|-----------|
| Easy | 6×6 | 5종류 | 1,000 | 90초 | 30회 |
| Normal | 7×7 | 6종류 | 2,000 | 60초 | 25회 |
| Hard | 8×8 | 7종류 | 3,500 | 60초 | 20회 |

---

## Part 2. 아키텍처 설계 (Architecture Design)

### 1. 클래스 구조

```
Assets/Scripts/ThreeMatch/
├─ Data/
│  ├─ ThreeMatchDataProvider.cs    (IGameDataProvider)
│  ├─ PieceTypeData.cs             (ScriptableObject)
│  ├─ DifficultyConfig.cs          (ScriptableObject)
│  └─ GameModeConfig.cs            (ScriptableObject)
├─ ThreeMatchGame.cs               (IMiniGame)
├─ ThreeMatchGameData.cs           (IGameData)
├─ Board/
│  ├─ BoardManager.cs              # 보드 생성, Deadlock 감지, Shuffle
│  ├─ MatchDetector.cs             # 매치 감지 및 점수 계산
│  └─ PuzzlePiece.cs               # 개별 퍼즐 (IPoolable)
├─ Input/
│  └─ InputController.cs           # 퍼즐 선택 및 교체
├─ Systems/
│  └─ ComboSystem.cs               # 콤보 추적 및 점수 배율
├─ Scenes/
│  └─ ThreeMatchScene.cs           # 씬 컨트롤러
└─ UI/
   └─ ThreeMatchUIPanel.cs         # 4-상태 UI 패널
```

### 2. 핵심 컴포넌트

#### 2.1 ThreeMatchDataProvider (게임 데이터 제공자)
```csharp
public class ThreeMatchDataProvider : IGameDataProvider
{
    public string GameID => "ThreeMatch";
    public bool IsLoaded { get; private set; }

    private Dictionary<int, PieceTypeData> _pieceTypes;
    private Dictionary<DifficultyLevel, DifficultyConfig> _difficultyConfigs;
    private Dictionary<GameMode, GameModeConfig> _gameModeConfigs;

    public void Initialize()
    {
        _pieceTypes = new Dictionary<int, PieceTypeData>();
        _difficultyConfigs = new Dictionary<DifficultyLevel, DifficultyConfig>();
        _gameModeConfigs = new Dictionary<GameMode, GameModeConfig>();
        IsLoaded = false;
    }

    public void LoadData()
    {
        // ScriptableObject에서 데이터 로드
        var pieceList = Resources.Load<PieceTypeDataList>("Data/ThreeMatch/ScriptableObjects/PieceTypeDataList");
        var difficultyList = Resources.Load<DifficultyConfigList>("Data/ThreeMatch/ScriptableObjects/DifficultyConfigList");
        var modeList = Resources.Load<GameModeConfigList>("Data/ThreeMatch/ScriptableObjects/GameModeConfigList");

        foreach (var piece in pieceList.PieceTypes)
            _pieceTypes.Add(piece.PieceId, piece);

        foreach (var config in difficultyList.Configs)
            _difficultyConfigs.Add(config.Level, config);

        foreach (var mode in modeList.Modes)
            _gameModeConfigs.Add(mode.Mode, mode);

        IsLoaded = true;
    }

    public void UnloadData()
    {
        _pieceTypes.Clear();
        _difficultyConfigs.Clear();
        _gameModeConfigs.Clear();
        IsLoaded = false;
    }

    public PieceTypeData GetPieceTypeData(int pieceId)
        => _pieceTypes.TryGetValue(pieceId, out var data) ? data : null;

    public DifficultyConfig GetDifficultyConfig(DifficultyLevel level)
        => _difficultyConfigs.TryGetValue(level, out var config) ? config : null;

    public GameModeConfig GetGameModeConfig(GameMode mode)
        => _gameModeConfigs.TryGetValue(mode, out var config) ? config : null;
}
```

#### 2.2 ThreeMatchGameData (런타임 게임 데이터)
```csharp
public class ThreeMatchGameData : IGameData
{
    public int Score { get; set; }
    public int TargetScore { get; set; }
    public int CurrentCombo { get; set; }
    public int MaxCombo { get; set; }
    public int RemainingMoves { get; set; }
    public float ElapsedTime { get; set; }
    public DifficultyLevel CurrentDifficulty { get; set; }
    public GameMode CurrentMode { get; set; }

    public void Initialize()
    {
        Score = 0;
        TargetScore = 0;
        CurrentCombo = 0;
        MaxCombo = 0;
        RemainingMoves = 0;
        ElapsedTime = 0f;
        CurrentDifficulty = DifficultyLevel.Normal;
        CurrentMode = GameMode.Classic;
    }

    public void Reset() => Initialize();

    public bool Validate()
    {
        return Score >= 0 && TargetScore > 0 && ElapsedTime >= 0f;
    }

    public void SaveState()
    {
        PlayerPrefs.SetInt("ThreeMatch_HighScore", Mathf.Max(Score, PlayerPrefs.GetInt("ThreeMatch_HighScore", 0)));
        PlayerPrefs.SetInt("ThreeMatch_MaxCombo", Mathf.Max(MaxCombo, PlayerPrefs.GetInt("ThreeMatch_MaxCombo", 0)));
        PlayerPrefs.Save();
    }

    public void LoadState()
    {
        // 하이스코어는 별도 로드 (게임 시작 시 표시)
    }
}
```

#### 2.3 BoardManager (보드 관리)
```csharp
public class BoardManager
{
    private int[,] _board;  // 0: 빈칸, 1~N: 퍼즐 ID
    private int _width;
    private int _height;
    private int _pieceTypeCount;

    public int Width => _width;
    public int Height => _height;
    public int[,] Board => _board;

    public void Initialize(int width, int height, int pieceTypeCount)
    {
        _width = width;
        _height = height;
        _pieceTypeCount = pieceTypeCount;
        _board = new int[width, height];

        GenerateInitialBoard();
    }

    // 초기 생성 시 매치 방지
    private void GenerateInitialBoard()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _board[x, y] = GetNoMatchRandomPiece(x, y);
            }
        }

        // Deadlock 확인 및 해결
        if (IsDeadlocked())
        {
            Debug.Log("[ThreeMatch] Initial board deadlocked, shuffling...");
            ShuffleBoard();
        }
    }

    // 생성 시 가로/세로 2개 연속되면 해당 타입 제외
    private int GetNoMatchRandomPiece(int x, int y)
    {
        List<int> possibleTypes = new List<int>();
        for (int i = 1; i <= _pieceTypeCount; i++)
            possibleTypes.Add(i);

        // 가로 2개 연속 체크
        if (x >= 2 && _board[x - 1, y] == _board[x - 2, y])
            possibleTypes.Remove(_board[x - 1, y]);

        // 세로 2개 연속 체크
        if (y >= 2 && _board[x, y - 1] == _board[x, y - 2])
            possibleTypes.Remove(_board[x, y - 1]);

        return possibleTypes[Random.Range(0, possibleTypes.Count)];
    }

    // Deadlock 감지: 가능한 매치가 하나도 없는지 확인
    public bool IsDeadlocked()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // 오른쪽 교체 시도
                if (x < _width - 1 && TrySwapAndCheckMatch(x, y, x + 1, y))
                    return false;

                // 아래쪽 교체 시도
                if (y < _height - 1 && TrySwapAndCheckMatch(x, y, x, y + 1))
                    return false;
            }
        }
        return true;
    }

    // 가상 스왑 후 매치 여부 확인
    private bool TrySwapAndCheckMatch(int x1, int y1, int x2, int y2)
    {
        // 스왑
        int temp = _board[x1, y1];
        _board[x1, y1] = _board[x2, y2];
        _board[x2, y2] = temp;

        // 매치 확인
        bool hasMatch = HasMatchAt(x1, y1) || HasMatchAt(x2, y2);

        // 되돌리기
        temp = _board[x1, y1];
        _board[x1, y1] = _board[x2, y2];
        _board[x2, y2] = temp;

        return hasMatch;
    }

    // 특정 위치에서 매치 여부 확인
    private bool HasMatchAt(int x, int y)
    {
        int type = _board[x, y];
        if (type == 0) return false;

        // 가로 체크
        int hCount = 1;
        for (int i = x - 1; i >= 0 && _board[i, y] == type; i--) hCount++;
        for (int i = x + 1; i < _width && _board[i, y] == type; i++) hCount++;
        if (hCount >= 3) return true;

        // 세로 체크
        int vCount = 1;
        for (int i = y - 1; i >= 0 && _board[x, i] == type; i--) vCount++;
        for (int i = y + 1; i < _height && _board[x, i] == type; i++) vCount++;
        if (vCount >= 3) return true;

        return false;
    }

    // 보드 재생성 (Shuffle)
    public void ShuffleBoard()
    {
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _board[x, y] = GetNoMatchRandomPiece(x, y);
                }
            }
            attempts++;
        } while (IsDeadlocked() && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogError("[ThreeMatch] Failed to generate valid board after max attempts!");
        }
        else
        {
            Debug.Log($"[ThreeMatch] Board shuffled successfully (attempts: {attempts})");
        }
    }

    // 실제 스왑 (유효성 검증 후 호출)
    public void SwapPieces(int x1, int y1, int x2, int y2)
    {
        int temp = _board[x1, y1];
        _board[x1, y1] = _board[x2, y2];
        _board[x2, y2] = temp;
    }

    // 특정 위치의 퍼즐 타입 조회
    public int GetPieceType(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return 0;
        return _board[x, y];
    }

    // 특정 위치의 퍼즐 타입 설정
    public void SetPieceType(int x, int y, int type)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return;
        _board[x, y] = type;
    }
}
```

#### 2.4 MatchDetector (매치 감지 및 점수 계산)
```csharp
public enum MatchType
{
    None,
    Basic3,     // 3개 일렬
    Basic4,     // 4개 일렬
    Basic5,     // 5개 일렬
    Cross33,    // 3+3 크로스
    Cross43,    // 4+3 크로스
    Cross53     // 5+3 크로스
}

public struct MatchResult
{
    public List<Vector2Int> MatchedCells;
    public MatchType Type;
    public int BaseScore;

    public MatchResult(List<Vector2Int> cells, MatchType type, int score)
    {
        MatchedCells = cells;
        Type = type;
        BaseScore = score;
    }
}

public class MatchDetector
{
    private int[,] _board;
    private int _width;
    private int _height;

    public MatchDetector(int[,] board, int width, int height)
    {
        _board = board;
        _width = width;
        _height = height;
    }

    // 모든 매치 찾기
    public List<MatchResult> FindAllMatches()
    {
        List<MatchResult> results = new List<MatchResult>();
        bool[,] visited = new bool[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (visited[x, y] || _board[x, y] == 0)
                    continue;

                var match = FindMatchAt(x, y, visited);
                if (match.MatchedCells.Count >= 3)
                {
                    results.Add(match);

                    // 매치된 셀 방문 표시
                    foreach (var cell in match.MatchedCells)
                        visited[cell.x, cell.y] = true;
                }
            }
        }

        return results;
    }

    // 특정 위치에서 매치 찾기
    private MatchResult FindMatchAt(int x, int y, bool[,] visited)
    {
        int type = _board[x, y];
        List<Vector2Int> hMatches = new List<Vector2Int>();
        List<Vector2Int> vMatches = new List<Vector2Int>();

        // 가로 매치 찾기
        hMatches.Add(new Vector2Int(x, y));
        for (int i = x - 1; i >= 0 && _board[i, y] == type && !visited[i, y]; i--)
            hMatches.Add(new Vector2Int(i, y));
        for (int i = x + 1; i < _width && _board[i, y] == type && !visited[i, y]; i++)
            hMatches.Add(new Vector2Int(i, y));

        // 세로 매치 찾기
        vMatches.Add(new Vector2Int(x, y));
        for (int i = y - 1; i >= 0 && _board[x, i] == type && !visited[x, i]; i--)
            vMatches.Add(new Vector2Int(x, i));
        for (int i = y + 1; i < _height && _board[x, i] == type && !visited[x, i]; i++)
            vMatches.Add(new Vector2Int(x, i));

        int hCount = hMatches.Count;
        int vCount = vMatches.Count;

        // 매치 타입 및 점수 결정
        MatchType matchType = MatchType.None;
        int baseScore = 0;
        List<Vector2Int> finalCells = new List<Vector2Int>();

        // 크로스 매치 (가로 + 세로 모두 3개 이상)
        if (hCount >= 3 && vCount >= 3)
        {
            // 중복 제거 (교차점)
            finalCells = new List<Vector2Int>(hMatches);
            foreach (var cell in vMatches)
            {
                if (!finalCells.Contains(cell))
                    finalCells.Add(cell);
            }

            if (hCount >= 5 || vCount >= 5)
            {
                matchType = MatchType.Cross53;
                baseScore = 2000;
            }
            else if (hCount >= 4 || vCount >= 4)
            {
                matchType = MatchType.Cross43;
                baseScore = 1500;
            }
            else
            {
                matchType = MatchType.Cross33;
                baseScore = 1000;
            }
        }
        // 가로 또는 세로 단일 매치
        else if (hCount >= 3 || vCount >= 3)
        {
            finalCells = hCount >= 3 ? hMatches : vMatches;
            int count = Mathf.Max(hCount, vCount);

            if (count >= 5)
            {
                matchType = MatchType.Basic5;
                baseScore = 1000;
            }
            else if (count >= 4)
            {
                matchType = MatchType.Basic4;
                baseScore = 500;
            }
            else
            {
                matchType = MatchType.Basic3;
                baseScore = 100;
            }
        }

        return new MatchResult(finalCells, matchType, baseScore);
    }

    // 점수 계산 (콤보 배율 적용)
    public int CalculateScore(MatchResult match, int comboMultiplier)
    {
        return match.BaseScore * Mathf.Max(1, comboMultiplier);
    }
}
```

#### 2.5 PuzzlePiece (개별 퍼즐)
```csharp
using UnityEngine;
using System.Collections;

public class PuzzlePiece : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public int xIndex;
    public int yIndex;
    public int pieceType;
    public bool isMoving { get; private set; }

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void SetPieceType(int type, Sprite sprite)
    {
        pieceType = type;
        _spriteRenderer.sprite = sprite;
    }

    public void MoveToPosition(Vector3 targetPos, float duration = 0.2f)
    {
        StartCoroutine(MoveRoutine(targetPos, duration));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    // IPoolable 구현
    public void OnSpawnedFromPool()
    {
        isMoving = false;
        gameObject.SetActive(true);
    }

    public void OnReturnedToPool()
    {
        isMoving = false;
        gameObject.SetActive(false);
    }
}
```

#### 2.6 InputController (입력 처리)
```csharp
using UnityEngine;
using System;
using System.Collections;

public class InputController
{
    public event Action<int, int, int, int> OnSwapRequested;  // x1, y1, x2, y2

    private PuzzlePiece _selectedPiece;
    private bool _isProcessing;
    private LayerMask _puzzleLayer;
    private Camera _mainCamera;

    public bool IsProcessing => _isProcessing;

    public void Initialize(LayerMask puzzleLayer)
    {
        _puzzleLayer = puzzleLayer;
        _mainCamera = Camera.main;
        _selectedPiece = null;
        _isProcessing = false;
    }

    public void ProcessInput()
    {
        if (_isProcessing) return;

        if (Input.GetMouseButtonDown(0))
        {
            SelectPiece();
        }
    }

    private void SelectPiece()
    {
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, _puzzleLayer);

        if (hit.collider != null)
        {
            PuzzlePiece clickedPiece = hit.collider.GetComponent<PuzzlePiece>();

            if (_selectedPiece == null)
            {
                // 첫 번째 퍼즐 선택
                _selectedPiece = clickedPiece;
                // TODO: 선택 표시 (하이라이트 등)
            }
            else
            {
                // 두 번째 퍼즐 선택 → 교체 시도
                if (IsAdjacent(_selectedPiece, clickedPiece))
                {
                    OnSwapRequested?.Invoke(
                        _selectedPiece.xIndex, _selectedPiece.yIndex,
                        clickedPiece.xIndex, clickedPiece.yIndex
                    );
                    _selectedPiece = null;
                }
                else
                {
                    // 인접하지 않으면 새로 선택
                    _selectedPiece = clickedPiece;
                }
            }
        }
    }

    private bool IsAdjacent(PuzzlePiece p1, PuzzlePiece p2)
    {
        return Mathf.Abs(p1.xIndex - p2.xIndex) + Mathf.Abs(p1.yIndex - p2.yIndex) == 1;
    }

    public void SetProcessing(bool processing)
    {
        _isProcessing = processing;
    }

    public void ClearSelection()
    {
        _selectedPiece = null;
    }
}
```

---

## Part 3. Manager 시스템 활용 (Manager Integration)

### 1. ResourceManager 활용
```csharp
// 퍼즐 프리팹 풀 생성
ResourceManager.Instance.CreatePool("Prefabs/ThreeMatch/Piece", 64, 200, true, () => {
    Debug.Log("[ThreeMatch] Puzzle pool created");
});

// 퍼즐 스프라이트 로드
ResourceManager.Instance.LoadAsync<Sprite>("Sprites/ThreeMatch/Piece_Red", (sprite) => {
    // 스프라이트 캐싱 또는 즉시 사용
});

// 퍼즐 인스턴스 생성 (풀링)
ResourceManager.Instance.InstantiateAsync("Prefabs/ThreeMatch/Piece", boardParent, (instance) => {
    var piece = instance.GetComponent<PuzzlePiece>();
    piece.SetCoord(x, y);
    piece.transform.localPosition = new Vector3(x, y, 0);
});

// 퍼즐 반환 (매치 후)
ResourceManager.Instance.ReleaseInstance(pieceObject);
```

### 2. SoundManager 활용
```csharp
// BGM 재생
SoundManager.Instance.PlayBGM("Audio/BGM/ThreeMatch/Theme", 1.5f);

// SFX 재생
SoundManager.Instance.PlaySFX("Audio/SFX/ThreeMatch/Match3", 0.8f);
SoundManager.Instance.PlaySFX("Audio/SFX/ThreeMatch/Combo", 1.0f);
SoundManager.Instance.PlaySFX("Audio/SFX/ThreeMatch/InvalidMove", 0.7f);
```

### 3. UIManager 활용
```csharp
// UI 패널 열기
UIManager.Instance.OpenPanel<ThreeMatchUIPanel>("UI/ThreeMatchUIPanel", (panel) => {
    panel.Initialize(_gameData);
});

// 일시정지 확인 팝업
UIManager.Instance.ShowConfirmDialog(
    "게임을 종료하시겠습니까?",
    () => CustomSceneManager.Instance.LoadScene("MainMenu"),
    () => Debug.Log("[ThreeMatch] Continue playing")
);
```

### 4. PoolManager 활용
```csharp
// 퍼즐 스폰
var piece = PoolManager.Instance.Spawn("Prefabs/ThreeMatch/Piece", position, Quaternion.identity);

// 퍼즐 반환 (애니메이션 후)
PoolManager.Instance.DespawnAfter("Prefabs/ThreeMatch/Piece", piece, 0.5f);

// 보드 초기화 시 모든 퍼즐 반환
PoolManager.Instance.DespawnAll("Prefabs/ThreeMatch/Piece");
```

### 5. DataManager 활용
```csharp
// 게임 시작 시 데이터 로드
DataManager.Instance.LoadGameData("ThreeMatch");

// 데이터 조회
var provider = DataManager.Instance.GetProvider<ThreeMatchDataProvider>("ThreeMatch");
var difficultyConfig = provider.GetDifficultyConfig(DifficultyLevel.Normal);
var pieceData = provider.GetPieceTypeData(1);

// 게임 종료 시 언로드
DataManager.Instance.UnloadGameData("ThreeMatch");
```

---

## Part 4. 리소스 구조 (Resource Structure)

### 1. Addressables 경로 규칙
```
Resources/
├─ Prefabs/ThreeMatch/
│  ├─ Piece                   # 퍼즐 프리팹 (IPoolable)
│  └─ MatchEffect             # 매치 이펙트
├─ Sprites/ThreeMatch/
│  ├─ Piece_Red
│  ├─ Piece_Blue
│  ├─ Piece_Green
│  ├─ Piece_Yellow
│  ├─ Piece_Purple
│  ├─ Piece_Orange
│  └─ Piece_Pink
├─ Audio/BGM/ThreeMatch/
│  └─ Theme
├─ Audio/SFX/ThreeMatch/
│  ├─ Match3
│  ├─ Match4
│  ├─ Match5
│  ├─ Combo
│  ├─ Swap
│  └─ InvalidMove
├─ Data/ThreeMatch/ScriptableObjects/
│  ├─ PieceTypeDataList
│  ├─ DifficultyConfigList
│  └─ GameModeConfigList
└─ UI/
   └─ ThreeMatchUIPanel
```

### 2. ScriptableObject 데이터 구조

#### PieceTypeData
```csharp
[CreateAssetMenu(fileName = "PieceTypeData", menuName = "ThreeMatch/PieceTypeData")]
public class PieceTypeData : ScriptableObject
{
    public int PieceId;              // 1~7
    public string PieceName;         // "Red", "Blue", etc.
    public Sprite PieceSprite;
    public Color PieceColor;
}
```

#### DifficultyConfig
```csharp
public enum DifficultyLevel { Easy, Normal, Hard }

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "ThreeMatch/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    public DifficultyLevel Level;
    public int BoardWidth;
    public int BoardHeight;
    public int PieceTypeCount;
    public int TargetScore;
    public float TimeLimit;          // 클래식 모드
    public int MovesLimit;           // 이동 모드
}
```

#### GameModeConfig
```csharp
public enum GameMode { Classic, MovesLimited, Endless }

[CreateAssetMenu(fileName = "GameModeConfig", menuName = "ThreeMatch/GameModeConfig")]
public class GameModeConfig : ScriptableObject
{
    public GameMode Mode;
    public string ModeName;
    public string Description;
    public bool HasTimeLimit;
    public bool HasMovesLimit;
}
```

---

## Part 5. UI 구성 (UI Layout)

### 1. ThreeMatchUIPanel (4-상태 패널)
```
StartMenuPanel:
┌─────────────────────────────────────┐
│      3-Match Puzzle Game            │
│                                     │
│  난이도 선택:                        │
│  [Easy]  [Normal]  [Hard]           │
│                                     │
│  게임 모드:                          │
│  [클래식]  [이동 횟수]  [무한]      │
│                                     │
│        [게임 시작]                   │
│        [메인으로]                    │
└─────────────────────────────────────┘

PlayingPanel:
┌─────────────────────────────────────┐
│  점수: 1,250    목표: 2,000         │
│  콤보: ×3       남은: 45s / 20회    │
├─────────────────────────────────────┤
│                                     │
│         [게임 보드 8×8]             │
│                                     │
│                                     │
├─────────────────────────────────────┤
│  [일시정지]           [힌트]        │
└─────────────────────────────────────┘

PausePanel:
┌─────────────────────────────────────┐
│            게임 일시정지             │
│                                     │
│          [재개]                      │
│          [재시작]                    │
│          [메인으로]                  │
└─────────────────────────────────────┘

GameOverPanel:
┌─────────────────────────────────────┐
│          게임 종료!                  │
│                                     │
│      최종 점수: 2,500               │
│      최대 콤보: ×8                  │
│      목표 달성: ✅                   │
│                                     │
│          [재시작]                    │
│          [메인으로]                  │
└─────────────────────────────────────┘
```

---

## Part 6. 구현 단계 (Implementation Phases)

### Phase 1: 데이터 구조 및 Manager 통합 ✅
- [ ] `ThreeMatchDataProvider` 구현
- [ ] `ThreeMatchGameData` 구현
- [ ] ScriptableObject 데이터 생성 (PieceTypeData, DifficultyConfig, GameModeConfig)
- [ ] DataManager에 제공자 등록

### Phase 2: 핵심 게임 로직 ✅
- [ ] `BoardManager` - 보드 생성, Deadlock 감지, Shuffle
- [ ] `MatchDetector` - 매치 감지, 점수 계산
- [ ] `InputController` - 퍼즐 선택 및 교체
- [ ] `PuzzlePiece` - 이동 애니메이션, IPoolable 구현
- [ ] `ComboSystem` - 콤보 추적 및 점수 배율

### Phase 3: ThreeMatchGame (IMiniGame 구현) ✅
- [ ] `ThreeMatchGame` 클래스 구현
- [ ] 게임 상태 관리 (StartMenu, Playing, Paused, GameOver)
- [ ] Activity Action 패턴 적용 (Sudoku 참고)
- [ ] InputManager 이벤트 구독/해제
- [ ] 매치 및 슬라이딩 로직 통합

### Phase 4: Unity 씬 및 UI 통합 ✅
- [ ] `ThreeMatchScene` - 씬 컨트롤러
- [ ] `ThreeMatchUIPanel` - 4-상태 패널
- [ ] UI 이벤트 → 게임 로직 연결
- [ ] Unity 씬 설정 (ThreeMatch.unity)
- [ ] 보드 시각화 및 퍼즐 배치

### Phase 5: 리소스 및 에셋 통합 ✅
- [ ] 퍼즐 스프라이트 에셋 준비 (7종류)
- [ ] Addressables 그룹 설정
- [ ] 오디오 클립 준비 (BGM, SFX)
- [ ] 풀 생성 및 프리로드

### Phase 6: 게임 모드 및 난이도 ✅
- [ ] 클래식 모드 구현 (시간 제한)
- [ ] 이동 횟수 모드 구현
- [ ] 무한 모드 구현
- [ ] 난이도별 설정 적용

### Phase 7: 폴리싱 및 테스트 ✅
- [ ] 매치 이펙트 및 사운드
- [ ] 콤보 UI 이펙트
- [ ] Deadlock 감지 및 자동 Shuffle
- [ ] 슬라이딩 애니메이션 최적화
- [ ] 통합 테스트 및 밸런스 조정

---

## Part 7. 테스트 계획 (Testing Plan)

### 1. 단위 테스트
- [ ] `BoardManager.IsDeadlocked()` - 교착 상태 감지 정확도
- [ ] `MatchDetector.FindAllMatches()` - 매치 감지 정확도
- [ ] `MatchDetector.CalculateScore()` - 점수 계산 검증
- [ ] `BoardManager.GetNoMatchRandomPiece()` - 초기 생성 매치 방지

### 2. 통합 테스트
- [ ] 게임 시작 → 플레이 → 게임 오버 전체 플로우
- [ ] 일시정지 → 재개 → 재시작
- [ ] Deadlock 감지 → Shuffle → 계속 플레이
- [ ] 콤보 시스템 - 연속 매치 점수 배율 적용

### 3. 성능 테스트
- [ ] 60 FPS 유지 (8×8 보드 기준)
- [ ] 메모리 사용량 모니터링
- [ ] GC Alloc 최소화 확인 (풀링 효과)

---

## Part 8. 참고 문서 (References)

- **Manager 시스템**: `Assets/Docs/MANAGERS_GUIDE.md` ⚠️ **필수 읽기**
- **아키텍처 가이드**: `CLAUDE.md` - Multi-Minigame Platform Architecture
- **Sudoku 참고**: `Assets/Scripts/Sudoku/` - Activity Action 패턴 참고
- **Undead Survivor 참고**: `Assets/Docs/UndeadSurvivor_Reference.md` - DataProvider 패턴 참고

---

**작성일**: 2025-11-24
**버전**: 2.0
**상태**: 구현 준비 완료 (ClaudeCode-Game-Create 통합)
