using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 캐릭터 이동 및 입력 처리
    /// UndeadSurvivorInputAdapter로부터 게임 전용 입력을 받아 Rigidbody2D로 이동 처리
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Input Adapter")]
        [SerializeField] private UndeadSurvivorInputAdapter _inputAdapter;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _moveDirection;

        private bool _isMovementEnabled = true;

        // 마지막 이동 방향 저장 (무기 조준용)
        public Vector2 LastMoveDirection { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            LastMoveDirection = Vector2.right; // 초기 방향: 오른쪽

            // InputAdapter가 인스펙터에 할당되지 않았으면 자동 찾기
            if (_inputAdapter == null)
            {
                _inputAdapter = FindObjectOfType<UndeadSurvivorInputAdapter>();
                if (_inputAdapter == null)
                {
                    Debug.LogError("[ERROR] UndeadSurvivor::PlayerController::Awake - UndeadSurvivorInputAdapter not found in scene!");
                }
            }
        }

        private void OnEnable()
        {
            // UndeadSurvivorInputAdapter의 게임 전용 입력 이벤트 구독
            if (_inputAdapter != null)
            {
                _inputAdapter.OnGameInput += HandleGameInput;
            }
        }

        private void OnDisable()
        {
            // MUST unsubscribe
            if (_inputAdapter != null)
            {
                _inputAdapter.OnGameInput -= HandleGameInput;
            }
        }

        private void Update()
        {
            // 이동 방향 벡터 계산
            _moveDirection = _moveInput.normalized;

            // 마지막 이동 방향 업데이트 (무기 조준용)
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                LastMoveDirection = _moveDirection;
            }
        }

        private void FixedUpdate()
        {
            if (!_isMovementEnabled)
            {
                return;
            }

            // Rigidbody2D.MovePosition으로 물리 기반 이동
            Vector2 nextPosition = _rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(nextPosition);
        }

        /// <summary>
        /// UndeadSurvivorInputAdapter로부터 게임 전용 입력 이벤트 수신
        /// </summary>
        private void HandleGameInput(UndeadSurvivorInputEventData inputData)
        {
            // Move 입력 처리
            if (inputData.InputType == UndeadSurvivorInputType.Move)
            {
                _moveInput = inputData.MoveDirection;
            }
        }

        /// <summary>
        /// 이동 속도 설정 (CharacterStat의 MoveSpeed 적용용)
        /// </summary>
        public void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
            Debug.Log($"[INFO] UndeadSurvivor::PlayerController::SetMoveSpeed - MoveSpeed set to {_moveSpeed:F2}");
        }

        /// <summary>
        /// 이동 활성화/비활성화 (레벨업 UI 표시 시 Time.timeScale=0 대신 사용 가능)
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            _isMovementEnabled = enabled;
            if (!enabled)
            {
                _moveInput = Vector2.zero;
                _moveDirection = Vector2.zero;
            }
            Debug.Log($"[INFO] UndeadSurvivor::PlayerController::SetMovementEnabled - Movement {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 현재 이동 중인지 여부
        /// </summary>
        public bool IsMoving()
        {
            return _moveDirection.sqrMagnitude > 0.01f;
        }
    }
}