using System.Collections;
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
    [SerializeField] private float _testInterval = 1f;
    [SerializeField] private bool _testSyncLoad = true;
    [SerializeField] private bool _testAsyncLoad = true;
    [SerializeField] private bool _testPreload = true;
    [SerializeField] private bool _testInstantiate = true;
    [SerializeField] private bool _testMultipleInstantiate = true;
    [SerializeField] private bool _testRelease = true;

    [Header("Test Resources")]
    [SerializeField] private List<string> _testAddresses = new List<string>
    {
        "Prefabs/TestCube",
        "Prefabs/TestSphere",
        "Prefabs/TestCapsule"
    };

    [Header("UI Settings")]
    [SerializeField] private int _maxLogLines = 15;
    [SerializeField] private int _fontSize = 16;

    private List<string> _logMessages = new List<string>();
    private string _currentTest = "";

    private void Start()
    {
        if (_runTestOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    /// <summary>
    /// 화면에 로그 표시
    /// </summary>
    private void OnGUI()
    {
        // GUI 스타일 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = _fontSize;
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.UpperLeft;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f));

        // 로그 영역
        float width = Screen.width * 0.95f;
        float height = Screen.height * 0.4f;
        float x = Screen.width * 0.025f;
        float y = Screen.height * 0.05f;

        GUILayout.BeginArea(new Rect(x, y, width, height), boxStyle);

        // 현재 테스트 표시
        if (!string.IsNullOrEmpty(_currentTest))
        {
            GUIStyle titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = _fontSize + 4;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.yellow;
            GUILayout.Label($"Current Test: {_currentTest}", titleStyle);
            GUILayout.Space(10);
        }

        // 로그 메시지 표시
        for (int i = Mathf.Max(0, _logMessages.Count - _maxLogLines); i < _logMessages.Count; i++)
        {
            GUILayout.Label(_logMessages[i], labelStyle);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// GUI 배경 텍스처 생성
    /// </summary>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// <summary>
    /// 로그 메시지 추가
    /// </summary>
    private void AddLog(string message)
    {
        _logMessages.Add(message);
        Debug.Log(message);
    }

    /// <summary>
    /// 현재 테스트 설정
    /// </summary>
    private void SetCurrentTest(string testName)
    {
        _currentTest = testName;
        AddLog($"===== {testName} =====");
    }

    /// <summary>
    /// 모든 테스트 실행 (1초 간격)
    /// </summary>
    public IEnumerator RunAllTests()
    {
        AddLog("========== ResourceManager Test Start ==========");

        if (_testSyncLoad)
        {
            SetCurrentTest("Sync Load Test");
            TestSyncLoad();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testAsyncLoad)
        {
            SetCurrentTest("Async Load Test");
            TestAsyncLoad();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testPreload)
        {
            SetCurrentTest("Preload Test");
            TestPreload();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testInstantiate)
        {
            SetCurrentTest("Instantiate Test");
            TestInstantiate();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testMultipleInstantiate)
        {
            SetCurrentTest("Multiple Instantiate & Destroy Test");
            yield return StartCoroutine(TestMultipleInstantiate());
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testRelease)
        {
            SetCurrentTest("Release Test");
            TestRelease();
            yield return new WaitForSeconds(_testInterval);
        }

        _currentTest = "";
        AddLog("========== ResourceManager Test End ==========");
    }
    
    

    #region 동기 로드 테스트

    /// <summary>
    /// 동기 로드 테스트
    /// </summary>
    private void TestSyncLoad()
    {
        if (_testAddresses.Count == 0)
        {
            AddLog("[WARNING] No test addresses provided");
            return;
        }

        string testAddress = _testAddresses[0];
        AddLog($"Loading: {testAddress}");

        GameObject prefab = ResourceManager.Instance.Load<GameObject>(testAddress);

        if (prefab != null)
        {
            AddLog($"✅ Load succeeded: {testAddress}");

            // 캐시 확인
            bool isCached = ResourceManager.Instance.IsCached(testAddress);
            AddLog($"Is cached: {isCached}");

            // 캐시에서 다시 로드
            GameObject cachedPrefab = ResourceManager.Instance.GetCached<GameObject>(testAddress);
            AddLog($"Got from cache: {cachedPrefab != null}");
        }
        else
        {
            AddLog($"❌ Load failed: {testAddress}");
        }
    }

    #endregion

    #region 비동기 로드 테스트

    /// <summary>
    /// 비동기 로드 테스트
    /// </summary>
    private void TestAsyncLoad()
    {
        if (_testAddresses.Count < 2)
        {
            AddLog("[WARNING] Not enough test addresses");
            return;
        }

        string testAddress = _testAddresses[1];
        AddLog($"Loading async: {testAddress}");

        ResourceManager.Instance.LoadAsync<GameObject>(testAddress, (prefab) =>
        {
            if (prefab != null)
            {
                AddLog($"✅ Async load succeeded: {prefab.name}");
            }
            else
            {
                AddLog($"❌ Async load failed");
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
        if (_testAddresses.Count == 0)
        {
            AddLog("[WARNING] No test addresses provided for preload");
            return;
        }

        AddLog($"Preloading {_testAddresses.Count} resources...");

        ResourceManager.Instance.PreloadAsync<GameObject>(_testAddresses, () =>
        {
            AddLog("✅ Preload completed!");
            AddLog($"Preload progress: {ResourceManager.Instance.PreloadProgress:P0}");

            // 모든 리소스가 캐시되었는지 확인
            int cachedCount = 0;
            foreach (var address in _testAddresses)
            {
                if (ResourceManager.Instance.IsCached(address))
                {
                    cachedCount++;
                }
            }

            AddLog($"Cached resources: {cachedCount}/{_testAddresses.Count}");
        });
    }

    #endregion

    #region 인스턴스 생성 테스트

    /// <summary>
    /// 인스턴스 생성 테스트
    /// </summary>
    private void TestInstantiate()
    {
        if (_testAddresses.Count == 0)
        {
            AddLog("[WARNING] No test addresses provided for instantiate");
            return;
        }

        string testAddress = _testAddresses[0];
        AddLog($"Instantiating prefab: {testAddress}");

        // 비동기 인스턴스 생성 1
        ResourceManager.Instance.InstantiateAsync(testAddress, transform, (instance1) =>
        {
            if (instance1 != null)
            {
                instance1.transform.position = new Vector3(-2f, 0f, 0f);
                AddLog($"✅ Async instantiate succeeded: {instance1.name}");
            }
            else
            {
                AddLog($"❌ Async instantiate failed");
            }
        });

        // 비동기 인스턴스 생성 2
        if (_testAddresses.Count > 1)
        {
            string testAddress2 = _testAddresses[1];
            AddLog($"Instantiating prefab asynchronously: {testAddress2}");

            ResourceManager.Instance.InstantiateAsync(testAddress2, transform, (instance2) =>
            {
                if (instance2 != null)
                {
                    instance2.transform.position = new Vector3(2f, 0f, 0f);
                    AddLog($"✅ Async instantiate succeeded: {instance2.name}");
                }
                else
                {
                    AddLog($"❌ Async instantiate failed");
                }
            });
        }
    }

    /// <summary>
    /// 다중 인스턴스 생성 및 해제 테스트
    /// </summary>
    private IEnumerator TestMultipleInstantiate()
    {
        if (_testAddresses.Count == 0)
        {
            AddLog("[WARNING] No test addresses provided for multiple instantiate");
            yield break;
        }

        AddLog($"Creating multiple instances from {_testAddresses.Count} prefabs...");

        // 다중 인스턴스 생성
        bool instantiateCompleted = false;
        List<GameObject> createdInstances = null;

        ResourceManager.Instance.InstantiateMultipleAsync(_testAddresses, transform, (instances) =>
        {
            createdInstances = instances;
            instantiateCompleted = true;

            AddLog($"✅ Multiple instantiate completed: {instances.Count} instances created");

            // 인스턴스 배치 (원형으로 배치)
            float radius = 3f;
            float angleStep = 360f / instances.Count;

            for (int i = 0; i < instances.Count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
                instances[i].transform.position = position;
            }

            AddLog($"Instances positioned in circle formation");
        });

        // 생성 완료 대기
        yield return new WaitUntil(() => instantiateCompleted);
        yield return new WaitForSeconds(2f); // 2초 동안 표시

        // 다중 인스턴스 해제
        if (createdInstances != null && createdInstances.Count > 0)
        {
            AddLog($"Destroying {createdInstances.Count} instances...");

            bool destroyCompleted = false;
            ResourceManager.Instance.DestroyMultipleAsync(createdInstances, () =>
            {
                destroyCompleted = true;
                AddLog($"✅ Multiple destroy completed");
            });

            // 삭제 완료 대기
            yield return new WaitUntil(() => destroyCompleted);
        }
    }

    #endregion

    #region 리소스 해제 테스트

    /// <summary>
    /// 리소스 해제 테스트
    /// </summary>
    private void TestRelease()
    {
        if (_testAddresses.Count == 0)
        {
            AddLog("[WARNING] No test addresses provided for release");
            return;
        }

        // 특정 리소스 해제
        string testAddress = _testAddresses[0];
        AddLog($"Releasing resource: {testAddress}");

        ResourceManager.Instance.Release(testAddress);

        bool isCached = ResourceManager.Instance.IsCached(testAddress);
        AddLog($"Is still cached after release: {isCached}");

        if (!isCached)
        {
            AddLog($"✅ Release succeeded");
        }
        else
        {
            AddLog($"❌ Release failed");
        }
    }

    /// <summary>
    /// 모든 리소스 해제 테스트
    /// </summary>
    public void TestReleaseAll()
    {
        SetCurrentTest("Release All");

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

        AddLog($"Cached resources after ReleaseAll: {cachedCount}");

        if (cachedCount == 0)
        {
            AddLog($"✅ ReleaseAll succeeded");
        }
        else
        {
            AddLog($"❌ ReleaseAll failed, still {cachedCount} cached");
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
            // OnGUI로 표시되므로 Update에서는 제거
        }
    }

    #endregion
}
