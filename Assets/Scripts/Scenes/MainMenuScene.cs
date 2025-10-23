using UnityEngine;

/// <summary>
/// 메인 메뉴 씬 컨트롤러
/// 게임 선택 UI를 표시하고 관리합니다.
/// </summary>
public class MainMenuScene : BaseScene
{
    [Header("UI 참조")]
    [SerializeField]
    [Tooltip("게임 선택 UI 패널")]
    private GameSelectUIPanel _gameSelectPanel;

    public override SceneID SceneID => SceneID.MainMenu;

    private const string PanelAddress = "GameSelectUIPanel";

    protected override void Awake()
    {
        base.Awake();

        // GameSelectUIPanel이 씬에 배치되어 있는지 확인
        if (_gameSelectPanel == null)
        {
            _gameSelectPanel = FindFirstObjectByType<GameSelectUIPanel>();
        }
    }

    protected override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        
        UIManager.Instance.OpenPanel<GameSelectUIPanel>(PanelAddress, (panel) =>
        {
            if (panel != null)
            {
                _gameSelectPanel = panel;
                Debug.Log("[INFO] MainMenuScene::OnSceneLoaded - GameSelectPanel loaded via Addressables");
            }
            else
            {
                Debug.LogError("[ERROR] MainMenuScene::OnSceneLoaded - Failed to load GameSelectPanel from Addressables");
            }
        });
        // 페이드 아웃으로 씬 시작
        UIManager.Instance.FadeOut(1f, () =>
        {
            // 씬에 패널이 있으면 직접 Open
            if (_gameSelectPanel != null)
            {
                _gameSelectPanel.Open();
                Debug.Log("[INFO] MainMenuScene::OnSceneLoaded - GameSelectPanel opened (from scene)");
            }
            else
            {
                // 씬에 없으면 UIManager를 통해 Addressables로 로드
                Debug.Log("[INFO] MainMenuScene::OnSceneLoaded - Loading GameSelectPanel via UIManager");
            }
        });
    }

    /// <summary>
    /// 씬 언로드 시 정리
    /// </summary>
    protected override void OnSceneUnloaded()
    {
        base.OnSceneUnloaded();

        // UI 패널 닫기
        if (_gameSelectPanel != null && _gameSelectPanel.IsOpen)
        {
            _gameSelectPanel.Close();
        }
    }
}
