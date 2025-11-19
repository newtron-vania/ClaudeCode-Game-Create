# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ClaudeCode-Game-Create** is a Unity 2D multi-minigame platform with pluggable game architecture using Universal Render Pipeline (URP).

- **Unity Version**: 6000.0.58f2 (Unity 6)
- **Render Pipeline**: Universal Render Pipeline (URP) 17.0.4
- **2D Framework**: Unity Feature 2D 2.0.1
- **Input System**: Unity Input System 1.14.2
- **Resource System**: Unity Addressables 1.22.3
- **Architecture**: IMiniGame interface + GameRegistry pattern + DataManager system
- **Implemented Games**: Tetris (완성), Undead Survivor (보류), Sudoku (개발 중)
- **Game Select Scene**: MainMenuScene (game selection menu)

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/                # Platform architecture
│   │   ├── IMiniGame.cs          # All games implement this interface
│   │   ├── IGameData.cs          # Game-specific data interface
│   │   ├── IGameDataProvider.cs  # Game data provider interface
│   │   ├── GameRegistry.cs       # Game factory registration
│   │   ├── GamePlayList.cs       # Playable games list management
│   │   ├── Singleton.cs          # Generic singleton base class
│   │   └── BaseScene.cs          # Scene base class
│   ├── Managers/           # Infrastructure managers
│   │   ├── MiniGameManager.cs     # Game lifecycle & switching
│   │   ├── DataManager.cs         # Multi-game data management (NEW)
│   │   ├── ResourceManager.cs     # Addressables loading
│   │   ├── PoolManager.cs         # GameObject pooling
│   │   ├── SoundManager.cs        # BGM/SFX audio
│   │   ├── UIManager.cs           # UI panel management
│   │   ├── CustomSceneManager.cs  # Scene transitions
│   │   └── InputManager.cs        # Input event distribution
│   ├── GameData/           # Per-game data implementations
│   │   └── TetrisGameData.cs
│   ├── UndeadSurvivor/     # Undead Survivor game (in progress)
│   │   ├── Data/                        # Game data structures
│   │   │   ├── UndeadSurvivorDataProvider.cs
│   │   │   ├── CharacterData.cs
│   │   │   ├── WeaponData.cs
│   │   │   ├── MonsterData.cs
│   │   │   └── ItemData.cs
│   │   ├── ScriptableObjects/           # Data lists
│   │   │   ├── CharacterDataList.cs
│   │   │   ├── WeaponDataList.cs
│   │   │   ├── MonsterDataList.cs
│   │   │   └── ItemDataList.cs
│   │   └── CharacterStat.cs
│   ├── UI/                 # UI components
│   │   ├── UIPanel.cs      # Base class for all UI panels
│   │   ├── FadePanel.cs    # Screen fade effects
│   │   └── TetrisUIPanel.cs
│   ├── Scenes/             # Scene controllers
│   │   ├── MainMenuScene.cs
│   │   └── TetrisScene.cs
│   └── Tests/              # Test scripts
├── Scenes/
│   ├── MainMenuScene.unity       # Game selection menu (entry point)
│   ├── Tetris.unity              # Tetris game scene
│   └── Undead Survivor.unity     # Undead Survivor scene (in progress)
├── Resources/              # Game-specific resource organization (NEW STRUCTURE)
│   ├── Prefabs/
│   │   ├── UI/
│   │   │   └── UndeadSurvivor/  # Game-specific UI prefabs
│   │   ├── Weapon/
│   │   │   └── UndeadSurvivor/
│   │   ├── Content/
│   │   │   └── UndeadSurvivor/
│   │   ├── Monster/
│   │   │   └── UndeadSurvivor/
│   │   └── Player/
│   │       └── UndeadSurvivor/
│   ├── Sprites/
│   │   └── UndeadSurvivor/
│   ├── Audio/
│   │   ├── BGM/
│   │   │   └── UndeadSurvivor/
│   │   └── SFX/
│   │       └── UndeadSurvivor/
│   ├── Materials/
│   │   └── UndeadSurvivor/
│   ├── Tiles/
│   │   └── UndeadSurvivor/
│   └── Data/               # ScriptableObject data
│       └── UndeadSurvivor/
│           └── ScriptableObjects/
├── Docs/                   # Documentation
│   ├── MANAGERS_GUIDE.md           ⚠️ **READ THIS FIRST EVERY SESSION**
│   ├── UndeadSurvivor_Reference.md # Undead Survivor implementation guide
│   ├── GameSelectUI_Setup_Guide.md
│   ├── Github-Flow.md
│   ├── SETUP_GUIDE.md
│   └── [유니티] 개발 표준 v2.md
├── Settings/               # URP and rendering settings
│   ├── Renderer2D.asset
│   └── UniversalRP.asset
└── InputSystem_Actions.inputactions

