using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 관리 매니저
/// UI 패널 열기/닫기, 팝업 관리, 페이드 효과 담당
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("Canvas Settings")]
    [SerializeField] private Canvas _baseCanvas;
    [SerializeField] private Canvas _popupCanvas;
    [SerializeField] private Canvas _systemCanvas;

    [Header("Fade Settings")]
    [SerializeField] private FadePanel _fadePanel;

    // 열린 패널 관리
    private Dictionary<Type, UIPanel> _openPanels = new Dictionary<Type, UIPanel>();
    private Stack<UIPanel> _panelStack = new Stack<UIPanel>();

    // 캐시된 패널
    private Dictionary<Type, UIPanel> _cachedPanels = new Dictionary<Type, UIPanel>();

    protected override void Awake()
    {
        base.Awake();
        InitializeCanvases();
        Debug.Log("[INFO] UIManager::Awake - UIManager initialized");
    }

    #region 초기화

    /// <summary>
    /// Canvas 초기화
    /// </summary>
    private void InitializeCanvases()
    {
        // Base Canvas 생성
        if (_baseCanvas == null)
        {
            _baseCanvas = CreateCanvas("BaseCanvas", 0);
        }

        // Popup Canvas 생성
        if (_popupCanvas == null)
        {
            _popupCanvas = CreateCanvas("PopupCanvas", 100);
        }

        // System Canvas 생성 (페이드, 로딩 등)
        if (_systemCanvas == null)
        {
            _systemCanvas = CreateCanvas("SystemCanvas", 200);
        }

        // Fade Panel 생성
        if (_fadePanel == null)
        {
            GameObject fadePanelObj = new GameObject("FadePanel");
            fadePanelObj.transform.SetParent(_systemCanvas.transform, false);

            RectTransform rectTransform = fadePanelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            _fadePanel = fadePanelObj.AddComponent<FadePanel>();
        }
    }

    /// <summary>
    /// Canvas 생성
    /// </summary>
    private Canvas CreateCanvas(string canvasName, int sortOrder)
    {
        GameObject canvasObj = new GameObject(canvasName);
        canvasObj.transform.SetParent(transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        return canvas;
    }

    #endregion

    #region 패널 관리

    /// <summary>
    /// 패널 열기 (Addressables)
    /// </summary>
    /// <typeparam name="T">패널 타입</typeparam>
    /// <param name="address">패널 프리팹 Addressable 주소</param>
    /// <param name="onComplete">완료 콜백</param>
    public void OpenPanel<T>(string address, Action<T> onComplete = null) where T : UIPanel
    {
        Type panelType = typeof(T);

        // 이미 열려있으면 무시
        if (_openPanels.ContainsKey(panelType))
        {
            Debug.LogWarning($"[WARNING] UIManager::OpenPanel - Panel already open: {panelType.Name}");
            onComplete?.Invoke(_openPanels[panelType] as T);
            return;
        }

        // 캐시된 패널 확인
        if (_cachedPanels.TryGetValue(panelType, out UIPanel cachedPanel))
        {
            OpenCachedPanel(cachedPanel as T, onComplete);
            return;
        }

        // Addressables로 패널 프리팹 로드
        ResourceManager.Instance.LoadAsync<GameObject>(address, (prefab) =>
        {
            if (prefab != null)
            {
                InstantiatePanel(prefab, onComplete);
            }
            else
            {
                Debug.LogError($"[ERROR] UIManager::OpenPanel - Failed to load panel: {address}");
                onComplete?.Invoke(null);
            }
        });
    }

    /// <summary>
    /// 패널 인스턴스 생성 및 열기
    /// </summary>
    private void InstantiatePanel<T>(GameObject prefab, Action<T> onComplete) where T : UIPanel
    {
        GameObject panelObj = Instantiate(prefab, _baseCanvas.transform);
        T panel = panelObj.GetComponent<T>();

        if (panel == null)
        {
            Debug.LogError($"[ERROR] UIManager::InstantiatePanel - Panel component not found: {typeof(T).Name}");
            Destroy(panelObj);
            onComplete?.Invoke(null);
            return;
        }

        // 캐시에 저장
        Type panelType = typeof(T);
        _cachedPanels[panelType] = panel;

        // 패널 열기
        OpenCachedPanel(panel, onComplete);
    }

    /// <summary>
    /// 캐시된 패널 열기
    /// </summary>
    private void OpenCachedPanel<T>(T panel, Action<T> onComplete) where T : UIPanel
    {
        Type panelType = typeof(T);

        panel.Open();
        _openPanels[panelType] = panel;
        _panelStack.Push(panel);

        Debug.Log($"[INFO] UIManager::OpenCachedPanel - Panel opened: {panelType.Name}");
        onComplete?.Invoke(panel);
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    /// <typeparam name="T">패널 타입</typeparam>
    public void ClosePanel<T>() where T : UIPanel
    {
        Type panelType = typeof(T);

        if (!_openPanels.TryGetValue(panelType, out UIPanel panel))
        {
            Debug.LogWarning($"[WARNING] UIManager::ClosePanel - Panel not open: {panelType.Name}");
            return;
        }

        panel.Close();
        _openPanels.Remove(panelType);

        // 스택에서 제거
        if (_panelStack.Count > 0 && _panelStack.Peek() == panel)
        {
            _panelStack.Pop();
        }

        Debug.Log($"[INFO] UIManager::ClosePanel - Panel closed: {panelType.Name}");
    }

    /// <summary>
    /// 최상위 패널 닫기
    /// </summary>
    public void CloseTopPanel()
    {
        if (_panelStack.Count == 0)
        {
            Debug.LogWarning("[WARNING] UIManager::CloseTopPanel - No panels to close");
            return;
        }

        UIPanel topPanel = _panelStack.Pop();
        topPanel.Close();

        Type panelType = topPanel.GetType();
        _openPanels.Remove(panelType);

        Debug.Log($"[INFO] UIManager::CloseTopPanel - Panel closed: {panelType.Name}");
    }

    /// <summary>
    /// 모든 패널 닫기
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (var panel in _openPanels.Values)
        {
            panel.Close();
        }

        _openPanels.Clear();
        _panelStack.Clear();

        Debug.Log("[INFO] UIManager::CloseAllPanels - All panels closed");
    }

    /// <summary>
    /// 패널이 열려있는지 확인
    /// </summary>
    /// <typeparam name="T">패널 타입</typeparam>
    /// <returns>열려있으면 true</returns>
    public bool IsPanelOpen<T>() where T : UIPanel
    {
        return _openPanels.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 열린 패널 가져오기
    /// </summary>
    /// <typeparam name="T">패널 타입</typeparam>
    /// <returns>패널 인스턴스 (없으면 null)</returns>
    public T GetOpenPanel<T>() where T : UIPanel
    {
        if (_openPanels.TryGetValue(typeof(T), out UIPanel panel))
        {
            return panel as T;
        }
        return null;
    }

    #endregion

    #region 팝업

    /// <summary>
    /// 단순 팝업 표시
    /// </summary>
    /// <param name="title">제목</param>
    /// <param name="message">메시지</param>
    /// <param name="onConfirm">확인 콜백</param>
    public void ShowPopup(string title, string message, Action onConfirm = null)
    {
        Debug.Log($"[INFO] UIManager::ShowPopup - Title: {title}, Message: {message}");
        // 실제 팝업 UI 구현 시 여기서 처리
        onConfirm?.Invoke();
    }

    /// <summary>
    /// 확인/취소 팝업 표시
    /// </summary>
    /// <param name="message">메시지</param>
    /// <param name="onYes">확인 콜백</param>
    /// <param name="onNo">취소 콜백</param>
    public void ShowConfirmDialog(string message, Action onYes, Action onNo = null)
    {
        Debug.Log($"[INFO] UIManager::ShowConfirmDialog - Message: {message}");
        // 실제 확인 대화상자 UI 구현 시 여기서 처리
        onYes?.Invoke();
    }

    #endregion

    #region 페이드 효과

    /// <summary>
    /// 페이드 인 (투명 → 불투명)
    /// </summary>
    /// <param name="duration">페이드 시간 (초)</param>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeIn(float duration, Action onComplete = null)
    {
        if (_fadePanel != null)
        {
            _fadePanel.FadeIn(duration, onComplete);
        }
        else
        {
            Debug.LogWarning("[WARNING] UIManager::FadeIn - FadePanel not initialized");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 페이드 인 (기본 시간)
    /// </summary>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeIn(Action onComplete = null)
    {
        if (_fadePanel != null)
        {
            _fadePanel.FadeIn(onComplete);
        }
        else
        {
            Debug.LogWarning("[WARNING] UIManager::FadeIn - FadePanel not initialized");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 페이드 아웃 (불투명 → 투명)
    /// </summary>
    /// <param name="duration">페이드 시간 (초)</param>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeOut(float duration, Action onComplete = null)
    {
        if (_fadePanel != null)
        {
            _fadePanel.FadeOut(duration, onComplete);
        }
        else
        {
            Debug.LogWarning("[WARNING] UIManager::FadeOut - FadePanel not initialized");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 페이드 아웃 (기본 시간)
    /// </summary>
    /// <param name="onComplete">완료 콜백</param>
    public void FadeOut(Action onComplete = null)
    {
        if (_fadePanel != null)
        {
            _fadePanel.FadeOut(onComplete);
        }
        else
        {
            Debug.LogWarning("[WARNING] UIManager::FadeOut - FadePanel not initialized");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 페이드 색상 설정
    /// </summary>
    /// <param name="color">페이드 색상</param>
    public void SetFadeColor(Color color)
    {
        if (_fadePanel != null)
        {
            _fadePanel.SetFadeColor(color);
        }
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 모든 패널 닫기
        CloseAllPanels();

        // 캐시된 패널 삭제
        foreach (var panel in _cachedPanels.Values)
        {
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }
        _cachedPanels.Clear();

        Debug.Log("[INFO] UIManager::OnDestroy - UIManager destroyed");
    }

    #endregion
}
