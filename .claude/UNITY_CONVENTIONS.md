# Unity Development Conventions

Based on the official Unity Development Standards Guide v2.

## 1. Naming Conventions

### 1.1. Variables

#### Private Variables
- Use `_` prefix + camelCase
```csharp
private int _variable;
private int _variableCount;
```

#### Public Variables
- Use PascalCase
- Prefer properties over public variables
```csharp
public int Variable;
public int VariableCount;
```

#### Properties and Backing Fields
- Backing field: `_` + camelCase
- Property: PascalCase
```csharp
private int _variableProperty;
public int VariableProperty
{
    get { return _variableProperty; }
    set { _variableProperty = value; }
}
```

#### Local Variables and Parameters
- Use camelCase (no `_` prefix)
```csharp
public void ProcessData()
{
    int processedCount = 0;
    for (int i = 0; i < 10; i++)
    {
        processedCount += i;
    }
}

public void SetName(string userName)
{
    _name = userName;
}
```

#### Constants
- Use UPPER_CASE with underscores
```csharp
public const int MAX_HEALTH = 100;
```

#### Readonly Fields
- Follow access modifier naming rules
- Initialize in constructor
```csharp
private readonly int _initialHealth;
public readonly int InitialHealth;

public Player(int health)
{
    _initialHealth = health;
    InitialHealth = health;
}
```

### 1.2. Classes and Structures

#### Classes
- Use PascalCase with nouns
- Script file name must match class name
```csharp
// PlayerController.cs
public class PlayerController : MonoBehaviour { }
```

#### Structures
- Use PascalCase
- Name clearly as data containers
```csharp
public struct PlayerStats { }
```

#### Interfaces
- Use `I` + PascalCase
```csharp
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

#### Enums
- Use PascalCase for enum and values
```csharp
public enum EnemyType
{
    Melee,
    Ranged,
    Boss
}
```

#### Bit Flag Enums
- Use PascalCase + "Flags" suffix
- Apply `[Flags]` attribute
- Use powers of 2 for values
```csharp
[Flags]
public enum ItemAttributesFlags
{
    None = 0,
    Flammable = 1 << 0,
    Edible = 1 << 1,
    Fragile = 1 << 2,
    Heavy = 1 << 3,
    Valuable = 1 << 4
}
```

### 1.3. Methods and Functions

#### General Methods
- Use PascalCase starting with verbs
- Express clear actions
```csharp
public void MovePlayer(Vector3 direction) { }
private void HandleInput() { }
```

#### Unity Event Methods
- Use exact Unity callback names
```csharp
private void Start() { }
private void Update() { }
private void OnCollisionEnter(Collision other) { }
```

#### Coroutine Methods
- Use PascalCase with "Coroutine" suffix
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

#### Events
- Use PascalCase with "OnSomethingHappened" format
```csharp
public event Action OnPlayerDeath;

private void Die()
{
    OnPlayerDeath?.Invoke();
}
```

### 1.4. Boolean Variables and Methods

- Use prefix patterns: `{_}? + prefix + {camelCase or PascalCase}`
- Avoid negative names
```csharp
private bool _isGrounded;
private bool _hasKey;

public bool IsGrounded => _isGrounded;
public bool HasKey => _hasKey;

// Use positive expressions
bool isActive = !isPaused;  // Good
```

### 1.5. Namespaces

- Use PascalCase reflecting project structure
```csharp
namespace MyGame.Player
{
    public class PlayerController { }
}
```

### 1.6. Abbreviation Rules

- Don't mix PascalCase/camelCase in abbreviations
- Allow short, clear abbreviations only
- Avoid abbreviations in global variables
- Class variables: allowed for data structures or specific roles
- Local variables: flexible abbreviation use based on context

```csharp
// Class-level abbreviations (allowed)
public class GameDataManager
{
    private Dictionary<string, int> _itemDict;
    private Database _playerDB;
    private int _scoreSum;
}

