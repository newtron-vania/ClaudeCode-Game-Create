using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 무기 타입
    /// </summary>
    public enum WeaponType
    {
        Melee,      // 근접 무기 (Knife, Spin)
        Ranged,     // 원거리 무기 (Fireball, Lightning, Shotgun)
        Area        // 범위 무기 (Poison)
    }

    /// <summary>
    /// 무기 레벨별 스탯
    /// </summary>
    [System.Serializable]
    public class WeaponLevelStat
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _cooldown;
        [SerializeField] private int _countPerCreate;

        /// <summary>
        /// 데미지
        /// </summary>
        public float Damage => _damage;

        /// <summary>
        /// 쿨다운 (초)
        /// </summary>
        public float Cooldown => _cooldown;

        /// <summary>
        /// 발사체 개수
        /// </summary>
        public int CountPerCreate => _countPerCreate;

        /// <summary>
        /// 생성자
        /// </summary>
        public WeaponLevelStat(float damage, float cooldown, int countPerCreate)
        {
            _damage = damage;
            _cooldown = cooldown;
            _countPerCreate = countPerCreate;
        }
    }

    /// <summary>
    /// 무기 데이터
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private WeaponType _type;
        [SerializeField] private WeaponLevelStat[] _levelStats;  // 레벨 0-4

        /// <summary>
        /// 무기 ID
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// 무기 이름
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 무기 타입
        /// </summary>
        public WeaponType Type => _type;

        /// <summary>
        /// 레벨별 스탯 (레벨 0-4)
        /// </summary>
        public WeaponLevelStat[] LevelStats => _levelStats;

        /// <summary>
        /// 생성자
        /// </summary>
        public WeaponData(int id, string name, WeaponType type, WeaponLevelStat[] levelStats)
        {
            _id = id;
            _name = name;
            _type = type;
            _levelStats = levelStats;
        }
    }
}
