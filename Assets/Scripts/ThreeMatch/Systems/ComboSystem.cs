using System;
using UnityEngine;

namespace ThreeMatch.Systems
{
    /// <summary>
    /// 콤보 시스템 (순수 C# 클래스)
    /// 연쇄 매치 시 콤보 증가 및 점수 배율 계산
    /// </summary>
    public class ComboSystem
    {
        // ========== 설정 가능한 필드 (생성자로 전달) ==========
        private float _comboTimeout;              // 콤보 타임아웃 시간
        private int _maxComboMultiplier;          // 최대 콤보 배율

        // ========== Private 필드 ==========
        private int _currentCombo;
        private int _maxCombo;
        private float _comboTimer;

        // ========== 이벤트 ==========
        public event Action<int, int> OnComboChanged;  // (currentCombo, multiplier)
        public event Action OnComboReset;

        // ========== 프로퍼티 ==========
        public int CurrentCombo => _currentCombo;
        public int MaxCombo => _maxCombo;
        public float ComboTimer => _comboTimer;

        // ========== 생성자 ==========

        /// <summary>
        /// ComboSystem 생성자
        /// </summary>
        /// <param name="comboTimeout">콤보 타임아웃 시간 (초)</param>
        /// <param name="maxComboMultiplier">최대 콤보 배율</param>
        public ComboSystem(float comboTimeout = 2f, int maxComboMultiplier = 5)
        {
            _comboTimeout = comboTimeout;
            _maxComboMultiplier = maxComboMultiplier;

            _currentCombo = 0;
            _maxCombo = 0;
            _comboTimer = 0f;
        }

        // ========== 콤보 관리 ==========

        /// <summary>
        /// 콤보 증가
        /// </summary>
        public void IncrementCombo()
        {
            _currentCombo++;
            _comboTimer = _comboTimeout;

            // 최대 콤보 갱신
            if (_currentCombo > _maxCombo)
            {
                _maxCombo = _currentCombo;
            }

            int multiplier = GetMultiplier();
            OnComboChanged?.Invoke(_currentCombo, multiplier);

            LogDebug($"Combo increased: {_currentCombo} (x{multiplier})");
        }

        /// <summary>
        /// 콤보 초기화
        /// </summary>
        public void ResetCombo()
        {
            if (_currentCombo > 0)
            {
                LogDebug($"Combo reset from {_currentCombo}");
                _currentCombo = 0;
                _comboTimer = 0f;
                OnComboReset?.Invoke();
            }
        }

        /// <summary>
        /// 콤보 타이머 업데이트 (매 프레임 호출)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_currentCombo > 0 && _comboTimer > 0f)
            {
                _comboTimer -= deltaTime;

                if (_comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        // ========== 배율 계산 ==========

        /// <summary>
        /// 현재 콤보 배율 계산
        /// 콤보 1~4: 1x, 2x, 3x, 4x
        /// 콤보 5 이상: 5x (최대 배율)
        /// </summary>
        public int GetMultiplier()
        {
            if (_currentCombo <= 0)
                return 1;

            int multiplier = Mathf.Min(_currentCombo, _maxComboMultiplier);
            return multiplier;
        }

        /// <summary>
        /// 점수에 콤보 배율 적용
        /// </summary>
        public int ApplyMultiplier(int baseScore)
        {
            int multiplier = GetMultiplier();
            return baseScore * multiplier;
        }

        // ========== 설정 변경 ==========

        /// <summary>
        /// 콤보 타임아웃 시간 변경
        /// </summary>
        public void SetComboTimeout(float timeout)
        {
            _comboTimeout = Mathf.Max(0.1f, timeout);
        }

        /// <summary>
        /// 최대 콤보 배율 변경
        /// </summary>
        public void SetMaxMultiplier(int maxMultiplier)
        {
            _maxComboMultiplier = Mathf.Max(1, maxMultiplier);
        }

        // ========== 상태 확인 ==========

        /// <summary>
        /// 콤보가 활성화되어 있는지 확인
        /// </summary>
        public bool IsComboActive()
        {
            return _currentCombo > 0 && _comboTimer > 0f;
        }

        /// <summary>
        /// 콤보 진행률 (0~1)
        /// </summary>
        public float GetComboProgress()
        {
            if (_comboTimeout <= 0f)
                return 0f;

            return Mathf.Clamp01(_comboTimer / _comboTimeout);
        }

        // ========== 디버깅 ==========

        private void LogDebug(string message)
        {
            #if UNITY_EDITOR
            Debug.Log($"[ComboSystem] {message}");
            #endif
        }

        /// <summary>
        /// 콤보 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"Combo: {_currentCombo} (x{GetMultiplier()}) | Max: {_maxCombo} | Timer: {_comboTimer:F2}s";
        }
    }
}
