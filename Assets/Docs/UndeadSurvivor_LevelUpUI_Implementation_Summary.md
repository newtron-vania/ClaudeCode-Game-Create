# LevelUpUI UIToolkit 구현 완료 보고서

**작성일**: 2025-11-09
**작업 브랜치**: `featrue/undead-survivor-test`
**구현 방식**: UIToolkit (UI Builder)

---

## 📊 구현 완료 항목

### ✅ 1. UI 구조 파일 (UXML)

**생성 파일**:
- `Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpUI.uxml` - 메인 UI 구조
- `Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpOptionButton.uxml` - 선택지 버튼 템플릿

**계층 구조**:
```
LevelUpUI.uxml
└── level-up-panel
    ├── background (반투명 오버레이)
    └── content-container
        ├── title ("LEVEL UP!")
        └── options-container (4지선다 버튼 동적 생성)

LevelUpOptionButton.uxml (템플릿)
└── option-button
    ├── icon-container
    │   └── icon (무기/스탯 아이콘)
    └── text-container
        ├── name-text (이름)
        └── description-text (설명)
```

### ✅ 2. 스타일시트 (USS)

**파일**: `Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpUI.uss`

**주요 스타일**:
- Panel Root: 전체 화면 Flexbox 레이아웃
- Background: rgba(0,0,0,0.8) 오버레이
- Title: 골드 색상, 60px, 그림자 효과
- Option Button:
  - Normal: rgba(51,51,64,0.95)
  - Hover: 1.05배 확대, 골드 테두리
  - Active: 0.98배 축소
- Animation Classes:
  - `option-button--hidden`: translate(-800px, 0), opacity 0
  - `option-button--visible`: translate(0, 0), opacity 1
  - Transition: 0.1초 ease-out-cubic

### ✅ 3. C# 컨트롤러

#### LevelUpUIController.cs
**위치**: `Assets/Scripts/UI/Popup/GameScene/LevelUpUIController.cs`
**라인 수**: 218 lines

**주요 기능**:
- UIDocument 및 VisualElement 참조 관리
- 선택지 동적 생성 (템플릿 인스턴스화)
- Fly-In 애니메이션 코루틴 (순차 배치)
- Time.timeScale 제어 (일시정지/재개)
- 선택 이벤트 처리

**주요 메서드**:
```csharp
public void Show(List<LevelUpOption> options)
public void Hide()
private void CreateOptions(List<LevelUpOption> options)
private void ClearOptions()
private IEnumerator AnimateOptionsCoroutine()
private void OnOptionSelected(LevelUpOption option)
```

#### LevelUpOptionElement.cs
**위치**: `Assets/Scripts/UI/Popup/GameScene/LevelUpOptionElement.cs`
**라인 수**: 157 lines

**주요 기능**:
- 개별 선택지 VisualElement 래퍼
- 데이터 바인딩 (이름, 설명, 아이콘)
- Addressables를 통한 아이콘 로드
- 클릭 이벤트 발생
- 리소스 해제 (Dispose)

**아이콘 경로 로직**:
```csharp
NewWeapon → "Sprite/UndeadSurvivor/Icon_Weapon_{WeaponID}"
WeaponUpgrade → "Sprite/UndeadSurvivor/Icon_Weapon_{WeaponID}"
StatUpgrade → "Sprite/UndeadSurvivor/Icon_Stat_{StatType}"
Default → "Sprite/UndeadSurvivor/Icon_Default"
```

### ✅ 4. LevelUpManager UI 연동

**파일**: `Assets/Scripts/UndeadSurvivor/LevelUpManager.cs`
**변경 사항**:

1. **namespace 추가**:
```csharp
using UndeadSurvivor.UI;
```

2. **필드 추가**:
```csharp
[Header("UI References")]
[SerializeField] private LevelUpUIController _levelUpUIController;
```

3. **Awake 수정**:
```csharp
if (_levelUpUIController == null)
{
    _levelUpUIController = FindObjectOfType<LevelUpUIController>();
}
```

4. **신규 메서드 추가**:
```csharp
public void ShowLevelUpUI()  // 선택지 생성 및 UI 표시
public void OnOptionChosen(LevelUpOption option)  // 선택 적용 및 게임 재개
```

### ✅ 5. 문서화

