using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace UndeadSurvivor.UI
{
    /// <summary>
    /// 레벨업 선택지 개별 요소
    /// UIToolkit VisualElement 래퍼 클래스
    /// </summary>
    public class LevelUpOptionElement
    {
        public VisualElement ButtonElement { get; private set; }
        public LevelUpOption Option { get; private set; }

        public event Action<LevelUpOption> OnClicked;

        private VisualElement _icon;
        private Label _nameText;
        private Label _descriptionText;

        public LevelUpOptionElement(VisualElement buttonElement, LevelUpOption option)
        {
            ButtonElement = buttonElement;
            Option = option;

            // UI 요소 참조
            _icon = buttonElement.Q<VisualElement>("icon");
            _nameText = buttonElement.Q<Label>("name-text");
            _descriptionText = buttonElement.Q<Label>("description-text");

            if (_icon == null)
            {
                Debug.LogError("[ERROR] LevelUpOptionElement::Constructor - icon element not found");
            }

            if (_nameText == null)
            {
                Debug.LogError("[ERROR] LevelUpOptionElement::Constructor - name-text element not found");
            }

            if (_descriptionText == null)
            {
                Debug.LogError("[ERROR] LevelUpOptionElement::Constructor - description-text element not found");
            }

            // 데이터 바인딩
            SetData(option);

            // 클릭 이벤트 등록
            buttonElement.RegisterCallback<ClickEvent>(OnClick);

            Debug.Log($"[INFO] LevelUpOptionElement::Constructor - Created element for: {option.Title}");
        }

        /// <summary>
        /// 옵션 데이터 설정
        /// </summary>
        private void SetData(LevelUpOption option)
        {
            // 텍스트 설정
            if (_nameText != null)
            {
                _nameText.text = option.Title;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = option.Description;
            }

            // 아이콘 로드
            LoadIcon(option);
        }

        /// <summary>
        /// 아이콘 로드 (Addressables)
        /// </summary>
        private void LoadIcon(LevelUpOption option)
        {
            string iconPath = GetIconPath(option);

            Debug.Log($"[INFO] LevelUpOptionElement::LoadIcon - Loading icon: {iconPath}");

            ResourceManager.Instance.LoadAsync<Sprite>(iconPath, (sprite) =>
            {
                if (sprite != null && _icon != null)
                {
                    _icon.style.backgroundImage = new StyleBackground(sprite);
                    Debug.Log($"[INFO] LevelUpOptionElement::LoadIcon - Icon loaded successfully: {iconPath}");
                }
                else
                {
                    Debug.LogWarning($"[WARNING] LevelUpOptionElement::LoadIcon - Failed to load: {iconPath}");

                    // 기본 아이콘 로드 시도
                    LoadDefaultIcon();
                }
            });
        }

        /// <summary>
        /// 기본 아이콘 로드
        /// </summary>
        private void LoadDefaultIcon()
        {
            string defaultIconPath = "Sprite/UndeadSurvivor/Icon_Default";

            ResourceManager.Instance.LoadAsync<Sprite>(defaultIconPath, (sprite) =>
            {
                if (sprite != null && _icon != null)
                {
                    _icon.style.backgroundImage = new StyleBackground(sprite);
                    Debug.Log("[INFO] LevelUpOptionElement::LoadDefaultIcon - Default icon loaded");
                }
                else
                {
                    Debug.LogWarning("[WARNING] LevelUpOptionElement::LoadDefaultIcon - Failed to load default icon");
                }
            });
        }

        /// <summary>
        /// 옵션 타입에 따른 아이콘 경로 반환
        /// </summary>
        private string GetIconPath(LevelUpOption option)
        {
            switch (option.Type)
            {
                case LevelUpOptionType.NewWeapon:
                    return $"Sprite/UndeadSurvivor/Icon_Weapon_{option.WeaponId}";

                case LevelUpOptionType.WeaponUpgrade:
                    return $"Sprite/UndeadSurvivor/Icon_Weapon_{option.WeaponId}";

                case LevelUpOptionType.StatUpgrade:
                    return $"Sprite/UndeadSurvivor/Icon_Stat_{option.StatType}";

                default:
                    return "Sprite/UndeadSurvivor/Icon_Default";
            }
        }

        /// <summary>
        /// 클릭 이벤트 핸들러
        /// </summary>
        private void OnClick(ClickEvent evt)
        {
            Debug.Log($"[INFO] LevelUpOptionElement::OnClick - Option clicked: {Option.Title}");
            OnClicked?.Invoke(Option);
        }

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            // 이벤트 구독 해제
            if (ButtonElement != null)
            {
                ButtonElement.UnregisterCallback<ClickEvent>(OnClick);
            }

            // 아이콘 리소스 해제
            if (_icon != null && _icon.style.backgroundImage.value != null)
            {
                var background = _icon.style.backgroundImage.value;
                if (background.sprite != null)
                {
                    // Addressables로 로드한 리소스 해제
                    string iconPath = GetIconPath(Option);
                    ResourceManager.Instance.Release(iconPath);
                }
            }

            Debug.Log($"[INFO] LevelUpOptionElement::Dispose - Disposed element for: {Option.Title}");
        }
    }
}
