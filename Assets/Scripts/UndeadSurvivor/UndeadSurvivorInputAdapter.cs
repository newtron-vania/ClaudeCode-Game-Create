using System;
using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// InputManager의 입력 이벤트를 Undead Survivor 게임 전용 입력으로 변환하는 어댑터
    /// InputManager의 InputType을 UndeadSurvivorInputType으로 매핑하여 게임 내부에서 사용합니다.
    /// </summary>
    public class UndeadSurvivorInputAdapter : MonoBehaviour
    {
        /// <summary>
        /// 게임 전용 입력 이벤트 (UndeadSurvivor 컴포넌트들이 구독)
        /// </summary>
        public event Action<UndeadSurvivorInputEventData> OnGameInput;

        [Header("Input Key Mapping")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode _dashKey = KeyCode.Space;
        [SerializeField] private KeyCode _specialSkillKey = KeyCode.Q;
        [SerializeField] private KeyCode _levelUpConfirmKey = KeyCode.Return;

        private Vector2 _currentMoveDirection;

        // WASD 키 매핑
        private readonly Dictionary<KeyCode, Vector2> _moveKeyMap = new Dictionary<KeyCode, Vector2>
        {
            { KeyCode.W, Vector2.up },
            { KeyCode.S, Vector2.down },
            { KeyCode.A, Vector2.left },
            { KeyCode.D, Vector2.right }
        };

        private readonly HashSet<KeyCode> _pressedMoveKeys = new HashSet<KeyCode>();

        private void OnEnable()
        {
            // InputManager의 입력 이벤트 구독
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInputEvent += HandleInputManagerEvent;
            }
        }

        private void OnDisable()
        {
            // MUST unsubscribe
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInputEvent -= HandleInputManagerEvent;
            }
        }

        /// <summary>
        /// InputManager로부터 입력 이벤트를 받아 게임 전용 입력으로 변환
        /// </summary>
        private void HandleInputManagerEvent(InputEventData inputData)
        {
            switch (inputData.Type)
            {
                case InputType.KeyDown:
                    HandleKeyDown(inputData.KeyCode);
                    break;

                case InputType.KeyUp:
                    HandleKeyUp(inputData.KeyCode);
                    break;
            }
        }

        /// <summary>
        /// 키 다운 이벤트 처리
        /// </summary>
        private void HandleKeyDown(KeyCode keyCode)
        {
            // WASD 이동 처리
            if (_moveKeyMap.ContainsKey(keyCode))
            {
                _pressedMoveKeys.Add(keyCode);
                UpdateMoveDirection();
                return;
            }

            // 일시정지
            if (keyCode == _pauseKey)
            {
                var gameInput = new UndeadSurvivorInputEventData(UndeadSurvivorInputType.Pause, true);
                OnGameInput?.Invoke(gameInput);
                Debug.Log($"[INFO] UndeadSurvivor::InputAdapter::HandleKeyDown - Pause pressed");
                return;
            }

            // 대시
            if (keyCode == _dashKey)
            {
                var gameInput = new UndeadSurvivorInputEventData(UndeadSurvivorInputType.Dash, true);
                OnGameInput?.Invoke(gameInput);
                Debug.Log($"[INFO] UndeadSurvivor::InputAdapter::HandleKeyDown - Dash pressed");
                return;
            }

            // 특수 스킬
            if (keyCode == _specialSkillKey)
            {
                var gameInput = new UndeadSurvivorInputEventData(UndeadSurvivorInputType.SpecialSkill, true);
                OnGameInput?.Invoke(gameInput);
                Debug.Log($"[INFO] UndeadSurvivor::InputAdapter::HandleKeyDown - SpecialSkill pressed");
                return;
            }

            // 레벨업 확인
            if (keyCode == _levelUpConfirmKey)
            {
                var gameInput = new UndeadSurvivorInputEventData(UndeadSurvivorInputType.LevelUpConfirm, true);
                OnGameInput?.Invoke(gameInput);
                Debug.Log($"[INFO] UndeadSurvivor::InputAdapter::HandleKeyDown - LevelUpConfirm pressed");
                return;
            }
        }

        /// <summary>
        /// 키 업 이벤트 처리
        /// </summary>
        private void HandleKeyUp(KeyCode keyCode)
        {
            // WASD 이동 처리
            if (_moveKeyMap.ContainsKey(keyCode))
            {
                _pressedMoveKeys.Remove(keyCode);
                UpdateMoveDirection();
                return;
            }

            // 버튼 release 이벤트 (필요 시 추가)
        }

        /// <summary>
        /// 현재 눌린 이동 키들을 기반으로 이동 방향 벡터 계산 및 전송
        /// </summary>
        private void UpdateMoveDirection()
        {
            Vector2 newDirection = Vector2.zero;

            foreach (var key in _pressedMoveKeys)
            {
                newDirection += _moveKeyMap[key];
            }

            // 대각선 이동 시 정규화 (속도 일정하게 유지)
            if (newDirection.sqrMagnitude > 1f)
            {
                newDirection.Normalize();
            }

            _currentMoveDirection = newDirection;

            // 이동 입력 이벤트 발생
            var gameInput = new UndeadSurvivorInputEventData(_currentMoveDirection);
            OnGameInput?.Invoke(gameInput);
        }

        /// <summary>
        /// 현재 이동 방향 가져오기 (외부에서 직접 조회용)
        /// </summary>
        public Vector2 GetCurrentMoveDirection()
        {
            return _currentMoveDirection;
        }
    }
}
