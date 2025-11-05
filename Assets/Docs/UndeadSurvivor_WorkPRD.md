# Undead Survivor 작업용 PRD

> **작성일**: 2025-11-05
>
> **목적**: Undead Survivor 구현 시 개발 작업에 직접 참조하는 실무 문서
>
> **참조**: UndeadSurvivor_Reference.md (원본 PRD)

---

## 📋 개발 체크리스트

### Phase 1: 핵심 시스템 구축
- [ ] **플레이어 시스템**
  - [ ] 플레이어 이동 (WASD)
  - [ ] 체력 시스템
  - [ ] 경험치 & 레벨업
  - [ ] 캐릭터 선택 (2종)
- [ ] **적 시스템**
  - [ ] 적 스폰 시스템
  - [ ] 적 AI (플레이어 추적)
  - [ ] 적 스탯 스케일링 (시간 기반)
  - [ ] 오브젝트 풀링 (최대 50마리)
- [ ] **무기 시스템**
  - [ ] 무기 베이스 클래스
  - [ ] 자동 공격 시스템
  - [ ] 무기 레벨업 (1-5)
- [ ] **맵 시스템**
  - [ ] 무한 맵 (3x3 타일 재배치)
  - [ ] 카메라 추적

### Phase 2: 레벨업 & 강화 시스템
- [ ] **레벨업 UI**
  - [ ] 4지선다 선택 UI
  - [ ] Time.timeScale 제어
  - [ ] 강화 선택지 생성 로직
- [ ] **캐릭터 스탯 강화**
  - [ ] 11종 스탯 구현
  - [ ] 스탯 누적 계산

### Phase 3: 아이템 & 웨이브 시스템
- [ ] **아이템 드롭**
  - [ ] 경험치 보석
  - [ ] 체력 회복 (Food)
  - [ ] 마그넷 (Magnet)
  - [ ] 보물 상자 (Item Box)
- [ ] **웨이브 시스템**
  - [ ] 시간 기반 난이도 증가
  - [ ] 중간 보스 (1분마다)
  - [ ] 최종 보스 (5분)

### Phase 4: 무기 구현 (MVP 6종)
- [ ] 파이어볼 (Fireball)
- [ ] 낫 (Scythe)
- [ ] 샷건 (Shotgun)
- [ ] 화염 부츠 (Flame Boots)
- [ ] 독 장판 (Poison Field)
- [ ] 폭탄 (Bomb)

### Phase 5: 최적화 & 완성
- [ ] 오브젝트 풀링 최적화
- [ ] Update/FixedUpdate/LateUpdate 분리
- [ ] UI/효과음/연출 추가
- [ ] 최종 테스트 & 밸런싱

---

## 🎮 핵심 게임 플로우

```
게임 시작
  ↓
캐릭터 선택 (Knight / Mage)
  ↓
5분 타이머 시작
  ↓
[0:00-0:59] 일반 적 스폰 (Bat)
  ↓
[1:00] 중간 보스 1 → 보물 상자 드롭
  ↓
[1:01-1:59] 난이도 증가 (Zombie 추가)
  ↓
[2:00] 중간 보스 2 → 보물 상자 드롭
  ↓
[2:01-2:59] 물량 1.5배 증가
  ↓
[3:00] 중간 보스 3 → 보물 상자 드롭
  ↓
[3:01-3:59] 엘리트 적 증가
  ↓
[4:00] 중간 보스 4 → 보물 상자 드롭
  ↓
[4:01-4:59] 원거리 적 추가 (Ghost), 물량 2배
  ↓
[5:00] 최종 보스 스폰
  ↓
보스 격파 → 승리!
```

---

## 📊 데이터 구조

### 1. CharacterData (캐릭터)