// Method-level abbreviations (flexible)
public void Move(Vector3 dir, float spd)
{
    Vector3 newPos = transform.position + dir * spd * Time.deltaTime;
    transform.position = newPos;
}
```

## 2. Code Conventions and Class Structure

### 2.1. Code Formatting

- Place braces `{}` on same line
- Add blank lines between code blocks for readability
```csharp
public void Jump()
{
    if (_isGrounded)
    {
        _rigidbody.AddForce(Vector3.up * jumpForce);
    }
}
```

### 2.2. Access Modifiers and Unity Object References

- Always explicitly declare access modifiers
- Default to `private`
- Use `[SerializeField]` for Unity Inspector references
```csharp
public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private int _damage;

    public void Attack() { }
}
```

### 2.3. Partial Class Rules

- Use to split large classes across multiple files
- Useful for separating auto-generated and custom code
```csharp
// PlayerInfo.cs
public partial class Player : IPlayer
{
    public string Name { get; set; }
    public int Health { get; set; }
}

// PlayerActions.cs
public partial class Player
{
    public void Move()
    {
        Debug.Log($"{Name} is moving.");
    }

    public void Attack()
    {
        Debug.Log($"{Name} is attacking.");
    }
}
```

### 2.4. Static Classes and Singleton Pattern

#### Static Classes
- Use for stateless utility functions
```csharp
public static class MathUtilities
{
    public static float Clamp(float value, float min, float max)
    {
        return Mathf.Max(min, Mathf.Min(max, value));
    }
}
```

#### Singleton Pattern
- Use for globally unique instances that maintain state
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

### 2.5. Extension Method Rules

- Write in static classes
- Use `this` keyword for first parameter
```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }
}
```

## 3. Flow Control and Exception Handling

### 3.1. Switch Statement Rules

- Always use `break`
- Recommend `default` block
- Allow multiple values per case
- Avoid nested switch statements
```csharp
switch (command)
{
    case "Start":
        Debug.Log("Game started.");
        break;
    case "Pause":
        Debug.Log("Game paused.");
        break;
    default:
        Debug.Log("Invalid command.");
        break;
}
```

### 3.2. Recursive Function Rules

- Clearly define base conditions
- Limit recursion depth
- Prefer iteration over recursion when possible
```csharp
// Recursive
public int Factorial(int n)
{
    if (n <= 1)
        return 1;
    return n * Factorial(n - 1);
}

// Iterative (preferred when possible)
public int FactorialIterative(int n)
{
    int result = 1;
    for (int i = 1; i <= n; i++)
    {
        result *= i;
    }
    return result;
}
```

### 3.3. Assert, Log, and Error Detection

#### Assert
- Use only for critical logic
- Don't use as replacement for essential logic
```csharp
Debug.Assert(Health > 0, "Health must be greater than zero.");
```

#### Log
- Use appropriate log levels
- Follow structured format
```csharp
Debug.Log("[INFO] PlayerController::Start() - Player initialized with health: 100");
Debug.LogWarning("[WARNING] PlayerHealth::TakeDamage() - Health is below 20");
Debug.LogError("[ERROR] NetworkManager::Connect() - Failed to connect. ErrorCode: 404");
```

#### Error Detection
- Use try-catch blocks
- Handle specific conditions
```csharp
public void LoadData(string path)
{
    try
    {
        string data = File.ReadAllText(path);
        Debug.Log("Data loaded successfully.");
    }
    catch (FileNotFoundException e)
    {
        Debug.LogError($"File not found: {e.Message}");
    }
    catch (Exception e)
    {
        Debug.LogError($"An unexpected error occurred: {e.Message}");
    }
}
```

## 4. C# Type Management

### 4.1. var Usage Rules

- **Local variables**: Use `var` for type inference
- **Loops**: Use `var` for collection elements
- **LINQ**: Use `var` for query results
- **Cautions**:
  - Avoid when return type is unclear (e.g., `object`)
  - Cannot use without initialization

```csharp
public class Example
{
    public void ProcessData()
    {
        // Local variables
        var message = "Hello, World!"; // Inferred as string
        Debug.Log(message);

        // Loops
        var numbers = new int[] { 1, 2, 3, 4, 5 };
        foreach (var num in numbers) // Inferred as int
        {
            Debug.Log(num);
        }

        // LINQ
        var evenNumbers = numbers.Where(n => n % 2 == 0);
        foreach (var evenNum in evenNumbers)
        {
            Debug.Log(evenNum);
        }
    }

    public object GetData()
    {
        return "Data";
    }

