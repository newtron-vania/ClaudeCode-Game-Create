using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace UndeadSurvivor.UI
{
    /// <summary>
    /// UIToolkit 기반 레벨업 UI 컨트롤러
    /// 4지선다 선택 UI 관리 및 애니메이션 제어
    /// </summary>
    public class LevelUpUIController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        [Header("UXML Templates")]
        [SerializeField] private VisualTreeAsset _optionButtonTemplate;

        [Header("Animation Settings")]
        [SerializeField] private float _flyInDuration = 0.1f;
        [SerializeField] private float _delayBetweenOptions = 0.05f;

        private VisualElement _root;
        private VisualElement _optionsContainer;
        private Label _titleLabel;

        private List<LevelUpOptionElement> _optionElements = new List<LevelUpOptionElement>();
        private LevelUpManager _levelUpManager;
        private bool _isVisible = false;

        private void Awake()
        {
            // LevelUpManager 찾기
            _levelUpManager = FindObjectOfType<LevelUpManager>();

            if (_levelUpManager == null)
            {
                Debug.LogError("[ERROR] LevelUpUIController::Awake - LevelUpManager not found");
                return;
            }

            // UI Document 검증
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
                if (_uiDocument == null)
                {
                    Debug.LogError("[ERROR] LevelUpUIController::Awake - UIDocument component not found");
                    return;
                }
            }

            // 템플릿 검증
            if (_optionButtonTemplate == null)
            {
                Debug.LogError("[ERROR] LevelUpUIController::Awake - OptionButtonTemplate not assigned");
                return;
            }

            // UI Document에서 루트 요소 가져오기
            _root = _uiDocument.rootVisualElement;

            // UI 요소 참조
            _optionsContainer = _root.Q<VisualElement>("options-container");
            _titleLabel = _root.Q<Label>("title");

            if (_optionsContainer == null)
            {
                Debug.LogError("[ERROR] LevelUpUIController::Awake - options-container not found");
                return;
            }

            if (_titleLabel == null)
            {
                Debug.LogWarning("[WARNING] LevelUpUIController::Awake - title label not found");
            }

            // 초기 상태: 숨김
            Hide();

            Debug.Log("[INFO] LevelUpUIController::Awake - Initialized successfully");
        }

        /// <summary>
        /// 레벨업 UI 표시
        /// </summary>
        public void Show(List<LevelUpOption> options)
        {
            if (_root == null || options == null || options.Count == 0)
            {
                Debug.LogError("[ERROR] LevelUpUIController::Show - Invalid state or empty options");
                return;
            }

            Debug.Log($"[INFO] LevelUpUIController::Show - Showing {options.Count} options");

            // Panel 표시
            _root.style.display = DisplayStyle.Flex;
            _isVisible = true;

            // 게임 일시정지
            Time.timeScale = 0f;

            // 기존 옵션 제거
            ClearOptions();

            // 새 옵션 생성
            CreateOptions(options);

            // 애니메이션 시작
            StartCoroutine(AnimateOptionsCoroutine());
        }

        /// <summary>
        /// 레벨업 UI 숨김
        /// </summary>
        public void Hide()
        {
            if (_root == null)
            {
                return;
            }

            Debug.Log("[INFO] LevelUpUIController::Hide - Hiding UI");

            _root.style.display = DisplayStyle.None;
            _isVisible = false;

            // 게임 재개
            Time.timeScale = 1f;

            ClearOptions();
        }

        /// <summary>
        /// 옵션 버튼 생성
        /// </summary>
        private void CreateOptions(List<LevelUpOption> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];

                // 템플릿 인스턴스 생성
                var optionElement = _optionButtonTemplate.Instantiate();
                var optionButton = optionElement.Q<VisualElement>("option-button");

                if (optionButton == null)
                {
                    Debug.LogError($"[ERROR] LevelUpUIController::CreateOptions - option-button not found in template at index {i}");
                    continue;
                }

                // 초기 상태: 숨김 (화면 좌측 밖)
                optionButton.AddToClassList("option-button--hidden");

                // LevelUpOptionElement 컴포넌트 생성
                var element = new LevelUpOptionElement(optionButton, option);
                element.OnClicked += OnOptionSelected;
                _optionElements.Add(element);

                // 컨테이너에 추가
                _optionsContainer.Add(optionElement);

                Debug.Log($"[INFO] LevelUpUIController::CreateOptions - Created option {i}: {option.Title}");
            }
        }

        /// <summary>
        /// 기존 옵션 제거
        /// </summary>
        private void ClearOptions()
        {
            foreach (var element in _optionElements)
            {
                element.OnClicked -= OnOptionSelected;
                element.Dispose();
            }
            _optionElements.Clear();
            _optionsContainer.Clear();

            Debug.Log("[INFO] LevelUpUIController::ClearOptions - Cleared all options");
        }

        /// <summary>
        /// Fly-In 애니메이션 코루틴
        /// </summary>
        private IEnumerator AnimateOptionsCoroutine()
        {
            Debug.Log($"[INFO] LevelUpUIController::AnimateOptionsCoroutine - Starting animation for {_optionElements.Count} options");

            for (int i = 0; i < _optionElements.Count; i++)
            {
                var element = _optionElements[i];
                var button = element.ButtonElement;

                // 딜레이 후 애니메이션 시작
                yield return new WaitForSecondsRealtime(_delayBetweenOptions * i);

                // CSS transition을 통해 자동 애니메이션
                button.RemoveFromClassList("option-button--hidden");
                button.AddToClassList("option-button--visible");

                Debug.Log($"[INFO] LevelUpUIController::AnimateOptionsCoroutine - Animating option {i}");
            }

            Debug.Log("[INFO] LevelUpUIController::AnimateOptionsCoroutine - Animation complete");
        }

        /// <summary>
        /// 옵션 선택 이벤트 핸들러
        /// </summary>
        private void OnOptionSelected(LevelUpOption option)
        {
            Debug.Log($"[INFO] LevelUpUIController::OnOptionSelected - Selected: {option.Title}");

            // LevelUpManager에 선택 전달
            if (_levelUpManager != null)
            {
                _levelUpManager.OnOptionChosen(option);
            }
            else
            {
                Debug.LogError("[ERROR] LevelUpUIController::OnOptionSelected - LevelUpManager is null");
            }

            // UI 닫기
            Hide();
        }

        /// <summary>
        /// UI 표시 여부
        /// </summary>
        public bool IsVisible => _isVisible;

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            ClearOptions();

            Debug.Log("[INFO] LevelUpUIController::OnDestroy - Destroyed");
        }
    }
}