```csharp
public class CharacterData
{
    public int Id;                  // 캐릭터 ID (0: Knight, 1: Mage)
    public string Name;             // 캐릭터 이름
    public float MaxHp;             // 최대 체력
    public float MoveSpeed;         // 이동 속도
    public float Damage;            // 공격력 증가 (%)
    public float Defense;           // 방어력
    public float Cooldown;          // 쿨다운 감소 (%)
    public int Amount;              // 투사체 개수 증가
    public int StartWeaponId;       // 시작 무기 ID
}
```

**캐릭터 스펙**:
- **Knight**: MaxHp +20, Defense +1, StartWeapon = Scythe(낫)
- **Mage**: Damage +10%, Cooldown -5%, StartWeapon = Fireball(파이어볼)

### 2. WeaponData (무기)

```csharp
public enum WeaponType { Melee, Ranged, Area }

public class WeaponData
{
    public int Id;                  // 무기 ID
    public string Name;             // 무기 이름
    public WeaponType Type;         // 무기 타입
    public WeaponLevelStat[] LevelStats; // 레벨별 스탯 (0-4)
}

public class WeaponLevelStat
{
    public float Damage;            // 데미지
    public float Cooldown;          // 쿨다운
    public int CountPerCreate;      // 생성 개수
    public float Area;              // 범위/크기 (선택적)
    public float Speed;             // 속도 (선택적)
}
```

### 3. MonsterData (몬스터)

```csharp
public class MonsterData
{
    public int Id;                  // 몬스터 ID
    public string Name;             // 몬스터 이름
    public float MaxHp;             // 기본 최대 HP
    public float MoveSpeed;         // 기본 이동 속도
    public float Damage;            // 기본 공격력
    public float Defense;           // 기본 방어력
    public float ExpMultiplier;     // 경험치 배율
}
```

**몬스터 스케일링 공식** (시간 기반):
```csharp
// t = 경과 시간(분)
몬스터 HP = 기본 HP × (100 + 10 × t) / 100 × 보스배율 × Random(0.9-1.1)
몬스터 속도 = 기본 속도 × (100 + t) / 100
몬스터 공격력 = 기본 공격력 × (100 + t) / 100 × Random(0.9-1.1)

// 보스배율: 일반 = 1, 중간보스 = 50, 최종보스 = 100
```

### 4. ItemData (아이템)

```csharp
public enum ItemType { Exp, Health, Magnet, Box }

public class ItemData
{
    public int Id;                  // 아이템 ID
    public string Name;             // 아이템 이름
    public ItemType Type;           // 아이템 타입
    public float Value;             // 효과 값
}
```

**아이템 효과**:
- **Exp (경험치 보석)**: Value만큼 경험치 획득
- **Health (Food)**: 체력 30% 즉시 회복
- **Magnet**: 화면 내 모든 경험치 보석 즉시 흡수
- **Box (보물 상자)**: 무기 또는 패시브 아이템 1종 즉시 획득

### 5. CharacterStat (캐릭터 스탯)

```csharp
public enum StatType
{
    MaxHp,          // 최대 체력
    MoveSpeed,      // 이동 속도
    Damage,         // 공격력
    Defense,        // 방어력
    Cooldown,       // 쿨다운
    Amount,         // 투사체 개수
    Area,           // 공격 범위
    Pierce,         // 관통력
    ExpMultiplier,  // 경험치 배율
    PickupRange,    // 아이템 획득 범위
    Luck            // 행운
}

public class CharacterStat
{
    public float MaxHp;
    public float MoveSpeed;
    public float Damage;
    public float Defense;
    public float Cooldown;
    public int Amount;
    public float Area;
    public int Pierce;
    public float ExpMultiplier;
    public float PickupRange;
    public float Luck;

    // 스탯 업그레이드 적용
    public void ApplyUpgrade(StatType statType, float value);
}
```

---

## 🔧 시스템 구현 가이드

### 1. 레벨업 강화 선택지 생성 로직

