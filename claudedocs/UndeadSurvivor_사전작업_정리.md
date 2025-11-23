# Undead Survivor 사전 작업 정리

**작업 기간**: 2025-11-05 ~ 2025-11-08
**브랜치**: `feature/undead-survivor`
**목적**: Undead Survivor 게임 개발을 위한 인프라 및 핵심 시스템 구축

---

## 📋 전체 작업 목록

### ✅ Phase 1: 플레이어 시스템 (완료)
- Player 통합 시스템 구현
- CharacterData 기반 초기화
- 컴포넌트 기반 설계 (Controller, Health, Experience, WeaponManager)
- 플레이어 이동, 체력, 경험치 시스템

**커밋**: `151e780` - feat: Undead Survivor Phase 1 완료

---

### ✅ Phase 2: 적 시스템 (완료)

#### 2-1. 적 베이스 시스템
**파일**:
- `Enemy.cs` (254줄) - 적 베이스 클래스
- `EnemySpawner.cs` (233줄) - 적 스폰 시스템
- `MonsterData.json` - 5개 몬스터 데이터 (Slime, Zombie, Skeleton, Ghost, Boss_Demon)

**주요 기능**:
- 플레이어 추적 AI
- 체력 및 데미지 시스템
- 난이도 스케일링 (30초마다 +10%)
- 죽을 시 경험치 드롭
- 레벨 기반 스탯 스케일링 (±10% 랜덤 변이)

**커밋**: `aea0b15` - feat: Phase 2 적 시스템 구현

#### 2-2. DataProvider 통합
**변경사항**:
- `UndeadSurvivorDataProvider.LoadMonsterData()` - JSON 로딩 방식 변경
- `UndeadSurvivorGame.StartGame()` - EnemySpawner 통합
- 적 스폰 생명주기 관리 (Start → Stop → Cleanup)

**커밋**: `a1ce7ba` - feat: Phase 2 적 시스템 연동 완료

---

### ✅ Phase 3: 무기 시스템 (완료)

#### 3-1. 무기 베이스 시스템
**파일**:
- `Weapon.cs` (268줄) - 무기 추상 베이스 클래스
- `Projectile.cs` (222줄) - 투사체 클래스

**주요 기능**:
- 자동 공격 타이머 시스템
- 타겟 찾기 (가장 가까운 적, 반경 내 모든 적)
- 플레이어 스탯 기반 최종 데미지 계산
- 레벨업 시스템 (0-4레벨, 표시 1-5레벨)
- 투사체: 관통 시스템, 생존 시간 제한

#### 3-2. 구체적인 무기 구현
**Fireball** (원거리 투사체):
- 가장 가까운 적에게 발사
- 부채꼴 다중 발사 (레벨업 시 개수 증가)
- 15도 간격 발사 패턴
- 관통력 증가

**Scythe** (근접 회전):
- 플레이어 주변 공전 (180도/초)
- 낫 자체 자전 (360도/초) - 위협감 제공
- 균등 배치 (n개 낫이 360/n도 간격)
- 데미지 쿨다운 (0.5초)

#### 3-3. PlayerWeaponManager 연동
**변경사항**:
- 무기 타입 매핑 (WeaponData ID → 무기 클래스)
- `CreateWeaponObject()` - 동적 무기 생성
- 무기 초기화 및 레벨업 자동 연동
- WeaponSlot에 WeaponComponent 참조 추가

**커밋**: `28eb01b` - feat: Implement Phase 3 - Weapon System

---

### ✅ Manager 리팩토링: 풀링 시스템 통합 (완료)

#### 목적
- Resources.Load 직접 사용 제거
- Instantiate/Destroy 패턴을 PoolManager 기반으로 전환
- 메모리 효율성 및 성능 최적화

#### 구현 내용

**1. IPoolable 인터페이스 생성** (`Assets/Scripts/Core/IPoolable.cs`):
```csharp
public interface IPoolable
{
    void OnSpawnedFromPool();  // 풀에서 스폰 시 호출
    void OnReturnedToPool();   // 풀로 반환 시 호출
}
```

**2. PoolManager 수정**:
- `Spawn()`: IPoolable 체크 후 OnSpawnedFromPool 자동 호출
- `Despawn()`: IPoolable 체크 후 OnReturnedToPool 자동 호출

**3. ResourceManager 수정**:
- `InstantiateFromResources<T>()`: 자동 풀 생성 및 PoolManager.Spawn 호출
- `InstantiateGameObjectFromResources()`: 자동 풀 생성 및 PoolManager.Spawn 호출
- 풀이 없으면 자동 생성 (초기 10개, 최대 100개)
- 풀이 있으면 재사용