.claude/
├── skills/                   # Automated workflows
│   ├── manager-guide.yml         # Manager API quick reference
│   └── pre-commit-check.yml      # Code quality validation
├── UNITY_CONVENTIONS.md      # Unity coding conventions
├── COMMIT_MESSAGE_RULES.md   # Git commit message rules
└── BRANCH_NAMING_RULES.md    # Branch naming conventions
```

## Development Commands

### Unity Editor
```bash
# Open project in Unity Editor
# Use Unity Hub to open: /Users/kimkyeongsoo/Desktop/Unity/ClaudeCode-Game-Create

# Unity 6 (6000.0.58f2) is required
```

### Testing
```bash
# Run tests via Unity Test Runner (Window → General → Test Runner)
# Test scripts are located in Assets/Scripts/Tests/

# Example test files:
# - ManagersTest.cs      # Manager system integration tests
# - ResourceManagerTest.cs   # Resource loading tests
```

### Build Commands
Unity builds are managed through the Unity Editor. No CLI build commands are configured yet.

## Code Architecture

### ⚠️ CRITICAL: Manager System Documentation

**MUST READ AT START OF EVERY SESSION**: Before any code changes, read `Assets/Docs/MANAGERS_GUIDE.md`

This guide contains:
- Complete API reference for all 8 managers
- Usage patterns and best practices
- Integration examples
- Memory management guidelines
- Performance optimization tips

**Why this is critical**:
- Prevents reimplementing existing functionality
- Ensures proper manager usage patterns
- Avoids common integration mistakes
- Maintains code consistency across the project

### DataManager System (Centralized Game Data Management)

**Purpose**: Manages game-specific data providers for all minigames

**Architecture**:
- `DataManager`: Central manager for all game data providers (Singleton)
- `IGameDataProvider`: Interface that all game data providers must implement
- Per-game data providers: `UndeadSurvivorDataProvider`, `TetrisDataProvider`, etc.

**Key Features**:
- **Lazy Loading**: Game data loaded only when game starts, unloaded when game ends
- **Provider Registration**: Each game registers its data provider with unique GameID
- **Memory Management**: Automatic data cleanup on game unload and app quit
- **Type-Safe Access**: Generic GetProvider<T> for type-safe data provider access

**Data Provider Interface**:
```csharp
public interface IGameDataProvider
{
    string GameID { get; }           // Unique game identifier
    bool IsLoaded { get; }           // Data load status
    void Initialize();               // Setup data structures
    void LoadData();                 // Load from ScriptableObject/JSON/etc
    void UnloadData();               // Release memory
    T GetData<T>(string key);        // Query data by key
    bool HasData(string key);        // Check data existence
}
```

**Usage Pattern**:
```csharp
// 1. Register provider (in game initialization or GameRegistry)
var provider = new UndeadSurvivorDataProvider();
DataManager.Instance.RegisterProvider(provider);

// 2. Load game data when game starts
DataManager.Instance.LoadGameData("UndeadSurvivor");