```csharp
// 의사코드
List<UpgradeOption> GenerateUpgradeOptions(int playerLevel, int weaponSlotCount)
{
    List<UpgradeOption> pool = new List<UpgradeOption>();

    // 캐릭터 스탯 강화 11종 추가
    pool.AddRange(GetStatUpgradeOptions());

    // 보유 무기 강화 추가
    pool.AddRange(GetWeaponUpgradeOptions());

    // 특수 규칙 1: 초기 무기 보장 (레벨 2-5)
    if (playerLevel >= 2 && playerLevel <= 5 && weaponSlotCount < 6)
    {
        var newWeaponOption = GetRandomNewWeaponOption();
        var options = GetRandomOptions(pool, 3);
        options.Insert(0, newWeaponOption); // 1개는 반드시 신규 무기
        return options;
    }

    // 특수 규칙 2: 무기 슬롯 포화 (6개)
    if (weaponSlotCount >= 6)
    {
        // 신규 무기 획득 제외
        return GetRandomOptions(pool, 4);
    }

    // 일반 로직
    if (weaponSlotCount < 6)
    {
        pool.AddRange(GetNewWeaponOptions());
    }

    return GetRandomOptions(pool, 4); // 중복 없이 4개 추출
}
```

### 2. 무기 시스템 베이스 클래스

```csharp
public abstract class Weapon : MonoBehaviour
{
    protected WeaponData _weaponData;
    protected int _currentLevel; // 0-4 (표시는 1-5)
    protected CharacterStat _playerStat;

    protected float _timer;

    public abstract void Initialize(WeaponData data, CharacterStat playerStat);
    public abstract void LevelUp();
    public abstract void Attack();

    protected virtual void Update()
    {
        _timer += Time.deltaTime;

        float cooldown = GetModifiedCooldown();
        if (_timer >= cooldown)
        {
            _timer = 0f;
            Attack();
        }
    }

    protected float GetModifiedCooldown()
    {
        var levelStat = _weaponData.LevelStats[_currentLevel];
        return levelStat.Cooldown * (1f - _playerStat.Cooldown / 100f);
    }

    protected float GetModifiedDamage()
    {
        var levelStat = _weaponData.LevelStats[_currentLevel];
        return levelStat.Damage * (1f + _playerStat.Damage / 100f);
    }
}
```

### 3. 적 스폰 시스템

```csharp
public class EnemySpawner : MonoBehaviour
{
    private float _gameTime; // 경과 시간 (초)
    private int _currentDifficulty; // 1-5
    private const int MAX_ENEMY_COUNT = 50;

    private void Update()
    {
        _gameTime += Time.deltaTime;
        UpdateDifficulty();

        if (GetActiveEnemyCount() < MAX_ENEMY_COUNT)
        {
            SpawnEnemy();
        }

        CheckBossSpawn();
    }

    private void UpdateDifficulty()
    {
        int minutes = Mathf.FloorToInt(_gameTime / 60f);

        if (minutes < 1) _currentDifficulty = 1;
        else if (minutes < 2) _currentDifficulty = 2;
        else if (minutes < 3) _currentDifficulty = 3;
        else if (minutes < 4) _currentDifficulty = 4;
        else _currentDifficulty = 5;
    }

    private void SpawnEnemy()
    {
        // 난이도별 스폰 로직
        switch (_currentDifficulty)
        {
            case 1: SpawnBatOnly(); break;
            case 2: SpawnBatAndZombie(); break;
            case 3: SpawnWithIncreasedRate(1.5f); break;
            case 4: SpawnWithMoreElites(); break;
            case 5: SpawnWithGhost(); break;
        }
    }

    private void CheckBossSpawn()
    {
        int minutes = Mathf.FloorToInt(_gameTime / 60f);

        // 1분마다 중간 보스
        if (minutes > 0 && minutes < 5 && !_bossSpawned[minutes])
        {
            SpawnMidBoss(minutes);
            _bossSpawned[minutes] = true;
        }

        // 5분에 최종 보스
        if (_gameTime >= 300f && !_finalBossSpawned)
        {
            SpawnFinalBoss();
            _finalBossSpawned = true;
        }
    }
}
```

