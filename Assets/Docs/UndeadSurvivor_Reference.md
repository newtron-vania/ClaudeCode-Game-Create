# Undead Survivor (Vampire Survivor Clone) - 코드 참조 문서

> **원본 프로젝트 경로**: `/Users/kimkyeongsoo/Desktop/Unity/Undead_Survivor-Vampire_Survivor-copy-practice-main`
>
> **작성일**: 2025-10-25
>
> **목적**: 현재 프로젝트에 Undead Survivor 게임 구현 시 참조

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [아키텍처 구조](#아키텍처-구조)
3. [주요 시스템](#주요-시스템)
4. [게임 메커니즘](#게임-메커니즘)
5. [데이터 구조](#데이터-구조)
6. [UI 시스템](#ui-시스템)
7. [코드 참조 가이드](#코드-참조-가이드)

---

## 프로젝트 개요

### 게임 장르
- **로그라이크 서바이벌 액션** (Vampire Survivors 클론)
- 플레이어가 무기를 업그레이드하며 무한히 스폰되는 적을 처치하는 게임
- 시간 기반 레벨링 및 보스 스폰 시스템

### 주요 특징
- **자동 공격 무기 시스템**: 6종류 무기 (Knife, Fireball, Spin, Poison, Lightning, Shotgun)
- **레벨업 강화 시스템**: 랜덤 강화 옵션 3개 제시
- **아이템 드롭 시스템**: 경험치, 체력, 마그넷, 아이템 박스
- **시간 기반 난이도**: 1분마다 중간 보스 스폰 (5분까지)
- **캐릭터 선택**: 2종류 캐릭터, 각기 다른 초기 무기

### 기술 스택
- Unity 2021.3.x
- TextMesh Pro
- Resources 폴더 기반 리소스 관리
- JSON 데이터 기반 밸런싱

---

## 아키텍처 구조

### 📁 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Managers/           # 핵심 매니저 시스템
│   │   ├── Managers.cs           # 통합 매니저 (싱글톤 패턴)
│   │   ├── GameManagerEx.cs      # 게임 로직 (스폰, 플레이어 관리)
│   │   ├── DataManager.cs        # JSON 데이터 로드/관리
│   │   ├── ResourceManager.cs    # Resources 폴더 리소스 관리
│   │   ├── PoolManager.cs        # 오브젝트 풀링
│   │   ├── UIManager.cs          # UI 생성/관리
│   │   ├── SoundManager.cs       # BGM/SFX 관리
│   │   ├── SceneManagerEx.cs     # 씬 전환
│   │   └── EventManager.cs       # 게임 이벤트
│   │
│   ├── Controller/         # 게임 오브젝트 컨트롤러
│   │   ├── BaseController.cs     # 컨트롤러 베이스 클래스
│   │   ├── PlayerController.cs   # 플레이어 움직임/전투
│   │   ├── EnemyController.cs    # 적 AI 및 전투
│   │   ├── BossController.cs     # 보스 전용 로직
│   │   ├── CameraController.cs   # 카메라 추적
│   │   └── ItemGetter.cs         # 아이템 획득 범위
│   │
│   ├── Weapons/            # 무기 시스템
│   │   ├── WeaponController.cs   # 무기 베이스 클래스
│   │   ├── KnifeController.cs    # 근접 회전 무기
│   │   ├── FireballController.cs # 원거리 투사체
│   │   ├── SpinController.cs     # 회전 무기
│   │   ├── PoisonController.cs   # 독 장판 무기
│   │   ├── LightningController.cs # 번개 관통 무기
│   │   ├── ShotgunController.cs  # 산탄총 무기
│   │   └── Projectile/           # 투사체 스크립트
│   │
│   ├── Contents/           # 게임 콘텐츠
│   │   ├── Spawner.cs            # 적 스폰 시스템
│   │   ├── RePosition.cs         # 무한 맵 타일 재배치
│   │   ├── WorldScrolling.cs     # 배경 스크롤
│   │   └── Items/                # 아이템 (Exp, Health, Magnet, Box)
│   │
│   ├── UI/                 # UI 시스템
│   │   ├── UI_Base.cs            # UI 베이스 클래스
│   │   ├── Popup/                # 팝업 UI
│   │   │   ├── UI_LevelUp.cs     # 레벨업 강화 선택
│   │   │   ├── UI_GameOver.cs    # 게임 오버
│   │   │   ├── UI_GameVictory.cs # 게임 승리
│   │   │   └── UI_CharacterSelect.cs # 캐릭터 선택
│   │   ├── Scene/                # 씬 UI
│   │   │   ├── UI_Player.cs      # 플레이어 HUD
│   │   │   └── UI_MainMenu.cs    # 메인 메뉴
│   │   └── SubItem/              # UI 서브 아이템
│   │       ├── UpgdPanel.cs      # 강화 옵션 패널
│   │       ├── WeaponInven.cs    # 무기 인벤토리 슬롯
│   │       └── PlayerInven.cs    # 플레이어 정보 슬롯
│   │
│   ├── Stat/               # 스탯 시스템
│   │   ├── Stat.cs               # 스탯 베이스 클래스
│   │   ├── PlayerStat.cs         # 플레이어 스탯 관리
│   │   └── EnemyStat.cs          # 적 스탯 관리
│   │
│   ├── Data/               # 데이터 구조
│   │   └── Data.Contents.cs      # Player, Monster, Weapon 데이터
│   │
│   ├── Scene/              # 씬 컨트롤러
│   │   ├── BaseScene.cs          # 씬 베이스 클래스
│   │   ├── GameScene.cs          # 게임 씬
│   │   ├── MainMenuScene.cs      # 메인 메뉴 씬
│   │   └── SplashScene.cs        # 스플래시 씬
│   │
│   └── Utils/              # 유틸리티
│       ├── Define.cs             # Enum 정의
│       ├── Extension.cs          # 확장 메서드
│       └── Util.cs               # 헬퍼 함수
│
└── Resources/              # 리소스 폴더
    ├── Data/               # JSON 데이터 파일
    │   ├── PlayerData.json
    │   ├── MonsterData.json
    │   └── WeaponData.json
    ├── Prefabs/            # 프리팹
    ├── Sprites/            # 스프라이트
    ├── Audio/              # 오디오
    └── Animations/         # 애니메이션
```

---

## 주요 시스템

### 1. 매니저 시스템 (`Managers.cs`)

**통합 싱글톤 매니저 패턴**

```csharp
public class Managers : MonoBehaviour
{
    static Managers _instance;
    public static Managers Instance { get { Init(); return _instance; } }

    // 게임 로직 매니저
    GameManagerEx _game = new GameManagerEx();
    public static GameManagerEx Game { get { return _instance._game; } }

    // 핵심 매니저들
    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    UIManager _ui = new UIManager();
    SoundManager _sound = new SoundManager();
    EventManager _event = new EventManager();
    SceneManagerEx _scene = new SceneManagerEx();

    // 전역 게임 시간
    public static float GameTime { get; set; } = 0;
    public static bool gameStop = false;

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            _instance._sound.Init();
            _instance._pool.Init();
            _instance._data.Init();
        }
    }

    public static void GamePause() { Time.timeScale = 0; gameStop = true; }
    public static void GamePlay() { Time.timeScale = 1; gameStop = false; }
}
```

**핵심 개념**:
- **DontDestroyOnLoad**: 씬 전환 시에도 매니저 유지
- **지연 초기화**: 첫 접근 시 자동 생성
- **전역 시간 관리**: `GameTime`으로 게임 진행 시간 추적
- **일시정지 시스템**: `GamePause()`/`GamePlay()`로 timeScale 제어

---

### 2. 게임 매니저 (`GameManagerEx.cs`)

**플레이어/적 스폰 및 관리**

```csharp
public class GameManagerEx
{
    GameObject _player;
    HashSet<GameObject> _monster = new HashSet<GameObject>();
    public Action<int> _OnSpawnEvent;

    public Data.Player StartPlayer { get; set; } = new Data.Player();
    public Vector3 MousePos { get; set; }
    public Vector3 WorldMousePos { get; set; }

    public GameObject Spawn(Define.WorldObject type, string path, Transform parent = null)
    {
        GameObject go = Managers.Resource.Instantiate(path, parent);

        switch (type)
        {
            case Define.WorldObject.Enemy:
                _monster.Add(go);
                _OnSpawnEvent.Invoke(1);  // 적 카운트 증가
                break;
            case Define.WorldObject.Player:
                _player = go;
                break;
        }
        return go;
    }

    public void Despawn(GameObject go, float time = 0)
    {
        Define.WorldObject type = GetWorldObjectType(go);

        switch (type)
        {
            case Define.WorldObject.Enemy:
                if (_monster.Contains(go))
                {
                    _monster.Remove(go);
                    _OnSpawnEvent.Invoke(-1);  // 적 카운트 감소
                }
                break;
        }
        Managers.Resource.Destroy(go, time);
    }
}
```

**핵심 기능**:
- **오브젝트 생명주기 관리**: Spawn/Despawn으로 생성/제거
- **적 카운트 추적**: HashSet으로 적 관리, 이벤트로 UI 업데이트
- **마우스 위치 추적**: 월드 좌표로 변환하여 제공

---

### 3. 데이터 매니저 (`DataManager.cs`)

**JSON 기반 데이터 로딩**

```csharp
public class DataManager
{
    public Dictionary<int, Data.WeaponData> WeaponData { get; private set; }
    public Dictionary<int, Data.Player> PlayerData { get; private set; }
    public Dictionary<int, Data.Monster> MonsterData { get; private set; }

    public void Init()
    {
        PlayerData = LoadJson<Data.PlayerData, int, Data.Player>("PlayerData").MakeDict();
        WeaponData = LoadJson<Data.WeaponDataLoader, int, Data.WeaponData>("WeaponData").MakeDict();
        MonsterData = LoadJson<Data.MonsterData, int, Data.Monster>("MonsterData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
```

**데이터 구조** (`Data.Contents.cs`):

```csharp
namespace Data
{
    [Serializable]
    public class Player
    {
        public int id;
        public string name;
        public int weaponID;       // 시작 무기
        public int maxHp;
        public int damage;
        public int defense;
        public float moveSpeed;
        public int coolDown;       // 쿨타임 감소 %
        public int amount;         // 발사체 개수 증가
    }

    [Serializable]
    public class Monster
    {
        public int id;
        public string name;
        public int maxHp;
        public int damage;
        public int defense;
        public float moveSpeed;
        public int expMul;         // 경험치 배율 (1~3배)
    }

    [Serializable]
    public class WeaponData
    {
        public int weaponID;
        public string weaponName;
        public List<WeaponLevelData> weaponLevelData;  // 레벨별 데이터
    }

    [Serializable]
    public class WeaponLevelData
    {
        public int level;          // 1~5 레벨
        public int damage;
        public float movSpeed;     // 투사체 속도
        public float force;        // 넉백 힘
        public float cooldown;
        public float size;         // 크기 배율
        public int penetrate;      // 관통 횟수
        public int countPerCreate; // 생성 개수
    }
}
```

**핵심 개념**:
- **JSON 직렬화**: Unity JsonUtility 사용
- **ILoader 인터페이스**: 데이터를 Dictionary로 변환
- **Resources 폴더**: `Resources/Data/` 경로에서 로드

---

## 게임 메커니즘

### 1. 플레이어 시스템 (`PlayerController.cs`)

**입력 및 이동**

```csharp
public class PlayerController : BaseController
{
    protected PlayerStat _stat;
    Vector2 _inputVec;
    public Vector2 _lastDirVec = new Vector2(1, 0);  // 마지막 이동 방향 (무기용)

    bool _isDamaged = false;
    float _invincibility_time = 0.2f;  // 무적 시간

    void Update()
    {
        _inputVec.x = Input.GetAxisRaw("Horizontal");
        _inputVec.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector2 nextVec = _inputVec.normalized * (_stat.MoveSpeed * Time.fixedDeltaTime);
        _rigid.MovePosition(_rigid.position + nextVec);

        if (_inputVec.normalized.magnitude != 0)
        {
            _lastDirVec = _inputVec.normalized;  // 무기 발사 방향 저장
        }
    }

    public void Init(Data.Player playerData)
    {
        _anime.runtimeAnimatorController = animeCon[playerData.id - 1];
        _stat.MaxHP = playerData.maxHp;
        _stat.HP = playerData.maxHp;
        _stat.Damage = playerData.damage;
        _stat.Defense = playerData.defense;
        _stat.MoveSpeed = playerData.moveSpeed;
        _stat.Cooldown = playerData.coolDown;
        _stat.Amount = playerData.amount;
        _stat.AddOrSetWeaponDict((Define.Weapons)playerData.weaponID, 1);  // 시작 무기
    }

    public void OnDamaged(Collision2D collision)
    {
        EnemyStat enemyStat = collision.transform.GetComponent<EnemyStat>();
        _stat.HP -= Mathf.Max(enemyStat.Damage - _stat.Defense, 1);  // 최소 1 데미지

        if (_stat.HP <= 0)
            OnDead();
    }

    IEnumerator OnDamagedColor()
    {
        _sprite.color = Color.red;
        yield return new WaitForSeconds(_invincibility_time);
        _isDamaged = false;
        _sprite.color = Color.white;
    }
}
```

**핵심 메커니즘**:
- **정규화된 이동**: 대각선 이동 시 속도 균등화
- **마지막 방향 저장**: 정지 시에도 무기 발사 방향 유지
- **무적 시간**: 0.2초 무적 + 색상 변경으로 피드백
- **최소 데미지**: 방어력이 높아도 최소 1 데미지 보장

---

### 2. 적 시스템 (`EnemyController.cs`)

**AI 및 전투**

```csharp
public class EnemyController : BaseController
{
    protected EnemyStat _stat;
    public Rigidbody2D _target;  // 플레이어 추적
    bool _isLive = true;
    bool _isRange = false;       // 원거리 공격 여부
    bool _isAttack = false;

    private void FixedUpdate()
    {
        if (!_isLive) return;

        OnMove();

        if (_isRange && !_isAttack)
        {
            StartCoroutine(RangeAttack());
        }
    }

    void OnMove()
    {
        Vector2 dirVec = _target.position - _rigid.position;
        Vector2 nextVec = dirVec.normalized * (_stat.MoveSpeed * Time.fixedDeltaTime);
        _rigid.MovePosition(_rigid.position + nextVec);
        _rigid.velocity = Vector2.zero;  // 관성 제거
    }

    IEnumerator RangeAttack()
    {
        _isAttack = true;
        SpawnBullet();
        yield return new WaitForSeconds(2f);
        _isAttack = false;
    }

    void SpawnBullet()
    {
        EnemyBullet bullet = Managers.Resource.Instantiate("Weapon/EnemyBullet", _rigid.position)
            .GetOrAddComponent<EnemyBullet>();
        bullet._damage = _stat.Damage;
        bullet._speed = 5f;
        bullet._dir = (_target.position - _rigid.position).normalized;
    }

    public void Init(Data.Monster monsterStat, int level, Define.MonsterType type)
    {
        int mul = 1;
        switch (type)
        {
            case Define.MonsterType.Enemy:
                mul = 1;
                break;
            case Define.MonsterType.middleBoss:
                mul = 50;  // 중간 보스는 50배 강함
                transform.localScale = Vector3.one * 2;  // 크기 2배
                break;
        }

        _anime.runtimeAnimatorController = _animeCon[monsterStat.id-1];
        _isRange = (monsterStat.id == 5);  // 5번 몬스터는 원거리

        _stat.MonsterStyle = (Define.MonsterStyle)Enum.Parse(typeof(Define.MonsterStyle), monsterStat.name);
        _stat.MonsterType = type;
        _stat.MoveSpeed = monsterStat.moveSpeed * ((100f + level) / 100f);
        _stat.MaxHP = SetRandomStat((int)(monsterStat.maxHp * ((100f + 10f * level) / 100f))) * mul;
        _stat.HP = _stat.MaxHP;
        _stat.Damage = SetRandomStat((int)(monsterStat.damage * ((100f + level) / 100f)));
        _stat.Defense = SetRandomStat((int)(monsterStat.defense * ((100f + level) / 100f)));
        _stat.ExpPoint = 10 * level;
        _stat.ExpMul = monsterStat.expMul;
    }

    int SetRandomStat(int value)
    {
        value = (int)(value * Random.Range(0.9f, 1.1f));  // ±10% 랜덤
        return value;
    }

    public override void OnDamaged(int damage, float force = 0)
    {
        _anime.SetTrigger("Hit");
        int calculateDamage = Mathf.Max(damage - _stat.Defense, 1);
        _stat.HP -= calculateDamage;
        _rigid.AddForce((_rigid.position - _target.position).normalized * (force * 200f));  // 넉백
        FloatDamageText(calculateDamage);  // 데미지 텍스트 표시

        OnDead();
    }

    public override void OnDead()
    {
        if(_stat.HP <= 0)
        {
            _isLive = false;
            SpawnExp();  // 경험치 드롭
            Managers.Event.DropItem(_stat, transform);  // 아이템 드롭
            Managers.Game.Despawn(gameObject);
        }
    }

    void SpawnExp()
    {
        GameObject expGo = Managers.Game.Spawn(Define.WorldObject.Unknown, "Content/Exp");
        expGo.transform.position = transform.position;
        Exp_Item expPoint = expGo.GetComponent<Exp_Item>();
        expPoint.SetExp(_stat.ExpPoint, _stat.ExpMul);

        // ExpMul에 따라 스프라이트 변경 (1배: 작음, 2배: 중간, 3배: 큼)
        if (expPoint._expMul == 1)
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[0];
        else if(expPoint._expMul == 2)
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[1];
        else
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[2];
    }
}
```

**핵심 메커니즘**:
- **플레이어 추적 AI**: 항상 플레이어를 향해 이동
- **레벨 스케일링**: 플레이어 레벨에 따라 스탯 증가
- **랜덤 스탯**: ±10% 변동으로 다양성 부여
- **중간 보스**: 일반 적의 50배 스탯, 크기 2배
- **원거리 공격**: 특정 몬스터는 2초마다 투사체 발사
- **넉백 시스템**: 데미지 시 뒤로 밀림
- **경험치 드롭**: ExpMul에 따라 다른 크기의 경험치

---

### 3. 적 스폰 시스템 (`Spawner.cs`)

**시간 기반 난이도 증가**

```csharp
public class Spawner : MonoBehaviour
{
    Dictionary<int, Data.Monster> _monsterStat;
    public Transform[] _spawnPoint;
    float _spawnTime = 0.5f;
    int _maxSpawnUnit = 50;  // 최대 동시 스폰 수
    public int enemyCount = 0;
    int timeLevel = 0;

    private void Update()
    {
        // 1분마다 timeLevel 증가
        if ((timeLevel + 1) * 60 < Managers.GameTime)
        {
            timeLevel = (int)Managers.GameTime / 60;
            if (timeLevel <= 5)
            {
                SpawnBoss(timeLevel);  // 1~5분에 중간 보스 스폰
            }
        }

        if (!_isSpawning)
            StartCoroutine(SpawnMonster());
    }

    void SpawnBoss(int timeLevel)
    {
        GameObject Boss = null;
        if (timeLevel < 5)
        {
            int level = Managers.Game.getPlayer().GetComponent<PlayerStat>().Level;
            Boss = Managers.Game.Spawn(Define.WorldObject.Enemy, "Monster/Enemy");
            Boss.GetComponent<EnemyController>().Init(_monsterStat[timeLevel], level, Define.MonsterType.middleBoss);
        }
        else
        {
            Boss = Managers.Game.Spawn(Define.WorldObject.Enemy, "Monster/Boss");  // 5분에 최종 보스
        }
        Boss.transform.position = _spawnPoint[Random.Range(1, _spawnPoint.Length)].position;
    }

    IEnumerator SpawnMonster()
    {
        _isSpawning = true;
        if (enemyCount < _maxSpawnUnit)
        {
            int monsterType = SetRandomMonster(timeLevel);
            int level = Managers.Game.getPlayer().GetComponent<PlayerStat>().Level;
            GameObject enemy = Managers.Game.Spawn(Define.WorldObject.Enemy, "Monster/Enemy");
            enemy.transform.position = _spawnPoint[Random.Range(1, _spawnPoint.Length)].position;
            enemy.GetComponent<EnemyController>().Init(_monsterStat[monsterType], level, Define.MonsterType.Enemy);
        }
        yield return new WaitForSeconds(_spawnTime);
        _isSpawning = false;
    }

    int SetRandomMonster(int timeLevel)
    {
        float rand1 = Random.Range(0, 100);
        float rand2 = Random.Range(0, 100);
        int rd = 1;

        if (rand1 < 50)
        {
            rd = (rand2 < 90 - (20 * timeLevel)) ? 1 : 2;  // 시간이 지날수록 엘리트 확률 증가
        }
        else if (rand1 < 90)
        {
            rd = (rand2 < 90 - (20 * timeLevel)) ? 3 : 4;
        }
        else
        {
            rd = 5;  // 원거리 공격 몬스터
        }

        return rd;
    }
}
```

**핵심 메커니즘**:
- **시간 기반 난이도**: 1분마다 중간 보스 스폰
- **동적 스폰 간격**: 3분 이후 0.1초로 단축
- **최대 스폰 제한**: 동시에 50마리까지만 존재
- **엘리트 확률 증가**: 시간이 지날수록 강한 적 확률 증가
- **5분 최종 보스**: 게임 클리어 조건

---

### 4. 무기 시스템 (`WeaponController.cs`)

**추상 무기 베이스 클래스**

```csharp
public abstract class WeaponController : MonoBehaviour
{
    protected GameObject _player = null;
    private PlayerStat _playerStat;
    private Dictionary<int, Data.WeaponData> _weaponData;
    private Dictionary<int, Data.WeaponLevelData> _weaponStat;

    public abstract int _weaponType { get; }  // 무기 ID (Define.Weapons enum)

    private int _level = 1;
    public int Level
    {
        get { return _level; }
        set
        {
            _level = value;
            SetWeaponStat();  // 레벨 변경 시 스탯 갱신
        }
    }

    // 무기 스탯
    public int _damage = 1;
    public float _movSpeed = 1;
    public float _force = 1;
    public float _cooldown = 1;
    public float _size = 1;
    public int _penetrate = 1;
    public int _countPerCreate = 1;

    void Awake()
    {
        _player = Managers.Game.getPlayer();
        _playerStat = _player.GetComponent<PlayerStat>();
        _weaponData = Managers.Data.WeaponData;
        _weaponStat = MakeLevelDataDict(_weaponType);
    }

    protected virtual void SetWeaponStat()
    {
        if (_level > 5) _level = 5;  // 최대 레벨 5

        // 플레이어 스탯과 무기 레벨 데이터 결합
        _damage = (int)(_weaponStat[_level].damage * ((100 + _playerStat.Damage) / 100f));
        _movSpeed = _weaponStat[_level].movSpeed;
        _force = _weaponStat[_level].force;
        _cooldown = _weaponStat[_level].cooldown * (100f / (100f + _playerStat.Cooldown));
        _size = _weaponStat[_level].size;
        _penetrate = _weaponStat[_level].penetrate;
        _countPerCreate = _weaponStat[_level].countPerCreate + _playerStat.Amount;
    }

    protected Dictionary<int, Data.WeaponLevelData> MakeLevelDataDict(int weaponID)
    {
        Dictionary<int, Data.WeaponLevelData> _weaponLevelData = new Dictionary<int, Data.WeaponLevelData>();
        foreach (Data.WeaponLevelData weaponLevelData in _weaponData[weaponID].weaponLevelData)
            _weaponLevelData.Add(weaponLevelData.level, weaponLevelData);
        return _weaponLevelData;
    }
}
```

**무기 종류 및 특징**:

| 무기 ID | 이름 | 타입 | 특징 |
|--------|------|------|------|
| 1 | Knife | 근접 회전 | 플레이어 주변 회전, 관통 공격 |
| 2 | Fireball | 투사체 | 직선 발사, 폭발 효과 |
| 3 | Spin | 근접 회전 | 플레이어 주위 원형 배치 |
| 4 | Poison | 투사체 | 독 장판 생성, 지속 데미지 |
| 101 | Lightning | 관통 투사체 | 가장 가까운 적 관통, 빠른 속도 |
| 102 | Shotgun | 산탄 투사체 | 여러 발 동시 발사 |

**핵심 메커니즘**:
- **레벨별 스탯**: JSON 데이터에서 레벨 1~5 스탯 로드
- **플레이어 스탯 연동**: 데미지%, 쿨타임%, 개수 보너스 적용
- **추상 클래스 패턴**: 각 무기는 상속받아 고유 로직 구현

---

### 5. 레벨업 시스템 (`UI_LevelUp.cs`)

**랜덤 강화 옵션 제시**

```csharp
public class UI_LevelUp : UI_Popup
{
    private int _maxUpgradeNum = 3;  // 3개 옵션 제시

    public override void Init()
    {
        base.Init();
        Managers.Sound.Play("LevelUp", Define.Sound.Effect);
        Bind<GameObject>(typeof(Panels));

        GameObject gridPanel = Get<GameObject>((int)Panels.GridPanel);

        // 기존 패널 제거
        foreach(Transform child in gridPanel.transform)
        {
            Managers.Resource.Destroy(child.gameObject);
        }

        PlayerStat player = Managers.Game.getPlayer().GetComponent<PlayerStat>();
        List<string[]> itemList = Managers.Event.SetRandomItem(player, 3);  // 랜덤 강화 옵션

        // 3개 강화 패널 생성
        for(int i = 0; i< _maxUpgradeNum; i++)
        {
            GameObject upgradePanel = Managers.UI.MakeSubItem<UpgdPanel>(parent: gridPanel.transform).gameObject;
            UpgdPanel upgradeDesc = upgradePanel.GetComponent<UpgdPanel>();
            upgradeDesc.SetData(itemList[i]);
            upgradeDesc.SetInfo(itemList[i][1], itemList[i][1]);
        }
    }
}
```

**강화 옵션 종류**:
- **새 무기 획득**: 보유하지 않은 무기 중 랜덤 선택
- **무기 레벨업**: 보유 중인 무기 레벨 +1 (최대 5)
- **스탯 강화**: HP, 공격력, 방어력, 이동속도, 쿨타임, 발사체 개수

**핵심 메커니즘**:
- **게임 일시정지**: 레벨업 UI 표시 시 `Managers.GamePause()`
- **동적 UI 생성**: 매번 새로운 옵션으로 패널 재생성
- **선택 시 즉시 적용**: 버튼 클릭 시 스탯/무기 즉시 반영

---

## 데이터 구조

### JSON 데이터 예시

**PlayerData.json**:
```json
{
  "_players": [
    {
      "id": 1,
      "name": "Farmer",
      "weaponID": 101,
      "maxHp": 100,
      "damage": 10,
      "defense": 0,
      "moveSpeed": 3.0,
      "coolDown": 0,
      "amount": 0
    },
    {
      "id": 2,
      "name": "Knight",
      "weaponID": 102,
      "maxHp": 120,
      "damage": 5,
      "defense": 5,
      "moveSpeed": 2.5,
      "coolDown": 10,
      "amount": 0
    }
  ]
}
```

**MonsterData.json**:
```json
{
  "_monsters": [
    {
      "id": 1,
      "name": "zombie",
      "maxHp": 50,
      "damage": 10,
      "defense": 0,
      "moveSpeed": 1.5,
      "expMul": 1
    },
    {
      "id": 2,
      "name": "zombieElite",
      "maxHp": 100,
      "damage": 20,
      "defense": 5,
      "moveSpeed": 2.0,
      "expMul": 2
    }
  ]
}
```

**WeaponData.json**:
```json
{
  "_weapons": [
    {
      "weaponID": 1,
      "weaponName": "Knife",
      "weaponLevelData": [
        {
          "level": 1,
          "damage": 10,
          "movSpeed": 5.0,
          "force": 1.0,
          "cooldown": 1.0,
          "size": 1.0,
          "penetrate": 1,
          "countPerCreate": 1
        },
        {
          "level": 2,
          "damage": 15,
          "movSpeed": 5.0,
          "force": 1.5,
          "cooldown": 0.9,
          "size": 1.1,
          "penetrate": 2,
          "countPerCreate": 2
        }
      ]
    }
  ]
}
```

---

## UI 시스템

### UI 계층 구조

```
UI_Base (추상 베이스)
├── UI_Scene (씬 UI)
│   ├── UI_Player (HUD - 체력, 경험치, 시간, 킬수)
│   └── UI_MainMenu (메인 메뉴)
│
└── UI_Popup (팝업 UI)
    ├── UI_LevelUp (레벨업 강화 선택)
    ├── UI_GameOver (게임 오버)
    ├── UI_GameVictory (게임 승리)
    ├── UI_CharacterSelect (캐릭터 선택)
    ├── UI_ItemBoxOpen (아이템 박스 열기)
    ├── UI_GameMenu (일시정지 메뉴)
    └── UI_TimeStop (시간 정지 아이템)
```

**서브 아이템**:
- `UpgdPanel`: 레벨업 강화 옵션 패널
- `WeaponInven`: 무기 인벤토리 슬롯 (아이콘 + 레벨)
- `PlayerInven`: 플레이어 정보 슬롯
- `StatInven`: 스탯 정보 슬롯

**월드 스페이스 UI**:
- `UI_HPBar`: 적 체력바
- `UI_DamageText`: 데미지 텍스트 (떠오르는 효과)

---

## 코드 참조 가이드

### 현재 프로젝트에 적용 시 고려사항

#### 1. 아키텍처 차이점

| 항목 | Undead Survivor | 현재 프로젝트 |
|------|----------------|--------------|
| 리소스 로딩 | Resources 폴더 | **Addressables** |
| 매니저 패턴 | 통합 싱글톤 (`Managers.cs`) | **개별 싱글톤** (`Singleton<T>`) |
| UI 시스템 | UIManager 동적 생성 | **UIManager + UIPanel** |
| 데이터 관리 | JSON + DataManager | **ScriptableObject 권장** |
| 씬 관리 | SceneManagerEx | **CustomSceneManager** |
| 오브젝트 풀링 | PoolManager | **PoolManager + ResourceManager 통합** |

#### 2. 통합 방법

**Step 1: 리소스 경로 변환**
```csharp
// Undead Survivor
Managers.Resource.Instantiate("Monster/Enemy");

// 현재 프로젝트 (Addressables)
ResourceManager.Instance.InstantiateAsync("Prefabs/Monster/UndeadSurvivor/Enemy", (instance) => {
    // 초기화
});
```

**Step 2: 매니저 접근 방식 변환**
```csharp
// Undead Survivor
Managers.Game.Spawn(...);
Managers.UI.ShowPopupUI<UI_LevelUp>();
Managers.Sound.Play("LevelUp", Define.Sound.Effect);

// 현재 프로젝트
MiniGameManager.Instance.LoadGame("UndeadSurvivor");
UIManager.Instance.ShowPanel<UndeadSurvivorUIPanel>();
SoundManager.Instance.PlaySFX("Audio/SFX/UndeadSurvivor/LevelUp");
```

**Step 3: 데이터 구조 통합**
```csharp
// Undead Survivor (JSON)
public class DataManager
{
    public Dictionary<int, Data.Monster> MonsterData { get; private set; }

    public void Init()
    {
        MonsterData = LoadJson<Data.MonsterData, int, Data.Monster>("MonsterData").MakeDict();
    }
}

// 현재 프로젝트 (ScriptableObject 권장)
[CreateAssetMenu(fileName = "MonsterData", menuName = "UndeadSurvivor/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    public List<MonsterData> monsters;

    private Dictionary<int, MonsterData> _monsterDict;

    public void Initialize()
    {
        _monsterDict = new Dictionary<int, MonsterData>();
        foreach (var monster in monsters)
            _monsterDict.Add(monster.id, monster);
    }

    public MonsterData GetMonster(int id) => _monsterDict[id];
}
```

**Step 4: IMiniGame 인터페이스 구현**
```csharp
public class UndeadSurvivorGameData : IGameData
{
    public int HighScore { get; set; }
    public int CurrentScore { get; set; }
    public float SurviveTime { get; set; }
    public int KillCount { get; set; }

    public void Initialize()
    {
        HighScore = PlayerPrefs.GetInt("UndeadSurvivor_HighScore", 0);
        CurrentScore = 0;
        SurviveTime = 0f;
        KillCount = 0;
    }

    public void Reset()
    {
        CurrentScore = 0;
        SurviveTime = 0f;
        KillCount = 0;
    }

    public bool Validate() => true;

    public void SaveState()
    {
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            PlayerPrefs.SetInt("UndeadSurvivor_HighScore", HighScore);
        }
        PlayerPrefs.Save();
    }

    public void LoadState() { Initialize(); }
}

public class UndeadSurvivorGame : IMiniGame
{
    private UndeadSurvivorGameData _gameData;
    private CommonPlayerData _commonData;

    private GameObject _player;
    private UndeadSurvivorSpawner _spawner;

    public void Initialize(CommonPlayerData commonData)
    {
        _commonData = commonData;
        _gameData = new UndeadSurvivorGameData();
        _gameData.Initialize();

        // 플레이어 스폰
        ResourceManager.Instance.InstantiateAsync("Prefabs/Player/UndeadSurvivor/Player", (player) => {
            _player = player;
            _player.GetComponent<UndeadSurvivorPlayer>().Init(_commonData);
        });

        // 스포너 초기화
        _spawner = new GameObject("Spawner").AddComponent<UndeadSurvivorSpawner>();
        _spawner.Initialize();
    }

    public void StartGame()
    {
        // UI 로드
        UIManager.Instance.ShowPanel<UndeadSurvivorHUD>();

        // BGM 재생
        SoundManager.Instance.PlayBGM("Audio/BGM/UndeadSurvivor/BGM_01");

        // 입력 이벤트 구독
        InputManager.Instance.OnInputEvent += HandleInput;
    }

    public void Update(float deltaTime)
    {
        _gameData.SurviveTime += deltaTime;

        // 시간 기반 보스 스폰 체크
        _spawner.CheckBossSpawn(_gameData.SurviveTime);
    }

    public void Cleanup()
    {
        // 입력 이벤트 해제
        InputManager.Instance.OnInputEvent -= HandleInput;

        // 리소스 정리
        if (_player != null)
            ResourceManager.Instance.ReleaseInstance(_player);

        // 데이터 저장
        _gameData.SaveState();
    }

    public IGameData GetData() => _gameData;

    private void HandleInput(InputEventData inputData)
    {
        if (_player == null) return;

        _player.GetComponent<UndeadSurvivorPlayer>().HandleInput(inputData);
    }
}
```

**Step 5: 게임 등록**
```csharp
// GameRegistry에 등록
public class GameRegistry : Singleton<GameRegistry>
{
    protected override void Awake()
    {
        base.Awake();
        RegisterGames();
    }

    private void RegisterGames()
    {
        RegisterGame("Tetris", () => new TetrisGame());
        RegisterGame("UndeadSurvivor", () => new UndeadSurvivorGame());  // 추가
    }
}

// GamePlayList에 추가
[CreateAssetMenu(fileName = "GamePlayList", menuName = "Game/PlayList")]
public class GamePlayList : ScriptableObject
{
    public List<GameInfo> games = new List<GameInfo>
    {
        new GameInfo { gameID = "Tetris", gameName = "테트리스", isPlayable = true },
        new GameInfo { gameID = "UndeadSurvivor", gameName = "언데드 서바이버", isPlayable = true }  // 추가
    };
}
```

#### 3. 핵심 시스템별 참조 우선순위

**높은 우선순위 (그대로 참조)**:
1. ✅ **플레이어 이동 시스템**: `PlayerController.cs` - 정규화된 입력, 마지막 방향 저장
2. ✅ **적 AI 시스템**: `EnemyController.cs` - 플레이어 추적, 레벨 스케일링
3. ✅ **무기 시스템 구조**: `WeaponController.cs` - 추상 베이스 클래스 패턴
4. ✅ **스폰 시스템**: `Spawner.cs` - 시간 기반 난이도, 보스 스폰 로직
5. ✅ **레벨업 시스템**: `UI_LevelUp.cs` - 랜덤 강화 옵션 제시

**중간 우선순위 (수정 필요)**:
1. ⚠️ **데이터 구조**: JSON → ScriptableObject 변환
2. ⚠️ **리소스 로딩**: Resources → Addressables 변환
3. ⚠️ **매니저 접근**: `Managers.XXX` → `XXXManager.Instance`

**낮은 우선순위 (재설계 권장)**:
1. ❌ **통합 매니저 패턴**: 현재 프로젝트의 개별 매니저 유지
2. ❌ **UI 생성 방식**: 현재 프로젝트의 UIManager 시스템 사용
3. ❌ **씬 관리**: CustomSceneManager 활용

---

## 추가 참고 사항

### 성능 최적화 포인트

1. **오브젝트 풀링**:
   - 적, 투사체, 경험치, 데미지 텍스트는 모두 풀링 사용
   - 최대 50마리 동시 스폰 제한으로 메모리 관리

2. **무한 맵 구현**:
   - `RePosition.cs`: 타일이 화면 밖으로 나가면 반대편으로 재배치
   - 3x3 타일로 무한 맵 구현

3. **프레임 최적화**:
   - `FixedUpdate`에서 물리 계산
   - `Update`에서 입력 처리
   - `LateUpdate`에서 애니메이션/렌더링

### 밸런싱 참조

**난이도 곡선**:
- 0~1분: 일반 적 위주
- 1~2분: 엘리트 확률 증가 + 첫 중간 보스
- 2~3분: 스폰 속도 증가
- 3~4분: 엘리트 비율 대폭 증가
- 4~5분: 원거리 공격 적 증가
- 5분: 최종 보스 (게임 클리어 조건)

**레벨 스케일링**:
- 몬스터 체력: `baseHP * (1 + 0.1 * level)`
- 몬스터 공격력: `baseDamage * (1 + 0.01 * level)`
- 중간 보스 배율: 일반 적의 50배

**무기 밸런싱**:
- 레벨 1~5: 데미지 10 → 15 → 20 → 30 → 50
- 쿨타임: 1.0초 → 0.9초 → 0.8초 → 0.7초 → 0.5초
- 발사체 개수: 1 → 2 → 3 → 4 → 5

---

## 결론

이 문서는 Undead Survivor 프로젝트의 핵심 시스템과 메커니즘을 분석하여, 현재 프로젝트에 통합할 때 참조할 수 있도록 작성되었습니다.

**핵심 참조 포인트**:
1. ✅ **게임 루프**: 시간 기반 난이도 증가, 보스 스폰 타이밍
2. ✅ **전투 시스템**: 플레이어/적 상호작용, 데미지 계산, 넉백
3. ✅ **무기 시스템**: 추상 베이스 클래스를 통한 확장 가능한 구조
4. ✅ **레벨업 시스템**: 랜덤 강화 옵션으로 다양한 빌드 지원
5. ✅ **데이터 구조**: JSON 기반 밸런싱 (ScriptableObject로 변환 권장)

**통합 시 주의사항**:
- Addressables 경로 변환 필수
- IMiniGame 인터페이스 구현 필수
- 현재 프로젝트의 매니저 시스템 활용
- ScriptableObject로 데이터 관리 권장

---

**문서 버전**: 1.0
**마지막 업데이트**: 2025-10-25
