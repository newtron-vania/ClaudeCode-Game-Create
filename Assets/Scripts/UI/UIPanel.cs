using UnityEngine;

/// <summary>
/// UI 패널의 베이스 클래스
/// 모든 UI 패널은 이 클래스를 상속받아야 함
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] protected Canvas _canvas;
    [SerializeField] protected CanvasGroup _canvasGroup;

    /// <summary>
    /// 패널이 열려있는지 여부
    /// </summary>
    public bool IsOpen { get; protected set; }

    /// <summary>
    /// 패널 타입 이름
    /// </summary>
    public string PanelName => GetType().Name;

    protected virtual void Awake()
    {
        // Canvas 자동 찾기
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        // CanvasGroup 자동 찾기 또는 생성
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 초기 상태는 닫힘
        IsOpen = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 패널 열기
    /// </summary>
    public virtual void Open()
    {
        if (IsOpen)
        {
            Debug.LogWarning($"[WARNING] UIPanel::Open - Panel already open: {PanelName}");
            return;
        }

        gameObject.SetActive(true);
        IsOpen = true;
        OnOpen();

        Debug.Log($"[INFO] UIPanel::Open - Panel opened: {PanelName}");
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    public virtual void Close()
    {
        if (!IsOpen)
        {
            Debug.LogWarning($"[WARNING] UIPanel::Close - Panel already closed: {PanelName}");
            return;
        }

        OnClose();
        IsOpen = false;
        gameObject.SetActive(false);

        Debug.Log($"[INFO] UIPanel::Close - Panel closed: {PanelName}");
    }

    /// <summary>
    /// 패널 활성화 시 호출 (오버라이드 가능)
    /// </summary>
    protected virtual void OnOpen()
    {
        // 자식 클래스에서 구현
    }

    /// <summary>
    /// 패널 비활성화 시 호출 (오버라이드 가능)
    /// </summary>
    protected virtual void OnClose()
    {
        // 자식 클래스에서 구현
    }

    /// <summary>
    /// 패널 표시/숨김 설정
    /// </summary>
    /// <param name="visible">표시 여부</param>
    public void SetVisible(bool visible)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }

    /// <summary>
    /// 패널 알파값 설정
    /// </summary>
    /// <param name="alpha">알파값 (0.0 ~ 1.0)</param>
    public void SetAlpha(float alpha)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }

    /// <summary>
    /// 패널 상호작용 가능 여부 설정
    /// </summary>
    /// <param name="interactable">상호작용 가능 여부</param>
    public void SetInteractable(bool interactable)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }
    }
}
