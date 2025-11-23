using UnityEngine;
using UndeadSurvivor;

/// <summary>
/// Undead Survivor 캐릭터 선택 씬 컨트롤러
/// 캐릭터 선택 후 UndeadSurvivorGameScene으로 전환합니다.
/// </summary>
public class UndeadSurvivorCharacterSelectScene : BaseScene
{
    /// <summary>
    /// 씬 ID
    /// </summary>
    public override SceneID SceneID => SceneID.UndeadSurvivorCharacterSelectionScene;

    private UndeadSurvivorDataProvider _dataProvider;
    private int _selectedCharacterId = 1; // 기본값: Knight

    #region BaseScene Overrides

    /// <summary>
    /// 씬 초기화
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // DataManager에 UndeadSurvivorDataProvider 등록
        InitializeDataProvider();
    }

    /// <summary>
    /// 씬 로드 완료
    /// </summary>
    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        // TODO: 캐릭터 선택 UI 로드
        // UIManager.Instance.ShowPanel<CharacterSelectPanel>();

        Debug.Log("[INFO] UndeadSurvivorCharacterSelectScene::OnSceneLoaded - Character select scene loaded");
    }

    /// <summary>
    /// 씬 언로드
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

        // DataProvider는 언로드하지 않음 (게임 씬에서 사용)
        Debug.Log("[INFO] UndeadSurvivorCharacterSelectScene::OnSceneUnloaded - Scene unloaded");
    }

    #endregion

    #region Initialization

    /// <summary>
    /// DataProvider 초기화 및 등록
    /// </summary>
    private void InitializeDataProvider()
    {
        // 이미 등록되어 있는지 확인
        var existingProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

        if (existingProvider == null)
        {
            // 등록되지 않았으면 생성 및 등록
            _dataProvider = new UndeadSurvivorDataProvider();
            _dataProvider.Initialize();
            DataManager.Instance.RegisterProvider(_dataProvider);

            Debug.Log("[INFO] UndeadSurvivorCharacterSelectScene::InitializeDataProvider - DataProvider registered");
        }
        else
        {
            _dataProvider = existingProvider;
        }

        // 데이터 로드
        if (!_dataProvider.IsLoaded)
        {
            DataManager.Instance.LoadGameData("UndeadSurvivor");
            Debug.Log("[INFO] UndeadSurvivorCharacterSelectScene::InitializeDataProvider - Game data loaded");
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 캐릭터 선택
    /// </summary>
    /// <param name="characterId">선택한 캐릭터 ID (1: Knight, 2: Mage)</param>
    public void SelectCharacter(int characterId)
    {
        var characterData = _dataProvider.GetCharacterData(characterId);
        if (characterData == null)
        {
            Debug.LogError($"[ERROR] UndeadSurvivorCharacterSelectScene::SelectCharacter - Invalid character ID: {characterId}");
            return;
        }

        _selectedCharacterId = characterId;
        Debug.Log($"[INFO] UndeadSurvivorCharacterSelectScene::SelectCharacter - Selected character: {characterData.Name}");

        // TODO: UI 업데이트
    }

    /// <summary>
    /// 전투 시작 버튼 클릭 → 게임 플레이 씬으로 전환
    /// </summary>
    public void OnStartBattleClicked()
    {
        if (_selectedCharacterId == 0)
        {
            Debug.LogWarning("[WARNING] UndeadSurvivorCharacterSelectScene::OnStartBattleClicked - No character selected");
            // TODO: UI 경고 메시지 표시
            return;
        }

        // 선택한 캐릭터 ID를 PlayerPrefs에 저장
        PlayerPrefs.SetInt("SelectedCharacterId", _selectedCharacterId);
        PlayerPrefs.Save();

        Debug.Log($"[INFO] UndeadSurvivorCharacterSelectScene::OnStartBattleClicked - Starting battle with character ID: {_selectedCharacterId}");

        // 게임 플레이 씬으로 전환
        LoadSceneWithFade(SceneID.UndeadSurvivorGameScene);
    }

    /// <summary>
    /// 이전으로 버튼 클릭 → 초기 화면으로 복귀
    /// </summary>
    public void OnBackClicked()
    {
        Debug.Log("[INFO] UndeadSurvivorCharacterSelectScene::OnBackClicked - Returning to initial scene");

        // DataProvider는 언로드하지 않음 (게임 씬에서 사용 가능성)
        LoadSceneWithFade(SceneID.UndeadSurvivor);
    }

    #endregion

    #region Test Methods (임시)

    /// <summary>
    /// 테스트용: Knight 선택 후 바로 전투 시작
    /// </summary>
    [ContextMenu("Test Start Battle with Knight")]
    private void TestStartWithKnight()
    {
        SelectCharacter(1);
        OnStartBattleClicked();
    }

    /// <summary>
    /// 테스트용: Mage 선택 후 바로 전투 시작
    /// </summary>
    [ContextMenu("Test Start Battle with Mage")]
    private void TestStartWithMage()
    {
        SelectCharacter(2);
        OnStartBattleClicked();
    }

    #endregion
}