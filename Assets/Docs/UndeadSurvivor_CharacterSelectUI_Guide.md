# UndeadSurvivor 캐릭터 선택 UI 가이드

**작성일**: 2024-11-10
**Phase**: Phase 5 - UI 시스템

---

## 📋 개요

UndeadSurvivor의 캐릭터 선택 화면 UI 시스템입니다. 플레이어가 여러 캐릭터 중 하나를 선택하고, 선택한 캐릭터의 상세 스탯을 확인한 후 게임을 시작할 수 있습니다.

---

## 🎯 UI 구조

### 전체 레이아웃
```
CharacterSelectUIPanel
├── LeftPanel (CharacterStatInfoPanel)      # 캐릭터 상세 정보
│   ├── Character Name & Portrait
│   └── Stats Display (11개 스탯)
├── RightPanel (Character List)             # 캐릭터 선택 목록
│   └── ScrollView
│       └── CharacterSelectSubItem (동적 생성)
├── BottomButtons
│   ├── StartButton                         # 게임 시작
│   └── CancelButton                        # 취소 (메인으로)
└── ErrorMessageText                        # 오류 메시지 (3초 표시)
```

---

## 📁 파일 구조

```
Assets/Scripts/UndeadSurvivor/UI/
├── CharacterSelectUIPanel.cs           # 메인 UI 컨트롤러
├── CharacterStatInfoPanel.cs           # 좌측 스탯 정보 패널
└── CharacterSelectSubItem.cs           # 개별 캐릭터 버튼

Assets/Scripts/UndeadSurvivor/Data/
└── UndeadSurvivorDataProvider.cs       # GetAllCharacters() 추가됨

Assets/Resources/Prefabs/UI/UndeadSurvivor/
└── CharacterSelectSubItem.prefab       # SubItem 프리팹 (생성 필요)

Assets/Resources/Sprites/UndeadSurvivor/
├── Knight_portrait.png                 # 캐릭터 초상화 (생성 필요)
└── Mage_portrait.png
```

---

## 🔧 컴포넌트 상세

### 1. CharacterSelectUIPanel.cs

**역할**: 전체 UI 관리 및 이벤트 통합

**주요 기능**:
- DataManager에서 모든 CharacterData 로드
- CharacterSelectSubItem 동적 생성
- 선택된 캐릭터 정보 관리
- 시작/취소 버튼 처리
- 에러 메시지 표시 (코루틴 3초)

**Inspector 설정 필요**:
```
CharacterSelectUIPanel Component:
├── Stat Info Panel: CharacterStatInfoPanel 연결
├── Character List Content: ScrollView/Content Transform 연결
├── Start Button: Button 연결
├── Cancel Button: Button 연결
├── Error Message Text: TextMeshProUGUI 연결
└── Character SubItem Prefab: CharacterSelectSubItem Prefab 연결
```

**주요 메서드**:
```csharp
public void Initialize();                                   // UI 초기화
private void LoadAllCharacters();                           // 모든 캐릭터 로드
private void OnCharacterSelected(int characterId);          // 캐릭터 선택 이벤트
private void OnStartButtonClicked();                        // 시작 버튼
private void OnCancelButtonClicked();                       // 취소 버튼
private void ShowErrorMessage(string message);              // 에러 표시
```

---

### 2. CharacterStatInfoPanel.cs

**역할**: 좌측 패널 - 선택된 캐릭터 스탯 표시

**주요 기능**:
- CharacterData 기반 모든 스탯 표시
- 캐릭터 이름, 초상화 표시
- 11개 스탯 + 시작 무기 정보

**Inspector 설정 필요**:
```
CharacterStatInfoPanel Component:
├── Character Info
│   ├── Character Name Text: TextMeshProUGUI
│   └── Character Sprite Image: Image
├── Base Stats (4개)
│   ├── Max Hp Text
│   ├── Damage Text
│   ├── Defense Text
│   └── Move Speed Text
├── Combat Stats (4개)
│   ├── Cooldown Text
│   ├── Area Text
│   ├── Amount Text
│   └── Pierce Text
├── Utility Stats (3개)
│   ├── Exp Multiplier Text
│   ├── Pickup Range Text
│   └── Luck Text
└── Start Weapon Text
```