### 4. 무한 맵 시스템

```csharp
public class RePosition : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area")) return;

        Vector3 playerPos = GameManager.Instance.Player.transform.position;
        Vector3 myPos = transform.position;

        float diffX = playerPos.x - myPos.x;
        float diffY = playerPos.y - myPos.y;

        Vector3 playerDir = new Vector3(
            Mathf.Abs(diffX) > Mathf.Abs(diffY) ? Mathf.Sign(diffX) : 0,
            Mathf.Abs(diffX) < Mathf.Abs(diffY) ? Mathf.Sign(diffY) : 0,
            0
        );

        // 타일 크기만큼 반대편으로 이동
        transform.position += playerDir * 40f;
    }
}
```

### 5. 오브젝트 풀링 (PoolManager 활용)

```csharp
// 게임 초기화 시
void InitializePools()
{
    // 적 풀 생성
    PoolManager.Instance.CreatePool("Enemy_Bat", batPrefab, 20, 50, true);
    PoolManager.Instance.CreatePool("Enemy_Zombie", zombiePrefab, 15, 50, true);
    PoolManager.Instance.CreatePool("Enemy_Ghost", ghostPrefab, 10, 30, true);

    // 투사체 풀 생성
    PoolManager.Instance.CreatePool("Projectile_Fireball", fireballPrefab, 30, 100, true);
    PoolManager.Instance.CreatePool("Projectile_Bullet", bulletPrefab, 50, 200, true);

    // 아이템 풀 생성
    PoolManager.Instance.CreatePool("Item_Exp", expPrefab, 100, 500, true);
    PoolManager.Instance.CreatePool("Item_Food", foodPrefab, 5, 20, true);
}

// 적 스폰
void SpawnEnemy()
{
    GameObject enemy = PoolManager.Instance.Spawn("Enemy_Bat", spawnPos, Quaternion.identity);
    // 적 초기화
}

// 적 사망
void OnEnemyDeath(GameObject enemy)
{
    // 경험치 드롭
    DropExperience(enemy.transform.position);

    // 풀로 반환
    PoolManager.Instance.Despawn(enemy);
}
```

---

## 🎯 무기 상세 스펙

### 1. Fireball (파이어볼)

**컨셉**: 궤도형 자동 포탑. 캐릭터 주위를 타원형으로 공전하는 마법서가 투사체 발사.

| 레벨 | 효과 | 데미지 | 쿨다운 | 발사 개수 | 폭발 범위 |
|:---:|:---|:---:|:---:|:---:|:---:|
| **1** | 마법서 1개, 파이어볼 1발 | 30 | 5.0초 | 1 | 1.5 |
| **2** | 피해량 +15 | 45 | 5.0초 | 1 | 1.5 |
| **3** | 폭발 범위 +25% | 45 | 5.0초 | 1 | 1.88 |
| **4** | 쿨타임 -1.0초 | 45 | 4.0초 | 1 | 1.88 |
| **5** | 발사체 +1 (총 2발 동시) | 45 | 4.0초 | 2 | 1.88 |

**구현 포인트**:
- 마법서 오브젝트가 플레이어 주위 타원 궤도로 공전
- 일정 쿨타임마다 가장 가까운 적 방향으로 발사
- 투사체는 적과 충돌 시 폭발 (Area 데미지)

### 2. Scythe (낫)

**컨셉**: 방어형 근접 궤도. 캐릭터 주위를 원형으로 공전하며 관통 피해.

| 레벨 | 효과 | 데미지 | 개수 | 특수 |
|:---:|:---|:---:|:---:|:---|
| **1** | 낫 1개 | 10 | 1 | 관통 +1, 0.8초당 1히트 |
| **2** | 낫 +1 | 10 | 2 | - |
| **3** | 피해량 +5 | 15 | 2 | - |
| **4** | 낫 +1 | 15 | 3 | - |
| **5** | 피해량 +10, 공전 속도 +20% | 25 | 3 | - |

