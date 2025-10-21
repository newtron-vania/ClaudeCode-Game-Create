using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PoolManager 테스트 스크립트
/// Unity 에디터에서 게임 실행 시 테스트 수행
/// </summary>
public class PoolManagerTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool _runTestOnStart = true;
    [SerializeField] private float _testInterval = 2f;
    [SerializeField] private bool _testCreatePool = true;
    [SerializeField] private bool _testSpawnDespawn = true;
    [SerializeField] private bool _testMultipleSpawn = true;
    [SerializeField] private bool _testPoolExpansion = true;
    [SerializeField] private bool _testAutoDespawn = true;

    [Header("Test Prefabs")]
    [SerializeField] private GameObject _testCubePrefab;
    [SerializeField] private GameObject _testSpherePrefab;
    [SerializeField] private GameObject _testCapsulePrefab;

    [Header("Test Resources (Addressables)")]
    [SerializeField] private List<string> _testAddresses = new List<string>
    {
        "Prefabs/TestCube",
        "Prefabs/TestSphere",
        "Prefabs/TestCapsule"
    };

    [Header("UI Settings")]
    [SerializeField] private int _maxLogLines = 20;
    [SerializeField] private int _fontSize = 14;

    private List<string> _logMessages = new List<string>();
    private string _currentTest = "";

    // 테스트용 풀 이름
    private const string POOL_CUBE = "TestCubePool";
    private const string POOL_SPHERE = "TestSpherePool";
    private const string POOL_CAPSULE = "TestCapsulePool";

    private void Start()
    {
        // 테스트용 기본 프리팹 생성 (Addressables 사용 안 할 경우)
        CreateDefaultPrefabs();

        if (_runTestOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    /// <summary>
    /// 테스트용 기본 프리팹 생성
    /// </summary>
    private void CreateDefaultPrefabs()
    {
        if (_testCubePrefab == null)
        {
            _testCubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _testCubePrefab.name = "TestCube";
            _testCubePrefab.GetComponent<Renderer>().material.color = Color.red;
            _testCubePrefab.SetActive(false);
        }

        if (_testSpherePrefab == null)
        {
            _testSpherePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _testSpherePrefab.name = "TestSphere";
            _testSpherePrefab.GetComponent<Renderer>().material.color = Color.blue;
            _testSpherePrefab.SetActive(false);
        }

        if (_testCapsulePrefab == null)
        {
            _testCapsulePrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _testCapsulePrefab.name = "TestCapsule";
            _testCapsulePrefab.GetComponent<Renderer>().material.color = Color.green;
            _testCapsulePrefab.SetActive(false);
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
        float height = Screen.height * 0.5f;
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
    /// 모든 테스트 실행
    /// </summary>
    public IEnumerator RunAllTests()
    {
        AddLog("========== PoolManager Test Start ==========");

        if (_testCreatePool)
        {
            SetCurrentTest("Create Pool Test");
            TestCreatePool();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testSpawnDespawn)
        {
            SetCurrentTest("Spawn & Despawn Test");
            TestSpawnDespawn();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testMultipleSpawn)
        {
            SetCurrentTest("Multiple Spawn Test");
            yield return StartCoroutine(TestMultipleSpawn());
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testPoolExpansion)
        {
            SetCurrentTest("Pool Expansion Test");
            TestPoolExpansion();
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testAutoDespawn)
        {
            SetCurrentTest("Auto Despawn Test");
            yield return StartCoroutine(TestAutoDespawn());
            yield return new WaitForSeconds(_testInterval);
        }

        _currentTest = "";
        AddLog("========== PoolManager Test End ==========");
    }

    #region 풀 생성 테스트

    /// <summary>
    /// 풀 생성 테스트
    /// </summary>
    private void TestCreatePool()
    {
        if (PoolManager.Instance == null)
        {
            AddLog("❌ PoolManager not initialized");
            return;
        }

        AddLog("✅ PoolManager initialized");

        // Cube 풀 생성
        AddLog($"Creating pool: {POOL_CUBE}");
        PoolManager.Instance.CreatePool(POOL_CUBE, _testCubePrefab, 5, 20, true);
        var cubeInfo = PoolManager.Instance.GetPoolInfo(POOL_CUBE);
        AddLog($"Pool created - Active: {cubeInfo.active}, Available: {cubeInfo.available}, Total: {cubeInfo.total}");

        // Sphere 풀 생성
        AddLog($"Creating pool: {POOL_SPHERE}");
        PoolManager.Instance.CreatePool(POOL_SPHERE, _testSpherePrefab, 3, 10, false);
        var sphereInfo = PoolManager.Instance.GetPoolInfo(POOL_SPHERE);
        AddLog($"Pool created - Active: {sphereInfo.active}, Available: {sphereInfo.available}, Total: {sphereInfo.total}");

        AddLog("✅ Pool creation test completed");
    }

    #endregion

    #region Spawn & Despawn 테스트

    /// <summary>
    /// Spawn & Despawn 테스트
    /// </summary>
    private void TestSpawnDespawn()
    {
        if (!PoolManager.Instance.HasPool(POOL_CUBE))
        {
            AddLog("❌ Pool not found. Run CreatePool test first.");
            return;
        }

        // Spawn 테스트
        AddLog("Spawning object from pool...");
        GameObject obj1 = PoolManager.Instance.Spawn(POOL_CUBE, new Vector3(-2, 0, 0), Quaternion.identity);

        if (obj1 != null)
        {
            AddLog($"✅ Spawned: {obj1.name}");
            var info = PoolManager.Instance.GetPoolInfo(POOL_CUBE);
            AddLog($"Pool state - Active: {info.active}, Available: {info.available}");
        }
        else
        {
            AddLog("❌ Spawn failed");
            return;
        }

        // Despawn 테스트
        AddLog("Despawning object...");
        PoolManager.Instance.Despawn(POOL_CUBE, obj1);

        var infoAfter = PoolManager.Instance.GetPoolInfo(POOL_CUBE);
        AddLog($"Pool state - Active: {infoAfter.active}, Available: {infoAfter.available}");
        AddLog("✅ Spawn & Despawn test completed");
    }

    #endregion

    #region 다중 Spawn 테스트

    /// <summary>
    /// 다중 Spawn 테스트
    /// </summary>
    private IEnumerator TestMultipleSpawn()
    {
        if (!PoolManager.Instance.HasPool(POOL_CUBE))
        {
            AddLog("❌ Pool not found. Run CreatePool test first.");
            yield break;
        }

        AddLog("Spawning multiple objects in circle formation...");

        List<GameObject> spawnedObjects = new List<GameObject>();
        int spawnCount = 8;
        float radius = 3f;

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = i * (360f / spawnCount) * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            GameObject obj = PoolManager.Instance.Spawn(POOL_CUBE, position, Quaternion.identity);
            if (obj != null)
            {
                spawnedObjects.Add(obj);
            }
        }

        var info = PoolManager.Instance.GetPoolInfo(POOL_CUBE);
        AddLog($"✅ Spawned {spawnedObjects.Count} objects");
        AddLog($"Pool state - Active: {info.active}, Available: {info.available}");

        yield return new WaitForSeconds(2f);

        // 모두 Despawn
        AddLog("Despawning all objects...");
        PoolManager.Instance.DespawnAll(POOL_CUBE);

        var infoAfter = PoolManager.Instance.GetPoolInfo(POOL_CUBE);
        AddLog($"Pool state - Active: {infoAfter.active}, Available: {infoAfter.available}");
        AddLog("✅ Multiple spawn test completed");
    }

    #endregion

    #region 풀 확장 테스트

    /// <summary>
    /// 풀 확장 테스트
    /// </summary>
    private void TestPoolExpansion()
    {
        // 확장 가능한 풀 생성
        string expandablePool = "ExpandablePool";
        AddLog($"Creating expandable pool: {expandablePool}");
        PoolManager.Instance.CreatePool(expandablePool, _testCapsulePrefab, 2, 5, true);

        var initialInfo = PoolManager.Instance.GetPoolInfo(expandablePool);
        AddLog($"Initial - Active: {initialInfo.active}, Available: {initialInfo.available}, Total: {initialInfo.total}");

        // 최대치 이상 Spawn
        AddLog("Spawning more than max size...");
        List<GameObject> objects = new List<GameObject>();

        for (int i = 0; i < 7; i++)
        {
            GameObject obj = PoolManager.Instance.Spawn(expandablePool, new Vector3(i, 0, 0), Quaternion.identity);
            if (obj != null)
            {
                objects.Add(obj);
            }
        }

        var expandedInfo = PoolManager.Instance.GetPoolInfo(expandablePool);
        AddLog($"After expansion - Active: {expandedInfo.active}, Available: {expandedInfo.available}, Total: {expandedInfo.total}");

        if (expandedInfo.total > initialInfo.total)
        {
            AddLog("✅ Pool expanded successfully");
        }
        else
        {
            AddLog("❌ Pool expansion failed");
        }

        // 정리
        PoolManager.Instance.DespawnAll(expandablePool);
        PoolManager.Instance.DestroyPool(expandablePool);
    }

    #endregion

    #region 자동 Despawn 테스트

    /// <summary>
    /// 자동 Despawn 테스트
    /// </summary>
    private IEnumerator TestAutoDespawn()
    {
        if (!PoolManager.Instance.HasPool(POOL_SPHERE))
        {
            AddLog("❌ Pool not found. Creating new pool...");
            PoolManager.Instance.CreatePool(POOL_SPHERE, _testSpherePrefab, 5, 10, true);
        }

        AddLog("Spawning object with auto despawn (3 seconds)...");
        GameObject obj = PoolManager.Instance.Spawn(POOL_SPHERE, Vector3.zero, Quaternion.identity);

        if (obj != null)
        {
            AddLog($"✅ Spawned: {obj.name}");
            PoolManager.Instance.DespawnAfter(POOL_SPHERE, obj, 3f);

            var infoBefore = PoolManager.Instance.GetPoolInfo(POOL_SPHERE);
            AddLog($"Before auto despawn - Active: {infoBefore.active}");

            yield return new WaitForSeconds(3.5f);

            var infoAfter = PoolManager.Instance.GetPoolInfo(POOL_SPHERE);
            AddLog($"After auto despawn - Active: {infoAfter.active}");

            if (infoAfter.active < infoBefore.active)
            {
                AddLog("✅ Auto despawn test completed");
            }
            else
            {
                AddLog("❌ Auto despawn failed");
            }
        }
        else
        {
            AddLog("❌ Spawn failed");
        }
    }

    #endregion

    #region 풀 정보 출력

    /// <summary>
    /// 모든 풀 정보 출력
    /// </summary>
    public void PrintAllPoolInfo()
    {
        SetCurrentTest("Pool Information");
        PoolManager.Instance.PrintAllPoolInfo();
    }

    #endregion
}
