using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임의 캐릭터 스탯 데이터
    /// 플레이어와 적 캐릭터의 기본 스탯 구조
    /// </summary>
    [System.Serializable]
    public class CharacterStat
    {
        [SerializeField] private float _maxHp;
        [SerializeField] private float _currentHp;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _damage;
        [SerializeField] private float _defense;

        // Undead Survivor 전용 추가 스탯
        [SerializeField] private float _cooldown;      // 쿨다운 감소 (%)
        [SerializeField] private int _amount;          // 발사체 개수 증가
        [SerializeField] private float _expMultiplier; // 경험치 획득량 배율

        /// <summary>
        /// 최대 체력
        /// </summary>
        public float MaxHp
        {
            get => _maxHp;
            set
            {
                _maxHp = Mathf.Max(0f, value);
                if (_currentHp > _maxHp)
                {
                    _currentHp = _maxHp;
                }
            }
        }

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHp
        {
            get => _currentHp;
            set => _currentHp = Mathf.Clamp(value, 0f, _maxHp);
        }

        /// <summary>
        /// 이동 속도
        /// </summary>
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 공격력 (%)
        /// </summary>
        public float Damage
        {
            get => _damage;
            set => _damage = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 방어력
        /// </summary>
        public float Defense
        {
            get => _defense;
            set => _defense = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 쿨다운 감소 (%)
        /// </summary>
        public float Cooldown
        {
            get => _cooldown;
            set => _cooldown = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 발사체 개수 증가
        /// </summary>
        public int Amount
        {
            get => _amount;
            set => _amount = Mathf.Max(0, value);
        }

        /// <summary>
        /// 경험치 획득량 배율
        /// </summary>
        public float ExpMultiplier
        {
            get => _expMultiplier;
            set => _expMultiplier = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 생존 여부
        /// </summary>
        public bool IsAlive => _currentHp > 0f;

        /// <summary>
        /// 체력 비율 (0.0 ~ 1.0)
        /// </summary>
        public float HpRatio => _maxHp > 0f ? _currentHp / _maxHp : 0f;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public CharacterStat()
        {
            _maxHp = 100f;
            _currentHp = 100f;
            _moveSpeed = 5f;
            _damage = 0f;
            _defense = 0f;
            _cooldown = 0f;
            _amount = 0;
            _expMultiplier = 1f;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        /// <param name="maxHp">최대 체력</param>
        /// <param name="moveSpeed">이동 속도</param>
        /// <param name="damage">공격력 (%)</param>
        /// <param name="defense">방어력</param>
        public CharacterStat(float maxHp, float moveSpeed, float damage = 0f, float defense = 0f)
        {
            _maxHp = Mathf.Max(0f, maxHp);
            _currentHp = _maxHp;
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _damage = Mathf.Max(0f, damage);
            _defense = Mathf.Max(0f, defense);
            _cooldown = 0f;
            _amount = 0;
            _expMultiplier = 1f;
        }

        /// <summary>
        /// 복사 생성자
        /// </summary>
        /// <param name="other">복사할 스탯</param>
        public CharacterStat(CharacterStat other)
        {
            _maxHp = other._maxHp;
            _currentHp = other._currentHp;
            _moveSpeed = other._moveSpeed;
            _damage = other._damage;
            _defense = other._defense;
            _cooldown = other._cooldown;
            _amount = other._amount;
            _expMultiplier = other._expMultiplier;
        }

        /// <summary>
        /// 스탯 초기화
        /// </summary>
        /// <param name="maxHp">최대 체력</param>
        /// <param name="moveSpeed">이동 속도</param>
        /// <param name="damage">공격력 (%)</param>
        /// <param name="defense">방어력</param>
        public void Initialize(float maxHp, float moveSpeed, float damage = 0f, float defense = 0f)
        {
            _maxHp = Mathf.Max(0f, maxHp);
            _currentHp = _maxHp;
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _damage = Mathf.Max(0f, damage);
            _defense = Mathf.Max(0f, defense);
            _cooldown = 0f;
            _amount = 0;
            _expMultiplier = 1f;
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        /// <param name="amount">회복량</param>
        /// <returns>실제 회복된 양</returns>
        public float Heal(float amount)
        {
            float previousHp = _currentHp;
            _currentHp = Mathf.Min(_currentHp + amount, _maxHp);
            float healedAmount = _currentHp - previousHp;

            Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::Heal - Healed {healedAmount:F1} HP ({previousHp:F1} → {_currentHp:F1})");
            return healedAmount;
        }

        /// <summary>
        /// 완전 회복
        /// </summary>
        public void HealFull()
        {
            float previousHp = _currentHp;
            _currentHp = _maxHp;
            Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::HealFull - Fully healed ({previousHp:F1} → {_currentHp:F1})");
        }

        /// <summary>
        /// 데미지 적용
        /// </summary>
        /// <param name="damage">데미지</param>
        /// <returns>실제 받은 데미지 (방어력 적용 후)</returns>
        public float TakeDamage(float damage)
        {
            float actualDamage = Mathf.Max(0f, damage - _defense);
            float previousHp = _currentHp;
            _currentHp = Mathf.Max(0f, _currentHp - actualDamage);

            Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::TakeDamage - Took {actualDamage:F1} damage ({previousHp:F1} → {_currentHp:F1})");

            return actualDamage;
        }

        /// <summary>
        /// 스탯 업그레이드 적용
        /// </summary>
        /// <param name="statType">스탯 타입</param>
        /// <param name="value">증가량</param>
        public void ApplyUpgrade(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.MaxHp:
                    float hpRatio = HpRatio;
                    _maxHp += value;
                    _currentHp = _maxHp * hpRatio; // 비율 유지
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - MaxHp +{value:F1} (Total: {_maxHp:F1})");
                    break;

                case StatType.MoveSpeed:
                    _moveSpeed += value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - MoveSpeed +{value:F1} (Total: {_moveSpeed:F1})");
                    break;

                case StatType.Damage:
                    _damage += value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - Damage +{value:F1}% (Total: {_damage:F1}%)");
                    break;

                case StatType.Defense:
                    _defense += value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - Defense +{value:F1} (Total: {_defense:F1})");
                    break;

                case StatType.Cooldown:
                    _cooldown += value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - Cooldown +{value:F1}% (Total: {_cooldown:F1}%)");
                    break;

                case StatType.Amount:
                    _amount += (int)value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - Amount +{value:F0} (Total: {_amount})");
                    break;

                case StatType.ExpMultiplier:
                    _expMultiplier += value;
                    Debug.Log($"[INFO] UndeadSurvivor::CharacterStat::ApplyUpgrade - ExpMultiplier +{value:F1} (Total: {_expMultiplier:F1})");
                    break;
            }
        }

        /// <summary>
        /// 스탯 검증
        /// </summary>
        /// <returns>유효하면 true</returns>
        public bool Validate()
        {
            bool isValid = true;

            if (_maxHp <= 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - MaxHp must be greater than 0");
                isValid = false;
            }

            if (_currentHp < 0f || _currentHp > _maxHp)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::CharacterStat::Validate - CurrentHp ({_currentHp}) out of range [0, {_maxHp}]");
                isValid = false;
            }

            if (_moveSpeed < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - MoveSpeed cannot be negative");
                isValid = false;
            }

            if (_damage < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - Damage cannot be negative");
                isValid = false;
            }

            if (_defense < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - Defense cannot be negative");
                isValid = false;
            }

            if (_cooldown < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - Cooldown cannot be negative");
                isValid = false;
            }

            if (_amount < 0)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - Amount cannot be negative");
                isValid = false;
            }

            if (_expMultiplier < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::CharacterStat::Validate - ExpMultiplier cannot be negative");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 스탯 리셋
        /// </summary>
        public void Reset()
        {
            _currentHp = _maxHp;
            Debug.Log("[INFO] UndeadSurvivor::CharacterStat::Reset - Stats reset");
        }

        /// <summary>
        /// 스탯 복사
        /// </summary>
        /// <param name="other">복사할 스탯</param>
        public void CopyFrom(CharacterStat other)
        {
            _maxHp = other._maxHp;
            _currentHp = other._currentHp;
            _moveSpeed = other._moveSpeed;
            _damage = other._damage;
            _defense = other._defense;
            _cooldown = other._cooldown;
            _amount = other._amount;
            _expMultiplier = other._expMultiplier;
        }

        /// <summary>
        /// 스탯 정보 문자열
        /// </summary>
        /// <returns>스탯 정보</returns>
        public override string ToString()
        {
            return $"HP: {_currentHp:F1}/{_maxHp:F1} | " +
                   $"Speed: {_moveSpeed:F1} | " +
                   $"Damage: {_damage:F1}% | " +
                   $"Defense: {_defense:F1} | " +
                   $"Cooldown: {_cooldown:F1}% | " +
                   $"Amount: {_amount} | " +
                   $"Exp: x{_expMultiplier:F1}";
        }
    }

    /// <summary>
    /// 스탯 타입 열거형
    /// </summary>
    public enum StatType
    {
        MaxHp,         // 최대 체력
        MoveSpeed,     // 이동 속도
        Damage,        // 공격력 (%)
        Defense,       // 방어력
        Cooldown,      // 쿨다운 감소 (%)
        Amount,        // 발사체 개수
        ExpMultiplier  // 경험치 배율
    }
}