    public void ExampleMethod()
    {
        // Avoid when return type is object
        object data = GetData(); // Explicit type preferred
    }
}
```

### 4.2. dynamic and object Usage

#### dynamic
- Type determined at runtime
- Use for external libraries, COM interop
- Minimize reflection usage
- **Cautions**:
  - Reduced type safety
  - Runtime errors possible

#### object
- Top-level type for all objects
- Boxing/unboxing causes performance overhead
- **Cautions**:
  - Requires type casting
  - Performance concerns

```csharp
public class Example
{
    public void UseDynamic()
    {
        dynamic api = GetExternalAPI();
        Debug.Log(api.SomeMethod()); // Runtime method resolution
    }

    public void UseObject()
    {
        object data = GetData();
        if (data is string)
        {
            string message = (string)data;
            Debug.Log(message);
        }
    }

    private dynamic GetExternalAPI()
    {
        return new ExternalAPI();
    }

    private object GetData()
    {
        return "Hello, World!";
    }
}
```

### 4.3. null Usage and Exception Handling

- **null checks**: Prevent NullReferenceException
- Use `?.` (null-conditional) and `??` (null-coalescing) operators
- **Initialization**: Consider null initialization
- **Exception handling**: Handle null-related exceptions
- **Cautions**:
  - Unnecessary null checks harm readability
  - Minimize null returns; prefer empty collections or defaults

```csharp
public class Example
{
    private string _name;

    public void ProcessName()
    {
        // Null-conditional operator
        string message = _name?.ToUpper(); // null if _name is null

        // Null-coalescing operator
        string displayName = _name ?? "Guest"; // Use "Guest" if _name is null

        Debug.Log(displayName);
    }

    public void Initialize()
    {
        _name = null; // Initialize as null
    }

    public void ExampleMethod()
    {
        try
        {
            Debug.Log(_name.Length); // Potential NullReferenceException
        }
        catch (NullReferenceException e)
        {
            Debug.LogError($"NullReferenceException: {e.Message}");
        }
    }
}
```

## 5. Directory and File Management

### 5.1. Directory Structure and Naming

```
Assets/
│
├── Scripts/           # Script files
│   ├── Managers/      # Game manager scripts
│   ├── UI/            # UI scripts
│   └── Player/        # Player control scripts
│
├── Resources/         # Runtime loading resources
│   ├── UI_Assets/     # UI assets
│   └── Prefabs/       # Prefab files
│
├── Audio/             # Audio files (BGM, SFX)
│   ├── BGM/           # Background music
│   └── SFX/           # Sound effects
│
└── Art/               # Art assets (models, textures)
    ├── Models/        # 3D model files
    └── Textures/      # Texture files
```

## 6. Comment Rules

### XML Comments
- Provide descriptions for public members
- Use `<summary>`, `<param>`, `<returns>` tags
```csharp
/// <summary>
/// 플레이어의 이동을 처리하는 메서드입니다.
/// </summary>
/// <param name="direction">이동 방향</param>
public void MovePlayer(Vector3 direction)
{
    // Calculate new position by multiplying direction with current speed
    Vector3 newPosition = transform.position + direction * _speed * Time.deltaTime;
    transform.position = newPosition;
}
```

### Code Comments
- Explain complex logic or important parts
- Keep concise and clear
- Remove unnecessary comments

### TODO Comments
- Mark tasks to handle later
- Use `// TODO: {task}` format
```csharp
// TODO: 애니메이션 추가
```

---

## Quick Reference Checklist

### Naming Quick Check
- [ ] Private variables: `_camelCase`
- [ ] Public variables/properties: `PascalCase`
- [ ] Local variables/parameters: `camelCase`
- [ ] Constants: `UPPER_CASE`
- [ ] Classes/Structs: `PascalCase`
- [ ] Interfaces: `IPascalCase`
- [ ] Methods: `PascalCase` (verb)
- [ ] Booleans: `isState`, `hasItem`
- [ ] Coroutines: `MethodCoroutine`
- [ ] Events: `OnEventHappened`

### Code Structure Quick Check
- [ ] Braces on same line
- [ ] Explicit access modifiers
- [ ] `[SerializeField]` for Unity references
- [ ] XML comments for public members
- [ ] Structured log messages with levels
- [ ] Try-catch for error-prone operations

### Type Usage Quick Check
- [ ] Use `var` for local variables
- [ ] Avoid `dynamic` unless necessary
- [ ] Use null-conditional `?.` and null-coalescing `??`
- [ ] Prefer properties over public fields
- [ ] Use `readonly` when appropriate