**구현 포인트**:
- 플레이어 중심 원형 궤도 회전
- 적과 충돌 시 관통 피해 (무한 관통)
- 동일 적에게 0.8초 쿨타임 (중복 피해 방지)

### 3. Shotgun (샷건)

**컨셉**: 방향성 원거리 광역. 가장 가까운 적 자동 조준.

| 레벨 | 효과 | 발당 데미지 | 쿨타임 | 투사체 수 | 발사 각도 |
|:---:|:---|:---:|:---:|:---:|:---:|
| **1** | 기본 | 10 | 1.5초 | 3 | 30° |
| **2** | 투사체 +1 | 10 | 1.5초 | 4 | 30° |
| **3** | 쿨타임 -0.2초 | 10 | 1.3초 | 4 | 30° |
| **4** | 투사체 +1, 각도 +30° | 10 | 1.3초 | 5 | 60° |
| **5** | 쿨타임 -0.3초, 각도 +40° | 10 | 1.0초 | 5 | 100° |

**구현 포인트**:
- 가장 가까운 적 자동 조준 (없으면 마지막 방향)
- 투사체를 발사 각도만큼 부채꼴로 분산
- 각 투사체는 개별 데미지 판정

### 4. Flame Boots (화염 부츠)

**컨셉**: 이동 기반 지역 장악. 이동 궤적에 화염 장판 생성.

| 레벨 | 효과 | 초당 데미지 | 지속시간 | 특수 |
|:---:|:---|:---:|:---:|:---|
| **1** | 기본 | 10 (0.5초당 5) | 2.0초 | - |
| **2** | 피해량 +3 | 16 (0.5초당 8) | 2.0초 | - |
| **3** | 지속시간 +1.0초 | 16 | 3.0초 | - |
| **4** | 피해량 +4 | 24 (0.5초당 12) | 3.0초 | - |
| **5** | 지속시간 +1.0초, 크기 +25% | 24 | 4.0초 | 크기 1.25배 |

**구현 포인트**:
- 플레이어 이동 시 일정 간격으로 화염 장판 생성
- 장판 중복 시 중복 피해 없음 (OnTriggerStay2D 쿨타임)
- 지속시간 종료 시 장판 제거

### 5. Poison Field (독 장판)

**컨셉**: 근접 지속 피해 오라. 플레이어를 따라다니는 원형 장판.

| 레벨 | 효과 | 초당 데미지 | 범위 | 특수 |
|:---:|:---|:---:|:---:|:---|
| **1** | 기본 | 3 | 2.0 | 약한 넉백 |
| **2** | 피해량 +2 | 5 | 2.0 | - |
| **3** | 범위 +25% | 5 | 2.5 | - |
| **4** | 피해량 +3 | 8 | 2.5 | - |
| **5** | 범위 +25%, 피해 주기 -0.2초 | 10 (0.8초당 8) | 3.125 | - |

**구현 포인트**:
- CircleCollider2D로 구현
- 플레이어 Transform 자식으로 배치 (자동 추적)
- OnTriggerStay2D로 지속 피해

### 6. Bomb (폭탄)

**컨셉**: 고위험 패닉 버튼. 화면 내 일반 적 즉사 (경험치 페널티).

| 레벨 | 효과 | 쿨타임 | 경험치 드롭 | 특수 |
|:---:|:---|:---:|:---:|:---|
| **1** | 화면 내 일반 적 즉사 | 120초 | 0% | 보스/중간보스 제외 |
| **2** | 쿨타임 -20초 | 100초 | 0% | - |
| **3** | 경험치 25%로 증가 | 100초 | 25% | - |
| **4** | 쿨타임 -20초 | 80초 | 25% | - |
| **5** | 경험치 50%로 증가 | 80초 | 50% | - |

