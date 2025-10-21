using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceManager 테스트 스크립트
/// Unity 에디터에서 게임 실행 시 테스트 수행
/// </summary>
public class ResourceManagerTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool _runTestOnStart = true;
    [SerializeField] private bool _testSyncLoad = true;
    [SerializeField] private bool _testAsyncLoad = true;
    [SerializeField] private bool _testPreload = true;
    [SerializeField] private bool _testInstantiate = true;
    [SerializeField] private bool _testRelease = true;

    [Header("Test Resources")]
    [SerializeField] private List<string> _testAddresses = new List<string>
    {
        "Prefabs/TestCube",
        "Prefabs/TestSphere",
        "Prefabs/TestCapsule"
    };

    private void Start()
    {
        if (_runTestOnStart)
        {
            RunAllTests();
        }
    }

    /// <summary>
    /// 모든 테스트 실행
    /// </summary>
    public void RunAllTests()
    {
        Debug.Log("========== ResourceManager Test Start ==========");

        if (_testSyncLoad)
        {
            TestSyncLoad();
        }

        if (_testAsyncLoad)
        {
            TestAsyncLoad();
        }

        if (_testPreload)
        {
            TestPreload();
        }

        if (_testInstantiate)
        {
            TestInstantiate();
        }

        if (_testRelease)
        {
            TestRelease();
        }

        Debug.Log("========== ResourceManager Test End ==========");
    }

    #region 동기 로드 테스트

    /// <summary>
    /// 동기 로드 테스트
    /// </summary>
    private void TestSyncLoad()
    {
        Debug.Log("===== Test: Sync Load =====");

        if (_testAddresses.Count == 0)
        {
            Debug.LogWarning("[TEST] No test addresses provided for sync load");
            return;
        }

        string testAddress = _testAddresses[0];
        Debug.Log($"[TEST] Loading resource synchronously: {testAddress}");

        GameObject prefab = ResourceManager.Instance.Load<GameObject>(testAddress);

        if (prefab != null)
        {
            Debug.Log($"[TEST] ✅ Sync load succeeded: {testAddress}");

            // 캐시 확인
            bool isCached = ResourceManager.Instance.IsCached(testAddress);
            Debug.Log($"[TEST] Is cached: {isCached}");

            // 캐시에서 다시 로드
            GameObject cachedPrefab = ResourceManager.Instance.GetCached<GameObject>(testAddress);
            Debug.Log($"[TEST] Got from cache: {cachedPrefab != null}");
        }
        else
        {
            Debug.LogError($"[TEST] ❌ Sync load failed: {testAddress}");
        }
    }

    #endregion

    #region 비동기 로드 테스트

    /// <summary>
    /// 비동기 로드 테스트
    /// </summary>
    private void TestAsyncLoad()
    {
        Debug.Log("===== Test: Async Load =====");

        if (_testAddresses.Count < 2)
        {
            Debug.LogWarning("[TEST] Not enough test addresses for async load");
            return;
        }

        string testAddress = _testAddresses[1];
        Debug.Log($"[TEST] Loading resource asynchronously: {testAddress}");

        ResourceManager.Instance.LoadAsync<GameObject>(testAddress, (prefab) =>
        {
            if (prefab != null)
            {
                Debug.Log($"[TEST] ✅ Async load succeeded: {testAddress}");
                Debug.Log($"[TEST] Loaded prefab name: {prefab.name}");
            }
            else
            {
                Debug.LogError($"[TEST] ❌ Async load failed: {testAddress}");
            }
        });
    }

    #endregion

    #region 프리로드 테스트

    /// <summary>
    /// 프리로드 테스트
    /// </summary>
    private void TestPreload()
    {
        Debug.Log("===== Test: Preload =====");

        if (_testAddresses.Count == 0)
        {
            Debug.LogWarning("[TEST] No test addresses provided for preload");
            return;
        }

        Debug.Log($"[TEST] Preloading {_testAddresses.Count} resources...");

        ResourceManager.Instance.PreloadAsync<GameObject>(_testAddresses, () =>
        {
            Debug.Log("[TEST] ✅ Preload completed!");
            Debug.Log($"[TEST] Preload progress: {ResourceManager.Instance.PreloadProgress:P0}");

            // 모든 리소스가 캐시되었는지 확인
            int cachedCount = 0;
            foreach (var address in _testAddresses)
            {
                if (ResourceManager.Instance.IsCached(address))
                {
                    cachedCount++;
                }
            }

            Debug.Log($"[TEST] Cached resources: {cachedCount}/{_testAddresses.Count}");
        });
    }

    #endregion

    #region 인스턴스 생성 테스트

    /// <summary>
    /// 인스턴스 생성 테스트
    /// </summary>
    private void TestInstantiate()
    {
        Debug.Log("===== Test: Instantiate =====");

        if (_testAddresses.Count == 0)
        {
            Debug.LogWarning("[TEST] No test addresses provided for instantiate");
            return;
        }

        string testAddress = _testAddresses[0];
        Debug.Log($"[TEST] Instantiating prefab: {testAddress}");

        // 동기 인스턴스 생성
        GameObject instance1 = ResourceManager.Instance.Instantiate(testAddress, transform);
        if (instance1 != null)
        {
            instance1.transform.position = new Vector3(-2f, 0f, 0f);
            Debug.Log($"[TEST] ✅ Sync instantiate succeeded: {instance1.name}");
        }
        else
        {
            Debug.LogError($"[TEST] ❌ Sync instantiate failed");
        }

        // 비동기 인스턴스 생성
        if (_testAddresses.Count > 1)
        {
            string testAddress2 = _testAddresses[1];
            Debug.Log($"[TEST] Instantiating prefab asynchronously: {testAddress2}");

            ResourceManager.Instance.InstantiateAsync(testAddress2, transform, (instance2) =>
            {
                if (instance2 != null)
                {
                    instance2.transform.position = new Vector3(2f, 0f, 0f);
                    Debug.Log($"[TEST] ✅ Async instantiate succeeded: {instance2.name}");
                }
                else
                {
                    Debug.LogError($"[TEST] ❌ Async instantiate failed");
                }
            });
        }
    }

    #endregion

    #region 리소스 해제 테스트

    /// <summary>
    /// 리소스 해제 테스트
    /// </summary>
    private void TestRelease()
    {
        Debug.Log("===== Test: Release =====");

        if (_testAddresses.Count == 0)
        {
            Debug.LogWarning("[TEST] No test addresses provided for release");
            return;
        }

        // 특정 리소스 해제
        string testAddress = _testAddresses[0];
        Debug.Log($"[TEST] Releasing resource: {testAddress}");

        ResourceManager.Instance.Release(testAddress);

        bool isCached = ResourceManager.Instance.IsCached(testAddress);
        Debug.Log($"[TEST] Is still cached after release: {isCached}");

        if (!isCached)
        {
            Debug.Log($"[TEST] ✅ Release succeeded");
        }
        else
        {
            Debug.LogError($"[TEST] ❌ Release failed");
        }
    }

    /// <summary>
    /// 모든 리소스 해제 테스트
    /// </summary>
    public void TestReleaseAll()
    {
        Debug.Log("===== Test: Release All =====");

        ResourceManager.Instance.ReleaseAll();

        // 모든 리소스가 해제되었는지 확인
        int cachedCount = 0;
        foreach (var address in _testAddresses)
        {
            if (ResourceManager.Instance.IsCached(address))
            {
                cachedCount++;
            }
        }

        Debug.Log($"[TEST] Cached resources after ReleaseAll: {cachedCount}");

        if (cachedCount == 0)
        {
            Debug.Log($"[TEST] ✅ ReleaseAll succeeded");
        }
        else
        {
            Debug.LogError($"[TEST] ❌ ReleaseAll failed, still {cachedCount} cached");
        }
    }

    #endregion

    #region UI 버튼용 메서드

    /// <summary>
    /// 프리로드 진행률 표시
    /// </summary>
    private void Update()
    {
        if (ResourceManager.Instance.IsPreloading)
        {
            Debug.Log($"[TEST] Preloading... {ResourceManager.Instance.PreloadProgress:P0}");
        }
    }

    #endregion
}
