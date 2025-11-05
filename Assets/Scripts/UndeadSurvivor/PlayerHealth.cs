using System;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 체력 관리 및 사망 처리
    /// 피격, 회복, 사망 이벤트 관리
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHp = 100f;
        [SerializeField] private float _currentHp;

        [Header("Invincibility Settings")]
        [SerializeField] private float _invincibilityDuration = 0.5f; // 피격 후 무적 시간
        private float _invincibilityTimer;
        private bool _isInvincible;

        // 이벤트
        public event Action<float, float> OnHealthChanged; // (currentHp, maxHp)
        public event Action<float> OnDamaged; // (damage)
        public event Action<float> OnHealed; // (healAmount)
        public event Action OnDeath;

        // 프로퍼티
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public float HealthPercentage => _maxHp > 0 ? _currentHp / _maxHp : 0f;
        public bool IsAlive => _currentHp > 0f;
        public bool IsInvincible => _isInvincible;

        private void Awake()
        {
            _currentHp = _maxHp;
        }

        private void Update()
        {
            // 무적 시간 처리
            if (_isInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    _isInvincible = false;
                }
            }
        }

        /// <summary>
        /// 최대 체력 설정 (CharacterStat 적용용)
        /// 현재 체력 비율 유지
        /// </summary>
        public void SetMaxHp(float maxHp, bool keepPercentage = true)
        {
            if (maxHp <= 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::PlayerHealth::SetMaxHp - maxHp must be greater than 0");
                return;
            }

            float previousPercentage = HealthPercentage;
            _maxHp = maxHp;

            if (keepPercentage)
            {
                // 비율 유지 (레벨업 시 체력 비율 보존)
                _currentHp = _maxHp * previousPercentage;
            }
            else
            {
                // 전체 회복
                _currentHp = _maxHp;
            }

            OnHealthChanged?.Invoke(_currentHp, _maxHp);
            Debug.Log($"[INFO] UndeadSurvivor::PlayerHealth::SetMaxHp - MaxHp set to {_maxHp:F1}, CurrentHp: {_currentHp:F1} ({HealthPercentage:P0})");
        }

        /// <summary>
        /// 피해 받기
        /// </summary>
        public void TakeDamage(float damage, float defense = 0f)
        {
            if (!IsAlive)
            {
                return;
            }

            if (_isInvincible)
            {
                Debug.Log("[INFO] UndeadSurvivor::PlayerHealth::TakeDamage - Invincible, damage ignored");
                return;
            }

            // 방어력 적용 (최소 피해 1)
            float actualDamage = Mathf.Max(1f, damage - defense);
            _currentHp -= actualDamage;

            Debug.Log($"[INFO] UndeadSurvivor::PlayerHealth::TakeDamage - Damage: {damage:F1}, Defense: {defense:F1}, Actual: {actualDamage:F1}, HP: {_currentHp:F1}/{_maxHp:F1}");

            // 무적 시간 시작
            _isInvincible = true;
            _invincibilityTimer = _invincibilityDuration;

            // 이벤트 발생
            OnDamaged?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(_currentHp, _maxHp);

            // 사망 체크
            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                Die();
            }
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float healAmount)
        {
            if (!IsAlive)
            {
                return;
            }

            float previousHp = _currentHp;
            _currentHp = Mathf.Min(_currentHp + healAmount, _maxHp);
            float actualHealAmount = _currentHp - previousHp;

            if (actualHealAmount > 0f)
            {
                Debug.Log($"[INFO] UndeadSurvivor::PlayerHealth::Heal - Healed: {actualHealAmount:F1}, HP: {_currentHp:F1}/{_maxHp:F1}");
                OnHealed?.Invoke(actualHealAmount);
                OnHealthChanged?.Invoke(_currentHp, _maxHp);
            }
        }

        /// <summary>
        /// 체력 회복 (퍼센트)
        /// </summary>
        public void HealPercentage(float percentage)
        {
            float healAmount = _maxHp * percentage;
            Heal(healAmount);
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            Debug.Log("[INFO] UndeadSurvivor::PlayerHealth::Die - Player died");
            OnDeath?.Invoke();
        }

        /// <summary>
        /// 체력 완전 회복
        /// </summary>
        public void FullHeal()
        {
            Heal(_maxHp - _currentHp);
        }

        /// <summary>
        /// 체력 초기화 (게임 시작 시)
        /// </summary>
        public void Initialize(float maxHp)
        {
            SetMaxHp(maxHp, keepPercentage: false);
            _isInvincible = false;
            _invincibilityTimer = 0f;
            Debug.Log($"[INFO] UndeadSurvivor::PlayerHealth::Initialize - MaxHp: {_maxHp:F1}, CurrentHp: {_currentHp:F1}");
        }

        /// <summary>
        /// 무적 상태 강제 설정 (디버그용 또는 특수 아이템용)
        /// </summary>
        public void SetInvincible(bool invincible, float duration = 0f)
        {
            _isInvincible = invincible;
            _invincibilityTimer = duration;
            Debug.Log($"[INFO] UndeadSurvivor::PlayerHealth::SetInvincible - Invincible: {invincible}, Duration: {duration:F1}s");
        }
    }
}