**구현 포인트**:
- Physics2D.OverlapCircleAll로 화면 내 적 탐지
- 보스/중간보스 태그 체크하여 제외
- 경험치 드롭 시 레벨에 따라 드롭률 조정

---

## 🎨 캐릭터 스탯 강화 11종

레벨업 시 선택 가능한 캐릭터 스탯 강화 목록입니다.

| 강화 항목 | 1회 선택 효과 | 적용 방식 | 비고 |
|:---|:---|:---:|:---|
| **공격력 (Might)** | 모든 무기 피해량 +5% | 곱연산 | 누적 가능 |
| **최대 체력 (Max HP)** | 최대 체력 +10% | 곱연산 | 현재 체력도 비례 증가 |
| **방어력 (Armor)** | 받는 피해 -1 감소 | 합연산 | 최소 피해 1 유지 |
| **이동 속도 (Speed)** | 이동 속도 +10% | 곱연산 | - |
| **범위 (Area)** | 무기 범위/크기 +10% | 곱연산 | 모든 무기 적용 |
| **쿨타임 (Cooldown)** | 무기 쿨타임 -5% | 곱연산 | 모든 무기 적용 |
| **투사체 개수 (Amount)** | 투사체 +1 | 합연산 | 투사체 무기만 적용 |
| **관통력 (Pierce)** | 관통 횟수 +1 | 합연산 | 모든 투사체 적용 |
| **경험치 획득 (Growth)** | 경험치 +10% | 곱연산 | - |
| **아이템 획득 범위 (Pickup)** | 획득 범위 +15% | 합연산 | - |
| **행운 (Luck)** | 드롭률 +10% | 곱연산 | 크리티컬/희귀 아이템 |

**구현 예시**:
```csharp
public void ApplyStatUpgrade(StatType statType)
{
    switch (statType)
    {
        case StatType.Damage:
            _characterStat.Damage += 5f; // +5%
            break;
        case StatType.MaxHp:
            float hpRatio = _currentHp / _characterStat.MaxHp;
            _characterStat.MaxHp *= 1.1f; // +10%
            _currentHp = _characterStat.MaxHp * hpRatio; // 비율 유지
            break;
        case StatType.Defense:
            _characterStat.Defense += 1f;
            break;
        case StatType.Amount:
            _characterStat.Amount += 1;
            break;
        // ... 나머지 스탯
    }
}
```

---

## ⚙️ 최적화 체크리스트

### 1. 오브젝트 풀링 (필수)
- [ ] 적 (Enemy) 풀링 구현
- [ ] 투사체 (Projectile) 풀링 구현
- [ ] 경험치 보석 (XP Gem) 풀링 구현
- [ ] 데미지 텍스트 풀링 구현
- [ ] Instantiate/Destroy 완전 제거

### 2. 동시 스폰 제한
- [ ] 화면 내 최대 적 50마리 제한
- [ ] 스폰 큐 구현 (대기열)
- [ ] 적 개체 수 모니터링

### 3. 무한 맵 구현
- [ ] 3x3 타일맵 배치 (총 9개)
- [ ] RePosition 스크립트로 타일 재배치
- [ ] 플레이어 중심 카메라 추적

### 4. Update Loop 분산
- [ ] **FixedUpdate**: 물리 계산 (Rigidbody.MovePosition)
- [ ] **Update**: 입력 처리, 쿨타임 계산
- [ ] **LateUpdate**: 카메라 추적, UI 갱신

### 5. 메모리 관리
- [ ] 사용하지 않는 리소스 Release
- [ ] 풀 크기 적절히 설정 (초기/최대)
- [ ] 프로파일러로 메모리 사용량 모니터링

---

## 📝 구현 우선순위

### 🔴 Critical (1주차)
1. 플레이어 이동 + 체력 시스템
2. 적 스폰 + AI (플레이어 추적)
3. 오브젝트 풀링 시스템
4. 기본 무기 2종 (Fireball, Scythe)
5. 경험치 & 레벨업 시스템

