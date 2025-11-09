using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UndeadSurvivor.UI;

namespace UndeadSurvivor
{
    /// <summary>
    /// 레벨업 선택지 생성 및 관리
    /// PRD 3.3 선택지 생성 로직 구현
    /// </summary>
    public class LevelUpManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _optionCount = 4; // 선택지 개수 (기본 4개)
        [SerializeField] private int _maxWeaponSlots = 6; // 최대 무기 슬롯

        [Header("References")]
        [SerializeField] private Player _player; // 플레이어 참조

        [Header("UI References")]
        [SerializeField] private LevelUpUIController _levelUpUIController; // UIToolkit UI 컨트롤러

        private UndeadSurvivorDataProvider _dataProvider;

        // 스탯 업그레이드 기본값
        private readonly Dictionary<StatType, float> _statUpgradeValues = new Dictionary<StatType, float>
        {
            { StatType.Damage, 5f },         // 공격력 +5%
            { StatType.MaxHp, 10f },         // 최대 체력 +10%
            { StatType.Defense, 1f },        // 방어력 +1
            { StatType.MoveSpeed, 10f },     // 이동 속도 +10%
            { StatType.Area, 10f },          // 범위 +10%
            { StatType.Cooldown, 5f },       // 쿨타임 -5%
            { StatType.Amount, 1f },         // 투사체 개수 +1
            { StatType.Pierce, 1f },         // 관통력 +1
            { StatType.ExpMultiplier, 10f }, // 경험치 획득 +10%
            { StatType.PickupRange, 15f },   // 아이템 획득 범위 +15%
            { StatType.Luck, 10f }           // 행운 +10%
        };

        private void Awake()
        {
            if (_player == null)
            {
                _player = FindObjectOfType<Player>();
            }

            if (_levelUpUIController == null)
            {
                _levelUpUIController = FindObjectOfType<LevelUpUIController>();
            }

            if (_levelUpUIController == null)
            {
                Debug.LogWarning("[WARNING] LevelUpManager::Awake - LevelUpUIController not found. UI will not be displayed.");
            }
        }

        /// <summary>
        /// 초기화 (DataProvider 연동)
        /// </summary>
        public void Initialize(UndeadSurvivorDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            Debug.Log("[INFO] LevelUpManager::Initialize - LevelUpManager initialized");
        }

