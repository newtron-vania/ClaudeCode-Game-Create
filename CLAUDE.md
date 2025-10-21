# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Claude-Undead-Survivor** is a Unity 2D game project using Universal Render Pipeline (URP). This is an early-stage project with minimal code scaffolding currently in place.

- **Unity Version**: 6000.0.58f2 (Unity 6)
- **Render Pipeline**: Universal Render Pipeline (URP) 17.0.4
- **2D Framework**: Unity Feature 2D 2.0.1
- **Input System**: Unity Input System 1.14.2

## Project Structure

```
Assets/
├── Scripts/              # Game scripts (C#)
├── Scenes/               # Unity scene files
│   └── SampleScene.unity
├── Settings/             # URP and rendering settings
│   ├── Renderer2D.asset
│   ├── UniversalRP.asset
│   └── Scenes/
├── InputSystem_Actions.inputactions  # Input System configuration
├── DefaultVolumeProfile.asset
├── UniversalRenderPipelineGlobalSettings.asset
├── Github-Flow.md        # Git workflow documentation
└── [유니티] 개발 표준 v2.md  # Unity development standards (Korean)

.claude/
├── UNITY_CONVENTIONS.md      # Unity coding conventions
└── COMMIT_MESSAGE_RULES.md   # Git commit message rules
```

## Development Commands

### Unity Editor
```bash
# Open project in Unity Editor
# Use Unity Hub to open: /Users/kimkyeongsoo/Desktop/Unity/Claude-Undead-Survivor

# Unity 6 (6000.0.58f2) is required
```

### Build Commands
Unity builds are managed through the Unity Editor. No CLI build commands are configured yet.

## Code Architecture

### Current State
This project has implemented a complete **Manager System** for core game infrastructure:

**Implemented Managers** (See `Assets/Docs/MANAGERS_GUIDE.md` for detailed usage):
- `Singleton<T>`: Generic singleton base class
- `GameManager<T>`: Generic game manager with data management
- `ResourceManager`: Addressables resource loading with PoolManager integration
- `PoolManager`: GameObject pooling for performance optimization
- `SoundManager`: Audio management (BGM/SFX with volume control)
- `UIManager`: UI panel and popup management with fade effects
- `CustomSceneManager`: Scene loading with transitions and loading screens

**⚠️ IMPORTANT: Read Manager Guide First**
At the start of each work session, you MUST read:
```
Assets/Docs/MANAGERS_GUIDE.md
```
This file contains complete API documentation and usage patterns. Always use these managers instead of implementing similar functionality manually.

### Architecture Principles
Based on Unity development standards, the project follows:

1. **MonoBehaviour Pattern**: Game logic components inherit from `MonoBehaviour`
2. **Manager Pattern**: Singleton managers for global systems (already implemented)
3. **Component-Based Design**: Separate components for different gameplay systems
4. **ScriptableObject Data**: Configuration and game data stored as ScriptableObjects
5. **Object Pooling**: Use PoolManager for frequent GameObject instantiation
6. **Resource Loading**: Use ResourceManager (Addressables) for all asset loading

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

### Singleton Pattern (When Needed)
```csharp
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(GameManager));
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

## Branch Workflow Management

### Automatic Branch Task Tracking
Claude Code automatically manages branch-specific task documents:

**Location**: `.claude/branches/{branch-name}.md`

**Triggers**:
- Creating new branch: `git checkout -b feature/name`
- Switching branches: `git checkout branch-name`
- Making commits: Auto-updates commit history
- User requests: "현재 브랜치 작업 상황 보여줘"

**Auto Actions**:
1. Detect current branch name
2. Check if `.claude/branches/{branch-name}.md` exists
3. If not exists: Create from template with current date/time
4. If exists: Load and display last work status
5. Update work log with timestamps
6. Track commit messages automatically

**File Naming**: `feature/enemy-ai` → `feature-enemy-ai.md`

### Work Document Structure
```markdown
# 브랜치: feature/example
## 브랜치 정보
- 생성일, 타입, 목적, 관련 이슈
## 작업 목표
- [ ] Checklist items
## 작업 내역
- Timestamped work logs
## 커밋 기록
- Automatic commit message tracking
## 완료 조건
- [ ] Completion criteria
```

**Usage**:
- "브랜치 작업 시작" - Initialize branch document
- "작업 기록: [내용]" - Add work log entry
- "작업 완료" - Mark tasks as done
- "브랜치 작업 상황" - Show current status

See `.claude/BRANCH_WORKFLOW.md` for detailed workflow guide.

## Documentation References

### Assets/Docs/ (Unity Project Documentation)
- **Manager System Guide**: `Assets/Docs/MANAGERS_GUIDE.md` ⚠️ **READ THIS FIRST EVERY SESSION**
- **Git Workflow**: `Assets/Docs/Github-Flow.md`
- **Unity Standards**: `Assets/Docs/[유니티] 개발 표준 v2.md` (Korean)
- **Setup Guide**: `Assets/Docs/SETUP_GUIDE.md`

### .claude/ (Claude Code Configuration)
- **Detailed Conventions**: `.claude/UNITY_CONVENTIONS.md`
- **Commit Rules**: `.claude/COMMIT_MESSAGE_RULES.md`
- **Branch Naming**: `.claude/BRANCH_NAMING_RULES.md`
- **Branch Task Management**: `.claude/BRANCH_WORKFLOW.md`

## Important Notes

1. **Manager System First** - ALWAYS use existing managers (ResourceManager, PoolManager, SoundManager, UIManager, CustomSceneManager) instead of implementing similar functionality
2. **Read MANAGERS_GUIDE.md** - At the start of EVERY work session, read `Assets/Docs/MANAGERS_GUIDE.md`
3. **This is a Korean-language project** - Documentation and comments may be in Korean
4. **Strict convention compliance** - Follow naming and style rules exactly as specified
5. **Complete implementations only** - No TODOs, no placeholders, no mock data
6. **Unity 6 features** - Take advantage of latest Unity 6 capabilities when appropriate
7. **2D game focus** - This is specifically a 2D game using URP's 2D renderer

## Language Policy

**Claude Code must respond in Korean (한국어) for all interactions in this project.**

- All explanations, answers, and communications should be in Korean
- Code comments should be in Korean when explaining complex logic
- Commit messages can use English for type prefix, but description should be in Korean
- Exception: Technical terms, class names, variable names follow English conventions
