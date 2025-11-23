using UnityEngine;

/// <summary>
/// Undead Survivor 초기 화면 씬 컨트롤러
/// 게임 시작, 설정, 게임 종료 UI 제공
/// </summary>
public class UndeadSurvivorInitialScene : BaseScene
{
    /// <summary>
    /// 씬 ID
    /// </summary>
    public override SceneID SceneID => SceneID.UndeadSurvivor;

    #region BaseScene Overrides

    /// <summary>
    /// 씬 로드 완료
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        // TODO: 초기 화면 UI 로드
        // UIManager.Instance.ShowPanel<UndeadSurvivorInitialPanel>();

        Debug.Log("[INFO] UndeadSurvivorInitialScene::OnSceneLoaded - Initial scene loaded");
    }

    #endregion

    #region Public API

    /// <summary>
    /// 게임 시작 버튼 클릭 → 캐릭터 선택 씬으로 이동
    /// </summary>
    public void OnGameStartClicked()
    {
        Debug.Log("[INFO] UndeadSurvivorInitialScene::OnGameStartClicked - Moving to character selection");
        LoadSceneWithFade(SceneID.UndeadSurvivorCharacterSelectionScene);
    }

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    public void OnSettingsClicked()
    {
        Debug.Log("[INFO] UndeadSurvivorInitialScene::OnSettingsClicked - Opening settings");
        // TODO: 설정 UI 열기
        // UIManager.Instance.ShowPanel<SettingsPanel>();
    }

    /// <summary>
    /// 게임 종료 버튼 클릭 → 메인 메뉴로 복귀
    /// </summary>
    public void OnExitGameClicked()
    {
        Debug.Log("[INFO] UndeadSurvivorInitialScene::OnExitGameClicked - Returning to main menu");
        LoadSceneWithFade(SceneID.MainMenu);
    }

    #endregion
}