**4. Projectile, ScytheBlade IPoolable 구현**:
- 상태 초기화 로직
- 물리 리셋 로직
- Trail Renderer 초기화

**5. Fireball, Scythe 리팩토링**:
- `Resources.Load` → `ResourceManager.InstantiateFromResources`
- 자동 풀링 지원

**시스템 흐름**:
```
Fireball.FireProjectile()
→ ResourceManager.InstantiateFromResources<Projectile>()
→ 풀 없음 → CreatePool() 자동 호출
→ PoolManager.Spawn()
→ Projectile.OnSpawnedFromPool() 자동 호출
→ 투사체 사용
→ (TODO: Projectile.DestroyProjectile에서 PoolManager.Despawn 호출)
→ Projectile.OnReturnedToPool() 자동 호출
```

**커밋**:
- `ba02248` - refactor: Integrate PoolManager with ResourceManager
- `9b30aa3` - refactor: Integrate PoolManager with ResourceManager (중복)

---

### ✅ 씬 구조 개선 (완료)

#### 3-씬 구조 구현
1. **UndeadSurvivor** (초기 화면) - 게임 시작, 설정, 종료
2. **UndeadSurvivorCharacterSelectionScene** - 캐릭터 선택
3. **UndeadSurvivorGameScene** - 실제 게임 플레이

**파일**:
- `UndeadSurvivorInitialScene.cs` (NEW)
- `UndeadSurvivorCharacterSelectScene.cs` (수정)
- `UndeadSurvivorGameScene.cs` (기존 UndeadSurvivorScene.cs에서 이름 변경)

**SceneID 확장**:
```csharp
UndeadSurvivor = 4,
UndeadSurvivorCharacterSelectionScene = 5,
UndeadSurvivorGameScene = 6,
```

**CustomSceneManager 확장**:
- `ReloadCurrentScene()` (3개 오버로드 추가)

**데이터 전달**:
- PlayerPrefs로 선택된 캐릭터 ID 전달 (임시 방법)

**커밋**: `b8743ba` - feat: Undead Survivor 3-씬 구조 구현

---

### ✅ 인프라 시스템 구축 (완료)

#### DataManager 시스템
**목적**: 멀티게임 데이터 프로바이더 중앙 관리

**구조**:
- `DataManager` - 싱글톤 중앙 매니저
- `IGameDataProvider` - 게임별 데이터 제공자 인터페이스
- `UndeadSurvivorDataProvider` - Undead Survivor 데이터 제공

**특징**:
- Lazy Loading (게임 시작 시 로드, 종료 시 언로드)
- 게임별 독립적 데이터 관리
- ScriptableObject + JSON 지원

**커밋**: `230a533` - feat: Implement centralized DataManager system

#### Resources 폴더 재구조화
**변경 전**: 타입별 구조 (`Prefabs/{Type}/`)
**변경 후**: 게임별 구조 (`Prefabs/{Type}/{GameID}/`)

**목적**: 게임 간 리소스 충돌 방지

**커밋**: `e307a65` - refactor: Resources 폴더 구조 재구성

#### 게임 선택 UI 시스템
**구현**:
- MainMenuScene에서 GameSelectUIPanel 동적 버튼 생성
- GamePlayList 기반 게임 목록 관리
- Addressables 경로: `Sprite/{GameID}_icon`

**커밋**: `5c608b3` - feat: 게임 선택 UI 시스템 구현

---

### ✅ 문서 작업 (완료)

#### MANAGERS_GUIDE.md 업데이트
**추가 내용**:
- IPoolable 인터페이스 섹션 (PoolManager 내)
- OnSpawnedFromPool/OnReturnedToPool 사용법
- 실제 코드 예제 (Projectile 클래스)
- OnEnable/OnDisable 충돌 방지 안내

#### manager-guide.yml 스킬 강화
**키워드 확장** (89개):
- 오브젝트 생성: instantiate, spawn, pool, 풀링
- 리소스 호출: resource load, addressable
- UI 관련: panel, popup, ShowPanel
- Scene 관련: 씬 전환, LoadScene
- 사운드: BGM, SFX, PlayBGM
- Input: input, InputManager
- 데이터: DataManager, data provider

**커밋**: `4d48bdf` - docs: Enhance manager-guide skill keywords

---

## 📊 구현 통계