// 3. Access provider in game code
var provider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");
MonsterData monsterData = provider.GetMonsterData(monsterId);

// 4. Unload when game ends
DataManager.Instance.UnloadGameData("UndeadSurvivor");
```

**Benefits**:
- Prevents data memory leaks across game switches
- Supports multiple games with different data structures
- Clean separation between game data and game logic
- Easy to add new games without modifying DataManager

### Game Selection UI System

**Entry Point**: MainMenuScene (MainMenuScene.unity)

The game selection system dynamically generates UI buttons based on available games:

**Components**:
- `MainMenuScene`: Scene controller that loads GameSelectUIPanel
- `GameSelectUIPanel`: Dynamically creates game buttons from GamePlayList
- `GameSelectButton`: Individual game button with icon loading
- `GamePlayList`: Maintains list of available games (Inspector-configurable)

**How it works**:
```
1. MainMenuScene loads GameSelectUIPanel via UIManager
2. GameSelectUIPanel reads playable games from GamePlayList
3. For each game, creates GameSelectButton from "SubItem/GameSelectButton"
4. GameSelectButton loads icon sprite from "Sprite/{GameID}_icon"
5. On click, transitions to game scene via CustomSceneManager
```

**Adding a new game to selection menu**:
1. Implement `IGameDataProvider` for game data
2. Register data provider in `DataManager`
3. Register game in `GameRegistry`
4. Add `GameInfo` to `GamePlayList` in Inspector with gameID matching scene name
5. Place icon sprite at Addressables path: `Sprite/{GameID}_icon`
6. Game will automatically appear in selection menu

**Addressables Path Conventions** (Game-Specific Organization):
```csharp
// NEW: Game-specific resource structure (Type → Game)
"Prefabs/UI/{GameID}/"              // Game UI prefabs
"Prefabs/Weapon/{GameID}/"          // Game weapon prefabs
"Prefabs/Content/{GameID}/"         // Game content objects
"Prefabs/Monster/{GameID}/"         // Game monster prefabs
"Prefabs/Player/{GameID}/"          // Game player prefabs

"Sprites/{GameID}/"                 // Game sprites
"Sprite/{GameID}_icon"              // Game selection icon

"Audio/BGM/{GameID}/"               // Game background music
"Audio/SFX/{GameID}/"               // Game sound effects

"Data/{GameID}/ScriptableObjects/"  // ScriptableObject data files

// Common UI components (shared across games)
"UI/{PanelName}"                    // Main UI panels
"SubItem/{ComponentName}"           // UI sub-components
```

**Important**: Resources folder structure changed from type-based to game-based organization to prevent resource conflicts between games.

### Multi-Minigame Platform Architecture

This project uses a **pluggable game architecture** that allows adding new games without modifying core platform code:

**Core Pattern (OCP - Open/Closed Principle)**:
```
1. GameRegistry: Factory pattern for game registration
2. MiniGameManager: Manages game lifecycle (load/unload/switch)
3. IMiniGame Interface: All games implement this contract
4. IGameData Interface: Game-specific data structure
```

**How Games Work Together**:
```
GameSelectScene (MainMenu)
    ↓ User selects game via UI button
CustomSceneManager.LoadScene("Tetris")
    ↓ Scene loads
TetrisScene initializes TetrisGame
    ↓ Runs in Tetris.unity scene
