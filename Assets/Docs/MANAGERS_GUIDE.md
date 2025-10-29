# Unity Managers 사용 가이드

프로젝트에서 사용 가능한 모든 매니저의 기능과 사용법을 정리한 문서입니다.

## 📋 목차
1. [ResourceManager](#resourcemanager) - 리소스 로드 및 관리
2. [PoolManager](#poolmanager) - 오브젝트 풀링
3. [SoundManager](#soundmanager) - 사운드 및 음악
4. [UIManager](#uimanager) - UI 패널 관리
5. [CustomSceneManager](#customscenemanager) - 씬 전환
6. [GameManager](#gamemanager) - 게임 상태 관리
7. [DataManager](#datamanager) - 게임 데이터 제공자 관리

---

## ResourceManager

**역할**: Addressables 기반 리소스 로드, 캐싱, 해제 (GameObject 인스턴스는 PoolManager 연동)

### 🎯 주요 기능

#### 1. 리소스 로드

```csharp
// 동기 로드
GameObject prefab = ResourceManager.Instance.Load<GameObject>("Prefabs/Enemy");
AudioClip clip = ResourceManager.Instance.Load<AudioClip>("Audio/BGM/MainTheme");

// 비동기 로드
ResourceManager.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (prefab) => {
    if (prefab != null) {
        // prefab 사용
    }
});
```

#### 2. 프리로드

```csharp
// 여러 리소스 미리 로드
List<string> addresses = new List<string> {
    "Prefabs/Enemy1",
    "Prefabs/Enemy2",
    "Prefabs/Boss"
};

ResourceManager.Instance.PreloadAsync<GameObject>(addresses, () => {
    Debug.Log("프리로드 완료!");
});

// 진행률 확인
if (ResourceManager.Instance.IsPreloading) {
    float progress = ResourceManager.Instance.PreloadProgress; // 0.0 ~ 1.0
}
```

#### 3. GameObject 인스턴스 생성 (PoolManager 연동)

```csharp
// 풀에서 인스턴스 가져오기 (자동 풀 생성)
ResourceManager.Instance.InstantiateAsync("Prefabs/Enemy", transform, (enemy) => {
    enemy.transform.position = spawnPoint;
});

// 다중 인스턴스 생성
List<string> prefabs = new List<string> { "Prefabs/Cube", "Prefabs/Sphere" };
ResourceManager.Instance.InstantiateMultipleAsync(prefabs, transform, (instances) => {
    foreach (var instance in instances) {
        // 각 인스턴스 사용
    }
});

// 인스턴스 풀로 반환 (재사용)
ResourceManager.Instance.ReleaseInstance(enemy);

// 모든 인스턴스 반환
ResourceManager.Instance.ReleaseAllInstances();
```

#### 4. 풀 미리 생성

```csharp
// 특정 리소스의 풀 미리 생성
ResourceManager.Instance.CreatePool("Prefabs/Bullet", 50, 200, true, () => {
    Debug.Log("총알 풀 생성 완료");
});

// 여러 리소스 프리로드 + 풀 생성
List<string> addresses = new List<string> {
    "Prefabs/Bullet",
    "Prefabs/Enemy"
};
ResourceManager.Instance.PreloadAndCreatePools(addresses, 20, 100, () => {
    Debug.Log("모든 풀 준비 완료");
});
```

#### 5. 캐시 관리

```csharp
// 캐시 확인
if (ResourceManager.Instance.IsCached("Prefabs/Enemy")) {
    GameObject cached = ResourceManager.Instance.GetCached<GameObject>("Prefabs/Enemy");
}

// 리소스 해제
ResourceManager.Instance.Release("Prefabs/Enemy");

// 모든 리소스 해제
ResourceManager.Instance.ReleaseAll();

// 특정 리소스만 유지하고 나머지 해제
List<string> keep = new List<string> { "Prefabs/Player" };
ResourceManager.Instance.ReleaseUnused(keep);
```

### 💡 사용 팁

- **Addressables 주소**: "Prefabs/ObjectName" 형식 사용
- **자동 풀링**: InstantiateAsync 사용 시 자동으로 풀링 적용
- **메모리 관리**: 사용하지 않는 리소스는 Release로 해제
- **프리로드**: 로딩 화면에서 자주 사용하는 리소스 미리 로드

---

## PoolManager

**역할**: GameObject 재사용을 통한 성능 최적화

### 🎯 주요 기능

#### 1. 풀 생성

```csharp
// 프리팹으로 풀 생성
PoolManager.Instance.CreatePool(
    "BulletPool",           // 풀 이름
    bulletPrefab,           // 프리팹
    20,                     // 초기 크기
    100,                    // 최대 크기
    true                    // 확장 가능 여부
);

// Addressables로 풀 생성
PoolManager.Instance.CreatePoolAsync(
    "EnemyPool",
    "Prefabs/Enemy",
    10,
    50,
    true,
    null,
    () => Debug.Log("풀 생성 완료")
);
```

#### 2. Spawn & Despawn

```csharp
// 풀에서 오브젝트 가져오기
GameObject bullet = PoolManager.Instance.Spawn("BulletPool");

// 위치와 회전 지정
GameObject enemy = PoolManager.Instance.Spawn("EnemyPool", position, rotation);

// 부모 지정
GameObject ui = PoolManager.Instance.Spawn("UIPool", parentTransform);

// 풀로 반환 (재사용)
PoolManager.Instance.Despawn("BulletPool", bullet);

// 자동으로 풀 찾기
PoolManager.Instance.Despawn(bullet);

// 3초 후 자동 반환
PoolManager.Instance.DespawnAfter("BulletPool", bullet, 3f);
```

#### 3. 풀 관리

```csharp
// 풀 존재 확인
if (PoolManager.Instance.HasPool("BulletPool")) {
    // 풀 사용
}

// 풀 정보 조회
var (active, available, total) = PoolManager.Instance.GetPoolInfo("BulletPool");
Debug.Log($"활성: {active}, 사용가능: {available}, 총: {total}");

// 특정 풀의 모든 오브젝트 반환
PoolManager.Instance.DespawnAll("BulletPool");

// 모든 풀의 오브젝트 반환
PoolManager.Instance.DespawnAll();

// 풀 삭제
PoolManager.Instance.DestroyPool("BulletPool");

// 모든 풀 정보 출력
PoolManager.Instance.PrintAllPoolInfo();
```

### 💡 사용 팁

- **초기 크기**: 동시에 사용할 최대 개수로 설정
- **최대 크기**: 메모리 한계 고려하여 설정
- **확장 가능**: true로 설정하면 부족 시 자동 확장
- **ResourceManager 연동**: ResourceManager.InstantiateAsync 사용 권장

### ⚠️ 주의사항

- Spawn한 오브젝트는 반드시 Despawn으로 반환
- Destroy 사용 금지 (풀 추적이 깨짐)
- 풀 이름은 고유해야 함

---

## SoundManager

**역할**: BGM, SFX 재생 및 볼륨 관리

### 🎯 주요 기능

#### 1. BGM 재생

```csharp
// BGM 재생 (페이드 인)
SoundManager.Instance.PlayBGM("Audio/BGM/MainTheme", 1.5f);

// BGM 중지 (페이드 아웃)
SoundManager.Instance.StopBGM(1f);

// 일시정지/재개
SoundManager.Instance.PauseBGM();
SoundManager.Instance.ResumeBGM();

// 현재 BGM 확인
if (SoundManager.Instance.IsBgmPlaying) {
    string currentBgm = SoundManager.Instance.CurrentBgm;
}
```

#### 2. SFX 재생

```csharp
// SFX 재생
SoundManager.Instance.PlaySFX("Audio/SFX/Click", 0.8f);

// 3D 위치에서 재생
SoundManager.Instance.PlaySFXAtPoint("Audio/SFX/Explosion", explosionPos, 1f);
```

#### 3. 볼륨 조절

```csharp
// 마스터 볼륨
SoundManager.Instance.SetMasterVolume(0.8f);

// BGM 볼륨
SoundManager.Instance.SetBGMVolume(0.7f);

// SFX 볼륨
SoundManager.Instance.SetSFXVolume(0.9f);

// 음소거
SoundManager.Instance.MuteAll(true);

// 현재 볼륨 확인
float masterVol = SoundManager.Instance.MasterVolume;
float bgmVol = SoundManager.Instance.BgmVolume;
float sfxVol = SoundManager.Instance.SfxVolume;
```

### 💡 사용 팁

- **페이드 효과**: BGM 전환 시 자연스러운 페이드 사용
- **AudioMixer**: 선택적으로 AudioMixer 연결 가능
- **AudioSource 풀링**: 최대 10개 SFX 동시 재생
- **Addressables**: 오디오 클립도 Addressables로 로드

---

## UIManager

**역할**: UI 패널 열기/닫기, 팝업, 페이드 효과

### 🎯 주요 기능

#### 1. 패널 관리

```csharp
// 패널 열기 (Addressables)
UIManager.Instance.OpenPanel<SettingsPanel>("UI/SettingsPanel", (panel) => {
    if (panel != null) {
        panel.Initialize(settings);
    }
});

// 패널 닫기
UIManager.Instance.ClosePanel<SettingsPanel>();

// 최상위 패널 닫기
UIManager.Instance.CloseTopPanel();

// 모든 패널 닫기
UIManager.Instance.CloseAllPanels();

// 패널 상태 확인
if (UIManager.Instance.IsPanelOpen<SettingsPanel>()) {
    SettingsPanel panel = UIManager.Instance.GetOpenPanel<SettingsPanel>();
}
```

#### 2. 팝업

```csharp
// 단순 팝업
UIManager.Instance.ShowPopup("알림", "저장이 완료되었습니다.", () => {
    Debug.Log("확인 클릭");
});

// 확인/취소 대화상자
UIManager.Instance.ShowConfirmDialog(
    "정말 삭제하시겠습니까?",
    () => Debug.Log("예 클릭"),
    () => Debug.Log("아니오 클릭")
);
```

#### 3. 페이드 효과

```csharp
// 페이드 인 (검은색 → 투명)
UIManager.Instance.FadeIn(1f, () => {
    Debug.Log("페이드 인 완료");
});

// 페이드 아웃 (투명 → 검은색)
UIManager.Instance.FadeOut(0.5f, () => {
    Debug.Log("페이드 아웃 완료");
});

// 페이드 색상 변경
UIManager.Instance.SetFadeColor(Color.white);
```

### 💡 UIPanel 베이스 클래스 사용

```csharp
public class MyPanel : UIPanel
{
    protected override void OnOpen()
    {
        // 패널 열릴 때 호출
    }

    protected override void OnClose()
    {
        // 패널 닫힐 때 호출
    }
}
```

### 💡 사용 팁

- **패널 스택**: 열린 순서대로 스택에 쌓임
- **Canvas 계층**: Base(0), Popup(100), System(200)
- **자동 캐싱**: 한번 로드한 패널은 캐시됨
- **페이드 활용**: 씬 전환 시 페이드 효과로 부드러운 전환

---

## CustomSceneManager

**역할**: 씬 로드 및 전환 관리

### 🎯 주요 기능

#### 1. 씬 로드

```csharp
// 동기 로드
CustomSceneManager.Instance.LoadScene("GameScene");
CustomSceneManager.Instance.LoadScene(1); // 빌드 인덱스

// 비동기 로드
CustomSceneManager.Instance.LoadSceneAsync("GameScene", () => {
    Debug.Log("로드 완료");
});

// 로딩 화면과 함께 로드
CustomSceneManager.Instance.LoadSceneWithLoading("GameScene", 2f);

// 진행률 확인
if (CustomSceneManager.Instance.IsLoading) {
    float progress = CustomSceneManager.Instance.LoadProgress; // 0.0 ~ 1.0
}
```

#### 2. 씬 스택 (히스토리)

```csharp
// 현재 씬 저장 후 새 씬 로드
CustomSceneManager.Instance.PushScene("SettingsScene");

// 이전 씬으로 돌아가기
CustomSceneManager.Instance.PopScene();

// 스택 초기화
CustomSceneManager.Instance.ClearSceneStack();

// 스택 개수 확인
int count = CustomSceneManager.Instance.SceneStackCount;
```

#### 3. Additive 씬 로드

```csharp
// 현재 씬 유지하며 추가 로드
CustomSceneManager.Instance.LoadSceneAdditive("UIScene", (scene) => {
    Debug.Log($"추가 로드 완료: {scene.name}");
});

// 씬 언로드
CustomSceneManager.Instance.UnloadScene("UIScene", () => {
    Debug.Log("언로드 완료");
});

// 활성 씬 설정
CustomSceneManager.Instance.SetActiveScene("GameScene");
```

#### 4. 유틸리티

```csharp
// 현재 씬 이름
string currentScene = CustomSceneManager.Instance.CurrentSceneName;

// 씬 로드 여부 확인
if (CustomSceneManager.Instance.IsSceneLoaded("UIScene")) {
    // 씬이 로드됨
}

// 씬 빌드 인덱스 조회
int index = CustomSceneManager.Instance.GetSceneBuildIndex("GameScene");
```

### 💡 사용 팁

- **페이드 연동**: UIManager 페이드와 함께 사용
- **로딩 화면**: 최소 로딩 시간으로 너무 빠른 전환 방지
- **씬 스택**: 설정 화면 등에서 이전 화면 복귀에 활용
- **Additive**: UI 씬을 별도로 관리할 때 유용

---

## GameManager

**역할**: 게임 상태 및 데이터 관리 (제네릭)

### 🎯 주요 기능

#### 1. 게임 시작/중지

```csharp
// 새 게임 시작
GameManager<TetrisGameData>.Instance.StartNewGame();

// 게임 상태 확인
if (GameManager<TetrisGameData>.Instance.IsPlaying) {
    // 게임 진행 중
}

// 일시정지/재개
GameManager<TetrisGameData>.Instance.PauseGame();
GameManager<TetrisGameData>.Instance.ResumeGame();
```

#### 2. 게임 데이터 관리

```csharp
// 현재 게임 데이터 접근
var data = GameManager<TetrisGameData>.Instance.CurrentGameData;
data.Score = 1000;
data.Level = 5;

// 게임 데이터 전환
var newData = new PuzzleGameData();
GameManager<PuzzleGameData>.Instance.SwitchGameData(newData);

// 데이터 검증
if (GameManager<TetrisGameData>.Instance.ValidateCurrentGameData()) {
    // 유효한 데이터
}
```

### 💡 IGameData 구현

```csharp
public class MyGameData : IGameData
{
    public int Score;
    public int Level;

    public void Initialize()
    {
        Score = 0;
        Level = 1;
    }

    public void Reset()
    {
        Initialize();
    }

    public bool Validate()
    {
        return Score >= 0 && Level > 0;
    }
}
```

---

## DataManager

**역할**: 게임별 데이터 제공자를 중앙에서 관리하며, 게임 데이터의 라이프사이클을 제어

### 🎯 주요 기능

#### 1. 데이터 제공자 등록 및 초기화

```csharp
// 데이터 제공자 등록 (게임 시작 시 한 번만 실행)
public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        // UndeadSurvivor 데이터 제공자 등록
        var undeadProvider = new UndeadSurvivorDataProvider();
        DataManager.Instance.RegisterProvider(undeadProvider);

        // 다른 게임 데이터 제공자 등록
        // DataManager.Instance.RegisterProvider(new TetrisDataProvider());
    }
}

// 제공자 존재 여부 확인
if (DataManager.Instance.HasProvider("UndeadSurvivor"))
{
    Debug.Log("UndeadSurvivor 데이터 제공자가 등록됨");
}
```

#### 2. 게임 데이터 로드 및 언로드

```csharp
// 게임 시작 시 데이터 로드
DataManager.Instance.LoadGameData("UndeadSurvivor");

// 데이터 로드 상태 확인
if (DataManager.Instance.IsGameDataLoaded("UndeadSurvivor"))
{
    Debug.Log("UndeadSurvivor 데이터가 로드됨");
}

// 게임 종료 시 데이터 언로드 (메모리 정리)
DataManager.Instance.UnloadGameData("UndeadSurvivor");

// 모든 게임 데이터 언로드
DataManager.Instance.UnloadAllGameData();
```

#### 3. 데이터 제공자 조회 및 사용

```csharp
// 제공자 인스턴스 가져오기
var provider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

if (provider != null && provider.IsLoaded)
{
    // 몬스터 데이터 조회
    MonsterData monster = provider.GetMonsterData(1001);
    Debug.Log($"몬스터: {monster.Name}, HP: {monster.MaxHp}");

    // 레벨에 따라 스케일링된 몬스터 스탯 조회
    CharacterStat scaledStat = provider.GetScaledMonsterStat(1001, 5, isBoss: false);
    Debug.Log($"레벨 5 몬스터 HP: {scaledStat.MaxHp}");

    // 무기 데이터 조회
    WeaponData weapon = provider.GetWeaponData(2001);
    Debug.Log($"무기: {weapon.Name}, 타입: {weapon.Type}");

    // 무기 레벨별 스탯 조회
    WeaponLevelStat levelStat = provider.GetWeaponLevelStat(2001, 3);
    Debug.Log($"레벨 3 데미지: {levelStat.Damage}, 쿨다운: {levelStat.Cooldown}");

    // 아이템 데이터 조회
    ItemData item = provider.GetItemData(3001);
    Debug.Log($"아이템: {item.Name}, 타입: {item.Type}, 효과: {item.Value}");

    // 캐릭터 데이터 조회
    CharacterData character = provider.GetCharacterData(4001);
    Debug.Log($"캐릭터: {character.Name}, HP: {character.MaxHp}");
}
```

#### 4. 데이터 존재 여부 확인

```csharp
var provider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

if (provider != null)
{
    // 특정 몬스터 존재 확인
    if (provider.HasMonsterData(1001))
    {
        MonsterData monster = provider.GetMonsterData(1001);
    }

    // 특정 무기 존재 확인
    if (provider.HasWeaponData(2001))
    {
        WeaponData weapon = provider.GetWeaponData(2001);
    }

    // 특정 아이템 존재 확인
    if (provider.HasItemData(3001))
    {
        ItemData item = provider.GetItemData(3001);
    }

    // 특정 캐릭터 존재 확인
    if (provider.HasCharacterData(4001))
    {
        CharacterData character = provider.GetCharacterData(4001);
    }
}
```

### 💡 UndeadSurvivor 데이터 구조

#### MonsterData (몬스터 데이터)
```csharp
public class MonsterData
{
    public int Id { get; }              // 몬스터 ID
    public string Name { get; }         // 몬스터 이름
    public float MaxHp { get; }         // 기본 최대 HP
    public float MoveSpeed { get; }     // 기본 이동 속도
    public float Damage { get; }        // 기본 공격력
    public float Defense { get; }       // 기본 방어력
    public float ExpMultiplier { get; } // 경험치 배율
}

// 레벨 스케일링 공식
// HP: baseHp × ((100 + 10 × level) / 100) × bossMultiplier × Random(0.9-1.1)
// Speed: baseSpeed × ((100 + level) / 100)
// Damage: baseDamage × ((100 + level) / 100) × Random(0.9-1.1)
// Boss Multiplier: 일반 몬스터 = 1, 보스 = 50
```

#### WeaponData (무기 데이터)
```csharp
public enum WeaponType { Melee, Ranged, Area }

public class WeaponData
{
    public int Id { get; }                        // 무기 ID
    public string Name { get; }                   // 무기 이름
    public WeaponType Type { get; }               // 무기 타입
    public WeaponLevelStat[] LevelStats { get; }  // 레벨별 스탯 (0-4)
}

public class WeaponLevelStat
{
    public float Damage { get; }         // 레벨별 데미지
    public float Cooldown { get; }       // 레벨별 쿨다운
    public int CountPerCreate { get; }   // 레벨별 생성 개수
}
```

#### ItemData (아이템 데이터)
```csharp
public enum ItemType { Exp, Health, Magnet, Box }

public class ItemData
{
    public int Id { get; }          // 아이템 ID
    public string Name { get; }     // 아이템 이름
    public ItemType Type { get; }   // 아이템 타입
    public float Value { get; }     // 효과 값
}
```

#### CharacterData (캐릭터 데이터)
```csharp
public class CharacterData
{
    public int Id { get; }              // 캐릭터 ID
    public string Name { get; }         // 캐릭터 이름
    public float MaxHp { get; }         // 최대 HP
    public float MoveSpeed { get; }     // 이동 속도
    public float Damage { get; }        // 공격력 증가 (%)
    public float Defense { get; }       // 방어력
    public float Cooldown { get; }      // 쿨다운 감소 (%)
    public int Amount { get; }          // 발사체 개수 증가
    public int StartWeaponId { get; }   // 시작 무기 ID
}
```

#### CharacterStat (캐릭터 스탯)
```csharp
public enum StatType
{
    MaxHp,        // 최대 HP
    MoveSpeed,    // 이동 속도
    Damage,       // 공격력
    Defense,      // 방어력
    Cooldown,     // 쿨다운
    Amount,       // 발사체 개수
    ExpMultiplier // 경험치 배율
}

public class CharacterStat
{
    public float MaxHp { get; }
    public float MoveSpeed { get; }
    public float Damage { get; }
    public float Defense { get; }
    public float Cooldown { get; }
    public int Amount { get; }
    public float ExpMultiplier { get; }

    // 스탯 업그레이드 적용
    public void ApplyUpgrade(StatType statType, float value);
}
```

### 💡 사용 팁

- **등록 시점**: 게임 시작 시 모든 데이터 제공자를 등록
- **로드 시점**: MiniGameManager가 게임을 로드할 때 자동으로 데이터 로드
- **언로드 시점**: 게임 종료 시 자동으로 데이터 언로드 (메모리 정리)
- **레벨 스케일링**: GetScaledMonsterStat()으로 레벨별 몬스터 스탯 계산
- **무기 레벨**: 무기는 0-4 레벨까지 총 5단계 업그레이드 가능
- **ScriptableObject**: Unity 에디터에서 ScriptableObject로 데이터 관리

### ⚠️ 주의사항

- DataManager는 자동으로 MiniGameManager와 연동됨
- 수동으로 LoadGameData/UnloadGameData 호출 불필요 (MiniGameManager가 자동 처리)
- ScriptableObject 파일은 `Resources/Data/{GameID}/ScriptableObjects/` 경로에 배치
- 데이터 제공자는 반드시 IGameDataProvider 인터페이스 구현 필요

---

## 🎯 매니저 사용 패턴

### 게임 시작 시

```csharp
void Start()
{
    // 1. 리소스 프리로드 + 풀 생성
    List<string> resources = new List<string> {
        "Prefabs/Enemy",
        "Prefabs/Bullet",
        "Prefabs/PowerUp"
    };
    ResourceManager.Instance.PreloadAndCreatePools(resources, 20, 100, () => {
        // 2. BGM 재생
        SoundManager.Instance.PlayBGM("Audio/BGM/MainTheme", 1f);

        // 3. UI 페이드 아웃
        UIManager.Instance.FadeOut(1f, () => {
            // 4. 게임 시작
            GameManager<MyGameData>.Instance.StartNewGame();
        });
    });
}
```

### 적 생성

```csharp
void SpawnEnemy()
{
    // 풀에서 가져오기
    ResourceManager.Instance.InstantiateAsync("Prefabs/Enemy", transform, (enemy) => {
        enemy.transform.position = spawnPoint;

        // SFX 재생
        SoundManager.Instance.PlaySFX("Audio/SFX/Spawn");

        // 5초 후 자동 반환
        PoolManager.Instance.DespawnAfter("Prefabs/Enemy", enemy, 5f);
    });
}
```

### 씬 전환

```csharp
void GoToNextLevel()
{
    // 1. 페이드 인
    UIManager.Instance.FadeIn(0.5f, () => {
        // 2. BGM 페이드 아웃
        SoundManager.Instance.StopBGM(0.5f);

        // 3. 씬 전환
        CustomSceneManager.Instance.LoadSceneWithLoading("Level2", 2f);
    });
}
```

### 일시정지 메뉴

```csharp
void ShowPauseMenu()
{
    // 1. 게임 일시정지
    GameManager<MyGameData>.Instance.PauseGame();

    // 2. BGM 일시정지
    SoundManager.Instance.PauseBGM();

    // 3. 일시정지 UI 열기
    UIManager.Instance.OpenPanel<PausePanel>("UI/PausePanel", (panel) => {
        panel.OnResumeClicked += () => {
            GameManager<MyGameData>.Instance.ResumeGame();
            SoundManager.Instance.ResumeBGM();
            UIManager.Instance.ClosePanel<PausePanel>();
        };
    });
}
```

---

## ⚠️ 중요 사항

### 초기화 순서
1. Singleton 매니저들은 자동 초기화됨
2. ResourceManager → PoolManager 순서로 초기화 권장
3. 게임 시작 전 필요한 리소스 프리로드

### 메모리 관리
- 사용하지 않는 리소스는 Release
- 씬 전환 시 불필요한 풀 정리
- OnDestroy에서 자동 정리되지만, 명시적 정리 권장

### 성능 최적화
- 자주 생성/삭제되는 오브젝트는 반드시 풀링
- Addressables 비동기 로드 활용
- 프리로드로 로딩 시간 분산

### 에러 처리
- 모든 매니저는 Instance가 null인지 확인
- 콜백에서 null 체크 필수
- Addressables 주소가 올바른지 확인

---

## 📚 추가 리소스

- **테스트 스크립트**: `Assets/Scripts/Tests/` 참고
- **예제 씬**: 각 매니저별 테스트 씬 확인
- **Unity 문서**: Addressables, Singleton 패턴 공식 문서
