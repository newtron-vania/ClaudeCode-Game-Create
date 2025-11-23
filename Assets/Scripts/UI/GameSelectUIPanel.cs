using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 선택 화면 UI 패널
/// GamePlayList의 정보를 바탕으로 플레이 가능한 게임 버튼들을 동적으로 생성합니다.
/// </summary>
public class GameSelectUIPanel : UIPanel
{
    [Header("게임 선택 설정")]
    [SerializeField]
    [Tooltip("게임 목록을 관리하는 컴포넌트")]
    private GamePlayList _gamePlayList;

    [SerializeField]
    [Tooltip("버튼이 생성될 위치 (중심점)")]
    private RectTransform _spawnPosition;

    [SerializeField]
    [Tooltip("게임 버튼 프리팹 주소 (Addressables)")]
    private string _buttonPrefabAddress = "SubItem/GameSelectButton";

    [Header("버튼 배치 설정")]
    [SerializeField]
    [Tooltip("버튼 간 간격")]
    private float _buttonSpacing = 150f;

    [SerializeField]
    [Tooltip("가로 방향으로 배치할 최대 버튼 수")]
    private int _maxButtonsPerRow = 3;

    /// <summary>
    /// 생성된 버튼 목록
    /// </summary>
    private List<GameSelectButton> _createdButtons = new List<GameSelectButton>();

    protected override void Awake()
    {
        base.Awake();

        // GamePlayList 자동 찾기
        if (_gamePlayList == null)
        {
            _gamePlayList = FindObjectOfType<GamePlayList>();
        }

        // spawnPosition이 없으면 현재 RectTransform 사용
        if (_spawnPosition == null)
        {
            _spawnPosition = GetComponent<RectTransform>();
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 패널이 열릴 때 버튼 생성
        CreateGameButtons();
    }

    protected override void OnClose()
    {
        base.OnClose();

        // 패널이 닫힐 때 버튼 정리
        ClearButtons();
    }

    /// <summary>
    /// 플레이 가능한 게임 버튼들을 동적으로 생성
    /// </summary>
    private void CreateGameButtons()
    {
        if (_gamePlayList == null)
        {
            Debug.LogError("[ERROR] GameSelectUIPanel::CreateGameButtons - GamePlayList is null");
            return;
        }

        if (string.IsNullOrEmpty(_buttonPrefabAddress))
        {
            Debug.LogError("[ERROR] GameSelectUIPanel::CreateGameButtons - Button prefab address is null or empty");
            return;
        }

        if (_spawnPosition == null)
        {
            Debug.LogError("[ERROR] GameSelectUIPanel::CreateGameButtons - Spawn position is null");
            return;
        }

        // 기존 버튼 정리
        ClearButtons();

        // 플레이 가능한 게임 목록 가져오기
        List<GameInfo> playableGames = _gamePlayList.GetPlayableGames();

        if (playableGames.Count == 0)
        {
            Debug.LogWarning("[WARNING] GameSelectUIPanel::CreateGameButtons - No playable games found");
            return;
        }

        Debug.Log($"[INFO] GameSelectUIPanel::CreateGameButtons - Creating {playableGames.Count} game buttons");

        // 버튼 프리팹 로드
        ResourceManager.Instance.LoadAsync<GameObject>(_buttonPrefabAddress, (buttonPrefab) =>
        {
            if (buttonPrefab == null)
            {
                Debug.LogError($"[ERROR] GameSelectUIPanel::CreateGameButtons - Failed to load button prefab from '{_buttonPrefabAddress}'");
                return;
            }

            // 버튼 생성 및 배치
            for (int i = 0; i < playableGames.Count; i++)
            {
                GameInfo gameInfo = playableGames[i];

                // 버튼 인스턴스 생성
                GameObject buttonObj = Instantiate(buttonPrefab, _spawnPosition);
                GameSelectButton button = buttonObj.GetComponent<GameSelectButton>();

                if (button == null)
                {
                    Debug.LogError($"[ERROR] GameSelectUIPanel::CreateGameButtons - Button prefab missing GameSelectButton component");
                    Destroy(buttonObj);
                    continue;
                }

                // 버튼 위치 계산
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                Vector2 position = CalculateButtonPosition(i);
                buttonRect.anchoredPosition = position;

                // 버튼 초기화
                button.Init(gameInfo.GameID, OnGameButtonClicked);

                // 생성된 버튼 목록에 추가
                _createdButtons.Add(button);

                Debug.Log($"[INFO] GameSelectUIPanel::CreateGameButtons - Created button for '{gameInfo.GameID}' at position {position}");
            }
        });
    }

    /// <summary>
    /// 버튼 위치 계산 (그리드 형태로 배치)
    /// </summary>
    /// <param name="index">버튼 인덱스</param>
    /// <returns>버튼의 anchoredPosition</returns>
    private Vector2 CalculateButtonPosition(int index)
    {
        // 행과 열 계산
        int row = index / _maxButtonsPerRow;
        int col = index % _maxButtonsPerRow;

        // 중심점을 기준으로 위치 계산
        float xOffset = (col - (_maxButtonsPerRow - 1) / 2f) * _buttonSpacing;
        float yOffset = -row * _buttonSpacing;

        return new Vector2(xOffset, yOffset);
    }

    /// <summary>
    /// 게임 버튼 클릭 시 호출되는 콜백
    /// </summary>
    /// <param name="gameID">클릭된 게임 ID</param>
    private void OnGameButtonClicked(string gameID)
    {
        Debug.Log($"[INFO] GameSelectUIPanel::OnGameButtonClicked - Game selected: {gameID}");

        // 씬 이름 생성 (예: "Tetris" → "Tetris")
        string sceneName = gameID;

        // UIManager 페이드 인 → CustomSceneManager 씬 전환
        CustomSceneManager.Instance.LoadSceneWithLoading(sceneName, 2f);
    }

    /// <summary>
    /// 생성된 모든 버튼 제거
    /// </summary>
    private void ClearButtons()
    {
        foreach (var button in _createdButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        _createdButtons.Clear();
        Debug.Log("[INFO] GameSelectUIPanel::ClearButtons - All buttons cleared");
    }

    /// <summary>
    /// 버튼 재생성 (게임 목록이 변경되었을 때 호출)
    /// </summary>
    public void RefreshButtons()
    {
        if (IsOpen)
        {
            CreateGameButtons();
            Debug.Log("[INFO] GameSelectUIPanel::RefreshButtons - Buttons refreshed");
        }
    }

    /// <summary>
    /// 패널 제거 시 리소스 정리
    /// </summary>
    private void OnDestroy()
    {
        ClearButtons();
    }
}
