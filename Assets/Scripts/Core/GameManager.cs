using UnityEngine;

/// <summary>
/// 여러 미니 게임을 관리하는 제네릭 게임 매니저
/// IGameData를 상속받은 다양한 게임 데이터 타입을 관리 가능
/// </summary>
/// <typeparam name="T">IGameData를 상속받은 게임 데이터 타입</typeparam>
public class GameManager<T> : Singleton<GameManager<T>> where T : class, IGameData, new()
{
    [SerializeField] private bool _isGamePaused = false;
    [SerializeField] private float _gameSpeed = 1f;

    private T _currentGameData;

    /// <summary>
    /// 현재 게임 데이터
    /// </summary>
    public T CurrentGameData
    {
        get
        {
            if (_currentGameData == null)
            {
                _currentGameData = new T();
                _currentGameData.Initialize();
            }
            return _currentGameData;
        }
        private set => _currentGameData = value;
    }

    /// <summary>
    /// 게임 일시정지 상태
    /// </summary>
    public bool IsGamePaused
    {
        get => _isGamePaused;
        private set => _isGamePaused = value;
    }

    /// <summary>
    /// 게임 속도 (1.0 = 정상 속도)
    /// </summary>
    public float GameSpeed
    {
        get => _gameSpeed;
        set
        {
            _gameSpeed = Mathf.Max(0f, value);
            Time.timeScale = _isGamePaused ? 0f : _gameSpeed;
        }
    }

    /// <summary>
    /// 초기화 - Singleton의 Awake를 오버라이드
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // Singleton 초기화 반드시 호출

        // GameManager 초기화
        InitializeGame();
    }

    /// <summary>
    /// 게임 초기화
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::InitializeGame - Game initialized");

        // 게임 데이터 초기화
        if (_currentGameData == null)
        {
            _currentGameData = new T();
            _currentGameData.Initialize();
        }

        // 초기 게임 속도 설정
        Time.timeScale = _gameSpeed;
    }

    /// <summary>
    /// 새로운 게임 시작
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::StartNewGame - Starting new game");

        // 기존 데이터 리셋
        if (_currentGameData != null)
        {
            _currentGameData.Reset();
        }
        else
        {
            _currentGameData = new T();
            _currentGameData.Initialize();
        }

        // 게임 상태 초기화
        _isGamePaused = false;
        _gameSpeed = 1f;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 다른 게임 데이터로 전환
    /// </summary>
    /// <param name="newGameData">새로운 게임 데이터</param>
    public void SwitchGameData(T newGameData)
    {
        if (newGameData == null)
        {
            Debug.LogError($"[ERROR] GameManager<{typeof(T).Name}>::SwitchGameData - New game data is null");
            return;
        }

        if (!newGameData.Validate())
        {
            Debug.LogError($"[ERROR] GameManager<{typeof(T).Name}>::SwitchGameData - New game data validation failed");
            return;
        }

        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::SwitchGameData - Switching to new game data");

        _currentGameData = newGameData;
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        if (_isGamePaused)
        {
            return;
        }

        _isGamePaused = true;
        Time.timeScale = 0f;

        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::PauseGame - Game paused");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        if (!_isGamePaused)
        {
            return;
        }

        _isGamePaused = false;
        Time.timeScale = _gameSpeed;

        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::ResumeGame - Game resumed");
    }

    /// <summary>
    /// 게임 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        if (_isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// 게임 데이터 검증
    /// </summary>
    /// <returns>현재 게임 데이터가 유효하면 true</returns>
    public bool ValidateCurrentGameData()
    {
        if (_currentGameData == null)
        {
            Debug.LogWarning($"[WARNING] GameManager<{typeof(T).Name}>::ValidateCurrentGameData - Game data is null");
            return false;
        }

        return _currentGameData.Validate();
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        Debug.Log($"[INFO] GameManager<{typeof(T).Name}>::QuitGame - Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
