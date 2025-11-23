# Undead Survivor 테스트 씬 구성 가이드

**작성일**: 2025-11-05
**Phase**: Phase 1 - 플레이어 시스템 테스트

---

## 📋 개요

Phase 1에서 구현한 플레이어 시스템(이동, 체력, 경험치, 무기 관리)을 Undead Survivor 씬에서 테스트하기 위한 Unity 에디터 설정 가이드입니다.

---

## 🎯 구현된 시스템 (Phase 1 완료)

### ✅ 플레이어 시스템
1. **Player.cs** - 통합 컴포넌트 (417 lines)
2. **PlayerController.cs** - WASD 이동 (111 lines)
3. **PlayerHealth.cs** - 체력 관리 (190 lines)
4. **PlayerExperience.cs** - 경험치 & 레벨업 (163 lines)
5. **PlayerWeaponManager.cs** - 무기 관리 (211 lines)

### ✅ 입력 시스템
6. **UndeadSurvivorInputAdapter.cs** - 게임 전용 입력 어댑터 (166 lines)
7. **UndeadSurvivorInputEventData.cs** - 입력 데이터 구조 (64 lines)
8. **UndeadSurvivorInputType.cs** - 입력 타입 enum (18 lines)

### ✅ 데이터 시스템
9. **CharacterStat.cs** - 스탯 관리 (확장 완료)
10. **CharacterData.cs** - 캐릭터 기본 정보
11. **CharacterDataList.cs** - JSON 로더
12. **CharacterData.json** - Knight, Mage 데이터
13. **UndeadSurvivorDataProvider.cs** - 데이터 제공자

---

## 🛠️ Unity 씬 설정 단계

### 1. InputAdapter GameObject 생성

**목적**: InputManager의 입력을 게임 전용 입력으로 변환

1. Unity 에디터에서 `Undead Survivor.unity` 씬 열기
2. Hierarchy에서 우클릭 → `Create Empty`
3. 이름을 `InputAdapter`로 변경
4. Inspector에서 `Add Component` → `UndeadSurvivorInputAdapter` 추가

**설정 확인**:
```
InputAdapter (GameObject)
└── UndeadSurvivorInputAdapter (Component)
    ├── Pause Key: Escape
    ├── Dash Key: Space
    ├── Special Skill Key: Q
    └── Level Up Confirm Key: Return
```

---

### 2. Player GameObject 생성

**목적**: 플레이어 통합 시스템 구성

#### 2-1. GameObject 생성
1. Hierarchy에서 우클릭 → `Create Empty`
2. 이름을 `Player`로 변경
3. Transform 위치를 `(0, 0, 0)`으로 설정

#### 2-2. 필수 컴포넌트 추가 (순서대로)
1. `Rigidbody2D` 추가
   - Body Type: Dynamic
   - Gravity Scale: 0 (2D 탑뷰이므로 중력 없음)
   - Constraints → Freeze Rotation Z: ✅ (회전 방지)

2. `CircleCollider2D` 또는 `BoxCollider2D` 추가
   - Radius: 0.5 (또는 적절한 크기)

3. **Player 시스템 컴포넌트 추가** (Add Component로 하나씩):
   - `PlayerController`
   - `PlayerHealth`
   - `PlayerExperience`
   - `PlayerWeaponManager`
   - `Player` (통합 컴포넌트, 마지막에 추가)

#### 2-3. PlayerController 설정
- **Input Adapter**: Hierarchy의 `InputAdapter` GameObject를 드래그 앤 드롭
- **Move Speed**: 5 (기본값)

#### 2-4. 시각적 표현 추가 (임시)
1. Player GameObject의 자식으로 `Sprite` 추가
   - Hierarchy에서 Player 우클릭 → `2D Object → Sprite`
   - 이름을 `PlayerSprite`로 변경
   - Sprite Renderer → Sprite: Circle (또는 적절한 스프라이트)
   - Color: 흰색 또는 원하는 색상

**최종 구조**:
```
Player (GameObject)
├── Rigidbody2D
├── CircleCollider2D
├── PlayerController
│   └── Input Adapter: InputAdapter
├── PlayerHealth
├── PlayerExperience
├── PlayerWeaponManager
├── Player
└── PlayerSprite (자식 GameObject)
    └── Sprite Renderer
```

---

### 3. 카메라 설정

