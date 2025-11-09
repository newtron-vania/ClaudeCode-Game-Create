# LevelUpUI UIToolkit 설정 가이드

## 📋 개요

이 문서는 UIToolkit으로 구현된 LevelUpUI를 Unity 에디터에서 설정하는 방법을 설명합니다.

## 🎯 Unity 에디터 설정 단계

### 1. PanelSettings 에셋 생성

**위치**: `Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpPanelSettings.asset`

**생성 방법**:
1. Project 창에서 `Assets/UI/UndeadSurvivor/LevelUpUI` 폴더 선택
2. 우클릭 → `Create` → `UI Toolkit` → `Panel Settings Asset`
3. 이름을 `LevelUpPanelSettings`로 변경

**설정값**:
```
Theme Style Sheet: None (또는 프로젝트 공통 테마)

Scaling:
├─ Scale Mode: Scale With Screen Size
├─ Reference Resolution: 1920 x 1080
├─ Screen Match Mode: Match Width Or Height
├─ Match: 0.5

Sorting:
└─ Sort Order: 100 (최상위 UI로 표시)

Target Texture:
└─ None (Screen Space로 렌더링)
```

### 2. UIDocument GameObject 생성

**Scene**: `Undead Survivor.unity`

**생성 방법**:
1. Hierarchy 창에서 우클릭 → `UI Toolkit` → `UI Document`
2. 이름을 `LevelUpUI`로 변경

**UIDocument 컴포넌트 설정**:
```
Inspector:
├─ Panel Settings: LevelUpPanelSettings (위에서 생성한 에셋)
├─ Source Asset: Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpUI.uxml
└─ Sort Order: 100
```

### 3. LevelUpUIController 컴포넌트 추가

**대상 GameObject**: 위에서 생성한 `LevelUpUI`

**추가 방법**:
1. `LevelUpUI` GameObject 선택
2. Inspector 창에서 `Add Component` 클릭
3. `LevelUpUIController` 검색 후 추가

**LevelUpUIController 컴포넌트 설정**:
```
Inspector:
├─ UI Document: (자동 연결됨 - 같은 GameObject의 UIDocument)
├─ Option Button Template: Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpOptionButton.uxml
├─ Fly In Duration: 0.1
└─ Delay Between Options: 0.05
```

### 4. LevelUpUI.uxml에 스타일시트 연결

**방법 1: UI Builder 사용**
1. Project 창에서 `LevelUpUI.uxml` 더블클릭
2. UI Builder 창이 열림
3. StyleSheets 섹션에서 `+` 버튼 클릭
4. `LevelUpUI.uss` 선택

**방법 2: 텍스트 에디터 직접 수정**
`LevelUpUI.uxml` 파일 상단에 추가:
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" editor-extension-mode="False">
    <Style src="project://database/Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpUI.uss?fileID=7433441132597879392&amp;guid=YOUR_GUID&amp;type=3#LevelUpUI" />
    <!-- 나머지 UI 구조 -->
</ui:UXML>
```

## 🎨 아이콘 스프라이트 설정

### 필요한 아이콘 목록

**무기 아이콘** (위치: `Assets/Resources/Sprites/UndeadSurvivor/`):
- `Icon_Weapon_1.png` (Fireball)
- `Icon_Weapon_2.png` (Scythe)
- `Icon_Weapon_3.png` ~ `Icon_Weapon_6.png` (향후 추가 무기)

**스탯 아이콘** (위치: `Assets/Resources/Sprites/UndeadSurvivor/`):
- `Icon_Stat_Damage.png` (공격력)
- `Icon_Stat_MaxHp.png` (최대 체력)
- `Icon_Stat_Defense.png` (방어력)
- `Icon_Stat_MoveSpeed.png` (이동 속도)
- `Icon_Stat_Area.png` (범위)
- `Icon_Stat_Cooldown.png` (쿨타임)
- `Icon_Stat_Amount.png` (투사체 개수)
- `Icon_Stat_Pierce.png` (관통력)
- `Icon_Stat_ExpMultiplier.png` (경험치 획득)
- `Icon_Stat_PickupRange.png` (아이템 획득 범위)
- `Icon_Stat_Luck.png` (행운)

**기본 아이콘**:
- `Icon_Default.png` (로드 실패 시 대체 아이콘)

### 스프라이트 Import 설정

각 아이콘 스프라이트 설정:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Filter Mode: Bilinear
Compression: None (또는 High Quality)
Max Size: 256
Format: RGBA 32 bit
```

### Addressables 설정

**중요**: 아이콘은 Addressables로 로드되므로 반드시 Addressables 그룹에 추가해야 합니다.

**설정 방법**:
1. Project 창에서 아이콘 스프라이트 선택
2. Inspector 창에서 `Addressable` 체크박스 활성화
3. Address 이름 설정:
   - 무기: `Sprite/UndeadSurvivor/Icon_Weapon_1`
   - 스탯: `Sprite/UndeadSurvivor/Icon_Stat_Damage`
   - 기본: `Sprite/UndeadSurvivor/Icon_Default`

## 🔧 LevelUpManager 연동

### LevelUpManager.cs 수정

