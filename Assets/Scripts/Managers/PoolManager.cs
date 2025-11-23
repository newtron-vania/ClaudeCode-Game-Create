using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링 관리 매니저
/// GameObject 재사용을 통한 성능 최적화
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    /// <summary>
    /// 풀 데이터 클래스
    /// </summary>
    [System.Serializable]
    public class Pool
    {
        public string PoolName;
        public GameObject Prefab;
        public int InitialSize = 10;
        public int MaxSize = 100;
        public bool CanExpand = true;
        public Transform ParentTransform;
    }

    [Header("Pool Settings")]
    [SerializeField] private List<Pool> _pools = new List<Pool>();
    [SerializeField] private Transform _poolRootTransform;

    // 풀 딕셔너리 (key: 풀 이름, value: 풀 데이터)
    private Dictionary<string, PoolData> _poolDictionary = new Dictionary<string, PoolData>();

    /// <summary>
    /// 풀 데이터 (내부 관리용)
    /// </summary>
    private class PoolData
    {
        public string PoolName;
        public GameObject Prefab;
        public int MaxSize;
        public bool CanExpand;
        public Transform ParentTransform;
        public Queue<GameObject> AvailableObjects = new Queue<GameObject>();
        public List<GameObject> ActiveObjects = new List<GameObject>();
        public int TotalCreated = 0;
    }

    protected override void Awake()
    {
        base.Awake();

        // 풀 루트 Transform 생성
        if (_poolRootTransform == null)
        {
            GameObject poolRoot = new GameObject("PoolRoot");
            poolRoot.transform.SetParent(transform);
            _poolRootTransform = poolRoot.transform;
        }

        // 사전 정의된 풀 초기화
        foreach (var pool in _pools)
        {
            CreatePool(pool.PoolName, pool.Prefab, pool.InitialSize, pool.MaxSize, pool.CanExpand, pool.ParentTransform);
        }

        Debug.Log("[INFO] PoolManager::Awake - PoolManager initialized");
    }

    #region 풀 생성 및 관리

    /// <summary>
    /// 새 풀 생성
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 생성 개수</param>
    /// <param name="maxSize">최대 개수</param>
    /// <param name="canExpand">최대 개수 초과 시 확장 가능 여부</param>
    /// <param name="parentTransform">부모 Transform (null이면 PoolRoot 사용)</param>
    public void CreatePool(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 100, bool canExpand = true, Transform parentTransform = null)
    {
        if (string.IsNullOrEmpty(poolName))
        {
            Debug.LogError("[ERROR] PoolManager::CreatePool - Pool name is null or empty");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError($"[ERROR] PoolManager::CreatePool - Prefab is null for pool: {poolName}");
            return;
        }

        // 이미 존재하는 풀이면 경고
        if (_poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"[WARNING] PoolManager::CreatePool - Pool already exists: {poolName}");
            return;
        }

        // 부모 Transform 설정
        Transform parent = parentTransform != null ? parentTransform : _poolRootTransform;

        // 풀 데이터 생성
        PoolData poolData = new PoolData
        {
            PoolName = poolName,
            Prefab = prefab,
            MaxSize = maxSize,
            CanExpand = canExpand,
            ParentTransform = parent
        };

        // 풀에 추가
        _poolDictionary[poolName] = poolData;

        // 초기 오브젝트 생성
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject(poolData);
        }

        Debug.Log($"[INFO] PoolManager::CreatePool - Pool created: {poolName} (Initial: {initialSize}, Max: {maxSize})");
    }

    /// <summary>
    /// Addressables를 사용한 풀 생성
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="prefabAddress">프리팹 Addressable 주소</param>
    /// <param name="initialSize">초기 생성 개수</param>
    /// <param name="maxSize">최대 개수</param>
    /// <param name="canExpand">확장 가능 여부</param>
    /// <param name="parentTransform">부모 Transform</param>
    /// <param name="onComplete">완료 콜백</param>
    public void CreatePoolAsync(string poolName, string prefabAddress, int initialSize = 10, int maxSize = 100, bool canExpand = true, Transform parentTransform = null, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(poolName) || string.IsNullOrEmpty(prefabAddress))
        {
            Debug.LogError("[ERROR] PoolManager::CreatePoolAsync - Pool name or address is null or empty");
            onComplete?.Invoke();
            return;
        }

        ResourceManager.Instance.LoadAsync<GameObject>(prefabAddress, (prefab) =>
        {
            if (prefab != null)
            {
                CreatePool(poolName, prefab, initialSize, maxSize, canExpand, parentTransform);
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError($"[ERROR] PoolManager::CreatePoolAsync - Failed to load prefab: {prefabAddress}");
                onComplete?.Invoke();
            }
        });
    }

    /// <summary>
    /// 풀 존재 여부 확인
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <returns>존재 여부</returns>
    public bool HasPool(string poolName)
    {
        return _poolDictionary.ContainsKey(poolName);
    }

    /// <summary>
    /// 풀 삭제
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    public void DestroyPool(string poolName)
    {
        if (!_poolDictionary.TryGetValue(poolName, out PoolData poolData))
        {
            Debug.LogWarning($"[WARNING] PoolManager::DestroyPool - Pool not found: {poolName}");
            return;
        }

        // 사용 가능한 오브젝트 삭제
        while (poolData.AvailableObjects.Count > 0)
        {
            GameObject obj = poolData.AvailableObjects.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // 활성 오브젝트 삭제
        foreach (var obj in poolData.ActiveObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        poolData.ActiveObjects.Clear();
        _poolDictionary.Remove(poolName);

        Debug.Log($"[INFO] PoolManager::DestroyPool - Pool destroyed: {poolName}");
    }

    #endregion

    #region 오브젝트 생성 및 반환

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <returns>풀링된 GameObject (실패 시 null)</returns>
    public GameObject Spawn(string poolName)
    {
        if (!_poolDictionary.TryGetValue(poolName, out PoolData poolData))
        {
            Debug.LogError($"[ERROR] PoolManager::Spawn - Pool not found: {poolName}");
            return null;
        }

        GameObject obj = null;

        // 사용 가능한 오브젝트가 있으면 재사용
        if (poolData.AvailableObjects.Count > 0)
        {
            obj = poolData.AvailableObjects.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            // 사용 가능한 오브젝트가 없으면 새로 생성
            if (poolData.CanExpand || poolData.TotalCreated < poolData.MaxSize)
            {
                obj = CreateNewObject(poolData);
                obj.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[WARNING] PoolManager::Spawn - Pool at max capacity and cannot expand: {poolName}");
                return null;
            }
        }

        poolData.ActiveObjects.Add(obj);

        // IPoolable 인터페이스 구현 체크 및 호출
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnSpawnedFromPool();
        }

        return obj;
    }

    /// <summary>
    /// 위치와 회전을 지정하여 오브젝트 가져오기
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    /// <returns>풀링된 GameObject</returns>
    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Spawn(poolName);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    /// <summary>
    /// 부모를 지정하여 오브젝트 가져오기
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="parent">부모 Transform</param>
    /// <returns>풀링된 GameObject</returns>
    public GameObject Spawn(string poolName, Transform parent)
    {
        GameObject obj = Spawn(poolName);
        if (obj != null)
        {
            obj.transform.SetParent(parent);
        }
        return obj;
    }

    /// <summary>
    /// 오브젝트를 풀로 반환
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="obj">반환할 GameObject</param>
    public void Despawn(string poolName, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[WARNING] PoolManager::Despawn - GameObject is null");
            return;
        }

        if (!_poolDictionary.TryGetValue(poolName, out PoolData poolData))
        {
            Debug.LogError($"[ERROR] PoolManager::Despawn - Pool not found: {poolName}");
            Destroy(obj);
            return;
        }

        // 활성 오브젝트 목록에서 제거
        poolData.ActiveObjects.Remove(obj);

        // IPoolable 인터페이스 구현 체크 및 호출
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnReturnedToPool();
        }

        // 오브젝트 비활성화 및 풀로 반환
        obj.SetActive(false);
        obj.transform.SetParent(poolData.ParentTransform);
        poolData.AvailableObjects.Enqueue(obj);
    }

    /// <summary>
    /// GameObject를 풀로 반환 (자동으로 풀 이름 찾기)
    /// </summary>
    /// <param name="obj">반환할 GameObject</param>
    public void Despawn(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[WARNING] PoolManager::Despawn - GameObject is null");
            return;
        }

        // 오브젝트가 속한 풀 찾기
        foreach (var kvp in _poolDictionary)
        {
            if (kvp.Value.ActiveObjects.Contains(obj))
            {
                Despawn(kvp.Key, obj);
                return;
            }
        }

        Debug.LogWarning($"[WARNING] PoolManager::Despawn - Object not found in any pool: {obj.name}");
        Destroy(obj);
    }

    /// <summary>
    /// 일정 시간 후 자동으로 풀로 반환
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <param name="obj">반환할 GameObject</param>
    /// <param name="delay">지연 시간 (초)</param>
    public void DespawnAfter(string poolName, GameObject obj, float delay)
    {
        if (obj != null)
        {
            StartCoroutine(DespawnAfterCoroutine(poolName, obj, delay));
        }
    }

    /// <summary>
    /// 자동 반환 코루틴
    /// </summary>
    private System.Collections.IEnumerator DespawnAfterCoroutine(string poolName, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
        {
            Despawn(poolName, obj);
        }
    }

    #endregion

    #region 풀 관리

    /// <summary>
    /// 특정 풀의 모든 활성 오브젝트 반환
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    public void DespawnAll(string poolName)
    {
        if (!_poolDictionary.TryGetValue(poolName, out PoolData poolData))
        {
            Debug.LogWarning($"[WARNING] PoolManager::DespawnAll - Pool not found: {poolName}");
            return;
        }

        // 활성 오브젝트를 모두 비활성화하고 풀로 반환
        List<GameObject> activeObjects = new List<GameObject>(poolData.ActiveObjects);
        foreach (var obj in activeObjects)
        {
            if (obj != null)
            {
                Despawn(poolName, obj);
            }
        }

        Debug.Log($"[INFO] PoolManager::DespawnAll - All objects returned to pool: {poolName}");
    }

    /// <summary>
    /// 모든 풀의 활성 오브젝트 반환
    /// </summary>
    public void DespawnAll()
    {
        foreach (var poolName in _poolDictionary.Keys)
        {
            DespawnAll(poolName);
        }

        Debug.Log("[INFO] PoolManager::DespawnAll - All objects returned to all pools");
    }

    /// <summary>
    /// 풀 정보 가져오기
    /// </summary>
    /// <param name="poolName">풀 이름</param>
    /// <returns>풀 정보 (활성 개수, 사용 가능 개수, 총 개수)</returns>
    public (int active, int available, int total) GetPoolInfo(string poolName)
    {
        if (!_poolDictionary.TryGetValue(poolName, out PoolData poolData))
        {
            return (0, 0, 0);
        }

        return (poolData.ActiveObjects.Count, poolData.AvailableObjects.Count, poolData.TotalCreated);
    }

    /// <summary>
    /// 모든 풀 정보 출력
    /// </summary>
    public void PrintAllPoolInfo()
    {
        Debug.Log("===== Pool Manager Info =====");
        foreach (var kvp in _poolDictionary)
        {
            var poolData = kvp.Value;
            Debug.Log($"Pool: {kvp.Key} - Active: {poolData.ActiveObjects.Count}, Available: {poolData.AvailableObjects.Count}, Total: {poolData.TotalCreated}");
        }
    }

    #endregion

    #region 내부 헬퍼 메서드

    /// <summary>
    /// 새 오브젝트 생성
    /// </summary>
    private GameObject CreateNewObject(PoolData poolData)
    {
        GameObject obj = Instantiate(poolData.Prefab, poolData.ParentTransform);
        obj.name = $"{poolData.PoolName}_{poolData.TotalCreated}";
        obj.SetActive(false);

        poolData.AvailableObjects.Enqueue(obj);
        poolData.TotalCreated++;

        return obj;
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 모든 풀 삭제
        List<string> poolNames = new List<string>(_poolDictionary.Keys);
        foreach (var poolName in poolNames)
        {
            DestroyPool(poolName);
        }

        Debug.Log("[INFO] PoolManager::OnDestroy - PoolManager destroyed");
    }

    #endregion
}