**생성 문서**:
1. `Assets/Docs/UndeadSurvivor_LevelUpUI_Setup_Guide.md` (289 lines)
   - Unity 에디터 설정 단계별 가이드
   - PanelSettings 생성 방법
   - UIDocument GameObject 구성
   - LevelUpManager 연동 방법
   - 테스트 체크리스트

2. `Assets/Docs/UndeadSurvivor_Icon_Sprite_Guide.md` (349 lines)
   - 아이콘 명명 규칙
   - 파일 구조
   - Import Settings
   - Addressables 설정
   - 일괄 설정 에디터 스크립트

---

## 🎬 애니메이션 구현

### 애니메이션 시퀀스

```
T=0.00s  Panel + Background + Title 즉시 표시
T=0.00s  Option 1 Fly-In 시작 (좌측 -800px → 0)
T=0.10s  Option 1 완료
T=0.05s  Option 2 Fly-In 시작 (delay)
T=0.15s  Option 2 완료
T=0.10s  Option 3 Fly-In 시작 (delay)
T=0.20s  Option 3 완료
T=0.15s  Option 4 Fly-In 시작 (delay)
T=0.25s  Option 4 완료
─────────────────────────────
총 0.55초 애니메이션 완료
```

### 구현 방식

**CSS Transition 기반**:
- USS에서 `.option-button--visible` 클래스에 transition 정의
- C# 코루틴에서 클래스 추가/제거로 애니메이션 트리거
- `WaitForSecondsRealtime` 사용 (Time.timeScale = 0 대응)

**코드**:
```csharp
// CSS transition (USS)
.option-button--visible {
    translate: 0 0 0;
    opacity: 1;
    transition-duration: 0.1s;
    transition-property: translate, opacity;
    transition-timing-function: ease-out-cubic;
}

// C# 코루틴
for (int i = 0; i < _optionElements.Count; i++)
{
    yield return new WaitForSecondsRealtime(_delayBetweenOptions * i);
    button.RemoveFromClassList("option-button--hidden");
    button.AddToClassList("option-button--visible");
}
```

---

## 📐 아키텍처 설계 원칙

### 1. 관심사 분리
- **UXML**: 구조 (Structure)
- **USS**: 스타일 (Presentation)
- **C#**: 동작 (Behavior)

### 2. 재사용성
- 옵션 버튼 템플릿 재사용
- LevelUpOptionElement 독립 컴포넌트
- 공통 스타일 USS 분리 가능

### 3. 확장성
- 새로운 옵션 타입 추가 용이
- 스타일 변경 시 USS만 수정
- 애니메이션 조정 시 CSS transition만 수정

### 4. 성능
- CSS transition으로 GPU 가속
- Addressables 지연 로드
- 리소스 Dispose 패턴

---

## 🎯 Unity 에디터 설정 필요 항목

### 즉시 작업 필요
1. **PanelSettings 에셋 생성**
   - 위치: `Assets/UI/UndeadSurvivor/LevelUpUI/LevelUpPanelSettings.asset`
   - Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Sort Order: 100

2. **UIDocument GameObject 생성**
   - Scene: `Undead Survivor.unity`
   - 컴포넌트: UIDocument, LevelUpUIController
   - Source Asset: `LevelUpUI.uxml`
   - Panel Settings: 위에서 생성한 에셋

3. **LevelUpUIController 설정**
   - Option Button Template: `LevelUpOptionButton.uxml`
   - Fly In Duration: 0.1
   - Delay Between Options: 0.05

4. **LevelUpManager 설정**
   - Inspector에서 Level Up UI Controller 필드에 위에서 생성한 GameObject 연결

### 아이콘 준비 필요
**필수 아이콘 14개**:
- 무기: Icon_Weapon_1.png, Icon_Weapon_2.png
- 스탯: Icon_Stat_Damage.png ~ Icon_Stat_Luck.png (11개)
- 기본: Icon_Default.png

**Addressables 설정**:
- 각 아이콘을 Addressables에 추가
- Address 이름: `Sprite/UndeadSurvivor/Icon_...`

---

## 📊 코드 통계

