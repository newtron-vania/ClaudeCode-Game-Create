using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Scythe 무기 (근접 회전 공격)
    /// 플레이어 주변을 회전하는 작은 낫 투사체들입니다.
    /// 레벨업 시 개수, 데미지, 크기가 증가합니다.
    /// </summary>
    public class Scythe : Weapon
    {
        [Header("Scythe Settings")]
        [SerializeField] private float _orbitSpeed = 180f; // 공전 속도 (도/초)
        [SerializeField] private float _spinSpeed = 360f; // 자전 속도 (도/초)
        [SerializeField] private float _orbitRadius = 2f; // 궤도 반경

        // ResourceManager 경로 (Resources 폴더)
        private const string BLADE_PATH = "Prefabs/Weapon/UndeadSurvivor/Scythe_Blade";

        [Header("Damage Settings")]
        [SerializeField] private float _damageInterval = 0.5f; // 데미지 간격 (초)

        private List<ScytheBlade> _scytheBlades = new List<ScytheBlade>(); // 생성된 낫들
        private float _currentOrbitAngle = 0f; // 현재 공전 각도

        // 데미지 쿨다운 관리
        private Dictionary<Enemy, float> _lastDamageTime = new Dictionary<Enemy, float>();

        #region Weapon Implementation

        /// <summary>
        /// Scythe 초기화
        /// </summary>
        public override void Initialize(Player owner, WeaponData weaponData, int level = 0)
        {
            base.Initialize(owner, weaponData, level);

            // 낫 생성 (ResourceManager 사용)
            CreateScytheBlades();

            Debug.Log($"[INFO] Scythe::Initialize - Scythe Lv.{_currentLevel + 1} initialized with {_currentStat.CountPerCreate} blades");
        }

        /// <summary>
        /// 공격 로직 (데미지는 OnTriggerStay2D에서 처리)
        /// </summary>
        protected override void Attack()
        {
            // Scythe는 지속적으로 회전하며 데미지를 줌
            // 여기서는 데미지 쿨다운 정리만 수행
            CleanupDamageTimers();
        }

        /// <summary>
        /// Update - 낫 공전 및 자전
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (!IsActive || _owner == null) return;

            // 공전 각도 업데이트
            _currentOrbitAngle += _orbitSpeed * Time.deltaTime;
            if (_currentOrbitAngle >= 360f)
            {
                _currentOrbitAngle -= 360f;
            }

            // 각 낫의 위치 및 회전 업데이트
            UpdateScytheBlades();
        }

        #endregion

        #region Scythe Blade Management

        /// <summary>
        /// 낫 생성
        /// </summary>
        private void CreateScytheBlades()
        {
            // 기존 낫 제거
            ClearScytheBlades();

            // 현재 레벨의 개수만큼 생성
            int bladeCount = _currentStat.CountPerCreate;

            for (int i = 0; i < bladeCount; i++)
            {
                // ResourceManager를 통한 낫 생성 (InstantiateFromResources)
                ScytheBlade blade = ResourceManager.Instance.InstantiateFromResources<ScytheBlade>(BLADE_PATH, transform);

                if (blade == null)
                {
                    Debug.LogError($"[ERROR] Scythe::CreateScytheBlades - Failed to instantiate blade {i} from Resources");
                    continue;
                }

                blade.name = $"ScytheBlade_{i}";

                // 초기화
                blade.Initialize(this, _spinSpeed);

                // 크기 적용 (레벨업 시 증가)
                float scale = 1f + (_currentLevel * 0.2f);
                blade.transform.localScale = Vector3.one * scale;

                _scytheBlades.Add(blade);
            }

            Debug.Log($"[INFO] Scythe::CreateScytheBlades - Created {bladeCount} blades");
        }

        /// <summary>
        /// 모든 낫 제거
        /// </summary>
        private void ClearScytheBlades()
        {
            foreach (var blade in _scytheBlades)
            {
                if (blade != null && blade.gameObject != null)
                {
                    // Destroy 유지 (낫은 무기 해제 시 완전히 제거)
                    Destroy(blade.gameObject);
                }
            }

            _scytheBlades.Clear();
        }

        /// <summary>
        /// 낫 위치 업데이트 (공전)
        /// </summary>
        private void UpdateScytheBlades()
        {
            int bladeCount = _scytheBlades.Count;
            if (bladeCount == 0) return;

            // 각 낫의 각도 (균등 분배)
            float angleStep = 360f / bladeCount;

            for (int i = 0; i < bladeCount; i++)
            {
                ScytheBlade blade = _scytheBlades[i];
                if (blade == null) continue;

                // 공전 각도
                float orbitAngle = _currentOrbitAngle + (angleStep * i);
                float orbitRad = orbitAngle * Mathf.Deg2Rad;

                // 위치 계산
                Vector2 offset = new Vector2(
                    Mathf.Cos(orbitRad) * _orbitRadius,
                    Mathf.Sin(orbitRad) * _orbitRadius
                );

                blade.transform.position = (Vector2)_owner.transform.position + offset;

                // 자전은 ScytheBlade에서 처리
            }
        }

        #endregion

        #region Damage Handling

        /// <summary>
        /// 적에게 데미지 (ScytheBlade에서 호출)
        /// </summary>
        public bool TryDamageEnemy(Enemy enemy)
        {
            if (enemy == null || !enemy.IsAlive)
            {
                return false;
            }

            // 데미지 쿨다운 체크
            if (_lastDamageTime.ContainsKey(enemy))
            {
                float timeSinceLastDamage = Time.time - _lastDamageTime[enemy];
                if (timeSinceLastDamage < _damageInterval)
                {
                    return false;
                }
            }

            // 데미지 적용
            float finalDamage = CalculateFinalDamage();
            enemy.TakeDamage(finalDamage);

            // 기록
            _lastDamageTime[enemy] = Time.time;

            return true;
        }

        /// <summary>
        /// 데미지 타이머 정리
        /// </summary>
        private void CleanupDamageTimers()
        {
            List<Enemy> toRemove = new List<Enemy>();

            foreach (var kvp in _lastDamageTime)
            {
                Enemy enemy = kvp.Key;
                float lastTime = kvp.Value;

                if (enemy == null || !enemy.IsAlive || Time.time - lastTime > 5f)
                {
                    toRemove.Add(enemy);
                }
            }

            foreach (var enemy in toRemove)
            {
                _lastDamageTime.Remove(enemy);
            }
        }

        #endregion

        #region Level Up

        /// <summary>
        /// 레벨업 시 낫 재생성
        /// </summary>
        protected override void OnLevelUp()
        {
            base.OnLevelUp();

            CreateScytheBlades();

            Debug.Log($"[INFO] Scythe::OnLevelUp - Lv.{_currentLevel + 1}, Count: {_currentStat.CountPerCreate}, Damage: {_currentStat.Damage}");
        }

        #endregion

        #region Cleanup

        public override void Deactivate()
        {
            base.Deactivate();
            ClearScytheBlades();
        }

        private void OnDestroy()
        {
            ClearScytheBlades();
        }

        #endregion

        #region Visual Helpers

        private void OnDrawGizmosSelected()
        {
            if (_owner == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_owner.transform.position, _orbitRadius);
        }

        #endregion
    }

    /// <summary>
    /// ScytheBlade 컴포넌트 (개별 낫)
    /// 자전 및 충돌 감지를 담당합니다.
    /// IPoolable 구현으로 풀링 시스템 지원
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ScytheBlade : MonoBehaviour, IPoolable
    {
        private Scythe _parentWeapon;
        private CircleCollider2D _collider;
        private float _spinSpeed = 360f; // 자전 속도
        private float _currentSpinAngle = 0f; // 현재 자전 각도

        public void Initialize(Scythe parentWeapon, float spinSpeed)
        {
            _parentWeapon = parentWeapon;
            _spinSpeed = spinSpeed;

            // Collider 설정
            _collider = GetComponent<CircleCollider2D>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<CircleCollider2D>();
            }
            _collider.isTrigger = true;

            // 초기 자전 각도 랜덤 설정 (시각적 다양성)
            _currentSpinAngle = Random.Range(0f, 360f);
        }

        private void Update()
        {
            // 자전 (낫 자체가 회전)
            _currentSpinAngle += _spinSpeed * Time.deltaTime;
            if (_currentSpinAngle >= 360f)
            {
                _currentSpinAngle -= 360f;
            }

            transform.rotation = Quaternion.Euler(0f, 0f, _currentSpinAngle);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                _parentWeapon.TryDamageEnemy(enemy);
            }
        }

        #region IPoolable Implementation

        /// <summary>
        /// 오브젝트 풀에서 스폰될 때 호출 (IPoolable)
        /// </summary>
        public void OnSpawnedFromPool()
        {
            // 자전 각도 랜덤 초기화 (시각적 다양성)
            _currentSpinAngle = Random.Range(0f, 360f);

            Debug.Log("[INFO] ScytheBlade::OnSpawnedFromPool - Blade spawned from pool");
        }

        /// <summary>
        /// 오브젝트 풀로 반환될 때 호출 (IPoolable)
        /// </summary>
        public void OnReturnedToPool()
        {
            // 회전 초기화
            _currentSpinAngle = 0f;
            transform.rotation = Quaternion.identity;

            Debug.Log("[INFO] ScytheBlade::OnReturnedToPool - Blade returned to pool");
        }

        #endregion
    }
}