1. Main Camera 선택
2. Transform Position: `(0, 0, -10)`
3. Camera → Projection: Orthographic
4. Size: 5 (또는 적절한 크기)
5. Background: 검은색

---

### 4. DataManager 초기화 (스크립트 필요)

Player가 CharacterData를 로드하려면 DataManager가 초기화되어 있어야 합니다.

#### 4-1. GameManager GameObject 생성 (임시)

1. Hierarchy에서 우클릭 → `Create Empty`
2. 이름을 `GameManager`로 변경
3. 다음 스크립트를 추가:

**임시 TestGameManager.cs 생성**:
```csharp
using UnityEngine;

namespace UndeadSurvivor
{
    public class TestGameManager : MonoBehaviour
    {
        [SerializeField] private int _testCharacterId = 1; // 1: Knight, 2: Mage

        private Player _player;
        private UndeadSurvivorDataProvider _dataProvider;

        private void Start()
        {
            // DataManager에 UndeadSurvivorDataProvider 등록
            _dataProvider = new UndeadSurvivorDataProvider();
            _dataProvider.Initialize();
            DataManager.Instance.RegisterProvider(_dataProvider);

            // 게임 데이터 로드
            DataManager.Instance.LoadGameData("UndeadSurvivor");

            // Player 찾기 및 초기화
            _player = FindObjectOfType<Player>();
            if (_player != null)
            {
                var characterData = _dataProvider.GetCharacterData(_testCharacterId);
                if (characterData != null)
                {
                    _player.Initialize(characterData);
                    Debug.Log($"[INFO] TestGameManager - Player initialized with {characterData.Name}");
                }
                else
                {
                    Debug.LogError($"[ERROR] TestGameManager - Character ID {_testCharacterId} not found");
                }
            }
        }

        private void OnDestroy()
        {
            // 게임 데이터 언로드
            if (DataManager.Instance != null)
            {
                DataManager.Instance.UnloadGameData("UndeadSurvivor");
            }
        }

        // 테스트용 메서드들 (Inspector에서 호출 가능)
        [ContextMenu("Damage Player 10")]
        private void TestDamage()
        {
            if (_player != null)
            {
                _player.TakeDamage(10f);
            }
        }

        [ContextMenu("Heal Player 20")]
        private void TestHeal()
        {
            if (_player != null)
            {
                _player.Heal(20f);
            }
        }

        [ContextMenu("Gain 50 Exp")]
        private void TestGainExp()
        {
            if (_player != null)
            {
                _player.GainExp(50);
            }
        }

        [ContextMenu("Level Up (1000 Exp)")]
        private void TestLevelUp()
        {
            if (_player != null)
            {
                _player.GainExp(1000);
            }
        }
    }
}
```

4. GameManager GameObject에 `TestGameManager` 컴포넌트 추가
5. Inspector에서 `Test Character Id` 설정:
   - `1`: Knight (체력 120, 방어력 2, 이동속도 4.5)
   - `2`: Mage (체력 80, 공격력 +10%, 쿨다운 -5%, 이동속도 5.0)

---

## 🎮 테스트 시나리오

### 1. 이동 테스트
1. Play 버튼 클릭
2. **WASD** 키로 플레이어 이동
3. 대각선 이동 시 속도가 일정한지 확인
4. Console에서 `[INFO] UndeadSurvivor::PlayerController` 로그 확인

**예상 결과**:
- Player가 WASD 입력에 따라 부드럽게 이동
- 대각선 이동 시 속도 정규화 (빠르지 않음)
- InputAdapter가 입력을 정상적으로 변환

---

### 2. 체력 테스트
1. Play 모드에서 GameManager 선택
2. Inspector에서 우클릭 → `Damage Player 10` 선택
3. Console에서 체력 변경 로그 확인
4. `Heal Player 20` 선택하여 회복 확인

**예상 결과**:
```
[INFO] UndeadSurvivor::PlayerHealth::TakeDamage - Took 8.0 damage (120.0 → 112.0)
[INFO] UndeadSurvivor::PlayerHealth::Heal - Healed 20.0 HP (112.0 → 120.0)
```

- Knight: 방어력 2이므로 10 피해 → 8 피해
- Mage: 방어력 0이므로 10 피해 그대로

---

### 3. 경험치 & 레벨업 테스트
1. GameManager → `Gain 50 Exp` 여러 번 클릭
2. 레벨업 시 Console 로그 확인
3. `Level Up (1000 Exp)` 클릭하여 다중 레벨업 확인

