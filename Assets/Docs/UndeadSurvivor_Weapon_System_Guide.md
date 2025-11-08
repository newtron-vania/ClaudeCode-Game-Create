# Undead Survivor 무기 시스템 가이드

**최종 업데이트**: 2025-11-09
**Phase**: Phase 3 - 무기 시스템 완료

---

## 📋 개요

Undead Survivor의 무기 시스템은 자동 공격, 레벨업, 다양한 무기 타입을 지원하는 확장 가능한 시스템입니다.

### 주요 특징
- ✅ **자동 공격 시스템**: 쿨다운 기반 자동 공격
- ✅ **레벨업 시스템**: 무기 레벨 0-4 (표시 1-5)
- ✅ **투사체 풀링**: IPoolable 인터페이스 지원
- ✅ **플레이어 스탯 연동**: CharacterStat 기반 데미지 계산
- ✅ **확장 가능**: 새로운 무기 타입 추가 용이

---

## 🏗️ 아키텍처

### 클래스 구조

```
Weapon (Abstract Base Class)
├── Fireball (원거리 투사체)
│   └── Projectile (투사체 오브젝트)
├── Scythe (근접 회전)
│   └── ScytheBlade (낫 오브젝트)
├── Shotgun (산탄 투사체) - TODO
└── [기타 무기...] - TODO

WeaponData (ScriptableObject)
└── WeaponLevelStat[] (레벨별 스탯)

PlayerWeaponManager
├── WeaponSlot[] (장착된 무기 목록)
└── WeaponTypeMap (WeaponData ID → 무기 클래스)
```

### 핵심 컴포넌트

#### 1. Weapon.cs (베이스 클래스)
**경로**: `Assets/Scripts/UndeadSurvivor/Weapon.cs`

**주요 기능**:
- 자동 공격 시스템 (쿨다운 관리)
- 레벨업 로직 (레벨 0-4)
- 적 탐지 (FindNearestEnemy, FindEnemiesInRadius)
- 최종 데미지 계산 (플레이어 스탯 적용)

**API**:
```csharp
// 초기화
public virtual void Initialize(Player owner, WeaponData weaponData, int level = 0)

// 레벨업
public virtual bool LevelUp()

// 활성화/비활성화
public virtual void Activate()
public virtual void Deactivate()

// 하위 클래스 구현 필요
protected abstract void Attack()
```

#### 2. PlayerWeaponManager.cs
**경로**: `Assets/Scripts/UndeadSurvivor/PlayerWeaponManager.cs`

**주요 기능**:
- 무기 슬롯 관리 (최대 6개)
- 무기 추가/레벨업
- 무기 오브젝트 생성 및 초기화
- 이벤트 발생 (추가, 레벨업, 슬롯 포화)

**API**:
```csharp
// 무기 추가 (신규)
public bool AddWeapon(WeaponData weaponData)

// 무기 레벨업
public bool LevelUpWeapon(int weaponId)

// 무기 소유 여부
public bool HasWeapon(int weaponId)

// 무기 최대 레벨 체크
public bool IsWeaponMaxLevel(int weaponId)
```

**이벤트**:
```csharp
event Action<int, string, int> OnWeaponAdded;    // (weaponId, weaponName, level)
event Action<int, int> OnWeaponLevelUp;          // (weaponId, newLevel)
event Action<int> OnWeaponSlotsFull;             // (slotCount)
```

---

## 🔫 구현된 무기

### 1. Fireball (원거리 투사체)
**클래스**: `Fireball.cs`
**WeaponData ID**: 1
**타입**: Ranged

**특징**:
- 가장 가까운 적을 향해 화염구 발사
- 레벨업 시 개수, 데미지, 관통력 증가
- 여러 개 발사 시 부채꼴 패턴 (15° 간격)

**레벨별 스탯** (WeaponData.json):
| 레벨 | 데미지 | 쿨다운 | 개수 | 관통 | 속도 |
|------|--------|--------|------|------|------|
| 1 | 20 | 5.0s | 1 | 1 | 8.0 |
| 2 | 24 | 4.5s | 1 | 1 | 9.0 |
| 3 | 29 | 4.0s | 1 | 1 | 10.0 |
| 4 | 35 | 4.0s | 1 | 1 | 11.0 |
| 5 | 42 | 4.0s | 2 | 1 | 12.0 |

**리소스 경로**:
```
Prefabs/Weapon/UndeadSurvivor/Fireball_Projectile
```

### 2. Scythe (근접 회전)
**클래스**: `Scythe.cs`, `ScytheBlade.cs`
**WeaponData ID**: 2
**타입**: Melee

**특징**:
- 플레이어 주변을 회전하는 낫
- 레벨업 시 개수, 데미지, 크기 증가
- 지속 피해 (0.5초 간격 데미지)
- 공전 속도: 180°/s, 자전 속도: 360°/s