기존 `LevelUpManager.cs`에서 UI 표시 부분을 수정합니다:

```csharp
using UndeadSurvivor.UI;

public class LevelUpManager : MonoBehaviour
{
    private LevelUpUIController _levelUpUIController;

    private void Awake()
    {
        // LevelUpUIController 찾기
        _levelUpUIController = FindObjectOfType<LevelUpUIController>();

        if (_levelUpUIController == null)
        {
            Debug.LogError("[ERROR] LevelUpManager::Awake - LevelUpUIController not found");
        }
    }

    public void ShowLevelUpUI(List<LevelUpOption> options)
    {
        if (_levelUpUIController != null)
        {
            _levelUpUIController.Show(options);
        }
        else
        {
            Debug.LogError("[ERROR] LevelUpManager::ShowLevelUpUI - LevelUpUIController is null");
        }
    }

    public void OnOptionChosen(LevelUpOption option)
    {
        // 기존 로직: 옵션 적용
        option.Apply(_player);

        // UI는 LevelUpUIController에서 자동으로 닫힘
    }
}
```

### Player.cs 레벨업 이벤트 연결

```csharp
private void OnPlayerLevelUp(int newLevel)
{
    Debug.Log($"[INFO] Player::OnPlayerLevelUp - Level {newLevel}");

    // 이동 비활성화
    _controller.SetMovementEnabled(false);

    // LevelUpManager에 레벨업 알림
    var levelUpManager = FindObjectOfType<LevelUpManager>();
    if (levelUpManager != null)
    {
        var options = levelUpManager.GenerateLevelUpOptions(this);
        levelUpManager.ShowLevelUpUI(options);
    }
}
```

## ✅ 테스트 체크리스트

### 기본 동작 확인
- [ ] Unity 에디터에서 Play 모드 진입
- [ ] 플레이어가 레벨업하면 UI가 표시되는지 확인
- [ ] Time.timeScale = 0으로 게임이 일시정지되는지 확인
- [ ] 4개의 선택지가 표시되는지 확인

### 애니메이션 확인
- [ ] Panel이 즉시 생성되는지 확인
- [ ] 선택지가 좌측에서 우측으로 Fly-In 하는지 확인
- [ ] 선택지가 순차적으로 배치되는지 확인 (0.05초 간격)
- [ ] 각 선택지 애니메이션이 0.1초 안에 완료되는지 확인

### 상호작용 확인
- [ ] 마우스 호버 시 선택지가 1.05배 확대되는지 확인
- [ ] 선택지 클릭 시 선택이 적용되는지 확인
- [ ] 선택 후 UI가 닫히는지 확인
- [ ] 선택 후 Time.timeScale = 1로 게임이 재개되는지 확인

### 데이터 바인딩 확인
- [ ] 무기 이름이 올바르게 표시되는지 확인
- [ ] 무기 설명이 올바르게 표시되는지 확인
- [ ] 아이콘이 올바르게 로드되는지 확인
- [ ] 아이콘 로드 실패 시 기본 아이콘이 표시되는지 확인

### 스탯 선택지 확인
- [ ] 스탯 선택지 이름이 한글로 표시되는지 확인
- [ ] 스탯 선택지 설명이 올바른지 확인
- [ ] 스탯 아이콘이 올바르게 표시되는지 확인

## 🐛 문제 해결

### UI가 표시되지 않는 경우
1. UIDocument의 Panel Settings가 올바르게 설정되었는지 확인
2. LevelUpUI.uxml에 스타일시트가 연결되었는지 확인
3. Sort Order가 다른 UI보다 높은지 확인 (100 이상)

### 아이콘이 표시되지 않는 경우
1. 스프라이트가 Addressables에 추가되었는지 확인
2. Address 이름이 올바른지 확인 (`Sprite/UndeadSurvivor/Icon_...`)
3. 스프라이트 Import 설정이 올바른지 확인
4. Console 창에서 로드 실패 로그 확인

### 애니메이션이 작동하지 않는 경우
1. USS 파일이 UXML에 올바르게 연결되었는지 확인
2. CSS transition 속성이 올바른지 확인
3. Time.timeScale = 0 상태에서 WaitForSecondsRealtime 사용 확인

### 클릭 이벤트가 작동하지 않는 경우
1. UIDocument의 Raycast Target 설정 확인
2. Event System이 Scene에 존재하는지 확인
3. 다른 UI가 LevelUpUI를 가리고 있는지 확인

## 📚 참고 문서

- **아키텍처 설계**: `Assets/Docs/UndeadSurvivor_LevelUpUI_Architecture.md` (이전 대화 참조)
- **LevelUp 시스템**: `Assets/Docs/UndeadSurvivor_Progress.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`
- **UIToolkit 공식 문서**: https://docs.unity3d.com/Manual/UIElements.html

## 🎯 다음 단계

LevelUpUI 설정 완료 후:
1. 테스트 플레이로 모든 기능 검증
2. 아이콘 스프라이트 제작 및 추가
3. 추가 무기/스탯에 대한 아이콘 준비
4. UI 애니메이션 세부 조정 (필요 시)
5. 접근성 개선 (키보드 네비게이션 등)