**예상 결과**:
```
[INFO] UndeadSurvivor::PlayerExperience::GainExp - Gained 50 exp (50/100, Level 1)
[INFO] UndeadSurvivor::PlayerExperience::CheckLevelUp - Level up! New level: 2
[INFO] UndeadSurvivor::Player::HandleLevelUp - Level up to 2
```

- 레벨 1 → 2: 100 경험치 필요
- 레벨 2 → 3: 120 경험치 필요 (1.2배 증가)
- 레벨업 시 이동 멈춤 (PlayerController::SetMovementEnabled(false))

---

### 4. 무기 관리 테스트 (추후)
현재는 WeaponData가 완전히 준비되지 않았으므로 무기 테스트는 Phase 3에서 진행

---

## 📊 예상 Console 로그 (정상 동작 시)

### 게임 시작 시:
```
[INFO] UndeadSurvivor::DataProvider::Initialize - Data provider initialized
[INFO] UndeadSurvivor::DataProvider::LoadData - Loading data
[INFO] UndeadSurvivor::DataProvider::LoadCharacterData - Loaded 2 characters from JSON
[INFO] UndeadSurvivor::DataProvider::LoadData - Data loaded successfully
[INFO] UndeadSurvivor::CharacterStat::Initialize - Initialized with Knight: HP=120, Speed=4.5, Damage=0%, Defense=2
[INFO] UndeadSurvivor::Player::ApplyStatsToComponents - Stats applied: HP=120, MoveSpeed=4.5, ExpMultiplier=1
[INFO] UndeadSurvivor::Player::Initialize - Player initialized with character: Knight
[INFO] TestGameManager - Player initialized with Knight
```

### WASD 이동 시:
```
[INFO] UndeadSurvivor::InputAdapter::UpdateMoveDirection - Move direction updated: (0.7, 0.7)
```

### 피격 시:
```
[INFO] UndeadSurvivor::PlayerHealth::TakeDamage - Took 8.0 damage (120.0 → 112.0)
[INFO] UndeadSurvivor::Player::HandleHealthChanged - Health changed: 112.0/120.0
```

### 레벨업 시:
```
[INFO] UndeadSurvivor::PlayerExperience::CheckLevelUp - Level up! New level: 2
[INFO] UndeadSurvivor::Player::HandleLevelUp - Level up to 2
[INFO] UndeadSurvivor::PlayerController::SetMovementEnabled - Movement disabled
```

---

## ⚠️ 문제 해결

### 1. Player가 이동하지 않음
- InputAdapter GameObject가 씬에 있는지 확인
- PlayerController의 Input Adapter 필드에 InputAdapter가 할당되었는지 확인
- Rigidbody2D의 Body Type이 Dynamic인지 확인
- Rigidbody2D의 Constraints → Freeze Position이 체크되지 않았는지 확인

### 2. "CharacterData.json not found" 오류
- `Assets/Resources/Data/UndeadSurvivor/Characters/CharacterData.json` 파일이 존재하는지 확인
- 파일 경로가 정확한지 확인 (Resources 폴더 하위여야 함)

### 3. "UndeadSurvivorDataProvider not found" 오류
- TestGameManager 스크립트가 GameManager GameObject에 추가되었는지 확인
- TestGameManager의 Start() 메서드에서 DataProvider를 등록하는지 확인

### 4. 레벨업 후 이동 안 됨
- 정상 동작입니다. 레벨업 UI가 구현되면 선택 후 `Player.ResumeMovement()` 호출 필요
- 테스트용으로 GameManager에서 3초 후 자동으로 ResumeMovement 호출하도록 수정 가능

---

## 🎯 다음 단계 (Phase 2)

Phase 1 테스트 완료 후:
1. **적 스폰 시스템** 구현
2. **적 AI** (플레이어 추적) 구현
3. **적 스탯 스케일링** (시간 기반)
4. **오브젝트 풀링** (PoolManager 활용)
5. **MonsterData** 활용

---

## 📝 참고 문서

- **작업 진행 상황**: `Assets/Docs/UndeadSurvivor_Progress.md`
- **작업용 PRD**: `Assets/Docs/UndeadSurvivor_WorkPRD.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`
- **코딩 규칙**: `.claude/UNITY_CONVENTIONS.md`