**레벨별 스탯** (WeaponData.json):
| 레벨 | 데미지 | 개수 | 속도 | 관통 |
|------|--------|------|------|------|
| 1 | 18 | 1 | 100 | 99 |
| 2 | 22 | 2 | 110 | 99 |
| 3 | 26 | 2 | 120 | 99 |
| 4 | 31 | 3 | 130 | 99 |
| 5 | 37 | 3 | 140 | 99 |

**리소스 경로**:
```
Prefabs/Weapon/UndeadSurvivor/Scythe_Blade
```

---

## 🚀 사용 방법

### Unity 에디터 설정

#### 1. Player GameObject 설정
```
Player (GameObject)
├── PlayerController
├── PlayerHealth
├── PlayerExperience
├── PlayerWeaponManager
│   ├── Max Weapon Slots: 6
│   └── Weapon Parent: (자식 Transform "Weapons")
└── Player
    └── (자동으로 모든 컴포넌트 연결)

    Weapons (Empty GameObject - 무기 부모)
```

#### 2. 무기 프리팹 생성

**Fireball 투사체 프리팹**:
```
Fireball_Projectile (GameObject)
├── SpriteRenderer (화염구 스프라이트)
├── CircleCollider2D (Trigger)
├── Rigidbody2D (Gravity Scale = 0)
├── TrailRenderer (옵션)
└── Projectile (Component)
```

**Scythe 블레이드 프리팹**:
```
Scythe_Blade (GameObject)
├── SpriteRenderer (낫 스프라이트)
├── CircleCollider2D (Trigger)
└── ScytheBlade (Component)
```

#### 3. Resources 폴더 구조
```
Assets/Resources/
└── Prefabs/
    └── Weapon/
        └── UndeadSurvivor/
            ├── Fireball_Projectile.prefab
            └── Scythe_Blade.prefab
```

---

## 💻 코드 사용 예제

### 무기 추가 (플레이어 초기화 시)
```csharp
// CharacterData로부터 시작 무기 추가
CharacterData knightData = dataProvider.GetCharacterData(1);
if (knightData.StartWeaponId > 0)
{
    WeaponData startWeapon = dataProvider.GetWeaponData(knightData.StartWeaponId);
    player.AddWeapon(startWeapon);
}
```

### 레벨업 시 무기 추가 (레벨업 UI)
```csharp
// 레벨업 선택지로 신규 무기 제공
WeaponData fireballData = dataProvider.GetWeaponData(1); // Fireball
bool added = player.AddWeapon(fireballData);

if (added)
{
    Debug.Log($"New weapon acquired: {fireballData.Name}");
}
else if (player.IsWeaponSlotsFull)
{
    Debug.Log("Weapon slots full!");
}
```

### 무기 레벨업 (레벨업 UI)
```csharp
// 레벨업 선택지로 기존 무기 강화
int weaponIdToUpgrade = 1; // Fireball

if (player.HasWeapon(weaponIdToUpgrade))
{
    bool leveledUp = player.LevelUpWeapon(weaponIdToUpgrade);

    if (leveledUp)
    {
        Debug.Log($"Weapon upgraded!");
    }
    else
    {
        Debug.Log($"Weapon is already max level");
    }
}
```

### 무기 이벤트 구독
```csharp
// Player 초기화 시
player.OnWeaponAdded += (weaponId, weaponName, level) =>
{
    Debug.Log($"Weapon Added: {weaponName} Lv.{level + 1}");
    // UI 갱신: 무기 아이콘 표시
};

player.OnWeaponLevelUp += (weaponId, newLevel) =>
{
    Debug.Log($"Weapon Leveled Up: ID {weaponId} → Lv.{newLevel + 1}");
    // UI 갱신: 무기 레벨 표시 업데이트
};

player.OnWeaponSlotsFull += (slotCount) =>
{
    Debug.Log($"All {slotCount} weapon slots are full!");
    // UI: 무기 슬롯 포화 알림
};
```

---

## 🔧 새 무기 추가 방법

### 1. 무기 클래스 작성
```csharp
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Shotgun 무기 (산탄 투사체)
    /// </summary>
    public class Shotgun : Weapon
    {
        [Header("Shotgun Settings")]
        [SerializeField] private float _spreadAngle = 30f; // 산탄 각도

        private const string PROJECTILE_PATH = "Prefabs/Weapon/UndeadSurvivor/Shotgun_Projectile";

        /// <summary>
        /// 공격 로직 구현
        /// </summary>
        protected override void Attack()
        {
            // 산탄 발사 로직
            Enemy target = FindNearestEnemy(15f);
            if (target == null) return;

            int bulletCount = _currentStat.CountPerCreate;
            float angleStep = _spreadAngle / (bulletCount - 1);
            float startAngle = -_spreadAngle / 2f;

            for (int i = 0; i < bulletCount; i++)
            {
                FireBullet(target, startAngle + angleStep * i);
            }
        }

        private void FireBullet(Enemy target, float angleOffset)
        {
            // Projectile 생성 및 초기화
            // (Fireball.cs 참조)
        }
    }
}
```

