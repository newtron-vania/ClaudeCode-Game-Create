using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 몬스터 기본 데이터
    /// </summary>
    [System.Serializable]
    public class MonsterData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private float _maxHp;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _damage;
        [SerializeField] private float _defense;
        [SerializeField] private float _expMultiplier;

        /// <summary>
        /// 몬스터 ID
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// 몬스터 이름
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 기본 최대 체력
        /// </summary>
        public float MaxHp => _maxHp;

        /// <summary>
        /// 기본 이동 속도
        /// </summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>
        /// 기본 공격력
        /// </summary>
        public float Damage => _damage;

        /// <summary>
        /// 기본 방어력
        /// </summary>
        public float Defense => _defense;

        /// <summary>
        /// 경험치 획득 배율
        /// </summary>
        public float ExpMultiplier => _expMultiplier;

        /// <summary>
        /// 생성자
        /// </summary>
        public MonsterData(int id, string name, float maxHp, float moveSpeed, float damage, float defense, float expMultiplier)
        {
            _id = id;
            _name = name;
            _maxHp = maxHp;
            _moveSpeed = moveSpeed;
            _damage = damage;
            _defense = defense;
            _expMultiplier = expMultiplier;
        }
    }
}
