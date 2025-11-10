using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 투사체 클래스
    /// 원거리 무기가 발사하는 발사체 (Fireball, Arrow, Bullet 등)
    /// IPoolable 구현으로 풀링 시스템 지원
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 10f; // 이동 속도

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer; // 스프라이트 렌더러
        [SerializeField] private TrailRenderer _trailRenderer; // 궤적 효과 (옵션)

        [Header("Effects")]
        [SerializeField] private GameObject _hitEffectPrefab; // 충돌 이펙트 프리팹

        private Rigidbody2D _rigidbody;
        private CircleCollider2D _collider;

        // 투사체 설정 (외부에서 설정)
        private float _damage = 0f; // 데미지
        private int _penetrationCount = 0; // 관통 횟수 (0 = 한 번만 충돌)
        private float _lifeTime = 5f; // 생존 시간 (초)
        private Vector2 _direction; // 이동 방향

        // 런타임 상태
        private int _currentPenetration = 0; // 현재 관통 횟수
        private float _aliveTime = 0f; // 생존 시간

        #region Unity Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();

            // Rigidbody2D 설정
            _rigidbody.gravityScale = 0f; // 중력 없음
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Collider를 Trigger로 설정
            _collider.isTrigger = true;
        }

        private void Update()
        {
            // 생존 시간 체크
            _aliveTime += Time.deltaTime;
            if (_aliveTime >= _lifeTime)
            {
                DestroyProjectile();
            }
        }

        private void FixedUpdate()
        {
            // 투사체 이동
            _rigidbody.linearVelocity = _direction * _moveSpeed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Enemy와 충돌 체크
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                // 데미지 적용
                enemy.TakeDamage(_damage);

                // 관통 카운트 증가
                _currentPenetration++;

                // 관통 횟수 초과 시 파괴
                if (_currentPenetration > _penetrationCount)
                {
                    OnHit(collision.transform.position);
                    DestroyProjectile();
                }
                else
                {
                    // 관통 시 작은 이펙트 (옵션)
                    OnPenetrate(collision.transform.position);
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 투사체 초기화
        /// </summary>
        /// <param name="direction">이동 방향</param>
        /// <param name="damage">데미지</param>
        /// <param name="penetrationCount">관통 횟수 (0 = 한 번만 충돌)</param>
        /// <param name="lifeTime">생존 시간 (초)</param>
        /// <param name="moveSpeed">이동 속도 (옵션, 기본값 사용)</param>
        public void Initialize(Vector2 direction, float damage, int penetrationCount = 0, float lifeTime = 5f, float moveSpeed = -1f)
        {
            _direction = direction.normalized;
            _damage = damage;
            _penetrationCount = penetrationCount;
            _lifeTime = lifeTime;

            if (moveSpeed > 0f)
            {
                _moveSpeed = moveSpeed;
            }

            _currentPenetration = 0;
            _aliveTime = 0f;

            // 방향에 맞게 회전 (스프라이트가 오른쪽을 향한다고 가정)
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Trail Renderer 초기화 (있으면)
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
            }

            Debug.Log($"[INFO] Projectile::Initialize - Direction: {_direction}, Damage: {_damage}, Penetration: {_penetrationCount}");
        }

        #endregion

        #region Hit & Destroy

        /// <summary>
        /// 충돌 시 호출 (투사체 파괴됨)
        /// </summary>
        private void OnHit(Vector2 hitPosition)
        {
            // 충돌 이펙트 생성 (있으면)
            if (_hitEffectPrefab != null)
            {
                // 임시: Instantiate 사용 (TODO: ResourceManager로 변경)
                GameObject effectObj = Instantiate(_hitEffectPrefab, hitPosition, Quaternion.identity);
                Destroy(effectObj, 2f); // 2초 후 이펙트 제거
            }

            Debug.Log($"[INFO] Projectile::OnHit - Hit at {hitPosition}, destroying projectile");
        }

        /// <summary>
        /// 관통 시 호출 (투사체는 계속 날아감)
        /// </summary>
        private void OnPenetrate(Vector2 penetratePosition)
        {
            // 관통 이펙트 (옵션)
            // TODO: 작은 스파크 이펙트 등 추가 가능

            Debug.Log($"[INFO] Projectile::OnPenetrate - Penetrated enemy at {penetratePosition} ({_currentPenetration}/{_penetrationCount})");
        }

        /// <summary>
        /// 투사체 파괴
        /// </summary>
        private void DestroyProjectile()
        {
            // 임시: Destroy 사용 (TODO: PoolManager 연동 시 ResourceManager.ReleaseInstance로 변경)
            Destroy(gameObject);
        }

        #endregion

        #region IPoolable Implementation

        /// <summary>
        /// 오브젝트 풀에서 스폰될 때 호출 (IPoolable)
        /// </summary>
        public void OnSpawnedFromPool()
        {
            // 상태 초기화
            _currentPenetration = 0;
            _aliveTime = 0f;

            // Trail Renderer 초기화
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
            }

            Debug.Log("[INFO] Projectile::OnSpawnedFromPool - Projectile spawned from pool");
        }

        /// <summary>
        /// 오브젝트 풀로 반환될 때 호출 (IPoolable)
        /// </summary>
        public void OnReturnedToPool()
        {
            // 물리 초기화
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }

            Debug.Log("[INFO] Projectile::OnReturnedToPool - Projectile returned to pool");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 투사체 속도 변경 (외부에서 호출 가능)
        /// </summary>
        public void SetSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        /// <summary>
        /// 투사체 데미지 변경 (외부에서 호출 가능)
        /// </summary>
        public void SetDamage(float damage)
        {
            _damage = damage;
        }

        /// <summary>
        /// 투사체 방향 변경 (유도탄 등에 사용)
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            _direction = direction.normalized;

            // 회전도 업데이트
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        #endregion
    }
}