**주요 메서드**:
```csharp
public void UpdateCharacterInfo(CharacterData characterData);  // 정보 업데이트
public void Clear();                                           // 초기화
```

**스탯 표시 형식**:
```csharp
체력: 120
공격력: +0%
방어력: 2
이동속도: 4.5
쿨타임: 0%
범위: +0%
개수: +0
관통: +0
경험치: +0%
획득범위: +0%
행운: +0%
시작 무기: Scythe Lv.2
```

---

### 3. CharacterSelectSubItem.cs

**역할**: 우측 패널 - 개별 캐릭터 선택 버튼

**주요 기능**:
- CharacterData 기반 이름/초상화 표시
- 선택 시 하이라이트 효과
- 클릭 이벤트 발생

**Inspector 설정 필요**:
```
CharacterSelectSubItem Component:
├── Character Name Text: TextMeshProUGUI
├── Character Sprite Image: Image
├── Background Image: Image (하이라이트용)
└── Button: Button Component
```

**주요 메서드**:
```csharp
public void Initialize(CharacterData characterData);   // 초기화
public void SetSelected(bool isSelected);              // 선택 상태 설정
```

**선택 색상**:
```csharp
Normal Color: (1, 1, 1, 0.5)      // 반투명 흰색
Selected Color: (1, 0.8, 0.2, 1)  // 황금색
```

---

## 🔄 동작 플로우

### 1. 초기화 단계
```
1. CharacterSelectUIPanel.Initialize()
   ↓
2. DataManager.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor")
   ↓
3. LoadAllCharacters()
   ↓
4. dataProvider.GetAllCharacters() (ID 순 정렬)
   ↓
5. foreach (CharacterData) → Instantiate CharacterSelectSubItem
   ↓
6. subItem.Initialize(characterData)
   ↓
7. subItem.OnCharacterClicked += OnCharacterSelected
   ↓
8. _statInfoPanel.Clear() (초기 상태)
```

### 2. 캐릭터 선택 단계
```
1. User Click CharacterSelectSubItem
   ↓
2. OnCharacterSelected(characterId) 호출
   ↓
3. 이전 선택 SubItem.SetSelected(false) (하이라이트 해제)
   ↓
4. 현재 선택 SubItem.SetSelected(true) (황금색 하이라이트)
   ↓
5. _selectedCharacterData = dataProvider.GetCharacterData(characterId)
   ↓
6. _statInfoPanel.UpdateCharacterInfo(_selectedCharacterData)
   ↓
7. 좌측 패널에 11개 스탯 + 시작 무기 표시
```

### 3. 시작 버튼 클릭
```
1. OnStartButtonClicked()
   ↓
2. if (_selectedCharacterData == null)
   ├─ ShowErrorMessage("캐릭터를 선택해주세요!")
   ├─ 붉은색 텍스트 3초 표시
   └─ return
   ↓
3. MiniGameManager 또는 static에 선택 캐릭터 저장 (선택)
   ↓
4. CustomSceneManager.LoadScene("Undead Survivor")
   ↓
5. GameScene에서 _selectedCharacterData 기반 Player 초기화
```

### 4. 취소 버튼 클릭
```
1. OnCancelButtonClicked()
   ↓
2. _selectedCharacterData = null
   ↓
3. _currentSelectedSubItem.SetSelected(false)
   ↓
4. _statInfoPanel.Clear()
   ↓
5. CustomSceneManager.LoadScene("Undead Survivor") (메인 씬)
```

---

## 🎨 Unity 에디터 설정 가이드

### Step 1: CharacterSelectSubItem Prefab 생성

