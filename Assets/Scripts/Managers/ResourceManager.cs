using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

/// <summary>
/// Addressables 기반 리소스 관리 매니저
/// 리소스 로드, 캐싱, 해제를 담당
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    // 로드된 리소스 캐시 (key: 리소스 주소, value: 로드된 오브젝트)
    private Dictionary<string, Object> _cachedResources = new Dictionary<string, Object>();

    // 비동기 작업 핸들 관리 (메모리 해제를 위해 보관)
    private Dictionary<string, AsyncOperationHandle> _asyncHandles = new Dictionary<string, AsyncOperationHandle>();

    // 인스턴스 핸들 관리 (Addressables로 생성된 인스턴스 추적)
    private List<AsyncOperationHandle<GameObject>> _instanceHandles = new List<AsyncOperationHandle<GameObject>>();

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

    #region 인스턴스 생성

    /// <summary>
    /// 프리팹 인스턴스 비동기 생성 (Addressables 직접 사용)
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

        // Addressables의 InstantiateAsync 사용
        var handle = Addressables.InstantiateAsync(address, parent);
        handle.Completed += (asyncHandle) =>
        {
            if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instance = asyncHandle.Result;
                _instanceHandles.Add(asyncHandle); // 핸들 추적

                Debug.Log($"[INFO] ResourceManager::InstantiateAsync - Instantiated prefab: {address}");
                onComplete?.Invoke(instance);
            }
            else
            {
                Debug.LogError($"[ERROR] ResourceManager::InstantiateAsync - Failed to instantiate: {address}");
                onComplete?.Invoke(null);
            }
        };
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
    /// GameObject 삭제 (Addressables로 생성된 경우 핸들도 해제)
    /// </summary>
    /// <param name="instance">삭제할 GameObject</param>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[WARNING] ResourceManager::ReleaseInstance - Instance is null");
            return;
        }

        // Addressables 핸들에서 해당 인스턴스 찾기
        AsyncOperationHandle<GameObject>? handleToRelease = null;
        foreach (var handle in _instanceHandles)
        {
            if (handle.IsValid() && handle.Result == instance)
            {
                handleToRelease = handle;
                break;
            }
        }

        // Addressables로 생성된 인스턴스면 핸들 해제
        if (handleToRelease.HasValue)
        {
            Addressables.ReleaseInstance(handleToRelease.Value);
            _instanceHandles.Remove(handleToRelease.Value);
            Debug.Log($"[INFO] ResourceManager::ReleaseInstance - Released Addressables instance: {instance.name}");
        }
        else
        {
            // 일반 인스턴스는 Destroy
            Object.Destroy(instance);
            Debug.Log($"[INFO] ResourceManager::ReleaseInstance - Destroyed instance: {instance.name}");
        }
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
    /// 모든 Addressables 인스턴스 해제
    /// </summary>
    public void ReleaseAllInstances()
    {
        Debug.Log($"[INFO] ResourceManager::ReleaseAllInstances - Releasing {_instanceHandles.Count} instances");

        // 모든 인스턴스 핸들 해제
        foreach (var handle in _instanceHandles)
        {
            if (handle.IsValid())
            {
                Addressables.ReleaseInstance(handle);
            }
        }

        _instanceHandles.Clear();
        Debug.Log("[INFO] ResourceManager::ReleaseAllInstances - All instances released");
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 모든 인스턴스 해제
        ReleaseAllInstances();

        // 모든 리소스 해제
        ReleaseAll();
    }

    #endregion
}
