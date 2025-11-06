using System;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 적 베이스 클래스
    /// 체력, 이동, 피격, 사망 관리
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;

        private MonsterData _monsterData;
        private float _currentHp;
        private float _currentMoveSpeed;
        private float _difficultyMultiplier = 1.0f;

        private bool _isAlive = true;
        private Player _targetPlayer;

        /// <summary>
        /// 적 사망 이벤트 (경험치 드롭용)
        /// </summary>
        public event Action<Enemy> OnDeath;

        /// <summary>
        /// 적 피격 이벤트
        /// </summary>
        public event Action<float> OnDamaged;

        #region Properties

        /// <summary>
        /// 현재 체력
        /// </summary>
        public float CurrentHp => _currentHp;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public float MaxHp => _monsterData != null ? _monsterData.MaxHp * _difficultyMultiplier : 0;

        /// <summary>
        /// 공격력
        /// </summary>
        public float Damage => _monsterData != null ? _monsterData.Damage * _difficultyMultiplier : 0;

        /// <summary>
        /// 경험치 배율
        /// </summary>
        public float ExpMultiplier => _monsterData != null ? _monsterData.ExpMultiplier : 1.0f;

        /// <summary>
        /// 생존 여부
        /// </summary>
        public bool IsAlive => _isAlive;

        /// <summary>
        /// 몬스터 데이터
        /// </summary>
        public MonsterData MonsterData => _monsterData;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 자동 참조 연결
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null)
                _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (!_isAlive) return;

            // AI 및 이동 업데이트 (플레이어 추적)
            UpdateMovement();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // 플레이어와 충돌 시 피해
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null && player.IsAlive)
                {
                    player.TakeDamage(Damage);
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 적 초기화 (MonsterData 기반)
        /// </summary>
        /// <param name="monsterData">몬스터 데이터</param>
        /// <param name="difficultyMultiplier">난이도 배율 (시간 기반)</param>
        /// <param name="targetPlayer">추적할 플레이어</param>
        public void Initialize(MonsterData monsterData, float difficultyMultiplier, Player targetPlayer)
        {
            _monsterData = monsterData;
            _difficultyMultiplier = difficultyMultiplier;
            _targetPlayer = targetPlayer;

            // 스탯 설정
            _currentHp = MaxHp;
            _currentMoveSpeed = _monsterData.MoveSpeed * _difficultyMultiplier;
            _isAlive = true;

            // 컴포넌트 활성화
            _collider.enabled = true;

            Debug.Log($"[INFO] Enemy::Initialize - {_monsterData.Name} spawned (HP: {MaxHp:F0}, Speed: {_currentMoveSpeed:F1}, Multiplier: {_difficultyMultiplier:F2})");
        }

        #endregion

        #region Movement

        /// <summary>
        /// 플레이어 추적 이동
        /// </summary>
        private void UpdateMovement()
        {
            if (_targetPlayer == null || !_targetPlayer.IsAlive)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            // 플레이어 방향 계산
            Vector2 direction = (_targetPlayer.transform.position - transform.position).normalized;

            // 이동
            _rigidbody.MovePosition(_rigidbody.position + direction * _currentMoveSpeed * Time.fixedDeltaTime);
        }

        #endregion

        #region Combat

        /// <summary>
        /// 피격 처리
        /// </summary>
        /// <param name="damage">받을 피해량</param>
        public void TakeDamage(float damage)
        {
            if (!_isAlive) return;

            // 방어력 적용
            float actualDamage = Mathf.Max(1, damage - _monsterData.Defense);
            _currentHp -= actualDamage;

            OnDamaged?.Invoke(actualDamage);

            Debug.Log($"[INFO] Enemy::TakeDamage - {_monsterData.Name} took {actualDamage:F1} damage ({_currentHp:F0}/{MaxHp:F0})");

            // 사망 체크
            if (_currentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            if (!_isAlive) return;

            _isAlive = false;
            _rigidbody.linearVelocity = Vector2.zero;
            _collider.enabled = false;

            Debug.Log($"[INFO] Enemy::Die - {_monsterData.Name} died");

            // 사망 이벤트 발생 (경험치 드롭)
            OnDeath?.Invoke(this);

            // 오브젝트 풀로 반환 또는 파괴
            ReturnToPool();
        }

        #endregion

        #region Pooling

        /// <summary>
        /// 오브젝트 풀로 반환
        /// </summary>
        private void ReturnToPool()
        {
            // TODO: PoolManager 연동
            // PoolManager.Instance.ReturnToPool("Enemy", gameObject);

            // 임시: 오브젝트 파괴
            Destroy(gameObject, 0.5f);
        }

        /// <summary>
        /// 풀에서 재사용 시 호출
        /// </summary>
        public void OnSpawnedFromPool()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 풀로 반환 시 호출
        /// </summary>
        public void OnReturnedToPool()
        {
            // 이벤트 구독 해제
            OnDeath = null;
            OnDamaged = null;

            _targetPlayer = null;
            _monsterData = null;

            gameObject.SetActive(false);
        }

        #endregion
    }
}
