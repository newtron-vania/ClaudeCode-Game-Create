using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UndeadSurvivor
{
    /// <summary>
    /// 캐릭터 선택 UI - 좌측 패널 (선택된 캐릭터 스탯 정보 표시)
    /// </summary>
    public class CharacterStatInfoPanel : MonoBehaviour
    {
        [Header("Character Info")]
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private Image _characterSpriteImage;

        [Header("Base Stats")]
        [SerializeField] private TextMeshProUGUI _maxHpText;
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _moveSpeedText;

        [Header("Combat Stats")]
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private TextMeshProUGUI _amountText;

        [Header("Start Weapon")]
        [SerializeField] private TextMeshProUGUI _startWeaponText;

        private CharacterData _currentCharacterData;

        /// <summary>
        /// 선택된 캐릭터 정보로 UI 업데이트
        /// </summary>
        /// <param name="characterData">표시할 캐릭터 데이터</param>
        public void UpdateCharacterInfo(CharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("[CharacterStatInfoPanel] UpdateCharacterInfo - characterData is null");
                Clear();
                return;
            }

            _currentCharacterData = characterData;

            // 캐릭터 기본 정보
            UpdateCharacterName(characterData.Name);
            UpdateCharacterSprite(characterData.Name);

            // 스탯 정보 업데이트
            UpdateBaseStats(characterData);
            UpdateCombatStats(characterData);
            UpdateStartWeapon(characterData);

            Debug.Log($"[CharacterStatInfoPanel] UpdateCharacterInfo - Updated info for {characterData.Name}");
        }

        /// <summary>
        /// 캐릭터 이름 업데이트
        /// </summary>
        private void UpdateCharacterName(string characterName)
        {
            if (_characterNameText != null)
            {
                _characterNameText.text = characterName;
            }
        }

        /// <summary>
        /// 캐릭터 스프라이트 업데이트
        /// </summary>
        private void UpdateCharacterSprite(string characterName)
        {
            if (_characterSpriteImage == null) return;

            string spritePath = $"Sprites/UndeadSurvivor/{characterName}_portrait";

            ResourceManager.Instance.LoadAsync<Sprite>(spritePath, (sprite) =>
            {
                if (sprite != null && _characterSpriteImage != null)
                {
                    _characterSpriteImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"[CharacterStatInfoPanel] UpdateCharacterSprite - Failed to load sprite: {spritePath}");
                }
            });
        }

        /// <summary>
        /// 기본 스탯 업데이트
        /// </summary>
        private void UpdateBaseStats(CharacterData data)
        {
            if (_maxHpText != null)
                _maxHpText.text = $"체력: {data.MaxHp:F0}";

            if (_damageText != null)
                _damageText.text = $"공격력: +{data.Damage * 100:F0}%";

            if (_defenseText != null)
                _defenseText.text = $"방어력: {data.Defense:F0}";

            if (_moveSpeedText != null)
                _moveSpeedText.text = $"이동속도: {data.MoveSpeed:F1}";
        }

        /// <summary>
        /// 전투 스탯 업데이트
        /// </summary>
        private void UpdateCombatStats(CharacterData data)
        {
            if (_cooldownText != null)
                _cooldownText.text = $"쿨타임: {FormatPercentage(data.Cooldown)}";

            if (_amountText != null)
                _amountText.text = $"개수: +{data.Amount:F0}";
        }

        /// <summary>
        /// 시작 무기 정보 업데이트
        /// </summary>
        private void UpdateStartWeapon(CharacterData data)
        {
            if (_startWeaponText != null)
            {
                if (data.StartWeaponId > 0)
                {
                    // WeaponData 로드하여 무기 이름 표시
                    var dataProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");
                    if (dataProvider != null)
                    {
                        var weaponData = dataProvider.GetWeaponData(data.StartWeaponId);
                        if (weaponData != null)
                        {
                            _startWeaponText.text = $"시작 무기: {weaponData.Name}";
                        }
                        else
                        {
                            _startWeaponText.text = $"시작 무기: ID {data.StartWeaponId}";
                        }
                    }
                    else
                    {
                        _startWeaponText.text = $"시작 무기: ID {data.StartWeaponId}";
                    }
                }
                else
                {
                    _startWeaponText.text = "시작 무기: 없음";
                }
            }
        }

        /// <summary>
        /// 퍼센트 값 포맷 (음수/양수 부호 표시)
        /// </summary>
        private string FormatPercentage(float value)
        {
            float percentage = value * 100f;
            if (percentage > 0)
                return $"+{percentage:F0}%";
            else if (percentage < 0)
                return $"{percentage:F0}%";
            else
                return "0%";
        }

        /// <summary>
        /// 패널 초기화 (선택 해제 시)
        /// </summary>
        public void Clear()
        {
            _currentCharacterData = null;

            if (_characterNameText != null)
                _characterNameText.text = "캐릭터를 선택하세요";

            if (_characterSpriteImage != null)
                _characterSpriteImage.sprite = null;

            // 모든 스탯 텍스트 초기화
            ClearStatTexts();

            Debug.Log("[CharacterStatInfoPanel] Clear - Panel cleared");
        }

        /// <summary>
        /// 모든 스탯 텍스트 초기화
        /// </summary>
        private void ClearStatTexts()
        {
            if (_maxHpText != null) _maxHpText.text = "체력: -";
            if (_damageText != null) _damageText.text = "공격력: -";
            if (_defenseText != null) _defenseText.text = "방어력: -";
            if (_moveSpeedText != null) _moveSpeedText.text = "이동속도: -";
            if (_cooldownText != null) _cooldownText.text = "쿨타임: -";
            if (_amountText != null) _amountText.text = "개수: -";
            if (_startWeaponText != null) _startWeaponText.text = "시작 무기: -";
        }

        /// <summary>
        /// 현재 표시 중인 캐릭터 데이터
        /// </summary>
        public CharacterData CurrentCharacterData => _currentCharacterData;
    }
}
