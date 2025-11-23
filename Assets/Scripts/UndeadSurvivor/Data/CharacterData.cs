using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 캐릭터 데이터
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private float _maxHp;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _damage;        // 공격력 증가 (%)
        [SerializeField] private float _defense;
        [SerializeField] private float _cooldown;      // 쿨다운 감소 (%)
        [SerializeField] private int _amount;          // 발사체 개수 증가
        [SerializeField] private int _startWeaponId;   // 시작 무기 ID

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// 캐릭터 이름
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public float MaxHp => _maxHp;

        /// <summary>
        /// 이동 속도
        /// </summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>
        /// 공격력 증가 (%)
        /// </summary>
        public float Damage => _damage;

        /// <summary>
        /// 방어력
        /// </summary>
        public float Defense => _defense;

        /// <summary>
        /// 쿨다운 감소 (%)
        /// </summary>
        public float Cooldown => _cooldown;

        /// <summary>
        /// 발사체 개수 증가
        /// </summary>
        public int Amount => _amount;

        /// <summary>
        /// 시작 무기 ID
        /// </summary>
        public int StartWeaponId => _startWeaponId;

        /// <summary>
        /// 생성자
        /// </summary>
        public CharacterData(int id, string name, float maxHp, float moveSpeed, float damage, float defense, float cooldown, int amount, int startWeaponId)
        {
            _id = id;
            _name = name;
            _maxHp = maxHp;
            _moveSpeed = moveSpeed;
            _damage = damage;
            _defense = defense;
            _cooldown = cooldown;
            _amount = amount;
            _startWeaponId = startWeaponId;
        }
    }
}