### 파일 생성/수정
**새로 생성된 파일**:
- Core: `IPoolable.cs`
- UndeadSurvivor:
  - `Weapon.cs`, `Projectile.cs`, `Fireball.cs`, `Scythe.cs`
  - `Enemy.cs`, `EnemySpawner.cs`
  - `UndeadSurvivorInitialScene.cs`
  - `UndeadSurvivorGame.cs`, `UndeadSurvivorGameData.cs`
  - `MonsterData.json`

**수정된 파일**:
- Managers: `PoolManager.cs`, `ResourceManager.cs`
- Scenes: `UndeadSurvivorCharacterSelectScene.cs`, `UndeadSurvivorGameScene.cs`
- Core: `SceneID.cs`, `CustomSceneManager.cs`
- Data: `UndeadSurvivorDataProvider.cs`, `MonsterDataList.cs`

### 코드 라인 수
- Weapon.cs: 268줄
- Enemy.cs: 254줄
- Projectile.cs: 222줄
- EnemySpawner.cs: 233줄
- Fireball.cs: 152줄
- Scythe.cs: 348줄 (ScytheBlade 포함)

**총 추가 코드**: 약 1,477줄

---

## 🎯 주요 성과

### 1. 아키텍처 개선
✅ Manager 패턴 일관성 확보 (ResourceManager + PoolManager 통합)
✅ IPoolable 인터페이스로 풀링 생명주기 명확화
✅ 3-씬 구조로 게임 흐름 체계화

### 2. 성능 최적화
✅ 자동 풀링 시스템 (메모리 할당 최소화)
✅ GC 압력 감소 (Instantiate/Destroy → Spawn/Despawn)
✅ 리소스 캐싱 및 재사용

### 3. 코드 품질
✅ Resources.Load 직접 사용 제거
✅ Manager 사용 패턴 표준화
✅ 명확한 생명주기 관리 (IPoolable)

### 4. 개발자 경험
✅ 89개 키워드로 Manager 가이드 자동 활성화
✅ MANAGERS_GUIDE.md 상세 문서화
✅ 일관된 코딩 패턴 확립

---

## 🚧 남은 작업 (TODO)

### Phase 3 완료 작업
- [ ] Projectile.DestroyProjectile에서 PoolManager.Despawn 호출
- [ ] 히트 이펙트 풀링 적용
- [ ] WeaponData JSON 파일 생성 (구조는 완료, 데이터 필요)

### Phase 4: 레벨업 UI 시스템
- [ ] 4지선다 레벨업 UI
- [ ] Time.timeScale 제어
- [ ] 업그레이드 선택 로직
- [ ] 11개 스탯 업그레이드 타입 구현

### Phase 5: 사운드 시스템
- [ ] PlayerWeaponManager SoundManager 연동
- [ ] 무기 발사 SFX
- [ ] 적 피격/사망 SFX
- [ ] BGM 재생

### 프리팹 및 리소스 작업
- [ ] Projectile 프리팹 생성 (Fireball_Projectile, Scythe_Blade)
- [ ] Enemy 프리팹 생성 (5개 몬스터)
- [ ] Sprite 리소스 준비

### Addressables 전환
- [ ] Resources 폴더 → Addressables 완전 전환
- [ ] 비동기 로딩 최적화

---

## 📝 기술 부채

1. **PlayerPrefs 임시 사용**
   - 현재: 씬 간 캐릭터 ID 전달에 PlayerPrefs 사용
   - 개선: SceneDataManager 구현 필요

2. **프리팹 미생성**
   - InstantiateFromResources 사용하지만 실제 프리팹 없음
   - Unity 에디터에서 프리팹 생성 필요

3. **TODO 주석**
   - Projectile.DestroyProjectile: PoolManager.Despawn 미적용
   - 히트 이펙트: 직접 Instantiate 사용 중

---

## 🔄 브랜치 상태

**현재 브랜치**: `feature/undead-survivor`
**커밋 수**: 10개
**총 변경**: +1,500줄, -100줄

**최신 커밋**:
```
4d48bdf docs: Enhance manager-guide skill keywords and add IPoolable documentation
9b30aa3 refactor: Integrate PoolManager with ResourceManager
28eb01b feat: Implement Phase 3 - Weapon System
a1ce7ba feat: Phase 2 적 시스템 연동 완료
```

---

## 📚 참고 문서

- `Assets/Docs/MANAGERS_GUIDE.md` - Manager API 완전 가이드
- `Assets/Docs/UndeadSurvivor_Reference.md` - 원본 게임 참고 자료
- `.claude/skills/manager-guide.yml` - Manager 가이드 스킬
- `CLAUDE.md` - 프로젝트 전체 가이드
