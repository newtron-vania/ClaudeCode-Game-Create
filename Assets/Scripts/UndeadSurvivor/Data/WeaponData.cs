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
        [SerializeField] private float _damage;           // 기본 데미지
        [SerializeField] private float _cooldown;         // 쿨다운 (초)
        [SerializeField] private int _countPerCreate;     // 발사체 개수
        [SerializeField] private float _area;             // 범위/크기 (폭발 범위, 장판 크기, 무기 크기 등)
        [SerializeField] private float _speed;            // 투사체 이동 속도 (원거리 무기), 회전 속도 (근접 무기)
        [SerializeField] private int _penetrate;          // 관통력 (관통 가능 적 수)
        [SerializeField] private float _duration;         // 지속 시간 (장판 무기 전용)

        /// <summary>
        /// 데미지
        /// 용도: 모든 무기의 기본 공격력
        /// </summary>
        public float Damage => _damage;

        /// <summary>
        /// 쿨다운 (초)
        /// 용도: 무기 재사용 대기 시간 (0이면 지속 발동)
        /// - Fireball: 5.0초 → 4.0초 (레벨업 시 감소)
        /// - Scythe: 0초 (지속 회전)
        /// - Shotgun: 1.5초 → 1.0초
        /// - Flame Boots: 0초 (이동 시 발동)
        /// - Poison Field: 0초 (지속 장판)
        /// - Bomb: 120초 → 80초 (레벨업 시 감소)
        /// </summary>
        public float Cooldown => _cooldown;

        /// <summary>
        /// 발사체 개수
        /// 용도: 동시 생성되는 투사체/무기 개수
        /// - Fireball: 1 → 2 (Lv.5)
        /// - Scythe: 1 → 3 (Lv.2, 4에서 증가)
        /// - Shotgun: 3 → 5 (레벨업마다 증가)
        /// - Flame Boots: 1 (고정)
        /// - Poison Field: 1 (고정)
        /// - Bomb: 1 (고정)
        /// </summary>
        public int CountPerCreate => _countPerCreate;

        /// <summary>
        /// 범위/크기
        /// 용도: 무기의 공격 범위 또는 크기
        /// - Fireball: 폭발 범위 (1.5 → 1.88)
        /// - Scythe: 무기 크기 (고정)
        /// - Shotgun: 발사 각도 (30° → 100°)
        /// - Flame Boots: 장판 크기 (+25% 증가)
        /// - Poison Field: 장판 반경 (2.0 → 3.125)
        /// - Bomb: 화면 전체 (고정)
        /// </summary>
        public float Area => _area;

        /// <summary>
        /// 이동 속도 / 회전 속도
        /// 용도: 투사체의 이동 속도 또는 무기의 회전 속도
        /// - Fireball: 투사체 비행 속도
        /// - Scythe: 회전 속도 (+20% 증가)
        /// - Shotgun: 투사체 비행 속도
        /// - Flame Boots: 장판 생성 간격 (이동 속도 기반)
        /// - Poison Field: 피해 주기 (1.0초 → 0.8초)
        /// - Bomb: 사용 안 함
        /// </summary>
        public float Speed => _speed;

        /// <summary>
        /// 관통력
        /// 용도: 투사체가 관통할 수 있는 적의 수
        /// - Fireball: 1 (폭발 시 소멸)
        /// - Scythe: 무한 관통 (99로 설정)
        /// - Shotgun: 투사체당 관통 횟수
        /// - Flame Boots: 무한 관통 (장판)
        /// - Poison Field: 무한 관통 (장판)
        /// - Bomb: 사용 안 함 (즉사)
        /// </summary>
        public int Penetrate => _penetrate;

        /// <summary>
        /// 지속 시간 (초)
        /// 용도: 장판 무기의 지속 시간
        /// - Fireball: 사용 안 함 (즉시 폭발)
        /// - Scythe: 사용 안 함 (영구 회전)
        /// - Shotgun: 사용 안 함 (투사체 수명은 별도)
        /// - Flame Boots: 장판 지속 시간 (2.0 → 4.0초)
        /// - Poison Field: 사용 안 함 (영구 장판)
        /// - Bomb: 사용 안 함 (즉시 발동)
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 생성자
        /// </summary>
        public WeaponLevelStat(float damage, float cooldown, int countPerCreate, float area = 1f, float speed = 0f, int penetrate = 1, float duration = 0f)
        {
            _damage = damage;
            _cooldown = cooldown;
            _countPerCreate = countPerCreate;
            _area = area;
            _speed = speed;
            _penetrate = penetrate;
            _duration = duration;
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