        /// <summary>
        /// 레벨업 선택지 생성
        /// PRD 3.3 선택지 생성 로직 구현
        /// </summary>
        public List<LevelUpOption> GenerateLevelUpOptions()
        {
            if (_player == null || _dataProvider == null)
            {
                Debug.LogError("[ERROR] LevelUpManager::GenerateLevelUpOptions - Player or DataProvider is null");
                return new List<LevelUpOption>();
            }

            List<LevelUpOption> options = new List<LevelUpOption>();
            List<LevelUpOption> pool = new List<LevelUpOption>();

            int playerLevel = _player.Level;
            int currentWeaponCount = _player.CurrentWeaponCount;
            bool isWeaponSlotsFull = currentWeaponCount >= _maxWeaponSlots;

            Debug.Log($"[INFO] LevelUpManager::GenerateLevelUpOptions - Player Level: {playerLevel}, Weapons: {currentWeaponCount}/{_maxWeaponSlots}");

            // === PRD 3.3.A: 특수 규칙 1 - 초기 무기 보장 (레벨 2-5) ===
            if (playerLevel >= 2 && playerLevel <= 5 && !isWeaponSlotsFull)
            {
                Debug.Log("[INFO] LevelUpManager - 특수 규칙 1: 초기 무기 보장 (레벨 2-5)");

                // 1개는 반드시 신규 무기
                LevelUpOption newWeaponOption = GetRandomNewWeaponOption();
                if (newWeaponOption != null)
                {
                    options.Add(newWeaponOption);
                }

                // 나머지 3개는 전체 풀에서 선택
                pool = GenerateFullPool(includeNewWeapons: true);
                pool = pool.Where(o => !options.Any(existing => IsDuplicateOption(existing, o))).ToList();

                while (options.Count < _optionCount && pool.Count > 0)
                {
                    LevelUpOption randomOption = GetRandomOptionFromPool(pool);
                    options.Add(randomOption);
                    pool.Remove(randomOption);
                }
            }
            // === PRD 3.3.B: 특수 규칙 2 - 무기 슬롯 포화 (N=6) ===
            else if (isWeaponSlotsFull)
            {
                Debug.Log("[INFO] LevelUpManager - 특수 규칙 2: 무기 슬롯 포화 (N=6)");

                // 신규 무기 제외, 무기 강화 + 스탯 강화만
                pool = GenerateFullPool(includeNewWeapons: false);

                while (options.Count < _optionCount && pool.Count > 0)
                {
                    LevelUpOption randomOption = GetRandomOptionFromPool(pool);
                    options.Add(randomOption);
                    pool.Remove(randomOption);
                }
            }
            // === PRD 3.3.C: 일반 생성 로직 ===
            else
            {
                Debug.Log("[INFO] LevelUpManager - 일반 생성 로직");

                pool = GenerateFullPool(includeNewWeapons: true);

                while (options.Count < _optionCount && pool.Count > 0)
                {
                    LevelUpOption randomOption = GetRandomOptionFromPool(pool);
                    options.Add(randomOption);
                    pool.Remove(randomOption);
                }
            }

            Debug.Log($"[INFO] LevelUpManager::GenerateLevelUpOptions - Generated {options.Count} options");
            return options;
        }

        #region Option Pool Generation

        /// <summary>
        /// 전체 선택지 풀 생성
        /// </summary>
        private List<LevelUpOption> GenerateFullPool(bool includeNewWeapons)
        {
            List<LevelUpOption> pool = new List<LevelUpOption>();

            // 1. 신규 무기 획득 옵션 (조건부)
            if (includeNewWeapons && _player.CurrentWeaponCount < _maxWeaponSlots)
            {
                List<LevelUpOption> newWeaponOptions = GetAllNewWeaponOptions();
                pool.AddRange(newWeaponOptions);
            }

            // 2. 보유 무기 강화 옵션
            List<LevelUpOption> weaponUpgradeOptions = GetAllWeaponUpgradeOptions();
            pool.AddRange(weaponUpgradeOptions);

            // 3. 캐릭터 스탯 강화 옵션
            List<LevelUpOption> statUpgradeOptions = GetAllStatUpgradeOptions();
            pool.AddRange(statUpgradeOptions);

            return pool;
        }

        /// <summary>
        /// 모든 신규 무기 옵션 생성
        /// </summary>
        private List<LevelUpOption> GetAllNewWeaponOptions()
        {
            List<LevelUpOption> options = new List<LevelUpOption>();

            List<int> allWeaponIds = _dataProvider.GetAllWeaponIds();

            foreach (int weaponId in allWeaponIds)
            {
                // 이미 보유한 무기는 제외
                if (_player.HasWeapon(weaponId))
                {
                    continue;
                }

                WeaponData weaponData = _dataProvider.GetWeaponData(weaponId);
                if (weaponData != null)
                {
                    LevelUpOption option = LevelUpOption.CreateNewWeaponOption(weaponData);
                    options.Add(option);
                }
            }

            return options;
        }

        /// <summary>
        /// 모든 무기 강화 옵션 생성
        /// </summary>
        private List<LevelUpOption> GetAllWeaponUpgradeOptions()
        {
            List<LevelUpOption> options = new List<LevelUpOption>();

            List<int> equippedWeaponIds = _player.GetEquippedWeaponIds();

            foreach (int weaponId in equippedWeaponIds)
            {
                // 최대 레벨 무기는 제외
                if (_player.IsWeaponMaxLevel(weaponId))
                {
                    continue;
                }

                WeaponData weaponData = _dataProvider.GetWeaponData(weaponId);
                if (weaponData != null)
                {
                    int currentLevel = _player.GetWeaponLevel(weaponId);
                    LevelUpOption option = LevelUpOption.CreateWeaponUpgradeOption(weaponData, currentLevel);
                    options.Add(option);
                }
            }

            return options;
        }

