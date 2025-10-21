using UnityEngine;

/// <summary>
/// MonoBehaviour를 상속하는 싱글톤 기본 클래스
/// 제네릭 타입을 사용하여 자식 클래스에서 간편하게 싱글톤 패턴 구현 가능
/// </summary>
/// <typeparam name="T">싱글톤으로 만들 클래스 타입</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하는 프로퍼티
    /// </summary>
    public static T Instance
    {
        get
        {
            // 애플리케이션 종료 중에는 인스턴스 생성하지 않음
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[WARNING] Singleton<{typeof(T)}>::Instance - Application is quitting. Returning null.");
                return null;
            }

            lock (_lock)
            {
                // 인스턴스가 없으면 씬에서 찾기
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    // 씬에도 없으면 새로 생성
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject($"{typeof(T).Name} (Singleton)");
                        _instance = singletonObject.AddComponent<T>();

                        Debug.Log($"[INFO] Singleton<{typeof(T)}>::Instance - Created new instance: {typeof(T).Name}");
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// MonoBehaviour Awake 이벤트
    /// 자식 클래스에서 Awake를 사용하려면 protected override void Awake() 사용
    /// </summary>
    protected virtual void Awake()
    {
        // 이미 인스턴스가 존재하는 경우
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[WARNING] Singleton<{typeof(T)}>::Awake - Duplicate instance detected. Destroying: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // 인스턴스 설정
        _instance = this as T;

        // 씬 전환 시에도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[INFO] Singleton<{typeof(T)}>::Awake - Instance initialized: {typeof(T).Name}");
    }

    /// <summary>
    /// 애플리케이션 종료 시 호출
    /// 종료 중에는 새 인스턴스 생성을 방지
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// 오브젝트 파괴 시 호출
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