1. **GameObject 생성**:
   ```
   Hierarchy:
   └── CharacterSelectSubItem (GameObject)
       ├── BackgroundImage (Image) - 하이라이트용
       ├── CharacterSpriteImage (Image) - 캐릭터 초상화
       └── CharacterNameText (TextMeshProUGUI) - 캐릭터 이름
   ```

2. **Button 컴포넌트 추가**:
   - CharacterSelectSubItem에 Button Component 추가
   - Transition: Color Tint

3. **CharacterSelectSubItem 스크립트 추가**:
   - Inspector에서 다음 연결:
     - Character Name Text: CharacterNameText
     - Character Sprite Image: CharacterSpriteImage
     - Background Image: BackgroundImage
     - Button: Button Component

4. **Prefab 저장**:
   - `Assets/Resources/Prefabs/UI/UndeadSurvivor/CharacterSelectSubItem.prefab`

---

### Step 2: CharacterSelectUI Scene 구성

1. **Canvas 생성** (이미 존재하면 사용):
   ```
   Canvas
   ├── LeftPanel (CharacterStatInfoPanel)
   │   ├── CharacterNameText
   │   ├── CharacterSpriteImage
   │   └── StatsContainer
   │       ├── MaxHpText
   │       ├── DamageText
   │       └── ... (11개 스탯 TextMeshProUGUI)
   ├── RightPanel
   │   └── ScrollView
   │       └── Content (CharacterSelectSubItem 생성 위치)
   ├── BottomButtons
   │   ├── StartButton
   │   └── CancelButton
   └── ErrorMessageText (초기 비활성화)
   ```

2. **CharacterStatInfoPanel 스크립트 추가**:
   - LeftPanel에 CharacterStatInfoPanel 컴포넌트 추가
   - Inspector에서 11개 스탯 TextMeshProUGUI 모두 연결

3. **CharacterSelectUIPanel 스크립트 추가**:
   - Canvas 또는 별도 GameObject에 추가
   - Inspector 연결:
     - Stat Info Panel: CharacterStatInfoPanel
     - Character List Content: ScrollView/Content Transform
     - Start Button: Button
     - Cancel Button: Button
     - Error Message Text: TextMeshProUGUI
     - Character SubItem Prefab: CharacterSelectSubItem.prefab

4. **Scene 초기화 스크립트**:
   ```csharp
   // UndeadSurvivorCharacterSelectScene.cs
   private void Start()
   {
       // DataProvider 로드
       if (!DataManager.Instance.HasProvider("UndeadSurvivor"))
       {
           var dataProvider = new UndeadSurvivorDataProvider();
           DataManager.Instance.RegisterProvider(dataProvider);
           DataManager.Instance.LoadGameData("UndeadSurvivor");
       }

       // UI 초기화
       var uiPanel = GetComponent<CharacterSelectUIPanel>();
       uiPanel.Initialize();
   }
   ```

---

## 📦 Addressables 리소스 경로

### 캐릭터 초상화 스프라이트
```
Sprites/UndeadSurvivor/{CharacterName}_portrait

예시:
- Sprites/UndeadSurvivor/Knight_portrait
- Sprites/UndeadSurvivor/Mage_portrait
```

### Prefab
```
Prefabs/UI/UndeadSurvivor/CharacterSelectSubItem
```

---

## 🔍 DataProvider 확장

### 추가된 메서드

**UndeadSurvivorDataProvider.cs**:
```csharp
/// <summary>
/// 모든 캐릭터 데이터 목록 반환 (ID 순 정렬)
/// </summary>
public List<CharacterData> GetAllCharacters()
{
    if (!IsLoaded)
    {
        Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::GetAllCharacters - Data not loaded");
        return new List<CharacterData>();
    }

    List<CharacterData> characters = new List<CharacterData>(_characterDict.Values);
    characters.Sort((a, b) => a.Id.CompareTo(b.Id)); // ID 순 정렬

    return characters;
}
```

**사용 예시**:
```csharp
var dataProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");
List<CharacterData> allCharacters = dataProvider.GetAllCharacters();

foreach (var character in allCharacters)
{
    Debug.Log($"Character: {character.Name}, ID: {character.Id}");
}
```