### 🟡 High (2주차)
1. 레벨업 4지선다 UI + 선택지 생성 로직
2. 캐릭터 스탯 강화 11종 구현
3. 무기 추가 구현 (Shotgun, Flame Boots)
4. 시간 기반 난이도 시스템
5. 아이템 드롭 (경험치, Food, Magnet)

### 🟢 Medium (3주차)
1. 중간 보스 시스템 (1분마다)
2. 보물 상자 (Item Box) 시스템
3. 무기 추가 구현 (Poison Field, Bomb)
4. 무한 맵 시스템
5. 캐릭터 선택 UI

### 🔵 Low (4주차)
1. 최종 보스 구현
2. 승리/패배 연출
3. 효과음/BGM
4. 파티클 효과
5. 최종 밸런싱 & 테스트

---

## 🐛 알려진 이슈 & 해결 방법

### 이슈 1: 적이 너무 많이 겹쳐서 렉 발생
**해결**:
- 최대 스폰 50마리 제한
- 적 간 충돌 무시 (Physics2D Layer 설정)
- 오브젝트 풀링으로 Instantiate 비용 제거

### 이슈 2: 무한 맵 타일 재배치 시 깜빡임
**해결**:
- OnTriggerExit2D에서 즉시 재배치 (프레임 지연 없음)
- 타일 크기 정확히 계산하여 이음새 없앰

### 이슈 3: 레벨업 선택지 중복 발생
**해결**:
- 선택지 풀에서 중복 없이 추출 (HashSet 사용)
- 이미 최대 레벨(5)인 무기는 풀에서 제외

### 이슈 4: 투사체가 화면 밖으로 나가면 사라지지 않음
**해결**:
- 투사체에 수명(lifetime) 설정
- 일정 시간 후 자동으로 풀로 반환

---

## 📂 리소스 경로 규칙

**Addressables Path Convention**:
```
Prefabs/UI/UndeadSurvivor/          # UI 프리팹
Prefabs/Player/UndeadSurvivor/      # 플레이어 프리팹
Prefabs/Monster/UndeadSurvivor/     # 몬스터 프리팹
Prefabs/Weapon/UndeadSurvivor/      # 무기 프리팹
Prefabs/Content/UndeadSurvivor/     # 아이템/투사체 프리팹

Sprites/UndeadSurvivor/             # 스프라이트
Audio/BGM/UndeadSurvivor/           # 배경 음악
Audio/SFX/UndeadSurvivor/           # 효과음
Data/UndeadSurvivor/ScriptableObjects/ # 데이터 파일
```

**ScriptableObject 파일명**:
```
CharacterDataList.asset             # 캐릭터 데이터 목록
WeaponDataList.asset                # 무기 데이터 목록
MonsterDataList.asset               # 몬스터 데이터 목록
ItemDataList.asset                  # 아이템 데이터 목록
```

---

## ✅ 완료 기준 (Definition of Done)

### MVP 완료 조건
- [ ] 캐릭터 2종 선택 가능
- [ ] 5분 생존 플레이 가능
- [ ] 무기 6종 모두 구현 완료
- [ ] 레벨업 시 4지선다 선택 UI 정상 작동
- [ ] 중간 보스 4회 + 최종 보스 구현
- [ ] 오브젝트 풀링으로 60fps 유지
- [ ] 무한 맵 시스템 버그 없음
- [ ] 승리/패배 연출 완료

### 품질 기준
- [ ] 메모리 누수 없음
- [ ] 프레임 드롭 없음 (최소 60fps)
- [ ] 크리티컬 버그 0건
- [ ] 게임 밸런스 테스트 완료 (승률 40-60%)

---

## 📞 참조 문서

- **원본 PRD**: `Assets/Docs/UndeadSurvivor_Reference.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`
- **코딩 규칙**: `.claude/UNITY_CONVENTIONS.md`
- **Git 워크플로우**: `Assets/Docs/Github-Flow.md`
