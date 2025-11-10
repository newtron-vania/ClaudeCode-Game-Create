using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UndeadSurvivor
{
    /// <summary>
    /// 캐릭터 선택 UI의 개별 캐릭터 버튼
    /// 캐릭터 이름, 스프라이트 표시 및 선택 하이라이트 기능
    /// </summary>
    public class CharacterSelectSubItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private Image _characterSpriteImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Button _button;

        [Header("Selection Colors")]
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.2f, 1f);

        private int _characterId;
        private bool _isSelected;

        /// <summary>
        /// 캐릭터 클릭 이벤트
        /// </summary>
        public event Action<int> OnCharacterClicked;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        /// <summary>
        /// CharacterData로 UI 초기화
        /// </summary>
        /// <param name="characterData">표시할 캐릭터 데이터</param>
        public void Initialize(CharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("[CharacterSelectSubItem] Initialize - characterData is null");
                return;
            }

            _characterId = characterData.Id;

            // 캐릭터 이름 설정
            if (_characterNameText != null)
            {
                _characterNameText.text = characterData.Name;
            }

            // 캐릭터 스프라이트 로드
            if (_characterSpriteImage != null)
            {
                LoadCharacterSprite(characterData.Id, characterData.Name);
            }

            // 초기 선택 해제 상태
            SetSelected(false);

            Debug.Log($"[CharacterSelectSubItem] Initialize - Character: {characterData.Name} (ID: {characterData.Id})");
        }

        /// <summary>
        /// 캐릭터 스프라이트 비동기 로드
        /// </summary>
        private void LoadCharacterSprite(int characterId, string characterName)
        {
            string spritePath = $"Sprites/UndeadSurvivor/{characterName}_portrait";

            ResourceManager.Instance.LoadAsync<Sprite>(spritePath, (sprite) =>
            {
                if (sprite != null && _characterSpriteImage != null)
                {
                    _characterSpriteImage.sprite = sprite;
                    Debug.Log($"[CharacterSelectSubItem] LoadCharacterSprite - Loaded sprite for {characterName}");
                }
                else
                {
                    Debug.LogWarning($"[CharacterSelectSubItem] LoadCharacterSprite - Failed to load sprite: {spritePath}");
                }
            });
        }

        /// <summary>
        /// 선택 상태 설정 (하이라이트)
        /// </summary>
        /// <param name="isSelected">선택 여부</param>
        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;

            if (_backgroundImage != null)
            {
                _backgroundImage.color = isSelected ? _selectedColor : _normalColor;
            }

            Debug.Log($"[CharacterSelectSubItem] SetSelected - Character ID: {_characterId}, Selected: {isSelected}");
        }

        /// <summary>
        /// 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[CharacterSelectSubItem] OnButtonClicked - Character ID: {_characterId}");
            OnCharacterClicked?.Invoke(_characterId);
        }

        /// <summary>
        /// 현재 캐릭터 ID
        /// </summary>
        public int CharacterId => _characterId;

        /// <summary>
        /// 현재 선택 상태
        /// </summary>
        public bool IsSelected => _isSelected;
    }
}
