using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 선택 화면의 개별 게임 버튼 컴포넌트
/// 게임 아이콘 이미지와 씬 이동 기능을 제공합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class GameSelectButton : MonoBehaviour
{
    [Header("컴포넌트 참조")]
    [SerializeField]
    [Tooltip("게임 아이콘을 표시할 Image 컴포넌트")]
    private Image _image;

    [SerializeField]
    [Tooltip("버튼 컴포넌트 (자동 할당)")]
    private Button _button;

    /// <summary>
    /// 이 버튼이 나타내는 게임 ID
    /// </summary>
    private string _gameID;

    /// <summary>
    /// 게임 ID 반환
    /// </summary>
    public string GameID => _gameID;

    private void Awake()
    {
        // 버튼 컴포넌트 자동 할당
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        // Image 컴포넌트 자동 찾기 (없으면 자식에서 찾기)
        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }

        // 버튼 설정: 클릭 시에만 하이라이트
        if (_button != null)
        {
            var colors = _button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.selectedColor = Color.white;
            _button.colors = colors;

            // Transition을 ColorTint로 설정
            _button.transition = Selectable.Transition.ColorTint;
        }
    }

    /// <summary>
    /// 게임 선택 버튼 초기화
    /// Addressables를 통해 아이콘 스프라이트를 로드하고 버튼 클릭 이벤트를 설정합니다.
    /// </summary>
    /// <param name="gameID">게임 ID</param>
    /// <param name="onClickCallback">버튼 클릭 시 실행할 콜백</param>
    public void Init(string gameID, System.Action<string> onClickCallback)
    {
        if (string.IsNullOrEmpty(gameID))
        {
            Debug.LogError("[ERROR] GameSelectButton::Init - gameID cannot be null or empty");
            return;
        }

        _gameID = gameID;

        // 아이콘 스프라이트 로드
        LoadIcon();

        // 버튼 클릭 이벤트 설정
        if (_button != null && onClickCallback != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClickCallback.Invoke(_gameID));
        }

        Debug.Log($"[INFO] GameSelectButton::Init - Initialized button for game '{gameID}'");
    }

    /// <summary>
    /// Addressables를 통해 게임 아이콘 스프라이트 로드
    /// 스프라이트 주소: "Sprites/{GameID}_icon"
    /// </summary>
    private void LoadIcon()
    {
        if (_image == null)
        {
            Debug.LogError("[ERROR] GameSelectButton::LoadIcon - Image component is null");
            return;
        }

        string iconAddress = $"Sprites/{_gameID}_icon";

        ResourceManager.Instance.LoadAsync<Sprite>(iconAddress, (sprite) =>
        {
            if (sprite != null)
            {
                _image.sprite = sprite;
                Debug.Log($"[INFO] GameSelectButton::LoadIcon - Loaded icon for '{_gameID}'");
            }
            else
            {
                Debug.LogWarning($"[WARNING] GameSelectButton::LoadIcon - Failed to load icon at '{iconAddress}'");

                // 기본 스프라이트 설정 (옵션)
                _image.color = new Color(0.8f, 0.8f, 0.8f);
            }
        });
    }

    /// <summary>
    /// 버튼 활성화 상태 설정
    /// </summary>
    /// <param name="isInteractable">상호작용 가능 여부</param>
    public void SetInteractable(bool isInteractable)
    {
        if (_button != null)
        {
            _button.interactable = isInteractable;
        }
    }

    /// <summary>
    /// 버튼 제거 시 리소스 정리
    /// </summary>
    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
        }

        // 스프라이트 리소스 해제
        if (!string.IsNullOrEmpty(_gameID))
        {
            string iconAddress = $"Sprites/{_gameID}_icon";
            ResourceManager.Instance.Release(iconAddress);
        }
    }

    /// <summary>
    /// Inspector에서 컴포넌트 자동 할당
    /// </summary>
    private void OnValidate()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }
    }
}