### 2. WeaponData 추가
**WeaponData.json**:
```json
{
  "weapons": [
    {
      "id": 3,
      "name": "Shotgun",
      "type": "Ranged",
      "levelStats": [
        {
          "damage": 12,
          "cooldown": 1.5,
          "countPerCreate": 3,
          "area": 30,
          "speed": 15.0,
          "penetrate": 2,
          "duration": 0
        }
      ]
    }
  ]
}
```

### 3. PlayerWeaponManager 매핑 추가
```csharp
private Dictionary<int, System.Type> _weaponTypeMap = new Dictionary<int, System.Type>
{
    { 1, typeof(Fireball) },
    { 2, typeof(Scythe) },
    { 3, typeof(Shotgun) }  // 추가
};
```

### 4. 프리팹 생성 및 배치
```
Assets/Resources/Prefabs/Weapon/UndeadSurvivor/
└── Shotgun_Projectile.prefab
```

---

## 🧪 테스트

### 테스트 스크립트 사용
**파일**: `Assets/Scripts/UndeadSurvivor/TestWeaponSystem.cs`

**Unity 에디터 실행**:
1. 빈 GameObject 생성
2. `TestWeaponSystem` 컴포넌트 추가
3. Inspector에서 `Test Player` 필드에 Player GameObject 할당
4. Play 모드 실행 → Console에서 로그 확인

**Context Menu 테스트**:
- Inspector에서 우클릭 → "Test Weapon System"
- Inspector에서 우클릭 → "Test Weapon Slots Full" (6개 무기 추가 테스트)

**테스트 항목**:
- ✅ WeaponData 로드
- ✅ 무기 추가 (Fireball, Scythe)
- ✅ 무기 레벨업
- ✅ 무기 슬롯 포화 (6개)
- ✅ 이벤트 발생

---

## 🎯 데미지 계산 공식

### 최종 데미지
```
최종 데미지 = 기본 데미지 × (1 + 플레이어 공격력 스탯 / 100)
```

**예시**:
- Fireball Lv.1 기본 데미지: 20
- 플레이어 공격력 스탯: +20%
- 최종 데미지 = 20 × (1 + 20/100) = 24

### 투사체 관통
- `Penetrate = 0`: 1번 충돌 후 파괴
- `Penetrate = 1`: 2번 충돌 후 파괴
- `Penetrate = 99`: 무한 관통 (Scythe)

---

## ⚠️ 주의사항

### 1. Resources 폴더 경로
- 모든 무기 프리팹은 `Resources/Prefabs/Weapon/UndeadSurvivor/` 하위에 배치
- `ResourceManager.InstantiateFromResources()` 사용 (확장자 제외)

### 2. Layer 설정
- Player Layer (Layer 6)
- Enemy Layer (Layer 7)
- Collision Matrix에서 Player ↔ Enemy 활성화

### 3. Pooling 통합
- Projectile, ScytheBlade는 IPoolable 구현
- 현재는 Destroy 사용 (TODO: PoolManager 통합)

### 4. 무기 타입 매핑
- 새 무기 추가 시 `PlayerWeaponManager._weaponTypeMap`에 등록 필수
- WeaponData ID와 무기 클래스 Type을 매핑

---

## 📊 성능 최적화

### 현재 구현
- Enemy 탐색: `FindObjectsOfType<Enemy>()` (프레임마다 실행 안 함, 쿨다운 기반)
- 투사체: IPoolable 인터페이스 구현 (향후 PoolManager 통합)

### TODO: 최적화
- [ ] PoolManager를 통한 투사체 풀링
- [ ] Enemy 탐색 최적화 (공간 분할, 캐싱)
- [ ] 무기 오브젝트 풀링 (재사용)

---

## 🔗 참조 문서

- **무기 데이터**: `Assets/Resources/Data/UndeadSurvivor/WeaponData.json`
- **원본 PRD**: `Assets/Docs/UndeadSurvivor_Reference.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`
- **진행 상황**: `Assets/Docs/UndeadSurvivor_Progress.md`

---

## 📝 변경 이력

### 2025-11-09
- ✅ 무기 시스템 가이드 작성
- ✅ TestWeaponSystem.cs 추가
- ✅ Weapon, Fireball, Scythe, Projectile 구현 확인
- ✅ PlayerWeaponManager 연동 검증
