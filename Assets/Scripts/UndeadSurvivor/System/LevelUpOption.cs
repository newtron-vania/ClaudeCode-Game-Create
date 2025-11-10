using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 레벨업 선택지 타입
    /// </summary>
    public enum LevelUpOptionType
    {
        NewWeapon,      // 신규 무기 획득
        WeaponUpgrade,  // 보유 무기 강화
        StatUpgrade     // 캐릭터 능력치 강화
    }

    /// <summary>
    /// 레벨업 선택지 데이터
    /// 레벨업 시 플레이어에게 제공되는 강화 옵션
    /// </summary>
    [System.Serializable]
    public class LevelUpOption
    {
        /// <summary>
        /// 선택지 타입
        /// </summary>
        public LevelUpOptionType Type;

        /// <summary>
        /// 선택지 제목 (UI 표시용)
        /// </summary>
        public string Title;

        /// <summary>
        /// 선택지 설명 (UI 표시용)
        /// </summary>
        public string Description;

        /// <summary>
        /// 아이콘 경로 (Addressables 또는 Resources)
        /// </summary>
        public string IconPath;

        // 타입별 데이터
        /// <summary>
        /// 무기 관련 선택지: WeaponData ID
        /// </summary>
        public int WeaponId;

        /// <summary>
        /// 스탯 관련 선택지: StatType
        /// </summary>
        public StatType StatType;

        /// <summary>
        /// 스탯 증가값
        /// </summary>
        public float StatValue;

        /// <summary>
        /// 무기 현재 레벨 (무기 강화용, 표시용)
        /// </summary>
        public int CurrentWeaponLevel;

        #region Factory Methods

        /// <summary>
        /// 신규 무기 획득 선택지 생성
        /// </summary>
        public static LevelUpOption CreateNewWeaponOption(WeaponData weaponData)
        {
            return new LevelUpOption
            {
                Type = LevelUpOptionType.NewWeapon,
                Title = $"[NEW] {weaponData.Name}",
                Description = $"새로운 무기를 획득합니다.\n기본 데미지: {weaponData.LevelStats[0].Damage}",
                IconPath = $"Sprites/UndeadSurvivor/Weapons/{weaponData.Name}_icon",
                WeaponId = weaponData.Id,
                CurrentWeaponLevel = 0
            };
        }

        /// <summary>
        /// 보유 무기 강화 선택지 생성
        /// </summary>
        public static LevelUpOption CreateWeaponUpgradeOption(WeaponData weaponData, int currentLevel)
        {
            int nextLevel = currentLevel + 1;
            WeaponLevelStat nextStat = weaponData.LevelStats[nextLevel];

            return new LevelUpOption
            {
                Type = LevelUpOptionType.WeaponUpgrade,
                Title = $"[Lv.{nextLevel + 1}] {weaponData.Name}",
                Description = $"무기를 강화합니다.\n데미지: {nextStat.Damage} | 쿨다운: {nextStat.Cooldown:F1}s",
                IconPath = $"Sprites/UndeadSurvivor/Weapons/{weaponData.Name}_icon",
                WeaponId = weaponData.Id,
                CurrentWeaponLevel = currentLevel
            };
        }

        /// <summary>
        /// 캐릭터 스탯 강화 선택지 생성
        /// </summary>
        public static LevelUpOption CreateStatUpgradeOption(StatType statType, float statValue)
        {
            string statName = GetStatName(statType);
            string statDescription = GetStatDescription(statType, statValue);
            string iconPath = $"Sprites/UndeadSurvivor/Stats/{statType}_icon";

            return new LevelUpOption
            {
                Type = LevelUpOptionType.StatUpgrade,
                Title = $"[STAT] {statName}",
                Description = statDescription,
                IconPath = iconPath,
                StatType = statType,
                StatValue = statValue
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// StatType에 해당하는 한글 이름 반환
        /// </summary>
        private static string GetStatName(StatType statType)
        {
            switch (statType)
            {
                case StatType.Damage: return "공격력 (Might)";
                case StatType.MaxHp: return "최대 체력 (Max HP)";
                case StatType.Defense: return "방어력 (Armor)";
                case StatType.MoveSpeed: return "이동 속도 (Speed)";
                case StatType.Area: return "범위 (Area)";
                case StatType.Cooldown: return "쿨타임 (Cooldown)";
                case StatType.Amount: return "투사체 개수 (Amount)";
                case StatType.Pierce: return "관통력 (Pierce)";
                case StatType.ExpMultiplier: return "경험치 획득 (Growth)";
                case StatType.PickupRange: return "아이템 획득 범위 (Pickup)";
                case StatType.Luck: return "행운 (Luck)";
                default: return statType.ToString();
            }
        }

        /// <summary>
        /// StatType에 해당하는 설명 생성
        /// </summary>
        private static string GetStatDescription(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.Damage:
                    return $"모든 무기의 피해량 +{value}% 증가";
                case StatType.MaxHp:
                    return $"최대 체력 +{value}% 증가";
                case StatType.Defense:
                    return $"받는 모든 피해량 -{value} 감소";
                case StatType.MoveSpeed:
                    return $"캐릭터 이동 속도 +{value}% 증가";
                case StatType.Area:
                    return $"모든 무기의 공격 범위 +{value}% 증가";
                case StatType.Cooldown:
                    return $"모든 무기의 재사용 대기시간 -{value}% 감소";
                case StatType.Amount:
                    return $"투사체 개수 +{(int)value} (적용 가능 무기)";
                case StatType.Pierce:
                    return $"모든 투사체의 관통 횟수 +{(int)value}";
                case StatType.ExpMultiplier:
                    return $"경험치 보석 획득량 +{value}% 증가";
                case StatType.PickupRange:
                    return $"아이템/경험치 획득 반경 +{value}% 증가";
                case StatType.Luck:
                    return $"드랍률, 선택지 희귀도 +{value}% 증가";
                default:
                    return $"{statType} +{value}";
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 선택지 적용 (Player에 효과 적용)
        /// </summary>
        public void Apply(Player player)
        {
            if (player == null)
            {
                Debug.LogError("[ERROR] LevelUpOption::Apply - Player is null");
                return;
            }

            switch (Type)
            {
                case LevelUpOptionType.NewWeapon:
                    ApplyNewWeapon(player);
                    break;

                case LevelUpOptionType.WeaponUpgrade:
                    ApplyWeaponUpgrade(player);
                    break;

                case LevelUpOptionType.StatUpgrade:
                    ApplyStatUpgrade(player);
                    break;
            }
        }

        /// <summary>
        /// 신규 무기 획득 적용
        /// </summary>
        private void ApplyNewWeapon(Player player)
        {
            // DataManager로부터 WeaponData 가져오기
            UndeadSurvivorDataProvider dataProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

            if (dataProvider == null)
            {
                Debug.LogError("[ERROR] LevelUpOption::ApplyNewWeapon - DataProvider not found");
                return;
            }

            WeaponData weaponData = dataProvider.GetWeaponData(WeaponId);
            if (weaponData == null)
            {
                Debug.LogError($"[ERROR] LevelUpOption::ApplyNewWeapon - WeaponData ID {WeaponId} not found");
                return;
            }

            bool added = player.AddWeapon(weaponData);
            if (added)
            {
                Debug.Log($"[INFO] LevelUpOption::ApplyNewWeapon - Added weapon: {weaponData.Name}");
            }
            else
            {
                Debug.LogWarning($"[WARNING] LevelUpOption::ApplyNewWeapon - Failed to add weapon: {weaponData.Name}");
            }
        }

        /// <summary>
        /// 보유 무기 강화 적용
        /// </summary>
        private void ApplyWeaponUpgrade(Player player)
        {
            bool leveledUp = player.LevelUpWeapon(WeaponId);
            if (leveledUp)
            {
                Debug.Log($"[INFO] LevelUpOption::ApplyWeaponUpgrade - Weapon ID {WeaponId} leveled up");
            }
            else
            {
                Debug.LogWarning($"[WARNING] LevelUpOption::ApplyWeaponUpgrade - Failed to level up weapon ID {WeaponId}");
            }
        }

        /// <summary>
        /// 캐릭터 스탯 강화 적용
        /// </summary>
        private void ApplyStatUpgrade(Player player)
        {
            player.ApplyStatUpgrade(StatType, StatValue);
            Debug.Log($"[INFO] LevelUpOption::ApplyStatUpgrade - Applied {StatType} +{StatValue}");
        }

        #endregion
    }
}
