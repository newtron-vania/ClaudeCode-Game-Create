using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 무기 베이스 클래스 (추상)
    /// 모든 무기의 공통 로직 (자동 공격, 타겟팅, 레벨업)
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Player _owner; // 무기 소유자 (플레이어)

        protected WeaponData _weaponData;
        protected WeaponLevelStat _currentStat;
        protected int _currentLevel = 0; // 0-4 (표시는 1-5)

        // 자동 공격 관련
        private float _cooldownTimer = 0f;
        private bool _isActive = false;

        #region Properties

        /// <summary>
        /// 무기 ID
        /// </summary>
        public int WeaponId => _weaponData?.Id ?? 0;

        /// <summary>
        /// 무기 이름
        /// </summary>
        public string WeaponName => _weaponData?.Name ?? "Unknown";

        /// <summary>
        /// 현재 레벨 (0-4)
        /// </summary>
        public int CurrentLevel => _currentLevel;

        /// <summary>
        /// 현재 스탯
        /// </summary>
        public WeaponLevelStat CurrentStat => _currentStat;

        /// <summary>
        /// 무기 활성화 여부
        /// </summary>
        public bool IsActive => _isActive;

        #endregion

        #region Unity Lifecycle

        protected virtual void Update()
        {
            if (!_isActive || _owner == null) return;

            // 쿨다운 타이머 업데이트
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            // 쿨다운이 끝나면 공격
            if (_cooldownTimer <= 0f)
            {
                PerformAttack();
                _cooldownTimer = _currentStat.Cooldown;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 무기 초기화
        /// </summary>
        /// <param name="owner">무기 소유자</param>
        /// <param name="weaponData">무기 데이터</param>
        /// <param name="level">시작 레벨 (0-4)</param>
        public virtual void Initialize(Player owner, WeaponData weaponData, int level = 0)
        {
            _owner = owner;
            _weaponData = weaponData;
            _currentLevel = Mathf.Clamp(level, 0, 4);

            // 현재 레벨 스탯 설정
            if (_weaponData.LevelStats != null && _currentLevel < _weaponData.LevelStats.Length)
            {
                _currentStat = _weaponData.LevelStats[_currentLevel];
            }
            else
            {
                Debug.LogError($"[ERROR] Weapon::Initialize - Invalid level or stats for {_weaponData.Name}");
                return;
            }

            _isActive = true;
            _cooldownTimer = 0f; // 즉시 첫 공격 가능

            Debug.Log($"[INFO] Weapon::Initialize - {_weaponData.Name} Lv.{_currentLevel + 1} initialized");
        }

        #endregion

        #region Level Up

        /// <summary>
        /// 무기 레벨업
        /// </summary>
        /// <returns>레벨업 성공 여부</returns>
        public virtual bool LevelUp()
        {
            if (_currentLevel >= 4)
            {
                Debug.LogWarning($"[WARNING] Weapon::LevelUp - {_weaponData.Name} is already max level");
                return false;
            }

            _currentLevel++;
            _currentStat = _weaponData.LevelStats[_currentLevel];

            OnLevelUp();

            Debug.Log($"[INFO] Weapon::LevelUp - {_weaponData.Name} leveled up to Lv.{_currentLevel + 1}");
            return true;
        }

        /// <summary>
        /// 레벨업 시 호출 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual void OnLevelUp()
        {
            // 하위 클래스에서 레벨업 시 추가 로직 구현
        }

        #endregion

        #region Attack

        /// <summary>
        /// 공격 수행 (쿨다운 체크 후 호출)
        /// </summary>
        private void PerformAttack()
        {
            if (!_isActive || _owner == null || !_owner.IsAlive)
            {
                return;
            }

            // 하위 클래스에서 구현
            Attack();
        }

        /// <summary>
        /// 실제 공격 로직 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void Attack();

        #endregion

        #region Target Finding

        /// <summary>
        /// 가장 가까운 적 찾기
        /// </summary>
        /// <param name="maxDistance">최대 탐지 거리</param>
        /// <returns>가장 가까운 적 (없으면 null)</returns>
        protected Enemy FindNearestEnemy(float maxDistance = 20f)
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            Enemy nearest = null;
            float minDistance = maxDistance;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 범위 내 모든 적 찾기
        /// </summary>
        /// <param name="radius">탐지 반경</param>
        /// <returns>범위 내 적 리스트</returns>
        protected List<Enemy> FindEnemiesInRadius(float radius)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            Enemy[] enemies = FindObjectsOfType<Enemy>();

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= radius)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            return enemiesInRange;
        }

        #endregion

        #region Damage Calculation

        /// <summary>
        /// 최종 데미지 계산 (플레이어 스탯 적용)
        /// </summary>
        /// <returns>최종 데미지</returns>
        protected float CalculateFinalDamage()
        {
            if (_owner == null || _currentStat == null)
            {
                return 0f;
            }

            // 기본 데미지
            float baseDamage = _currentStat.Damage;

            // 플레이어 공격력 배율 적용
            float damageMultiplier = 1f + (_owner.CharacterStat.GetStat(StatType.Damage) / 100f);

            float finalDamage = baseDamage * damageMultiplier;

            return finalDamage;
        }

        #endregion

        #region Activation

        /// <summary>
        /// 무기 활성화
        /// </summary>
        public virtual void Activate()
        {
            _isActive = true;
            gameObject.SetActive(true);
            Debug.Log($"[INFO] Weapon::Activate - {_weaponData.Name} activated");
        }

        /// <summary>
        /// 무기 비활성화
        /// </summary>
        public virtual void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
            Debug.Log($"[INFO] Weapon::Deactivate - {_weaponData.Name} deactivated");
        }

        #endregion
    }
}