User returns → Back to GameSelectScene
```

**Adding a New Game** (Complete Flow):
1. Create game data structures inheriting from appropriate base classes
2. Create `MyGameDataProvider : IGameDataProvider` implementation
3. Create `MyGameData : IGameData` class for runtime game state
4. Create `MyGame : IMiniGame` implementation
5. Register data provider: `DataManager.Instance.RegisterProvider(new MyGameDataProvider())`
6. Register game: `GameRegistry.Instance.RegisterGame("MyGame", () => new MyGame())`
7. Add to `GamePlayList` in Inspector with matching GameID
8. Organize resources in `Resources/{Type}/MyGame/` folders
9. Place game icon at `Resources/Sprites/MyGame_icon`

### Infrastructure Managers

**Implemented Managers** (See `Assets/Docs/MANAGERS_GUIDE.md` for complete API):
1. `MiniGameManager`: Game lifecycle, switching, common player data
2. `DataManager`: Multi-game data provider management with lazy loading
3. `ResourceManager`: Addressables resource loading with PoolManager integration
4. `PoolManager`: GameObject pooling for performance optimization
5. `SoundManager`: Audio management (BGM/SFX with volume control)
6. `UIManager`: UI panel and popup management with fade effects
7. `CustomSceneManager`: Scene loading with transitions and loading screens
8. `InputManager`: Event-based input distribution to active game

### Architecture Principles

1. **IMiniGame Contract**: All games implement Initialize/StartGame/Update/Cleanup/GetData
2. **Manager Pattern**: Singleton managers for global infrastructure
3. **Factory Pattern**: GameRegistry creates game instances on demand
4. **Event-Driven Input**: InputManager broadcasts events, games subscribe
5. **Addressables**: All runtime resource loading uses ResourceManager
6. **Object Pooling**: PoolManager for frequent GameObject instantiation
7. **Scene Separation**: GameSelect scene + per-game scenes

## Coding Conventions

### Naming Conventions (Strictly Enforced)

**Variables:**
- Private: `_camelCase` → `private int _health;`
- Public: `PascalCase` → `public int Health;`
- Local/Parameters: `camelCase` → `int playerCount;`
- Constants: `UPPER_CASE` → `const int MAX_HEALTH = 100;`

**Classes & Types:**
- Classes/Structs: `PascalCase` → `public class PlayerController`
- Interfaces: `IPascalCase` → `public interface IDamageable`
- Enums: `PascalCase` → `public enum EnemyType`

**Methods:**
- General: `PascalCase` (verb) → `public void MovePlayer()`
- Coroutines: `MethodCoroutine` → `IEnumerator MovePlayerCoroutine()`
- Events: `OnEventHappened` → `public event Action OnPlayerDeath;`
- Booleans: `isState`, `hasItem` → `private bool _isGrounded;`

**File Structure:**
- Script name must match class name exactly
- One primary class per file
- Use partial classes for large classes split across files

### Code Style

```csharp
// Braces on same line, explicit access modifiers
public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed;  // Unity Inspector references
    [SerializeField] private int _damage;

    private void Start()  // Unity lifecycle methods
    {
        InitializeEnemy();
    }

    public void Attack()
    {
        if (_isReady)
        {
            PerformAttack();
        }
    }
}
```

### Type Usage
- Use `var` for local variables: `var message = "Hello";`
- Avoid `dynamic` unless necessary (external APIs)
- Use null-conditional `?.` and null-coalescing `??` operators
- Prefer properties over public fields

### Logging
```csharp
Debug.Log("[INFO] ClassName::MethodName - Message");
Debug.LogWarning("[WARNING] ClassName::MethodName - Warning message");
Debug.LogError("[ERROR] ClassName::MethodName - Error message");
```

### Complete Implementation Policy
- **Never leave TODO comments** for core functionality
- **No placeholder implementations** - all methods must be fully implemented
- **No mock/fake data** - use real implementations or ScriptableObjects
- If you start a feature, complete it to working state

## Git Workflow

### Branch Naming Rules

**Format**: `{type}/{description}`

**Types**:
- `feature/*` - 새로운 기능 개발
- `fix/*` - 버그 수정
- `refactor/*` - 코드 리팩토링
- `hotfix/*` - 긴급 버그 수정
- `docs/*` - 문서 작업
- `test/*` - 테스트 코드
- `design/*` - UI/UX 변경
- `chore/*` - 빌드, 설정 등

**Rules**:
- ✅ 소문자 사용: `feature/enemy-ai`
- ✅ 하이픈으로 단어 구분: `fix/player-movement`
- ✅ 명확한 설명: `feature/inventory-system`
- ✅ 이슈 번호 포함 가능: `fix/collision-#123`
- ❌ 공백 금지: `feature/enemy ai`
- ❌ 언더스코어 지양: `feature/enemy_ai`
- ❌ 한글 지양: `feature/적생성`
- ❌ 모호한 이름: `feature/update`, `feature/temp`

**Examples**:
```
feature/player-movement
fix/ui-button-click
refactor/enemy-manager
hotfix/critical-crash
design/main-menu
```

See `.claude/BRANCH_NAMING_RULES.md` for detailed rules.

### Branch Strategy (GitHub Flow)
```bash
# Always branch from main
git checkout main
git pull origin main
git checkout -b feature/feature-name

# Work and commit frequently
git add <files>
git commit -m "type: Title"
git push origin feature/feature-name

# Create Pull Request via GitHub
# After merge, delete branch
```

### Commit Message Format
```
type: Title (50 chars max, imperative mood)

Body explaining what and why (72 chars per line)

Resolves: #123
Ref: #456
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code formatting (no logic change)
- `design`: UI changes (no logic change)
- `test`: Test code
- `refactor`: Code refactoring
- `chore`: Build/config changes
- `rename`: File/folder rename only
- `remove`: File/folder deletion only

**Examples:**
```
feat: Add Enemy Manager

-적을 관리하는 클래스를 추가함
```

```
fix: Enemy not move in specific Scenes

-적이 특정 신에서 움직이지 않는 버그를 고침

Resolves: #219
```

### Commit Rules
- Title: Capitalize first letter, no period, imperative mood
- Commit frequently in logical units
- Explain "what" and "why", not "how"
- Reference issues with `Resolves:` (closes issue) or `Ref:` (reference only)

## Unity-Specific Guidelines

### Input System
- Use Unity's new Input System (InputSystem_Actions.inputactions)
- Access via generated C# class or Input Action Asset

### 2D Rendering
- Universal Render Pipeline configured with 2D Renderer
- Pixel Perfect Camera available
- Lit2D scene template available

### Serialization
- Use `[SerializeField]` for private fields visible in Inspector
- Avoid public fields; use properties with backing fields

### Coroutines
```csharp
private IEnumerator MovePlayerCoroutine(Vector3 destination)
{
    while (Vector3.Distance(transform.position, destination) > 0.1f)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            _moveSpeed * Time.deltaTime
        );
        yield return null;
    }
}
```

### Addressables Resource Loading
```csharp
// ALWAYS use ResourceManager for resource loading
ResourceManager.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (prefab) => {
    if (prefab != null) {
        // Use prefab
    }
});

// For instantiation with pooling (RECOMMENDED)
ResourceManager.Instance.InstantiateAsync("Prefabs/Enemy", transform, (instance) => {
    instance.transform.position = spawnPoint;
});

// Release when done
ResourceManager.Instance.ReleaseInstance(instance);
```

### Manager Usage Pattern

**Basic Manager Usage**:
```csharp
public class MyGameController : MonoBehaviour
{
    private void Start()
    {
        // Load resources
        ResourceManager.Instance.LoadAsync<AudioClip>("Audio/BGM/Theme", (clip) => {
            // Play BGM
            SoundManager.Instance.PlayBGM("Audio/BGM/Theme");
        });

        // Get current game data
        var gameData = MiniGameManager.Instance.GetCurrentGameData<TetrisGameData>();
    }
}
```

**Implementing a New Minigame**:
```csharp
// 1. Create data provider for game-specific data
public class MyGameDataProvider : IGameDataProvider
{
    public string GameID => "MyGame";
    public bool IsLoaded { get; private set; }

    private Dictionary<int, MyGameEntityData> _entityDict;

    public void Initialize()
    {
        _entityDict = new Dictionary<int, MyGameEntityData>();
        IsLoaded = false;
    }

    public void LoadData()
    {
        // Load from ScriptableObject/JSON/Resources
        MyGameDataList dataList = Resources.Load<MyGameDataList>("Data/MyGame/ScriptableObjects/MyGameDataList");
        foreach (var data in dataList.Entities)
        {
            _entityDict.Add(data.Id, data);
        }
        IsLoaded = true;
    }

    public void UnloadData()
    {
        _entityDict.Clear();
        IsLoaded = false;
    }

    public T GetData<T>(string key) where T : class
    {
        // Implement generic data access if needed
        return null;
    }

    public bool HasData(string key) => false;

    // Game-specific data access methods
    public MyGameEntityData GetEntityData(int entityId)
    {
        return _entityDict.TryGetValue(entityId, out var data) ? data : null;
    }
}

// 2. Create game runtime data
public class MyGameData : IGameData
{
    public int Score;
    public void Initialize() { Score = 0; }
    public void Reset() { Initialize(); }
    public bool Validate() { return Score >= 0; }
    public void SaveState() { /* PlayerPrefs or file save */ }
    public void LoadState() { /* PlayerPrefs or file load */ }
}