        /// <summary>
        /// 모든 스탯 강화 옵션 생성
        /// </summary>
        private List<LevelUpOption> GetAllStatUpgradeOptions()
        {
            List<LevelUpOption> options = new List<LevelUpOption>();

            foreach (var kvp in _statUpgradeValues)
            {
                StatType statType = kvp.Key;
                float statValue = kvp.Value;

                LevelUpOption option = LevelUpOption.CreateStatUpgradeOption(statType, statValue);
                options.Add(option);
            }

            return options;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 랜덤 신규 무기 옵션 1개 가져오기
        /// </summary>
        private LevelUpOption GetRandomNewWeaponOption()
        {
            List<LevelUpOption> newWeaponOptions = GetAllNewWeaponOptions();

            if (newWeaponOptions.Count == 0)
            {
                Debug.LogWarning("[WARNING] LevelUpManager::GetRandomNewWeaponOption - No new weapons available");
                return null;
            }

            int randomIndex = Random.Range(0, newWeaponOptions.Count);
            return newWeaponOptions[randomIndex];
        }

        /// <summary>
        /// 풀에서 랜덤 옵션 1개 가져오기
        /// </summary>
        private LevelUpOption GetRandomOptionFromPool(List<LevelUpOption> pool)
        {
            if (pool.Count == 0)
            {
                Debug.LogWarning("[WARNING] LevelUpManager::GetRandomOptionFromPool - Pool is empty");
                return null;
            }

            int randomIndex = Random.Range(0, pool.Count);
            return pool[randomIndex];
        }

        /// <summary>
        /// 중복 옵션 체크
        /// </summary>
        private bool IsDuplicateOption(LevelUpOption option1, LevelUpOption option2)
        {
            if (option1.Type != option2.Type)
            {
                return false;
            }

            switch (option1.Type)
            {
                case LevelUpOptionType.NewWeapon:
                case LevelUpOptionType.WeaponUpgrade:
                    return option1.WeaponId == option2.WeaponId;

                case LevelUpOptionType.StatUpgrade:
                    return option1.StatType == option2.StatType;

                default:
                    return false;
            }
        }

        #endregion

        #region UI Integration

        /// <summary>
        /// 레벨업 UI 표시
        /// </summary>
        public void ShowLevelUpUI()
        {
            if (_levelUpUIController == null)
            {
                Debug.LogError("[ERROR] LevelUpManager::ShowLevelUpUI - LevelUpUIController is null");
                return;
            }

            // 선택지 생성
            List<LevelUpOption> options = GenerateLevelUpOptions();

            if (options.Count == 0)
            {
                Debug.LogError("[ERROR] LevelUpManager::ShowLevelUpUI - No options generated");
                return;
            }

            // UI 표시
            _levelUpUIController.Show(options);

            Debug.Log($"[INFO] LevelUpManager::ShowLevelUpUI - Displayed {options.Count} options");
        }

        /// <summary>
        /// 선택지 적용 (UI에서 호출)
        /// </summary>
        public void OnOptionChosen(LevelUpOption option)
        {
            if (option == null || _player == null)
            {
                Debug.LogError("[ERROR] LevelUpManager::OnOptionChosen - Invalid option or player");
                return;
            }

            Debug.Log($"[INFO] LevelUpManager::OnOptionChosen - Applying option: {option.Title}");

            // 옵션 적용
            option.Apply(_player);

            // 플레이어 이동 재개
            _player.ResumeMovement();

            Debug.Log("[INFO] LevelUpManager::OnOptionChosen - Option applied successfully");
        }

        #endregion
    }
}
