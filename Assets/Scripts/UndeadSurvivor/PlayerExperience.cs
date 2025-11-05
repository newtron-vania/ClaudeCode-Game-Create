using System;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 경험치 및 레벨업 시스템
    /// 경험치 획득, 레벨업, 레벨업 이벤트 관리
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [Header("Experience Settings")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _currentExp;
        [SerializeField] private int _expForNextLevel = 10; // 레벨 1→2 필요 경험치

        [Header("Level Up Settings")]
        [SerializeField] private float _expMultiplier = 1.0f; // 경험치 획득 배율

        // 레벨당 필요 경험치 증가율
        private const float EXP_INCREASE_RATE = 1.2f;

        // 이벤트
        public event Action<int> OnLevelUp; // (newLevel)
        public event Action<int, int, int> OnExpChanged; // (currentExp, expForNextLevel, currentLevel)
        public event Action<int> OnExpGained; // (expAmount)

        // 프로퍼티
        public int CurrentLevel => _currentLevel;
        public int CurrentExp => _currentExp;
        public int ExpForNextLevel => _expForNextLevel;
        public float ExpProgress => _expForNextLevel > 0 ? (float)_currentExp / _expForNextLevel : 0f;
        public float ExpMultiplier => _expMultiplier;

        private void Awake()
        {
            _currentExp = 0;
            _currentLevel = 1;
            CalculateExpForNextLevel();
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void GainExp(int expAmount)
        {
            if (expAmount <= 0)
            {
                return;
            }

            // 경험치 배율 적용
            int actualExpAmount = Mathf.RoundToInt(expAmount * _expMultiplier);
            _currentExp += actualExpAmount;

            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::GainExp - Gained {actualExpAmount} exp (base: {expAmount}, multiplier: {_expMultiplier:F2}), Current: {_currentExp}/{_expForNextLevel}");

            OnExpGained?.Invoke(actualExpAmount);
            OnExpChanged?.Invoke(_currentExp, _expForNextLevel, _currentLevel);

            // 레벨업 체크 (여러 레벨 한번에 오를 수 있음)
            while (_currentExp >= _expForNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 레벨업 처리
        /// </summary>
        private void LevelUp()
        {
            _currentExp -= _expForNextLevel;
            _currentLevel++;

            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::LevelUp - Level up! New level: {_currentLevel}");

            // 다음 레벨 필요 경험치 계산
            CalculateExpForNextLevel();

            // 레벨업 이벤트 발생
            OnLevelUp?.Invoke(_currentLevel);
            OnExpChanged?.Invoke(_currentExp, _expForNextLevel, _currentLevel);
        }

        /// <summary>
        /// 다음 레벨 필요 경험치 계산
        /// 공식: 기본 경험치 * (증가율 ^ (레벨 - 1))
        /// </summary>
        private void CalculateExpForNextLevel()
        {
            // 레벨 1→2: 10
            // 레벨 2→3: 12
            // 레벨 3→4: 14 (반올림)
            // ...
            _expForNextLevel = Mathf.RoundToInt(10 * Mathf.Pow(EXP_INCREASE_RATE, _currentLevel - 1));
            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::CalculateExpForNextLevel - Level {_currentLevel} requires {_expForNextLevel} exp for next level");
        }

        /// <summary>
        /// 경험치 배율 설정 (CharacterStat의 ExpMultiplier 적용용)
        /// </summary>
        public void SetExpMultiplier(float multiplier)
        {
            if (multiplier < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::PlayerExperience::SetExpMultiplier - Multiplier cannot be negative");
                return;
            }

            _expMultiplier = multiplier;
            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::SetExpMultiplier - Exp multiplier set to {_expMultiplier:F2} ({(_expMultiplier - 1f) * 100f:+0;-0}%)");
        }

        /// <summary>
        /// 경험치 배율 증가
        /// </summary>
        public void AddExpMultiplier(float addValue)
        {
            _expMultiplier += addValue;
            if (_expMultiplier < 0f)
            {
                _expMultiplier = 0f;
            }
            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::AddExpMultiplier - Exp multiplier increased by {addValue:F2}, new value: {_expMultiplier:F2}");
        }

        /// <summary>
        /// 초기화 (게임 시작 시)
        /// </summary>
        public void Initialize(int startLevel = 1, float expMultiplier = 1.0f)
        {
            _currentLevel = startLevel;
            _currentExp = 0;
            _expMultiplier = expMultiplier;
            CalculateExpForNextLevel();

            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::Initialize - Level: {_currentLevel}, ExpMultiplier: {_expMultiplier:F2}, ExpForNextLevel: {_expForNextLevel}");

            OnExpChanged?.Invoke(_currentExp, _expForNextLevel, _currentLevel);
        }

        /// <summary>
        /// 강제 레벨업 (디버그/테스트용)
        /// </summary>
        public void ForceLevelUp()
        {
            _currentExp = _expForNextLevel;
            LevelUp();
        }

        /// <summary>
        /// 경험치 직접 설정 (디버그/테스트용)
        /// </summary>
        public void SetExp(int exp)
        {
            _currentExp = Mathf.Max(0, exp);
            OnExpChanged?.Invoke(_currentExp, _expForNextLevel, _currentLevel);
            Debug.Log($"[INFO] UndeadSurvivor::PlayerExperience::SetExp - Exp set to {_currentExp}/{_expForNextLevel}");
        }
    }
}