// 3. Implement IMiniGame
public class MyGame : IMiniGame
{
    private MyGameData _data;
    private CommonPlayerData _commonData;
    private MyGameDataProvider _dataProvider;

    public void Initialize(CommonPlayerData commonData)
    {
        _commonData = commonData;
        _data = new MyGameData();
        _data.Initialize();

        // Load game data via DataManager
        DataManager.Instance.LoadGameData("MyGame");
        _dataProvider = DataManager.Instance.GetProvider<MyGameDataProvider>("MyGame");
    }

    public void StartGame()
    {
        // Subscribe to input
        InputManager.Instance.OnInputEvent += HandleInput;
    }

    public void Update(float deltaTime)
    {
        // Game loop logic using _dataProvider
        var entityData = _dataProvider.GetEntityData(1);
    }

    public void Cleanup()
    {
        // MUST unsubscribe
        InputManager.Instance.OnInputEvent -= HandleInput;

        // Unload game data
        DataManager.Instance.UnloadGameData("MyGame");
    }

    public IGameData GetData() => _data;

    private void HandleInput(InputEventData inputData)
    {
        // Handle input events
    }
}

// 4. Register data provider and game (typically in GameRegistry or scene Awake)
var dataProvider = new MyGameDataProvider();
DataManager.Instance.RegisterProvider(dataProvider);
GameRegistry.Instance.RegisterGame("MyGame", () => new MyGame());

