using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

/// <summary>
/// Addressables 및 Resources 기반 리소스 관리 매니저
/// 리소스 로드, 캐싱, 해제 담당 (GameObject 인스턴스는 PoolManager 연동)
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    // 로드된 리소스 캐시 (key: 리소스 주소, value: 로드된 오브젝트)
    private Dictionary<string, Object> _cachedResources = new Dictionary<string, Object>();

    // 비동기 작업 핸들 관리 (메모리 해제를 위해 보관)
    private Dictionary<string, AsyncOperationHandle> _asyncHandles = new Dictionary<string, AsyncOperationHandle>();

    // Resources 폴더에서 로드된 리소스 추적 (Addressables와 구분하기 위해)
    private HashSet<string> _resourcesFolderKeys = new HashSet<string>();

    // 프리로드 진행 상황
    private bool _isPreloading = false;
    private float _preloadProgress = 0f;

    /// <summary>
    /// 프리로드 진행 중 여부
    /// </summary>
    public bool IsPreloading => _isPreloading;

    /// <summary>
    /// 프리로드 진행률 (0.0 ~ 1.0)
    /// </summary>
    public float PreloadProgress => _preloadProgress;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[INFO] ResourceManager::Awake - ResourceManager initialized");
    }

    #region 동기 로드

    /// <summary>
    /// 리소스 동기 로드 (캐시 먼저 확인)
    /// </summary>
    /// <typeparam name="T">리소스 타입</typeparam>
    /// <param name="address">Addressable 주소</param>
    /// <returns>로드된 리소스</returns>
    public T Load<T>(string address) where T : Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[ERROR] ResourceManager::Load - Address is null or empty");
            return null;
        }

        // 캐시에 있으면 반환
        if (_cachedResources.TryGetValue(address, out Object cachedResource))
        {
            Debug.Log($"[INFO] ResourceManager::Load - Returning cached resource: {address}");
            return cachedResource as T;
        }

        // Addressables로 동기 로드
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            T resource = handle.WaitForCompletion();

            if (resource == null)
            {
                Debug.LogError($"[ERROR] ResourceManager::Load - Failed to load resource: {address}");
                return null;
            }

            // 캐시에 저장
            _cachedResources[address] = resource;
            _asyncHandles[address] = handle;

            Debug.Log($"[INFO] ResourceManager::Load - Loaded and cached resource: {address}");
            return resource;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ERROR] ResourceManager::Load - Exception while loading {address}: {e.Message}");
            return null;
        }
    }

    #endregion

    #region 비동기 로드

    /// <summary>
    /// 리소스 비동기 로드 (캐시 먼저 확인)
    /// </summary>
    /// <typeparam name="T">리소스 타입</typeparam>
    /// <param name="address">Addressable 주소</param>
    /// <param name="onComplete">로드 완료 콜백</param>
    public void LoadAsync<T>(string address, Action<T> onComplete) where T : Object
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[ERROR] ResourceManager::LoadAsync - Address is null or empty");
            onComplete?.Invoke(null);
            return;
        }

        // 캐시에 있으면 즉시 반환
        if (_cachedResources.TryGetValue(address, out Object cachedResource))
        {
            Debug.Log($"[INFO] ResourceManager::LoadAsync - Returning cached resource: {address}");
            onComplete?.Invoke(cachedResource as T);
            return;
        }

        // Addressables로 비동기 로드
        var handle = Addressables.LoadAssetAsync<T>(address);
        handle.Completed += (asyncHandle) =>
        {
            if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
            {
                T resource = asyncHandle.Result;

                // 캐시에 저장
                _cachedResources[address] = resource;
                _asyncHandles[address] = handle;

                Debug.Log($"[INFO] ResourceManager::LoadAsync - Loaded and cached resource: {address}");
                onComplete?.Invoke(resource);
            }
            else
            {
                Debug.LogError($"[ERROR] ResourceManager::LoadAsync - Failed to load resource: {address}");
                onComplete?.Invoke(null);
            }
        };
    }

    #endregion

    #region 리소스 캐싱

    /// <summary>
    /// 리소스가 캐시되어 있는지 확인
    /// </summary>
    /// <param name="address">Addressable 주소</param>
    /// <returns>캐시 여부</returns>
    public bool IsCached(string address)
    {
        return _cachedResources.ContainsKey(address);
    }

    /// <summary>
    /// 캐시에서 리소스 가져오기
    /// </summary>
    /// <typeparam name="T">리소스 타입</typeparam>
    /// <param name="address">Addressable 주소</param>
    /// <returns>캐시된 리소스 (없으면 null)</returns>
    public T GetCached<T>(string address) where T : Object
    {
        if (_cachedResources.TryGetValue(address, out Object resource))
        {
            return resource as T;
        }

        return null;
    }

    #endregion

    #region 리소스 해제

    /// <summary>
    /// 특정 리소스 해제
    /// </summary>
    /// <param name="address">Addressable 주소</param>
    public void Release(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning("[WARNING] ResourceManager::Release - Address is null or empty");
            return;
        }

        // 캐시에서 제거
        if (_cachedResources.ContainsKey(address))
        {
            _cachedResources.Remove(address);
        }

        // 핸들 해제
        if (_asyncHandles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle);
            _asyncHandles.Remove(address);
            Debug.Log($"[INFO] ResourceManager::Release - Released resource: {address}");
        }
    }

    /// <summary>
    /// 모든 캐시된 리소스 해제
    /// </summary>
    public void ReleaseAll()
    {
        Debug.Log("[INFO] ResourceManager::ReleaseAll - Releasing all cached resources");

        // 모든 핸들 해제
        foreach (var handle in _asyncHandles.Values)
        {
            Addressables.Release(handle);
        }

        // 캐시 초기화
        _cachedResources.Clear();
        _asyncHandles.Clear();

        Debug.Log("[INFO] ResourceManager::ReleaseAll - All resources released");
    }

    /// <summary>
    /// 사용하지 않는 리소스만 해제
    /// </summary>
    /// <param name="addressesToKeep">유지할 리소스 주소 목록</param>
    public void ReleaseUnused(List<string> addressesToKeep)
    {
        if (addressesToKeep == null)
        {
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseUnused - addressesToKeep is null");
            return;
        }

        List<string> addressesToRelease = new List<string>();

        // 유지 목록에 없는 리소스 찾기
        foreach (var address in _cachedResources.Keys)
        {
            if (!addressesToKeep.Contains(address))
            {
                addressesToRelease.Add(address);
            }
        }

        // 해제
        foreach (var address in addressesToRelease)
        {
            Release(address);
        }

        Debug.Log($"[INFO] ResourceManager::ReleaseUnused - Released {addressesToRelease.Count} unused resources");
    }

    #endregion

    #region 프리로드

    /// <summary>
    /// 여러 리소스 프리로드
    /// </summary>
    /// <typeparam name="T">리소스 타입</typeparam>
    /// <param name="addresses">Addressable 주소 목록</param>
    /// <param name="onComplete">프리로드 완료 콜백</param>
    public void PreloadAsync<T>(List<string> addresses, Action onComplete = null) where T : Object
    {
        if (addresses == null || addresses.Count == 0)
        {
            Debug.LogWarning("[WARNING] ResourceManager::PreloadAsync - Addresses list is null or empty");
            onComplete?.Invoke();
            return;
        }

        _isPreloading = true;
        _preloadProgress = 0f;

        int totalCount = addresses.Count;
        int loadedCount = 0;

        Debug.Log($"[INFO] ResourceManager::PreloadAsync - Preloading {totalCount} resources");

        foreach (var address in addresses)
        {
            LoadAsync<T>(address, (resource) =>
            {
                loadedCount++;
                _preloadProgress = (float)loadedCount / totalCount;

                Debug.Log($"[INFO] ResourceManager::PreloadAsync - Progress: {loadedCount}/{totalCount} ({_preloadProgress:P0})");

                // 모든 리소스 로드 완료
                if (loadedCount >= totalCount)
                {
                    _isPreloading = false;
                    _preloadProgress = 1f;
                    Debug.Log("[INFO] ResourceManager::PreloadAsync - Preload completed");
                    onComplete?.Invoke();
                }
            });
        }
    }

    #endregion

    #region 인스턴스 생성 (PoolManager 연동)

    /// <summary>
    /// 프리팹 인스턴스 비동기 생성 (PoolManager 사용)
    /// 풀이 없으면 자동으로 생성하고, 풀에서 오브젝트를 가져옴
    /// </summary>
    /// <param name="address">프리팹 Addressable 주소</param>
    /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
    /// <param name="onComplete">생성 완료 콜백</param>
    public void InstantiateAsync(string address, Transform parent, Action<GameObject> onComplete)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[ERROR] ResourceManager::InstantiateAsync - Address is null or empty");
            onComplete?.Invoke(null);
            return;
        }

        // PoolManager가 없으면 경고
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[ERROR] ResourceManager::InstantiateAsync - PoolManager is not initialized");
            onComplete?.Invoke(null);
            return;
        }

        // 풀이 존재하는지 확인
        if (PoolManager.Instance.HasPool(address))
        {
            // 이미 풀이 있으면 바로 Spawn
            GameObject instance = PoolManager.Instance.Spawn(address, parent);
            Debug.Log($"[INFO] ResourceManager::InstantiateAsync - Spawned from existing pool: {address}");
            onComplete?.Invoke(instance);
        }
        else
        {
            // 풀이 없으면 프리팹 로드 후 풀 생성
            LoadAsync<GameObject>(address, (prefab) =>
            {
                if (prefab != null)
                {
                    // 풀 생성
                    PoolManager.Instance.CreatePool(address, prefab, 10, 100, true);

                    // Spawn
                    GameObject instance = PoolManager.Instance.Spawn(address, parent);
                    Debug.Log($"[INFO] ResourceManager::InstantiateAsync - Created pool and spawned: {address}");
                    onComplete?.Invoke(instance);
                }
                else
                {
                    Debug.LogError($"[ERROR] ResourceManager::InstantiateAsync - Failed to load prefab: {address}");
                    onComplete?.Invoke(null);
                }
            });
        }
    }

    /// <summary>
    /// 여러 프리팹 인스턴스 비동기 일괄 생성
    /// </summary>
    /// <param name="addresses">프리팹 Addressable 주소 목록</param>
    /// <param name="parent">부모 Transform (null이면 씬 루트)</param>
    /// <param name="onComplete">모든 생성 완료 콜백 (생성된 인스턴스 목록 전달)</param>
    public void InstantiateMultipleAsync(List<string> addresses, Transform parent, Action<List<GameObject>> onComplete)
    {
        if (addresses == null || addresses.Count == 0)
        {
            Debug.LogWarning("[WARNING] ResourceManager::InstantiateMultipleAsync - Addresses list is null or empty");
            onComplete?.Invoke(new List<GameObject>());
            return;
        }

        List<GameObject> instances = new List<GameObject>();
        int totalCount = addresses.Count;
        int completedCount = 0;

        Debug.Log($"[INFO] ResourceManager::InstantiateMultipleAsync - Instantiating {totalCount} prefabs");

        foreach (var address in addresses)
        {
            InstantiateAsync(address, parent, (instance) =>
            {
                if (instance != null)
                {
                    instances.Add(instance);
                }

                completedCount++;

                // 모든 인스턴스 생성 완료
                if (completedCount >= totalCount)
                {
                    Debug.Log($"[INFO] ResourceManager::InstantiateMultipleAsync - Completed: {instances.Count}/{totalCount} instances created");
                    onComplete?.Invoke(instances);
                }
            });
        }
    }

    /// <summary>
    /// GameObject 삭제 (PoolManager를 통해 풀로 반환)
    /// </summary>
    /// <param name="instance">삭제할 GameObject</param>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseInstance - Instance is null");
            return;
        }

        // PoolManager가 없으면 그냥 Destroy
        if (PoolManager.Instance == null)
        {
            Object.Destroy(instance);
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseInstance - PoolManager not available, destroyed instance");
            return;
        }

        // PoolManager를 통해 Despawn
        PoolManager.Instance.Despawn(instance);
        Debug.Log($"[INFO] ResourceManager::ReleaseInstance - Returned instance to pool: {instance.name}");
    }

    /// <summary>
    /// 여러 GameObject 비동기 일괄 삭제
    /// </summary>
    /// <param name="instances">삭제할 GameObject 목록</param>
    /// <param name="onComplete">모든 삭제 완료 콜백</param>
    public void DestroyMultipleAsync(List<GameObject> instances, Action onComplete = null)
    {
        if (instances == null || instances.Count == 0)
        {
            Debug.LogWarning("[WARNING] ResourceManager::DestroyMultipleAsync - Instances list is null or empty");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[INFO] ResourceManager::DestroyMultipleAsync - Destroying {instances.Count} instances");

        // 모든 인스턴스 삭제
        foreach (var instance in instances)
        {
            ReleaseInstance(instance);
        }

        instances.Clear();
        Debug.Log("[INFO] ResourceManager::DestroyMultipleAsync - All instances destroyed");
        onComplete?.Invoke();
    }

    /// <summary>
    /// 모든 풀 인스턴스 해제 (PoolManager 연동)
    /// </summary>
    public void ReleaseAllInstances()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseAllInstances - PoolManager not available");
            return;
        }

        Debug.Log("[INFO] ResourceManager::ReleaseAllInstances - Returning all instances to pools");
        PoolManager.Instance.DespawnAll();
        Debug.Log("[INFO] ResourceManager::ReleaseAllInstances - All instances returned to pools");
    }

    #endregion

    #region 풀 관리 (PoolManager 연동)

    /// <summary>
    /// 특정 주소의 풀 미리 생성
    /// </summary>
    /// <param name="address">프리팹 Addressable 주소</param>
    /// <param name="initialSize">초기 개수</param>
    /// <param name="maxSize">최대 개수</param>
    /// <param name="canExpand">확장 가능 여부</param>
    /// <param name="onComplete">완료 콜백</param>
    public void CreatePool(string address, int initialSize = 10, int maxSize = 100, bool canExpand = true, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[ERROR] ResourceManager::CreatePool - Address is null or empty");
            onComplete?.Invoke();
            return;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError("[ERROR] ResourceManager::CreatePool - PoolManager is not initialized");
            onComplete?.Invoke();
            return;
        }

        // Addressables로 풀 생성
        PoolManager.Instance.CreatePoolAsync(address, address, initialSize, maxSize, canExpand, null, onComplete);
        Debug.Log($"[INFO] ResourceManager::CreatePool - Creating pool for: {address}");
    }

    /// <summary>
    /// 프리로드와 동시에 풀 생성
    /// </summary>
    /// <param name="addresses">프리팹 Addressable 주소 목록</param>
    /// <param name="initialSize">각 풀의 초기 개수</param>
    /// <param name="maxSize">각 풀의 최대 개수</param>
    /// <param name="onComplete">완료 콜백</param>
    public void PreloadAndCreatePools(List<string> addresses, int initialSize = 10, int maxSize = 100, Action onComplete = null)
    {
        if (addresses == null || addresses.Count == 0)
        {
            Debug.LogWarning("[WARNING] ResourceManager::PreloadAndCreatePools - Addresses list is null or empty");
            onComplete?.Invoke();
            return;
        }

        int totalCount = addresses.Count;
        int completedCount = 0;

        Debug.Log($"[INFO] ResourceManager::PreloadAndCreatePools - Creating {totalCount} pools");

        foreach (var address in addresses)
        {
            CreatePool(address, initialSize, maxSize, true, () =>
            {
                completedCount++;

                if (completedCount >= totalCount)
                {
                    Debug.Log("[INFO] ResourceManager::PreloadAndCreatePools - All pools created");
                    onComplete?.Invoke();
                }
            });
        }
    }

    #endregion

    #region Resources 폴더 로드 (Unity 기본 시스템)

    /// <summary>
    /// Resources 폴더에서 리소스 동기 로드 (제네릭)
    /// </summary>
    /// <typeparam name="T">리소스 타입</typeparam>
    /// <param name="path">Resources 폴더 내 경로 (예: "Prefabs/Enemy")</param>
    /// <returns>로드된 리소스</returns>
    public T LoadFromResources<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[ERROR] ResourceManager::LoadFromResources - Path is null or empty");
            return null;
        }

        string cacheKey = $"Resources:{path}";

        // 캐시에 있으면 반환
        if (_cachedResources.TryGetValue(cacheKey, out Object cachedResource))
        {
            Debug.Log($"[INFO] ResourceManager::LoadFromResources - Returning cached resource: {path}");
            return cachedResource as T;
        }

        // Resources.Load로 로드
        T resource = Resources.Load<T>(path);

        if (resource == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::LoadFromResources - Failed to load resource from Resources folder: {path}");
            return null;
        }

        // 캐시에 저장
        _cachedResources[cacheKey] = resource;
        _resourcesFolderKeys.Add(cacheKey);

        Debug.Log($"[INFO] ResourceManager::LoadFromResources - Loaded and cached resource from Resources folder: {path}");
        return resource;
    }

    /// <summary>
    /// Resources 폴더에서 GameObject 로드 (제네릭)
    /// </summary>
    /// <param name="path">Resources 폴더 내 경로</param>
    /// <returns>로드된 GameObject</returns>
    public GameObject LoadGameObjectFromResources(string path)
    {
        return LoadFromResources<GameObject>(path);
    }

    /// <summary>
    /// Resources 폴더에서 GameObject 인스턴스 생성 (제네릭)
    /// </summary>
    /// <typeparam name="T">컴포넌트 타입</typeparam>
    /// <param name="path">Resources 폴더 내 경로</param>
    /// <param name="parent">부모 Transform</param>
    /// <returns>생성된 GameObject의 컴포넌트</returns>
    public T InstantiateFromResources<T>(string path, Transform parent = null) where T : Component
    {
        GameObject prefab = LoadFromResources<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::InstantiateFromResources - Failed to load prefab: {path}");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);

        if (instance == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::InstantiateFromResources - Failed to instantiate: {path}");
            return null;
        }

        T component = instance.GetComponent<T>();

        if (component == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::InstantiateFromResources - Component {typeof(T).Name} not found on instantiated object");
            return null;
        }

        Debug.Log($"[INFO] ResourceManager::InstantiateFromResources - Instantiated from Resources: {path}");
        return component;
    }

    /// <summary>
    /// Resources 폴더에서 GameObject 인스턴스 생성 (GameObject 반환)
    /// </summary>
    /// <param name="path">Resources 폴더 내 경로</param>
    /// <param name="parent">부모 Transform</param>
    /// <returns>생성된 GameObject</returns>
    public GameObject InstantiateGameObjectFromResources(string path, Transform parent = null)
    {
        GameObject prefab = LoadFromResources<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::InstantiateGameObjectFromResources - Failed to load prefab: {path}");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);

        if (instance == null)
        {
            Debug.LogError($"[ERROR] ResourceManager::InstantiateGameObjectFromResources - Failed to instantiate: {path}");
            return null;
        }

        Debug.Log($"[INFO] ResourceManager::InstantiateGameObjectFromResources - Instantiated from Resources: {path}");
        return instance;
    }

    /// <summary>
    /// Resources 폴더에서 GameObject 프리팹 로드 후 인스턴스 생성 (GameObject 오버로드)
    /// </summary>
    /// <param name="prefab">미리 로드된 GameObject 프리팹</param>
    /// <param name="parent">부모 Transform</param>
    /// <returns>생성된 GameObject</returns>
    public GameObject InstantiateFromResources(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("[ERROR] ResourceManager::InstantiateFromResources - Prefab is null");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);

        if (instance == null)
        {
            Debug.LogError("[ERROR] ResourceManager::InstantiateFromResources - Failed to instantiate from prefab");
            return null;
        }

        Debug.Log($"[INFO] ResourceManager::InstantiateFromResources - Instantiated from prefab: {prefab.name}");
        return instance;
    }

    /// <summary>
    /// Resources 폴더에서 로드한 리소스 해제
    /// </summary>
    /// <param name="path">Resources 폴더 내 경로</param>
    public void ReleaseFromResources(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseFromResources - Path is null or empty");
            return;
        }

        string cacheKey = $"Resources:{path}";

        // 캐시에서 제거
        if (_cachedResources.ContainsKey(cacheKey))
        {
            _cachedResources.Remove(cacheKey);
            _resourcesFolderKeys.Remove(cacheKey);

            // Resources.UnloadUnusedAssets는 명시적으로 호출하지 않음 (Unity가 자동 관리)
            Debug.Log($"[INFO] ResourceManager::ReleaseFromResources - Released resource: {path}");
        }
    }

    /// <summary>
    /// Resources 폴더에서 로드한 모든 리소스 해제
    /// </summary>
    public void ReleaseAllFromResources()
    {
        Debug.Log("[INFO] ResourceManager::ReleaseAllFromResources - Releasing all Resources folder assets");

        // Resources 폴더 키 복사 (반복 중 수정 방지)
        List<string> keysToRemove = new List<string>(_resourcesFolderKeys);

        foreach (var key in keysToRemove)
        {
            _cachedResources.Remove(key);
        }

        _resourcesFolderKeys.Clear();

        // Unity에게 미사용 리소스 언로드 요청
        Resources.UnloadUnusedAssets();

        Debug.Log("[INFO] ResourceManager::ReleaseAllFromResources - All Resources folder assets released");
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 모든 인스턴스 풀로 반환
        ReleaseAllInstances();

        // 모든 리소스 해제
        ReleaseAll();
    }

    #endregion
}
