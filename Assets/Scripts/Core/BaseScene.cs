using UnityEngine;

/// <summary>
/// 모든 씬의 베이스 클래스
/// 각 씬은 이 클래스를 상속받아 고유한 ID와 생명주기를 구현합니다.
/// </summary>
public abstract class BaseScene : MonoBehaviour
{
    /// <summary>
    /// 씬의 고유 ID
    /// 각 씬은 반드시 고유한 SceneID를 반환해야 합니다.
    /// </summary>
    public abstract SceneID SceneID { get; }

    /// <summary>
    /// 씬 초기화
    /// Unity의 Start 이전에 호출됩니다.
    /// </summary>
    protected virtual void Awake()
    {
        Debug.Log($"[INFO] BaseScene::Awake - {SceneID} scene awakened");
    }

    /// <summary>
    /// 씬 시작
    /// 씬이 로드된 후 호출됩니다.
    /// </summary>
    protected virtual void Start()
    {
        Debug.Log($"[INFO] BaseScene::Start - {SceneID} scene started");
        OnSceneLoaded();
    }

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// 자식 클래스에서 오버라이드하여 씬별 초기화 로직을 구현합니다.
    /// </summary>
    protected virtual void OnSceneLoaded()
    {
        // 자식 클래스에서 구현
    }

    /// <summary>
    /// 씬 업데이트
    /// 매 프레임 호출됩니다.
    /// </summary>
    protected virtual void Update()
    {
        // 자식 클래스에서 필요 시 오버라이드
    }

    /// <summary>
    /// 씬 종료 시 호출
    /// 리소스 정리 등을 수행합니다.
    /// </summary>
    protected virtual void OnDestroy()
    {
        Debug.Log($"[INFO] BaseScene::OnDestroy - {SceneID} scene destroyed");
        OnSceneUnloaded();
    }

    /// <summary>
    /// 씬 언로드 시 호출
    /// 자식 클래스에서 오버라이드하여 씬별 정리 로직을 구현합니다.
    /// </summary>
    protected virtual void OnSceneUnloaded()
    {
        // 자식 클래스에서 구현
    }

    /// <summary>
    /// 다른 씬으로 전환
    /// </summary>
    /// <param name="sceneID">전환할 씬 ID</param>
    protected void LoadScene(SceneID sceneID)
    {
        CustomSceneManager.Instance.LoadScene(sceneID);
    }

    /// <summary>
    /// 다른 씬으로 전환 (페이드 효과 포함)
    /// </summary>
    /// <param name="sceneID">전환할 씬 ID</param>
    /// <param name="fadeDuration">페이드 지속 시간</param>
    protected void LoadSceneWithFade(SceneID sceneID, float fadeDuration = 0.5f)
    {
        CustomSceneManager.Instance.LoadSceneWithFade(sceneID, fadeDuration);
    }

    /// <summary>
    /// 다른 씬으로 전환 (로딩 화면 포함)
    /// </summary>
    /// <param name="sceneID">전환할 씬 ID</param>
    /// <param name="minLoadingTime">최소 로딩 시간</param>
    protected void LoadSceneWithLoadingScreen(SceneID sceneID, float minLoadingTime = 1.0f)
    {
        CustomSceneManager.Instance.LoadSceneWithLoadingScreen(sceneID, minLoadingTime);
    }
}
