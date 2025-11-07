using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Fireball 무기 (원거리 투사체)
    /// 가장 가까운 적을 향해 화염구를 발사합니다.
    /// 레벨업 시 개수, 데미지, 관통력이 증가합니다.
    /// </summary>
    public class Fireball : Weapon
    {
        [Header("Fireball Settings")]
        [SerializeField] private GameObject _projectilePrefab; // 투사체 프리팹 (Projectile 컴포넌트 포함)
        [SerializeField] private float _projectileSpeed = 10f; // 투사체 속도
        [SerializeField] private float _projectileLifeTime = 3f; // 투사체 생존 시간

        #region Weapon Implementation

        /// <summary>
        /// Fireball 초기화
        /// </summary>
        public override void Initialize(Player owner, WeaponData weaponData, int level = 0)
        {
            base.Initialize(owner, weaponData, level);

            // 투사체 프리팹 로드 (없으면)
            if (_projectilePrefab == null)
            {
                // TODO: Addressables로 변경
                _projectilePrefab = Resources.Load<GameObject>("Prefabs/Weapon/UndeadSurvivor/Fireball_Projectile");

                if (_projectilePrefab == null)
                {
                    Debug.LogError("[ERROR] Fireball::Initialize - Projectile prefab not found at Resources/Prefabs/Weapon/UndeadSurvivor/Fireball_Projectile");
                }
            }

            Debug.Log($"[INFO] Fireball::Initialize - Fireball Lv.{_currentLevel + 1} initialized with {_currentStat.CountPerCreate} projectiles");
        }

        /// <summary>
        /// 공격 로직 구현 (가장 가까운 적에게 발사)
        /// </summary>
        protected override void Attack()
        {
            if (_owner == null || !_owner.IsAlive)
            {
                return;
            }

            // 가장 가까운 적 찾기
            Enemy targetEnemy = FindNearestEnemy(20f);

            if (targetEnemy == null)
            {
                Debug.Log("[INFO] Fireball::Attack - No enemy found in range");
                return;
            }

            // 현재 레벨의 개수만큼 발사 (1개 ~ 5개)
            int projectileCount = _currentStat.CountPerCreate;

            // 발사 각도 계산 (여러 개일 경우 부채꼴 모양)
            float angleStep = 15f; // 각 투사체 사이 각도
            float startAngle = -(angleStep * (projectileCount - 1)) / 2f; // 시작 각도

            for (int i = 0; i < projectileCount; i++)
            {
                FireProjectile(targetEnemy, startAngle + angleStep * i);
            }

            Debug.Log($"[INFO] Fireball::Attack - Fired {projectileCount} fireballs at {targetEnemy.MonsterData.Name}");
        }

        #endregion

        #region Projectile Firing

        /// <summary>
        /// 투사체 발사
        /// </summary>
        /// <param name="target">목표 적</param>
        /// <param name="angleOffset">발사 각도 오프셋 (도 단위)</param>
        private void FireProjectile(Enemy target, float angleOffset)
        {
            if (_projectilePrefab == null)
            {
                Debug.LogError("[ERROR] Fireball::FireProjectile - Projectile prefab is null");
                return;
            }

            // 투사체 생성
            Vector3 spawnPosition = _owner.transform.position;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);

            // Projectile 컴포넌트 가져오기
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError("[ERROR] Fireball::FireProjectile - Projectile component not found on prefab");
                Destroy(projectileObj);
                return;
            }

            // 발사 방향 계산 (적 방향 + 각도 오프셋)
            Vector2 direction = (target.transform.position - _owner.transform.position).normalized;

            // 각도 오프셋 적용
            if (Mathf.Abs(angleOffset) > 0.1f)
            {
                float angleRad = angleOffset * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angleRad);
                float sin = Mathf.Sin(angleRad);
                direction = new Vector2(
                    direction.x * cos - direction.y * sin,
                    direction.x * sin + direction.y * cos
                );
            }

            // 투사체 초기화 (데미지는 플레이어 스탯 적용)
            float finalDamage = CalculateFinalDamage();
            int penetration = _currentStat.Penetrate; // 레벨업 시 관통력 증가

            projectile.Initialize(direction, finalDamage, penetration, _projectileLifeTime, _projectileSpeed);
        }

        #endregion

        #region Level Up

        /// <summary>
        /// 레벨업 시 추가 로직 (개수/데미지/관통력 증가)
        /// </summary>
        protected override void OnLevelUp()
        {
            base.OnLevelUp();

            Debug.Log($"[INFO] Fireball::OnLevelUp - Fireball upgraded to Lv.{_currentLevel + 1}");
            Debug.Log($"    Count: {_currentStat.CountPerCreate}, Damage: {_currentStat.Damage}, Penetration: {_currentStat.Penetrate}");
        }

        #endregion

        #region Visual Helpers

        /// <summary>
        /// 디버그용: 공격 범위 표시 (Gizmos)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_owner == null) return;

            // 공격 범위 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_owner.transform.position, 20f);
        }

        #endregion
    }
}
