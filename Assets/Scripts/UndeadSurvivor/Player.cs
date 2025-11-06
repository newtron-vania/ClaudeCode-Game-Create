using System;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 플레이어 통합 컴포넌트
    /// PlayerController, PlayerHealth, PlayerExperience, PlayerWeaponManager를 통합 관리하고
    /// CharacterData 기반 초기화 및 이벤트 통합을 담당합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(PlayerWeaponManager))]
    public class Player : MonoBehaviour
    {
        [Header("Player Components")]
        private PlayerController _controller;
        private PlayerHealth _health;
        private PlayerExperience _experience;
        private PlayerWeaponManager _weaponManager;

        [Header("Character Data")]
        private CharacterData _characterData;
        private CharacterStat _characterStat;

        [Header("Player State")]
        private bool _isInitialized = false;
        private bool _isAlive = true;

        /// <summary>
        /// 플레이어 사망 이벤트
        /// </summary>
        public event Action OnPlayerDeath;

        /// <summary>
        /// 플레이어 레벨업 이벤트 (레벨업 UI 표시용)
        /// </summary>
        public event Action<int> OnPlayerLevelUp;

        /// <summary>
        /// 플레이어 체력 변경 이벤트 (UI 갱신용)
        /// </summary>
        public event Action<float, float> OnPlayerHealthChanged;

        /// <summary>
        /// 플레이어 경험치 변경 이벤트 (UI 갱신용)
        /// </summary>
        public event Action<int, int, int> OnPlayerExpChanged;

        #region Properties

        /// <summary>현재 레벨</summary>
        public int Level => _experience != null ? _experience.CurrentLevel : 1;

        /// <summary>현재 체력</summary>
        public float CurrentHp => _health != null ? _health.CurrentHp : 0f;

        /// <summary>최대 체력</summary>
        public float MaxHp => _health != null ? _health.MaxHp : 0f;

        /// <summary>현재 경험치</summary>
        public int CurrentExp => _experience != null ? _experience.CurrentExp : 0;

        /// <summary>생존 여부</summary>
        public bool IsAlive => _isAlive;

        /// <summary>캐릭터 데이터</summary>
        public CharacterData CharacterData => _characterData;

        /// <summary>캐릭터 스탯</summary>
        public CharacterStat CharacterStat => _characterStat;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 컴포넌트 참조 가져오기
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<PlayerHealth>();
            _experience = GetComponent<PlayerExperience>();
            _weaponManager = GetComponent<PlayerWeaponManager>();

            if (_controller == null || _health == null || _experience == null || _weaponManager == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Player::Awake - Required components missing!");
                return;
            }

            Debug.Log("[INFO] UndeadSurvivor::Player::Awake - Player components initialized");
        }

        private void OnEnable()
        {
            // 각 컴포넌트의 이벤트 구독
            if (_health != null)
            {
                _health.OnHealthChanged += HandleHealthChanged;
                _health.OnDeath += HandleDeath;
            }

            if (_experience != null)
            {
                _experience.OnLevelUp += HandleLevelUp;
                _experience.OnExpChanged += HandleExpChanged;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponAdded += HandleWeaponAdded;
                _weaponManager.OnWeaponLevelUp += HandleWeaponLevelUp;
            }
        }

        private void OnDisable()
        {
            // MUST unsubscribe
            if (_health != null)
            {
                _health.OnHealthChanged -= HandleHealthChanged;
                _health.OnDeath -= HandleDeath;
            }

            if (_experience != null)
            {
                _experience.OnLevelUp -= HandleLevelUp;
                _experience.OnExpChanged -= HandleExpChanged;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponAdded -= HandleWeaponAdded;
                _weaponManager.OnWeaponLevelUp -= HandleWeaponLevelUp;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// CharacterData를 기반으로 플레이어 초기화
        /// </summary>
        /// <param name="characterData">캐릭터 데이터</param>
        public void Initialize(CharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Player::Initialize - CharacterData is null!");
                return;
            }

            _characterData = characterData;
            _isAlive = true;

            // CharacterStat 생성 및 초기화
            _characterStat = new CharacterStat();
            _characterStat.Initialize(characterData);

            // 각 컴포넌트에 초기 스탯 적용
            ApplyStatsToComponents();

            // 시작 무기 추가 (CharacterData에 StartWeaponId가 있다면)
            // TODO: WeaponData 로드 후 추가
            // if (characterData.StartWeaponId > 0)
            // {
            //     var weaponData = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor")
            //         .GetWeaponData(characterData.StartWeaponId);
            //     _weaponManager.AddWeapon(weaponData);
            // }

            _isInitialized = true;
            Debug.Log($"[INFO] UndeadSurvivor::Player::Initialize - Player initialized with character: {characterData.Name}");
        }

        /// <summary>
        /// CharacterStat의 스탯을 각 컴포넌트에 적용
        /// </summary>
        private void ApplyStatsToComponents()
        {
            if (_characterStat == null)
            {
                Debug.LogWarning("[WARNING] UndeadSurvivor::Player::ApplyStatsToComponents - CharacterStat is null");
                return;
            }

            // 체력 적용
            _health.SetMaxHp(_characterStat.MaxHp, keepPercentage: false);

            // 이동 속도 적용
            _controller.SetMoveSpeed(_characterStat.MoveSpeed);

            // 경험치 배율 적용
            _experience.SetExpMultiplier(_characterStat.ExpMultiplier);

            Debug.Log($"[INFO] UndeadSurvivor::Player::ApplyStatsToComponents - Stats applied: HP={_characterStat.MaxHp}, MoveSpeed={_characterStat.MoveSpeed}, ExpMultiplier={_characterStat.ExpMultiplier}");
        }

        #endregion

        #region Stat Management

        /// <summary>
        /// 스탯 업그레이드 적용 (레벨업 선택지에서 호출)
        /// </summary>
        /// <param name="statType">업그레이드할 스탯 타입</param>
        /// <param name="value">증가량 (절대값 또는 퍼센트)</param>
        public void ApplyStatUpgrade(StatType statType, float value)
        {
            if (_characterStat == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Player::ApplyStatUpgrade - CharacterStat is null!");
                return;
            }

            // CharacterStat에 업그레이드 적용
            _characterStat.ApplyUpgrade(statType, value);

            // 변경된 스탯을 컴포넌트에 반영
            switch (statType)
            {
                case StatType.MaxHp:
                    _health.SetMaxHp(_characterStat.MaxHp, keepPercentage: true);
                    break;

                case StatType.Defense:
                    // Defense는 TakeDamage에서 직접 참조
                    break;

                case StatType.MoveSpeed:
                    _controller.SetMoveSpeed(_characterStat.MoveSpeed);
                    break;

                case StatType.ExpMultiplier:
                    _experience.SetExpMultiplier(_characterStat.ExpMultiplier);
                    break;

                // TODO: 무기 관련 스탯은 WeaponManager 또는 개별 Weapon에 적용
                case StatType.Damage:
                case StatType.Cooldown:
                case StatType.Amount:
                case StatType.Area:
                case StatType.Pierce:
                case StatType.PickupRange:
                case StatType.Luck:
                    // 이 스탯들은 무기 시스템, 아이템 획득 범위 등에서 사용됨
                    // 현재는 CharacterStat에 저장만 하고, 각 시스템에서 GetStat()으로 조회하여 사용
                    break;
            }

            Debug.Log($"[INFO] UndeadSurvivor::Player::ApplyStatUpgrade - {statType} upgraded by {value}");
        }

        /// <summary>
        /// 특정 스탯 값 가져오기
        /// </summary>
        public float GetStat(StatType statType)
        {
            if (_characterStat == null)
            {
                return 0f;
            }

            return _characterStat.GetStat(statType);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 체력 변경 이벤트 핸들러
        /// </summary>
        private void HandleHealthChanged(float currentHp, float maxHp)
        {
            OnPlayerHealthChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>
        /// 사망 이벤트 핸들러
        /// </summary>
        private void HandleDeath()
        {
            _isAlive = false;
            _controller.SetMovementEnabled(false);

            OnPlayerDeath?.Invoke();
            Debug.Log("[INFO] UndeadSurvivor::Player::HandleDeath - Player died");
        }

        /// <summary>
        /// 레벨업 이벤트 핸들러
        /// </summary>
        private void HandleLevelUp(int newLevel)
        {
            // 레벨업 UI 표시 (Time.timeScale = 0 또는 이동 제한)
            _controller.SetMovementEnabled(false);

            OnPlayerLevelUp?.Invoke(newLevel);
            Debug.Log($"[INFO] UndeadSurvivor::Player::HandleLevelUp - Level up to {newLevel}");
        }

        /// <summary>
        /// 경험치 변경 이벤트 핸들러
        /// </summary>
        private void HandleExpChanged(int currentExp, int expForNextLevel, int currentLevel)
        {
            OnPlayerExpChanged?.Invoke(currentExp, expForNextLevel, currentLevel);
        }

        /// <summary>
        /// 무기 추가 이벤트 핸들러
        /// </summary>
        private void HandleWeaponAdded(int weaponId, string weaponName, int currentLevel)
        {
            Debug.Log($"[INFO] UndeadSurvivor::Player::HandleWeaponAdded - Weapon added: {weaponName} (Level {currentLevel})");
            // TODO: UI 알림
        }

        /// <summary>
        /// 무기 레벨업 이벤트 핸들러
        /// </summary>
        private void HandleWeaponLevelUp(int weaponId, int newLevel)
        {
            Debug.Log($"[INFO] UndeadSurvivor::Player::HandleWeaponLevelUp - Weapon {weaponId} leveled up to {newLevel}");
            // TODO: UI 알림
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 피격 처리 (외부에서 호출)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!_isAlive || _health == null)
            {
                return;
            }

            float defense = _characterStat != null ? _characterStat.Defense : 0f;
            _health.TakeDamage(damage, defense);
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float healAmount)
        {
            if (!_isAlive || _health == null)
            {
                return;
            }

            _health.Heal(healAmount);
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void GainExp(int expAmount)
        {
            if (!_isAlive || _experience == null)
            {
                return;
            }

            _experience.GainExp(expAmount);
        }

        /// <summary>
        /// 무기 추가
        /// </summary>
        public bool AddWeapon(WeaponData weaponData)
        {
            if (_weaponManager == null)
            {
                return false;
            }

            return _weaponManager.AddWeapon(weaponData);
        }

        /// <summary>
        /// 무기 레벨업
        /// </summary>
        public bool LevelUpWeapon(int weaponId)
        {
            if (_weaponManager == null)
            {
                return false;
            }

            return _weaponManager.LevelUpWeapon(weaponId);
        }

        /// <summary>
        /// 레벨업 선택 완료 후 이동 재개
        /// </summary>
        public void ResumeMovement()
        {
            if (_controller != null && _isAlive)
            {
                _controller.SetMovementEnabled(true);
                Debug.Log("[INFO] UndeadSurvivor::Player::ResumeMovement - Movement resumed");
            }
        }

        #endregion
    }
}
