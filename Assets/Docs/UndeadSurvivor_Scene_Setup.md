# Undead Survivor 씬 구성 및 사전 작업 전체 정리 (3-씬 구조)

**작성일**: 2025-11-06
**업데이트**: 2025-11-06 (3-씬 구조로 재설계)
**목적**: Undead Survivor 씬을 Unity 에디터에서 완전히 구성하기 위한 종합 가이드
**Phase 1 상태**: ✅ 100% 완료 (코드 구현)
**Unity 씬 상태**: ⏳ 구성 대기

---

## ⚠️ 중요: 3-씬 구조

Undead Survivor는 **3개의 씬**으로 구성됩니다:

```
1. UndeadSurvivor (초기 화면)
   - 게임 시작, 설정, 게임 종료

2. UndeadSurvivorCharacterSelectionScene (캐릭터 선택)
   - 캐릭터 선택, 전투 시작, 이전으로

3. UndeadSurvivorGameScene (게임 플레이)
   - 실제 전투, 몬스터 스폰, 게임 오버
```

---

## 📋 목차

1. [씬 구조 및 흐름](#씬-구조-및-흐름)
2. [구현된 시스템 개요](#구현된-시스템-개요)
3. [씬별 상세 구성](#씬별-상세-구성)
4. [씬 간 데이터 전달](#씬-간-데이터-전달)
5. [리소스 파일 구조](#리소스-파일-구조)
6. [테스트 시나리오](#테스트-시나리오)
7. [문제 해결](#문제-해결)

---

## 🏗️ 씬 구조 및 흐름

### 씬 흐름도

```
MainMenuScene (플랫폼 메인 메뉴)
    ↓ "Undead Survivor" 선택

🎮 UndeadSurvivor (초기 화면)
   📍 SceneID.UndeadSurvivor = 4
   📄 UndeadSurvivorInitialScene.cs

   [UI 버튼]
   - 게임 시작 → UndeadSurvivorCharacterSelectionScene
   - 설정
   - 게임 종료 → MainMenuScene

    ↓ "게임 시작" 클릭

👤 UndeadSurvivorCharacterSelectionScene (캐릭터 선택)
   📍 SceneID.UndeadSurvivorCharacterSelectionScene = 5
   📄 UndeadSurvivorCharacterSelectScene.cs

   [UI 버튼]
   - 캐릭터 선택 (Knight / Mage)
   - 전투 시작 → UndeadSurvivorGameScene
   - 이전으로 → UndeadSurvivor

    ↓ "전투 시작" 클릭 (캐릭터 ID 저장)

⚔️ UndeadSurvivorGameScene (게임 플레이)
   📍 SceneID.UndeadSurvivorGameScene = 6
   📄 UndeadSurvivorGameScene.cs (기존 UndeadSurvivorScene.cs)

   [게임 오버 후 UI 버튼]
   - 재시작 → 씬 재로드
   - 캐릭터 재선택 → UndeadSurvivorCharacterSelectionScene
   - 초기 화면 → UndeadSurvivor
   - 메인 메뉴 → MainMenuScene
```

---

## 🎯 구현된 시스템 개요

### Phase 1 완료 항목 (코드)

#### 1. 플레이어 시스템 (통합)
- **Player.cs** (417 lines) - 플레이어 통합 컴포넌트
  - CharacterData 기반 초기화
  - 4개 하위 컴포넌트 통합 관리
  - 11종 스탯 업그레이드 시스템
  - 이벤트 통합 관리

#### 2. 플레이어 하위 컴포넌트
- **PlayerController.cs** (111 lines) - WASD 이동
- **PlayerHealth.cs** (190 lines) - 체력, 피격, 회복
- **PlayerExperience.cs** (163 lines) - 경험치, 레벨업
- **PlayerWeaponManager.cs** (211 lines) - 무기 슬롯 관리

#### 3. 입력 시스템
- **UndeadSurvivorInputAdapter.cs** (166 lines) - 게임 전용 입력
- **UndeadSurvivorInputEventData.cs** (64 lines) - 입력 데이터
- **UndeadSurvivorInputType.cs** (18 lines) - 입력 타입 enum

#### 4. 데이터 시스템
- **CharacterStat.cs** - 11종 스탯 관리
- **CharacterData.cs** - 캐릭터 기본 정보
- **CharacterDataList.cs** - JSON 로더
- **CharacterData.json** - Knight, Mage 데이터
- **UndeadSurvivorDataProvider.cs** - DataManager 연동

#### 5. 게임 로직
- **UndeadSurvivorGame.cs** (315 lines) - IMiniGame 구현
- **UndeadSurvivorGameData.cs** - 게임 런타임 데이터
- **UndeadSurvivorScene.cs** (238 lines) - 씬 컨트롤러

---

## 🧩 필수 GameObject 및 컴포넌트

### 씬 구조 (Hierarchy)

```
Undead Survivor (Scene)
├── Managers (Empty GameObject)
│   └── [자동 생성] DataManager, InputManager, UIManager, etc.
│
├── SceneController (GameObject)
│   └── UndeadSurvivorScene (Component)
│
├── InputAdapter (GameObject)
│   └── UndeadSurvivorInputAdapter (Component)
│
├── Player (GameObject)
│   ├── Rigidbody2D
│   ├── CircleCollider2D
│   ├── PlayerController
│   ├── PlayerHealth
│   ├── PlayerExperience
│   ├── PlayerWeaponManager
│   ├── Player (통합 컴포넌트)
│   └── PlayerSprite (자식 GameObject)
│       └── SpriteRenderer
│
├── PlayerSpawnPoint (Empty GameObject)
│   └── Transform (0, 0, 0)
│
├── Camera (Main Camera)
│   └── Camera (Orthographic)
│
└── Canvas (UI - 추후 추가)
    └── [Phase 4] LevelUpPanel
```

---

## 🛠️ Unity 씬 구성 단계별 가이드

### **Step 1: 씬 생성 및 기본 설정**

#### 1-1. 씬 열기 또는 생성
1. Unity 에디터에서 `Assets/Scenes/Undead Survivor.unity` 열기
2. 없으면 새로 생성: `File → New Scene → 2D (URP)` → 저장

#### 1-2. Build Settings 등록
1. `File → Build Settings`
2. `Undead Survivor.unity` 씬을 Scenes In Build에 추가
3. Scene Index 확인

---

### **Step 2: SceneController 구성**

#### 2-1. GameObject 생성
1. Hierarchy 우클릭 → `Create Empty`
2. 이름: `SceneController`

#### 2-2. UndeadSurvivorScene 컴포넌트 추가
1. Inspector → `Add Component`
2. `UndeadSurvivorScene` 검색 후 추가

**역할**:
- BaseScene 상속으로 씬 생명주기 관리
- DataProvider 등록 및 로드
- MiniGameManager.LoadGame("UndeadSurvivor") 호출
- 게임 정리 및 언로드

---

### **Step 3: InputAdapter 구성**

#### 3-1. GameObject 생성
1. Hierarchy 우클릭 → `Create Empty`
2. 이름: `InputAdapter`

#### 3-2. UndeadSurvivorInputAdapter 추가
1. Inspector → `Add Component`
2. `UndeadSurvivorInputAdapter` 검색 후 추가

#### 3-3. 키 매핑 설정 (Inspector)
```
Pause Key: Escape
Dash Key: Space
Special Skill Key: Q
Level Up Confirm Key: Return
```

**역할**:
- InputManager의 KeyDown/KeyUp → 게임 전용 입력으로 변환
- WASD 동시 입력 처리 및 대각선 정규화
- 게임 전용 입력 이벤트 발생

---

### **Step 4: Player 구성**

#### 4-1. Player GameObject 생성
1. Hierarchy 우클릭 → `Create Empty`
2. 이름: `Player`
3. Transform Position: `(0, 0, 0)`

#### 4-2. 물리 컴포넌트 추가
**Rigidbody2D 추가**:
- Body Type: `Dynamic`
- Gravity Scale: `0` (탑뷰 2D)
- Constraints → Freeze Rotation Z: ✅

**CircleCollider2D 추가**:
- Radius: `0.5`

#### 4-3. Player 시스템 컴포넌트 추가 (순서 중요)
1. `PlayerController` 추가
2. `PlayerHealth` 추가
3. `PlayerExperience` 추가
4. `PlayerWeaponManager` 추가
5. **`Player`** 추가 (통합 컴포넌트, 마지막)

#### 4-4. PlayerController 설정
- **Input Adapter**: Hierarchy의 `InputAdapter` GameObject 드래그 앤 드롭
- **Move Speed**: `5.0` (CharacterData에서 오버라이드됨)

#### 4-5. 시각적 표현 추가 (임시)
1. Player 우클릭 → `2D Object → Sprites → Circle`
2. 이름: `PlayerSprite`
3. SpriteRenderer:
   - Sprite: `Circle` (기본 제공)
   - Color: 흰색
   - Sorting Layer: `Default`
   - Order in Layer: `1`

**최종 Player 구조**:
```
Player
├── Rigidbody2D
├── CircleCollider2D
├── PlayerController (Input Adapter: InputAdapter)
├── PlayerHealth
├── PlayerExperience
├── PlayerWeaponManager
├── Player
└── PlayerSprite (자식)
    └── SpriteRenderer
```

---

### **Step 5: PlayerSpawnPoint 구성**

1. Hierarchy 우클릭 → `Create Empty`
2. 이름: `PlayerSpawnPoint`
3. Transform Position: `(0, 0, 0)` (원하는 스폰 위치)

**역할**:
- UndeadSurvivorGame.SpawnPlayer()에서 스폰 위치로 사용
- 없으면 (0, 0, 0)으로 기본 설정됨

---

### **Step 6: Camera 설정**

#### Main Camera 설정
1. Main Camera 선택
2. Transform:
   - Position: `(0, 0, -10)`
3. Camera:
   - Projection: `Orthographic`
   - Size: `5`
   - Background: 검은색 `#000000`

#### (선택) CameraFollow 스크립트 추가
추후 플레이어 추적 카메라 구현 시 추가

---

### **Step 7: GameRegistry에 게임 등록**

#### 7-1. GameRegistry.cs 확인
경로: `Assets/Scripts/Core/GameRegistry.cs`

#### 7-2. RegisterDefaultGames() 메서드에 추가

**현재 코드**:
```csharp
private void RegisterDefaultGames()
{
    RegisterGame("Tetris", () => new TetrisGame());
    // UndeadSurvivor는 아직 미등록
}
```

**수정 필요**:
```csharp
private void RegisterDefaultGames()
{
    RegisterGame("Tetris", () => new TetrisGame());
    RegisterGame("UndeadSurvivor", () => new UndeadSurvivorGame());
}
```

**파일 수정 위치**: `Assets/Scripts/Core/GameRegistry.cs:XX`

---

### **Step 8: GamePlayList에 게임 추가**

#### 8-1. GamePlayList 에셋 찾기
경로: `Assets/Resources/Data/GamePlayList.asset` (ScriptableObject)

#### 8-2. Inspector에서 게임 정보 추가
1. GamePlayList 에셋 선택
2. Inspector → `Playable Games` 섹션
3. `+` 버튼 클릭
4. 다음 정보 입력:
   ```
   Game ID: UndeadSurvivor
   Display Name: Undead Survivor
   Is Playable: ✅
   ```

**역할**:
- MainMenuScene의 GameSelectUIPanel에서 동적으로 버튼 생성
- 게임 선택 가능 여부 제어

---

### **Step 9: 리소스 파일 배치**

#### 9-1. CharacterData.json 위치 확인
경로: `Assets/Resources/Data/UndeadSurvivor/Characters/CharacterData.json`

**내용**:
```json
{
  "characters": [
    {
      "id": 1,
      "name": "Knight",
      "maxHp": 120,
      "damage": 0,
      "cooldown": 0,
      "defense": 2,
      "moveSpeed": 4.5,
      "pickupRange": 0,
      "expMultiplier": 0,
      "projectileCount": 0,
      "projectileSpeed": 0,
      "duration": 0,
      "criticalChance": 0,
      "startWeaponId": 2
    },
    {
      "id": 2,
      "name": "Mage",
      "maxHp": 80,
      "damage": 10,
      "cooldown": -5,
      "defense": 0,
      "moveSpeed": 5.0,
      "pickupRange": 0,
      "expMultiplier": 0,
      "projectileCount": 0,
      "projectileSpeed": 0,
      "duration": 0,
      "criticalChance": 0,
      "startWeaponId": 1
    }
  ]
}
```

#### 9-2. 게임 아이콘 배치 (선택)
경로: `Assets/Resources/Sprites/UndeadSurvivor_icon.png`

**역할**:
- MainMenuScene의 게임 선택 버튼 아이콘
- Addressables 경로: `Sprite/UndeadSurvivor_icon`

---

## 🗂️ 리소스 파일 구조

### 필수 폴더 구조

```
Assets/
├── Resources/
│   ├── Data/
│   │   └── UndeadSurvivor/
│   │       └── Characters/
│   │           └── CharacterData.json ✅
│   │
│   ├── Prefabs/
│   │   ├── UI/
│   │   │   └── UndeadSurvivor/
│   │   ├── Weapon/
│   │   │   └── UndeadSurvivor/
│   │   ├── Monster/
│   │   │   └── UndeadSurvivor/
│   │   └── Player/
│   │       └── UndeadSurvivor/
│   │           └── Player.prefab (선택)
│   │
│   └── Sprites/
│       └── UndeadSurvivor/
│           └── UndeadSurvivor_icon.png (선택)
│
├── Scenes/
│   └── Undead Survivor.unity ✅
│
└── Scripts/
    ├── UndeadSurvivor/
    │   ├── Player.cs ✅
    │   ├── PlayerController.cs ✅
    │   ├── PlayerHealth.cs ✅
    │   ├── PlayerExperience.cs ✅
    │   ├── PlayerWeaponManager.cs ✅
    │   ├── UndeadSurvivorGame.cs ✅
    │   ├── UndeadSurvivorGameData.cs ✅
    │   ├── UndeadSurvivorInputAdapter.cs ✅
    │   ├── UndeadSurvivorInputEventData.cs ✅
    │   ├── UndeadSurvivorInputType.cs ✅
    │   ├── CharacterStat.cs ✅
    │   └── Data/
    │       └── UndeadSurvivorDataProvider.cs ✅
    │
    └── Scenes/
        └── UndeadSurvivorScene.cs ✅
```

---

## 🎮 테스트 시나리오

### 1. 씬 실행 테스트

#### 1-1. Play 버튼 클릭 전 체크리스트
- [ ] SceneController GameObject 존재
- [ ] InputAdapter GameObject 존재
- [ ] Player GameObject 완전히 구성됨
- [ ] PlayerController의 Input Adapter 필드 연결됨
- [ ] CharacterData.json 파일 존재
- [ ] GameRegistry에 UndeadSurvivor 등록됨

#### 1-2. Play 버튼 클릭

**예상 Console 로그**:
```
[INFO] CustomSceneManager::Awake - CustomSceneManager initialized
[INFO] MiniGameManager::Awake - Common player data initialized
[INFO] DataManager::RegisterProvider - Provider registered: UndeadSurvivor
[INFO] DataManager::LoadGameData - Loading data for UndeadSurvivor
[INFO] UndeadSurvivor::DataProvider::LoadData - Loading data
[INFO] UndeadSurvivor::DataProvider::LoadCharacterData - Loaded 2 characters from JSON
[INFO] UndeadSurvivor::DataProvider::LoadData - Data loaded successfully
[INFO] MiniGameManager::LoadGame - Loading data for game: UndeadSurvivor
[INFO] MiniGameManager::LoadGame - Game 'UndeadSurvivor' initialized
[INFO] UndeadSurvivor::Game::SpawnPlayer - Player spawned at (0, 0, 0) with character Knight
[INFO] MiniGameManager::LoadGame - Game 'UndeadSurvivor' started
[INFO] UndeadSurvivorScene::InitializeGame - Game initialized via MiniGameManager
```

---

### 2. 이동 테스트

#### 2-1. WASD 이동
1. **W** 키: 위로 이동
2. **A** 키: 왼쪽으로 이동
3. **S** 키: 아래로 이동
4. **D** 키: 오른쪽으로 이동
5. **WD** 동시 입력: 대각선 (정규화)

**예상 결과**:
- Player GameObject가 WASD 입력에 따라 부드럽게 이동
- 대각선 이동 속도가 직선과 동일 (정규화 적용)
- Console에 `[INFO] UndeadSurvivor::InputAdapter::UpdateMoveDirection` 로그

---

### 3. 스탯 확인 (Inspector)

Play 모드에서 Player GameObject 선택 후 확인:

**PlayerController**:
- Move Speed: CharacterData에 따라 동적 설정됨 (Knight: 4.5, Mage: 5.0)

**PlayerHealth**:
- Current Hp: CharacterData.MaxHp
- Max Hp: CharacterData.MaxHp (Knight: 120, Mage: 80)

**PlayerExperience**:
- Current Level: 1
- Current Exp: 0
- Exp For Next Level: 100

**Player (통합)**:
- Character Data: Knight 또는 Mage 정보 표시
- Character Stat: 11종 스탯 표시

---

### 4. 테스트 메서드 (TestGameManager 사용)

#### TestGameManager.cs 추가 (선택)

**경로**: `Assets/Scripts/UndeadSurvivor/TestGameManager.cs`

**사용법**:
1. Hierarchy에 `TestGameManager` GameObject 생성
2. TestGameManager 컴포넌트 추가
3. Play 모드에서 TestGameManager 우클릭:
   - `Damage Player 10`: 10 피해 입히기
   - `Heal Player 20`: 20 회복
   - `Gain 50 Exp`: 50 경험치 획득
   - `Level Up (1000 Exp)`: 즉시 레벨업

**Console 로그 예시**:
```
[INFO] UndeadSurvivor::PlayerHealth::TakeDamage - Took 8.0 damage (120.0 → 112.0)
[INFO] UndeadSurvivor::Player::HandleHealthChanged - Health changed: 112.0/120.0

[INFO] UndeadSurvivor::PlayerHealth::Heal - Healed 20.0 HP (112.0 → 120.0)

[INFO] UndeadSurvivor::PlayerExperience::GainExp - Gained 50 exp (50/100, Level 1)

[INFO] UndeadSurvivor::PlayerExperience::CheckLevelUp - Level up! New level: 2
[INFO] UndeadSurvivor::Player::HandleLevelUp - Level up to 2
[INFO] UndeadSurvivor::PlayerController::SetMovementEnabled - Movement disabled
```

---

## ⚠️ 문제 해결

### 문제 1: Player가 이동하지 않음

**원인**:
- InputAdapter GameObject가 씬에 없음
- PlayerController의 Input Adapter 필드 미연결
- Rigidbody2D 설정 오류

**해결**:
1. Hierarchy에서 `InputAdapter` GameObject 존재 확인
2. PlayerController Inspector → Input Adapter 필드에 InputAdapter 드래그
3. Rigidbody2D:
   - Body Type: `Dynamic`
   - Constraints → Freeze Position 체크 해제

---

### 문제 2: "CharacterData.json not found" 오류

**원인**:
- JSON 파일이 잘못된 경로에 있음
- Resources 폴더 구조 오류

**해결**:
1. 정확한 경로 확인:
   ```
   Assets/Resources/Data/UndeadSurvivor/Characters/CharacterData.json
   ```
2. Resources 폴더 하위에 있어야 함
3. 파일명 대소문자 확인 (정확히 `CharacterData.json`)

---

### 문제 3: "Failed to create game: UndeadSurvivor" 오류

**원인**:
- GameRegistry에 UndeadSurvivor 미등록

**해결**:
1. `Assets/Scripts/Core/GameRegistry.cs` 열기
2. `RegisterDefaultGames()` 메서드에 추가:
   ```csharp
   RegisterGame("UndeadSurvivor", () => new UndeadSurvivorGame());
   ```
3. using 구문 추가:
   ```csharp
   using UndeadSurvivor;
   ```

---

### 문제 4: "UndeadSurvivorDataProvider not loaded" 오류

**원인**:
- UndeadSurvivorScene.cs의 InitializeDataProvider() 미실행
- DataManager 초기화 실패

**해결**:
1. SceneController GameObject 존재 확인
2. UndeadSurvivorScene 컴포넌트 추가 확인
3. Console에서 DataManager 초기화 로그 확인

---

### 문제 5: 레벨업 후 이동 불가

**원인**:
- 정상 동작입니다.
- 레벨업 시 PlayerController.SetMovementEnabled(false) 호출됨

**해결**:
- Phase 4에서 레벨업 UI 구현 후 선택 완료 시 `Player.ResumeMovement()` 호출
- 현재는 테스트용으로 3초 후 자동 재개 추가 가능:
  ```csharp
  // Player.HandleLevelUp 메서드에 추가
  StartCoroutine(ResumeAfterDelay(3f));
  ```

---

### 문제 6: Player GameObject가 보이지 않음

**원인**:
- PlayerSprite 자식 GameObject 미생성
- SpriteRenderer 미설정

**해결**:
1. Player 자식으로 `PlayerSprite` 추가
2. SpriteRenderer 컴포넌트:
   - Sprite: Circle (Unity 기본 제공)
   - Color: 흰색
   - Sorting Layer: Default
   - Order in Layer: 1

---

## 🎯 다음 작업 (Phase 2)

### Phase 2: 적 시스템

씬 구성 완료 및 Phase 1 테스트 후 진행:

1. **Enemy.cs** - 적 베이스 클래스
   - 플레이어 추적 AI
   - 체력 관리
   - 피격 & 사망 처리

2. **EnemySpawner.cs** - 적 스폰 시스템
   - 시간 기반 스폰 주기
   - 플레이어 주변 랜덤 위치
   - 스폰 난이도 증가

3. **MonsterData.cs** - 적 데이터
   - 적 종류별 스탯
   - 시간 기반 스케일링

4. **PoolManager 연동** - 적 오브젝트 풀링
   - 성능 최적화 (60fps 유지)

---

## 📚 참조 문서

- **Phase 1 진행 상황**: `Assets/Docs/UndeadSurvivor_Progress.md`
- **테스트 가이드**: `Assets/Docs/UndeadSurvivor_TestScene_Guide.md`
- **작업용 PRD**: `Assets/Docs/UndeadSurvivor_WorkPRD.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`
- **코딩 규칙**: `.claude/UNITY_CONVENTIONS.md`
- **게임 선택 UI**: `Assets/Docs/GameSelectUI_Setup_Guide.md`

---

## ✅ 체크리스트 (씬 구성 완료 확인)

### GameObject 구성
- [ ] SceneController GameObject + UndeadSurvivorScene 컴포넌트
- [ ] InputAdapter GameObject + UndeadSurvivorInputAdapter 컴포넌트
- [ ] Player GameObject (완전 구성):
  - [ ] Rigidbody2D
  - [ ] CircleCollider2D
  - [ ] PlayerController (Input Adapter 연결)
  - [ ] PlayerHealth
  - [ ] PlayerExperience
  - [ ] PlayerWeaponManager
  - [ ] Player (통합)
  - [ ] PlayerSprite (자식)
- [ ] PlayerSpawnPoint GameObject
- [ ] Main Camera (Orthographic)

### 리소스 파일
- [ ] CharacterData.json 존재
- [ ] GameRegistry에 UndeadSurvivor 등록
- [ ] GamePlayList에 UndeadSurvivor 추가

### 테스트
- [ ] Play 시 오류 없음
- [ ] WASD 이동 정상 작동
- [ ] Console 로그 정상 출력
- [ ] Inspector에서 스탯 정상 표시

---

**구성 완료 시 다음 단계**: Phase 2 - 적 시스템 구현
