using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 피격 판정용 Trigger Collider
    /// Enemy와의 충돌을 Trigger로 처리하여 Player에게 피해 전달
    /// Player의 물리 Collider와는 별도로 동작
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerHitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private BoxCollider2D _hitboxCollider;
        [SerializeField] private Vector2 _hitboxSize = new Vector2(1f, 1f);
        [SerializeField] private Vector2 _hitboxOffset = Vector2.zero;

        private Player _player;

        private void Awake()
        {
            // Hitbox Collider 설정
            if (_hitboxCollider == null)
            {
                _hitboxCollider = GetComponent<BoxCollider2D>();
            }

            if (_hitboxCollider == null)
            {
                Debug.LogError("[ERROR] PlayerHitbox::Awake - BoxCollider2D component not found!");
                return;
            }

            // Trigger로 설정 (Enemy와 겹침 감지)
            _hitboxCollider.isTrigger = true;
            _hitboxCollider.size = _hitboxSize;
            _hitboxCollider.offset = _hitboxOffset;

            // Player 컴포넌트 참조
            _player = GetComponentInParent<Player>();

            if (_player == null)
            {
                Debug.LogError("[ERROR] PlayerHitbox::Awake - Player component not found in parent!");
                return;
            }

            // GameObject Layer 설정
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        /// <summary>
        /// Enemy와 트리거 진입 시
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy"))
                return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null || !enemy.IsAlive)
                return;

            if (_player == null || !_player.IsAlive)
                return;

            // Enemy의 공격력만큼 피해
            _player.TakeDamage(enemy.Damage);
        }

        /// <summary>
        /// Enemy와 트리거 지속 중 (지속 피해)
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy"))
                return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null || !enemy.IsAlive)
                return;

            if (_player == null || !_player.IsAlive)
                return;

            // 지속 피해 (무적 시간으로 제어됨)
            _player.TakeDamage(enemy.Damage);
        }

        /// <summary>
        /// Hitbox 크기 설정 (캐릭터별 조정용)
        /// </summary>
        public void SetHitboxSize(Vector2 size, Vector2 offset)
        {
            _hitboxSize = size;
            _hitboxOffset = offset;
            if (_hitboxCollider != null)
            {
                _hitboxCollider.size = _hitboxSize;
                _hitboxCollider.offset = _hitboxOffset;
            }
        }

        /// <summary>
        /// Hitbox 크기만 설정 (offset 유지)
        /// </summary>
        public void SetHitboxSize(Vector2 size)
        {
            _hitboxSize = size;
            if (_hitboxCollider != null)
            {
                _hitboxCollider.size = _hitboxSize;
            }
        }

        /// <summary>
        /// Hitbox offset만 설정 (size 유지)
        /// </summary>
        public void SetHitboxOffset(Vector2 offset)
        {
            _hitboxOffset = offset;
            if (_hitboxCollider != null)
            {
                _hitboxCollider.offset = _hitboxOffset;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gizmos로 Hitbox 시각화
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 center = transform.position + (Vector3)_hitboxOffset;
            Gizmos.DrawWireCube(center, _hitboxSize);
        }
#endif
    }
}