// 5. Add to GamePlayList in Inspector
// GameID: "MyGame", IsPlayable: true

// 6. Load the game via scene transition
CustomSceneManager.Instance.LoadScene("MyGame");
```

## Claude Code Skills

프로젝트에 특화된 자동화 워크플로우가 `.claude/skills/` 폴더에 정의되어 있습니다:

### Available Skills
1. **manager-guide**: Manager API 빠른 참조 및 사용 예제
   - 트리거: "매니저 사용법", "manager guide"
   - 모든 Manager의 완전한 API 문서 제공

2. **pre-commit-check**: 커밋 전 코드 품질 자동 검사
   - 트리거: "커밋 체크", "pre-commit check"
   - 네이밍 컨벤션, 금지 패턴, Manager 사용 검증

**사용 방법**:
```bash
# Claude Code에게 요청
"매니저 사용법 보여줘"
"커밋 전에 코드 체크해줘"
```

## Documentation References

### Assets/Docs/ (Unity Project Documentation)
- **Manager System Guide**: `Assets/Docs/MANAGERS_GUIDE.md` ⚠️ **READ THIS FIRST EVERY SESSION**
- **Undead Survivor Reference**: `Assets/Docs/UndeadSurvivor_Reference.md` - Original game implementation reference
- **Game Select UI Setup**: `Assets/Docs/GameSelectUI_Setup_Guide.md` - Dynamic button generation guide
- **Git Workflow**: `Assets/Docs/Github-Flow.md`
- **Unity Standards**: `Assets/Docs/[유니티] 개발 표준 v2.md` (Korean)
- **Setup Guide**: `Assets/Docs/SETUP_GUIDE.md`

### .claude/ (Claude Code Configuration)
- **Skills**: `.claude/skills/` - Automated workflows (manager-guide, pre-commit-check)
- **Conventions**: `.claude/UNITY_CONVENTIONS.md`
- **Commit Rules**: `.claude/COMMIT_MESSAGE_RULES.md`
- **Branch Naming**: `.claude/BRANCH_NAMING_RULES.md`

## Important Notes

1. **⚠️ READ MANAGERS_GUIDE.md FIRST** - At the start of EVERY work session, read `Assets/Docs/MANAGERS_GUIDE.md` before making ANY code changes
2. **Manager System First** - ALWAYS use existing managers (ResourceManager, PoolManager, SoundManager, UIManager, CustomSceneManager, MiniGameManager, DataManager) instead of implementing similar functionality
3. **DataManager for Game Data** - Use DataManager + IGameDataProvider pattern for all game-specific data (ScriptableObjects, JSON, etc.)
4. **Game-Based Resource Organization** - Resources are organized by game: `Resources/{Type}/{GameID}/` (NOT by type only)
5. **This is a Korean-language project** - Documentation and comments may be in Korean
6. **Strict convention compliance** - Follow naming and style rules exactly as specified
7. **Complete implementations only** - No TODOs, no placeholders, no mock data
8. **Unity 6 features** - Take advantage of latest Unity 6 capabilities when appropriate
9. **2D game focus** - This is specifically a 2D game using URP's 2D renderer
10. **Addressables paths** - Follow the game-based path conventions documented in Game Selection UI System section

## Current Work Context

**Active Branch**: `feature/sudoku`

**Current Focus**: Sudoku 게임 개발
- 스도쿠 게임 메커닉 구현 (퍼즐 생성, 유효성 검증, UI)
- IMiniGame 인터페이스 구현으로 플랫폼 통합
- DataManager와 SudokuDataProvider 통합 (필요시)

## Language Policy

**Claude Code must respond in Korean (한국어) for all interactions in this project.**

- All explanations, answers, and communications should be in Korean
- Code comments should be in Korean when explaining complex logic
- Commit messages can use English for type prefix, but description should be in Korean
- Exception: Technical terms, class names, variable names follow English conventions

## Quick Start Checklist for New Sessions

새로운 작업 세션을 시작할 때마다 다음 순서로 진행하세요:

1. ✅ **`Assets/Docs/MANAGERS_GUIDE.md` 읽기** (필수)
2. ✅ **현재 브랜치 확인**: `git status && git branch`
3. ✅ **현재 작업 맥락 파악**: CLAUDE.md의 "Current Work Context" 섹션 확인
4. ✅ **Manager 시스템 우선 사용**: 기능 구현 전 기존 Manager 활용 검토
5. ✅ **코딩 컨벤션 준수**: 네이밍 규칙, 파일 구조, 완전 구현 원칙

## Debugging & Troubleshooting

### Unity Console Errors
```csharp
// 로그 포맷
Debug.Log("[INFO] ClassName::MethodName - Message");
Debug.LogWarning("[WARNING] ClassName::MethodName - Warning");
Debug.LogError("[ERROR] ClassName::MethodName - Error");
```

### Common Issues
1. **Manager null reference**: Singleton이 초기화되었는지 확인
2. **Addressables load fail**: 주소가 정확한지, Addressables 그룹에 등록되었는지 확인
3. **Pool not found**: CreatePool 호출 또는 InstantiateAsync로 자동 생성 확인
4. **InputManager 이벤트 누락**: StartGame에서 구독, Cleanup에서 구독 해제 확인