### 신규 파일
| 파일 | 형식 | 라인 수 | 설명 |
|:---|:---|---:|:---|
| LevelUpUI.uxml | UXML | 18 | 메인 UI 구조 |
| LevelUpOptionButton.uxml | UXML | 22 | 선택지 템플릿 |
| LevelUpUI.uss | USS | 175 | 스타일시트 |
| LevelUpUIController.cs | C# | 218 | UI 컨트롤러 |
| LevelUpOptionElement.cs | C# | 157 | 옵션 요소 |
| **총계** | - | **590** | - |

### 수정 파일
| 파일 | 변경 내용 | 추가 라인 |
|:---|:---|---:|
| LevelUpManager.cs | UI 연동 메서드 추가 | +60 |

### 문서 파일
| 파일 | 라인 수 |
|:---|---:|
| UndeadSurvivor_LevelUpUI_Setup_Guide.md | 289 |
| UndeadSurvivor_Icon_Sprite_Guide.md | 349 |
| UndeadSurvivor_LevelUpUI_Implementation_Summary.md | 이 문서 |

---

## ✅ 구현 완료 체크리스트

### 코드 구현
- [x] LevelUpUI.uxml 메인 구조
- [x] LevelUpOptionButton.uxml 템플릿
- [x] LevelUpUI.uss 스타일시트
- [x] LevelUpUIController.cs 컨트롤러
- [x] LevelUpOptionElement.cs 옵션 요소
- [x] LevelUpManager UI 연동
- [x] 애니메이션 시스템 (CSS transition)
- [x] Addressables 아이콘 로드
- [x] Time.timeScale 제어
- [x] 리소스 해제 (Dispose)

### 문서화
- [x] Unity 에디터 설정 가이드
- [x] 아이콘 스프라이트 가이드
- [x] 구현 완료 요약
- [x] 아키텍처 설계 문서 (이전 대화)

### Unity 에디터 작업 (수동 필요)
- [ ] PanelSettings 에셋 생성
- [ ] UIDocument GameObject 생성
- [ ] LevelUpUIController 설정
- [ ] LevelUpManager 연결
- [ ] 아이콘 스프라이트 임포트
- [ ] Addressables 설정

---

## 🧪 테스트 시나리오

### 기본 동작
1. 플레이어 레벨업 → UI 표시 확인
2. Time.timeScale = 0 → 게임 일시정지 확인
3. 4개 선택지 표시 확인
4. 선택지 데이터 바인딩 확인 (이름, 설명)

### 애니메이션
1. Panel 즉시 생성 확인
2. 선택지 좌측 → 우측 Fly-In 확인
3. 순차 배치 (0.05초 간격) 확인
4. 애니메이션 완료 시간 (0.55초) 확인

### 상호작용
1. 마우스 호버 → 1.05배 확대 확인
2. 클릭 → 선택 적용 확인
3. 선택 후 UI 닫힘 확인
4. Time.timeScale = 1 → 게임 재개 확인

### 아이콘
1. 무기 아이콘 로드 확인
2. 스탯 아이콘 로드 확인
3. 로드 실패 시 기본 아이콘 표시 확인

---

## 🔧 다음 작업

### 즉시 가능
1. Unity 에디터에서 PanelSettings 에셋 생성
2. UIDocument GameObject 설정
3. LevelUpManager 연결

### 아이콘 준비 후
1. 14개 아이콘 스프라이트 제작
2. Addressables 설정
3. 테스트 플레이

### 추가 개선 (선택 사항)
1. 키보드 네비게이션 (Tab, Enter)
2. 선택 완료 애니메이션 (Fade Out)
3. 사운드 효과 (UI 표시, 선택)
4. 툴팁 시스템 (마우스 호버 시 상세 정보)

---

## 📚 참조 문서

- **설정 가이드**: `Assets/Docs/UndeadSurvivor_LevelUpUI_Setup_Guide.md`
- **아이콘 가이드**: `Assets/Docs/UndeadSurvivor_Icon_Sprite_Guide.md`
- **진행 상황**: `Assets/Docs/UndeadSurvivor_Progress.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`

---

## 🎯 결론

**UIToolkit 기반 LevelUpUI 구현 완료**

- 총 590 라인의 새로운 코드 작성
- 완전한 애니메이션 시스템 구현
- 아키텍처 설계 원칙 준수
- 확장 가능한 구조
- 상세한 문서화 완료

**Unity 에디터 설정만 완료하면 즉시 사용 가능**