---

## ⚠️ 주의사항

### 1. DataManager 초기화
CharacterSelectScene 진입 시 반드시 DataProvider가 로드되어 있어야 합니다:
```csharp
if (!DataManager.Instance.HasProvider("UndeadSurvivor"))
{
    var dataProvider = new UndeadSurvivorDataProvider();
    DataManager.Instance.RegisterProvider(dataProvider);
    DataManager.Instance.LoadGameData("UndeadSurvivor");
}
```

### 2. 캐릭터 선택 정보 전달
GameScene으로 선택된 캐릭터를 전달하는 방법:
- **옵션 1**: MiniGameManager에 static 변수 추가
- **옵션 2**: CommonPlayerData 확장
- **옵션 3**: PlayerPrefs 사용

### 3. 리소스 로딩
- 캐릭터 초상화는 비동기 로드 (ResourceManager.LoadAsync)
- 로딩 실패 시 Warning 로그 출력하지만 UI는 계속 동작

### 4. UI 초기화 순서
```
1. DataProvider 로드 확인
2. CharacterSelectUIPanel.Initialize() 호출
3. SubItem 동적 생성
4. 스탯 패널 초기화 (Clear)
```

---

## 🧪 테스트 시나리오

### 시나리오 1: 정상 선택 및 시작
```
1. CharacterSelectScene 진입
2. 캐릭터 목록 표시 확인 (Knight, Mage)
3. Knight 클릭
   - Knight 버튼 황금색 하이라이트 확인
   - 좌측 패널에 Knight 스탯 표시 확인
4. 시작 버튼 클릭
   - GameScene으로 전환 확인
```

### 시나리오 2: 선택 없이 시작
```
1. CharacterSelectScene 진입
2. 캐릭터 선택하지 않음
3. 시작 버튼 클릭
   - "캐릭터를 선택해주세요!" 에러 메시지 표시
   - 3초 후 자동으로 메시지 사라짐
```

### 시나리오 3: 캐릭터 변경
```
1. CharacterSelectScene 진입
2. Knight 클릭 → Knight 정보 표시
3. Mage 클릭 → Mage 정보로 변경, Knight 하이라이트 해제
```

### 시나리오 4: 취소
```
1. CharacterSelectScene 진입
2. Knight 선택
3. 취소 버튼 클릭
   - Undead Survivor 메인 씬으로 이동
   - 선택 정보 초기화
```

---

## 📊 코드 통계

| 파일 | 라인 수 | 주요 기능 |
|-----|--------|----------|
| CharacterSelectUIPanel.cs | ~280 lines | 전체 UI 관리, 이벤트 통합 |
| CharacterStatInfoPanel.cs | ~220 lines | 스탯 정보 표시 |
| CharacterSelectSubItem.cs | ~130 lines | 개별 캐릭터 버튼 |
| **총합** | **~630 lines** | |

---

## 🔗 관련 문서

- **UndeadSurvivor_Progress.md**: 전체 개발 진행 상황
- **MANAGERS_GUIDE.md**: DataManager, ResourceManager 사용법
- **CharacterData.json**: 캐릭터 데이터 구조

---

## 📝 다음 단계

### Unity 에디터 작업 필요
1. ✅ CharacterSelectSubItem.prefab 생성
2. ✅ CharacterSelectUI Scene 구성
3. ✅ 캐릭터 초상화 스프라이트 준비 (Knight_portrait, Mage_portrait)
4. ✅ UndeadSurvivorCharacterSelectScene 초기화 스크립트 작성
5. ✅ 씬 전환 테스트 (메인 → 캐릭터 선택 → 게임)

### 코드 작업 필요
6. ⏳ CommonPlayerData 또는 MiniGameManager에 선택 캐릭터 저장 메커니즘 추가
7. ⏳ GameScene에서 선택된 캐릭터로 Player 초기화 구현

---

**작성자**: Claude Code
**최종 수정일**: 2024-11-10
