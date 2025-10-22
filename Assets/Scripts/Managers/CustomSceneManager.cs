using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

/// <summary>
/// 씬 전환 및 로딩 관리 매니저
/// Unity SceneManager와 이름 충돌 방지를 위해 CustomSceneManager 사용
/// </summary>
public class CustomSceneManager : Singleton<CustomSceneManager>
{
    [Header("Loading Settings")]
    [SerializeField] private string _loadingSceneName = "LoadingScene";
    [SerializeField] private float _minimumLoadTime = 1f;

    // 씬 스택 (이전 씬으로 돌아가기 위함)
    private Stack<string> _sceneStack = new Stack<string>();

    // 로딩 상태
    private bool _isLoading = false;
    private float _loadProgress = 0f;

    /// <summary>
    /// 로딩 중 여부
    /// </summary>
    public bool IsLoading => _isLoading;

    /// <summary>
    /// 로딩 진행률 (0.0 ~ 1.0)
    /// </summary>
    public float LoadProgress => _loadProgress;

    /// <summary>
    /// 현재 씬 이름
    /// </summary>
    public string CurrentSceneName => UnitySceneManager.GetActiveScene().name;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[INFO] CustomSceneManager::Awake - CustomSceneManager initialized");
    }

    #region 씬 로드 (동기)

    /// <summary>
    /// 씬 로드 (SceneID 기반)
    /// </summary>
    /// <param name="sceneID">씬 ID</param>
    public void LoadScene(SceneID sceneID)
    {
        string sceneName = sceneID.ToString();
        LoadScene(sceneName);
    }

    /// <summary>
    /// 씬 로드 (동기)
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::LoadScene - Scene name is null or empty");
            return;
        }

        Debug.Log($"[INFO] CustomSceneManager::LoadScene - Loading scene: {sceneName}");
        UnitySceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 씬 로드 (빌드 인덱스)
    /// </summary>
    /// <param name="buildIndex">빌드 인덱스</param>
    public void LoadScene(int buildIndex)
    {
        Debug.Log($"[INFO] CustomSceneManager::LoadScene - Loading scene by index: {buildIndex}");
        UnitySceneManager.LoadScene(buildIndex);
    }

    #endregion

    #region 씬 로드 (비동기)

    /// <summary>
    /// 씬 비동기 로드
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <param name="onComplete">완료 콜백</param>
    public void LoadSceneAsync(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::LoadSceneAsync - Scene name is null or empty");
            onComplete?.Invoke();
            return;
        }

        if (_isLoading)
        {
            Debug.LogWarning("[WARNING] CustomSceneManager::LoadSceneAsync - Already loading a scene");
            return;
        }

        StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// 씬 비동기 로드 (빌드 인덱스)
    /// </summary>
    /// <param name="buildIndex">빌드 인덱스</param>
    /// <param name="onComplete">완료 콜백</param>
    public void LoadSceneAsync(int buildIndex, Action onComplete = null)
    {
        if (_isLoading)
        {
            Debug.LogWarning("[WARNING] CustomSceneManager::LoadSceneAsync - Already loading a scene");
            return;
        }

        StartCoroutine(LoadSceneAsyncCoroutine(buildIndex, onComplete));
    }

    /// <summary>
    /// 씬 비동기 로드 코루틴 (씬 이름)
    /// </summary>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action onComplete)
    {
        _isLoading = true;
        _loadProgress = 0f;

        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAsyncCoroutine - Loading scene: {sceneName}");

        AsyncOperation asyncLoad = UnitySceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 로딩 진행
        while (!asyncLoad.isDone)
        {
            // 90%까지만 진행 (allowSceneActivation = false 때문)
            _loadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 로딩 완료되면 씬 활성화
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        _loadProgress = 1f;
        _isLoading = false;

        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAsyncCoroutine - Scene loaded: {sceneName}");
        onComplete?.Invoke();
    }

    /// <summary>
    /// 씬 비동기 로드 코루틴 (빌드 인덱스)
    /// </summary>
    private IEnumerator LoadSceneAsyncCoroutine(int buildIndex, Action onComplete)
    {
        _isLoading = true;
        _loadProgress = 0f;

        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAsyncCoroutine - Loading scene by index: {buildIndex}");

        AsyncOperation asyncLoad = UnitySceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            _loadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        _loadProgress = 1f;
        _isLoading = false;

        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAsyncCoroutine - Scene loaded: {buildIndex}");
        onComplete?.Invoke();
    }

    #endregion

    #region 로딩 화면과 함께 씬 전환

    /// <summary>
    /// 로딩 화면과 함께 씬 로드 (SceneID 기반)
    /// </summary>
    /// <param name="sceneID">씬 ID</param>
    /// <param name="minimumLoadTime">최소 로딩 시간 (초)</param>
    public void LoadSceneWithLoadingScreen(SceneID sceneID, float minimumLoadTime = -1f)
    {
        string sceneName = sceneID.ToString();
        LoadSceneWithLoading(sceneName, minimumLoadTime);
    }

    /// <summary>
    /// 로딩 화면과 함께 씬 로드
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <param name="minimumLoadTime">최소 로딩 시간 (초)</param>
    public void LoadSceneWithLoading(string sceneName, float minimumLoadTime = -1f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::LoadSceneWithLoading - Scene name is null or empty");
            return;
        }

        if (_isLoading)
        {
            Debug.LogWarning("[WARNING] CustomSceneManager::LoadSceneWithLoading - Already loading a scene");
            return;
        }

        float actualMinimumLoadTime = minimumLoadTime >= 0f ? minimumLoadTime : _minimumLoadTime;
        StartCoroutine(LoadSceneWithLoadingCoroutine(sceneName, actualMinimumLoadTime));
    }

    /// <summary>
    /// 로딩 화면과 함께 씬 로드 코루틴
    /// </summary>
    private IEnumerator LoadSceneWithLoadingCoroutine(string sceneName, float minimumLoadTime)
    {
        _isLoading = true;
        _loadProgress = 0f;
        float startTime = Time.time;

        Debug.Log($"[INFO] CustomSceneManager::LoadSceneWithLoadingCoroutine - Loading scene with loading screen: {sceneName}");

        // 페이드 인
        UIManager.Instance.FadeIn(0.5f);
        yield return new WaitForSeconds(0.5f);

        // 로딩 씬으로 전환 (있는 경우)
        if (UnitySceneManager.GetSceneByName(_loadingSceneName).IsValid())
        {
            UnitySceneManager.LoadScene(_loadingSceneName);
            yield return null;
        }

        // 대상 씬 비동기 로드
        AsyncOperation asyncLoad = UnitySceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 로딩 진행
        while (!asyncLoad.isDone)
        {
            _loadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 최소 로딩 시간과 실제 로딩 모두 완료되면 씬 활성화
            float elapsedTime = Time.time - startTime;
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        _loadProgress = 1f;

        // 페이드 아웃
        UIManager.Instance.FadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);

        _isLoading = false;
        Debug.Log($"[INFO] CustomSceneManager::LoadSceneWithLoadingCoroutine - Scene loaded: {sceneName}");
    }

    /// <summary>
    /// 페이드 효과와 함께 씬 로드 (SceneID 기반)
    /// </summary>
    /// <param name="sceneID">씬 ID</param>
    /// <param name="fadeDuration">페이드 지속 시간</param>
    public void LoadSceneWithFade(SceneID sceneID, float fadeDuration = 0.5f)
    {
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneID.ToString(), fadeDuration));
    }

    /// <summary>
    /// 페이드 효과와 함께 씬 로드 코루틴
    /// </summary>
    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName, float fadeDuration)
    {
        // 페이드 인
        UIManager.Instance.FadeIn(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // 씬 로드
        LoadScene(sceneName);

        // 페이드 아웃
        UIManager.Instance.FadeOut(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
    }

    #endregion

    #region 씬 스택 관리

    /// <summary>
    /// 씬 스택에 현재 씬 푸시 후 새 씬 로드
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    public void PushScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::PushScene - Scene name is null or empty");
            return;
        }

        // 현재 씬을 스택에 푸시
        string currentScene = CurrentSceneName;
        _sceneStack.Push(currentScene);

        Debug.Log($"[INFO] CustomSceneManager::PushScene - Pushed scene: {currentScene}, Loading: {sceneName}");

        // 새 씬 로드
        LoadScene(sceneName);
    }

    /// <summary>
    /// 씬 스택에서 팝하여 이전 씬으로 돌아가기
    /// </summary>
    public void PopScene()
    {
        if (_sceneStack.Count == 0)
        {
            Debug.LogWarning("[WARNING] CustomSceneManager::PopScene - Scene stack is empty");
            return;
        }

        string previousScene = _sceneStack.Pop();
        Debug.Log($"[INFO] CustomSceneManager::PopScene - Popped scene: {previousScene}");

        LoadScene(previousScene);
    }

    /// <summary>
    /// 씬 스택 초기화
    /// </summary>
    public void ClearSceneStack()
    {
        _sceneStack.Clear();
        Debug.Log("[INFO] CustomSceneManager::ClearSceneStack - Scene stack cleared");
    }

    /// <summary>
    /// 씬 스택 카운트
    /// </summary>
    public int SceneStackCount => _sceneStack.Count;

    #endregion

    #region 씬 추가 로드 (Additive)

    /// <summary>
    /// 씬 추가 로드 (현재 씬은 유지)
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <param name="onComplete">완료 콜백</param>
    public void LoadSceneAdditive(string sceneName, Action<UnityScene> onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::LoadSceneAdditive - Scene name is null or empty");
            onComplete?.Invoke(default);
            return;
        }

        StartCoroutine(LoadSceneAdditiveCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// 씬 추가 로드 코루틴
    /// </summary>
    private IEnumerator LoadSceneAdditiveCoroutine(string sceneName, Action<UnityScene> onComplete)
    {
        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAdditiveCoroutine - Loading scene additively: {sceneName}");

        AsyncOperation asyncLoad = UnitySceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return asyncLoad;

        UnityScene loadedScene = UnitySceneManager.GetSceneByName(sceneName);
        Debug.Log($"[INFO] CustomSceneManager::LoadSceneAdditiveCoroutine - Scene loaded additively: {sceneName}");

        onComplete?.Invoke(loadedScene);
    }

    /// <summary>
    /// 씬 언로드
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <param name="onComplete">완료 콜백</param>
    public void UnloadScene(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ERROR] CustomSceneManager::UnloadScene - Scene name is null or empty");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// 씬 언로드 코루틴
    /// </summary>
    private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
    {
        Debug.Log($"[INFO] CustomSceneManager::UnloadSceneCoroutine - Unloading scene: {sceneName}");

        AsyncOperation asyncUnload = UnitySceneManager.UnloadSceneAsync(sceneName);
        yield return asyncUnload;

        Debug.Log($"[INFO] CustomSceneManager::UnloadSceneCoroutine - Scene unloaded: {sceneName}");
        onComplete?.Invoke();
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 씬이 로드되어 있는지 확인
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <returns>로드 여부</returns>
    public bool IsSceneLoaded(string sceneName)
    {
        UnityScene scene = UnitySceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    /// <summary>
    /// 씬 빌드 인덱스 가져오기
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <returns>빌드 인덱스 (-1이면 빌드에 포함되지 않음)</returns>
    public int GetSceneBuildIndex(string sceneName)
    {
        UnityScene scene = UnitySceneManager.GetSceneByName(sceneName);
        return scene.buildIndex;
    }

    /// <summary>
    /// 활성 씬 설정
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    public void SetActiveScene(string sceneName)
    {
        UnityScene scene = UnitySceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            UnitySceneManager.SetActiveScene(scene);
            Debug.Log($"[INFO] CustomSceneManager::SetActiveScene - Active scene set to: {sceneName}");
        }
        else
        {
            Debug.LogWarning($"[WARNING] CustomSceneManager::SetActiveScene - Scene not valid: {sceneName}");
        }
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _sceneStack.Clear();
        _isLoading = false;
        _loadProgress = 0f;

        Debug.Log("[INFO] CustomSceneManager::OnDestroy - CustomSceneManager destroyed");
    }

    #endregion
}
