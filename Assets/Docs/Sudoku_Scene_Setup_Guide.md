# Sudoku 씬 설정 가이드

이 문서는 Unity 에디터에서 Sudoku.unity 씬을 생성하고 통합하는 절차를 설명합니다.

---

## 전체 작업 흐름

1. **Sudoku.unity 씬 생성**
2. **SudokuScene 컴포넌트 배치**
3. **Canvas 및 UI 계층 구조 설정**
4. **SudokuUIPanel Prefab 배치**
5. **GamePlayList에 Sudoku 추가**
6. **게임 아이콘 스프라이트 배치**
7. **씬 빌드 설정에 추가**
8. **테스트 및 검증**

---

## 1. Sudoku.unity 씬 생성

### Unity 에디터에서 새 씬 생성

1. Unity 에디터 열기
2. **File → New Scene** 선택
3. **2D (URP)** 템플릿 선택
4. **File → Save As** 선택
5. 저장 위치: `Assets/Scenes/Sudoku.unity`
6. 씬 이름: `Sudoku`

---

## 2. SudokuScene 컴포넌트 배치

### 빈 GameObject 생성

1. Hierarchy에서 우클릭 → **Create Empty**
2. 이름을 `SudokuSceneManager`로 변경
3. Inspector에서 **Add Component** 클릭
4. `SudokuScene` 스크립트 추가
   - 위치: `Assets/Scripts/Scenes/SudokuScene.cs`

### 검증

- SudokuScene 컴포넌트가 제대로 추가되었는지 확인
- SceneID가 `Sudoku`로 자동 설정되는지 확인

---

## 3. Canvas 및 UI 계층 구조 설정

### Canvas 생성 (이미 있다면 생략)

1. Hierarchy에서 우클릭 → **UI → Canvas**
2. Canvas 설정:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
     - Match: Width or Height (0.5)
   - **Graphic Raycaster**: 활성화

### EventSystem 확인

- EventSystem이 자동 생성되었는지 확인
- 없으면 Hierarchy에서 우클릭 → **UI → Event System** 생성

---

## 4. SudokuUIPanel Prefab 배치

### UI Panel Prefab 생성 (Unity 에디터 작업)

**⚠️ 중요**: SudokuUIPanel은 현재 스크립트만 작성된 상태입니다. Unity 에디터에서 직접 UI를 구성해야 합니다.

#### 4.1. 빈 Panel 생성

1. Canvas 하위에 우클릭 → **UI → Panel**
2. 이름을 `SudokuUIPanel`로 변경
3. **Add Component** → `SudokuUIPanel` 스크립트 추가

#### 4.2. UI 계층 구조 구성

```
SudokuUIPanel (Panel)
├── StartMenuPanel (Panel) - 시작 메뉴 상태
│   ├── TitleText (TextMeshPro) - "Sudoku"
│   ├── DifficultyPanel (Panel)
│   │   ├── EasyButton (Button) - "쉬움"
│   │   ├── MediumButton (Button) - "중간"
│   │   └── HardButton (Button) - "어려움"
│   └── BackButton (Button) - "메인 메뉴"
│
├── LoadingPanel (Panel) - 맵 생성 중 상태
│   ├── LoadingText (TextMeshPro) - "Generating..."
│   └── LoadingSpinner (Image) - 회전 애니메이션
│
├── PlayingPanel (Panel) - 게임 플레이 상태
│   ├── GridContainer (Panel) - 9x9 그리드 배치 영역
│   │   └── SudokuGridUI (컴포넌트) - 81개 셀 버튼 동적 생성
│   │       └── 셀 버튼 Prefab 필요: SudokuCellButton.prefab
│   │           ├── Button (컴포넌트)
│   │           ├── Image (배경)
│   │           ├── TextMeshPro (숫자 표시)
│   │           └── SudokuCellButton (스크립트)
│   │
│   ├── NumPadPanel (Panel) - 숫자 입력 패드 영역
│   │   └── NumPadUI (컴포넌트)
│   │       ├── NumberButtons (1-9 버튼 배열)
│   │       └── ClearButton (지우기 버튼)
│   │
│   ├── TimerPanel (Panel) - 타이머 표시 영역
│   │   ├── TimerText (TextMeshPro) - "00:00"
│   │   └── TimerUI (컴포넌트) - 타이머 로직
│   │
│   ├── MistakesText (TextMeshPro) - "실수: 0"
│   ├── HintsText (TextMeshPro) - "힌트: 0"
│   ├── HintButton (Button) - 힌트 요청
│   ├── UndoButton (Button) - 실행 취소
│   ├── EraseButton (Button) - 지우기
│   └── PauseButton (Button) - 일시정지
│
└── GameEndPanel (Panel) - 게임 완료 상태
    ├── WinText (TextMeshPro) - "축하합니다!"
    ├── TimeText (TextMeshPro) - "클리어 타임: 00:00"
    ├── StatsPanel (Panel) - 통계 정보 영역
    │   ├── HintsUsedText (TextMeshPro) - "힌트 사용: 0"
    │   └── MistakesText (TextMeshPro) - "실수: 0"
    ├── NewGameButton (Button) - "새 게임"
    └── MainMenuButton (Button) - "메인 메뉴"
```

#### 4.3. SudokuUIPanel 스크립트 연결

Inspector에서 SudokuUIPanel 컴포넌트의 SerializeField들을 UI 요소와 연결합니다:

**Main Panels (4개)**:
- `_startMenuPanel` → StartMenuPanel GameObject
- `_loadingPanel` → LoadingPanel GameObject
- `_playingPanel` → PlayingPanel GameObject
- `_gameEndPanel` → GameEndPanel GameObject

**Start Menu UI (5개)**:
- `_titleText` → StartMenuPanel/TitleText
- `_difficultyPanel` → StartMenuPanel/DifficultyPanel
- `_easyButton` → DifficultyPanel/EasyButton
- `_mediumButton` → DifficultyPanel/MediumButton
- `_hardButton` → DifficultyPanel/HardButton
- `_backButton` → StartMenuPanel/BackButton

**Loading UI (2개)**:
- `_loadingText` → LoadingPanel/LoadingText
- `_loadingSpinner` → LoadingPanel/LoadingSpinner

**Playing UI (13개)**:
- `_gridContainer` → PlayingPanel/GridContainer
- `_gridUI` → GridContainer 컴포넌트 (SudokuGridUI)
- `_numPadPanel` → PlayingPanel/NumPadPanel
- `_numPadUI` → NumPadPanel 컴포넌트 (NumPadUI)
- `_timerText` → PlayingPanel/TimerPanel/TimerText
- `_timerUI` → TimerPanel 컴포넌트 (TimerUI)
- `_mistakesText` → PlayingPanel/MistakesText
- `_hintsText` → PlayingPanel/HintsText
- `_hintButton` → PlayingPanel/HintButton
- `_undoButton` → PlayingPanel/UndoButton
- `_eraseButton` → PlayingPanel/EraseButton
- `_pauseButton` → PlayingPanel/PauseButton

**GameEnd UI (5개)**:
- `_winText` → GameEndPanel/WinText
- `_timeText` → GameEndPanel/TimeText
- `_statsPanel` → GameEndPanel/StatsPanel
- `_newGameButton` → GameEndPanel/NewGameButton
- `_mainMenuButton` → GameEndPanel/MainMenuButton

**총 32개 SerializeField 연결 필요**

#### 4.4. SudokuCellButton Prefab 생성

**⚠️ 중요**: SudokuGridUI가 동적으로 81개 셀을 생성하려면 SudokuCellButton Prefab이 필요합니다.

1. **빈 GameObject 생성**:
   - Hierarchy에서 우클릭 → **UI → Button**
   - 이름을 `SudokuCellButton`으로 변경

2. **컴포넌트 구성**:
   ```
   SudokuCellButton (Button)
   ├── Button (컴포넌트) - 자동 생성됨
   ├── Image (컴포넌트) - 자동 생성됨 (배경)
   ├── TextMeshProUGUI (자식 오브젝트) - 숫자 표시
   └── SudokuCellButton (스크립트 추가)
   ```

3. **SudokuCellButton 스크립트 설정**:
   - **Add Component** → `SudokuCellButton` 스크립트 추가
   - Inspector에서 연결:
     - `_button` → Button 컴포넌트
     - `_background` → Image 컴포넌트
     - `_numberText` → TextMeshProUGUI 자식 오브젝트

4. **비주얼 설정** (선택 사항):
   - Normal Color: `(1, 1, 1, 1)` - 흰색
   - Selected Color: `(0.7, 0.9, 1, 1)` - 연한 파란색
   - Fixed Color: `(0.9, 0.9, 0.9, 1)` - 회색
   - Error Color: `(1, 0.6, 0.6, 1)` - 연한 빨간색

5. **Prefab 저장**:
   - Project 창으로 드래그
   - 저장 위치: `Assets/Resources/Prefabs/UI/Sudoku/SudokuCellButton.prefab`

6. **SudokuGridUI 연결**:
   - GridContainer의 SudokuGridUI 컴포넌트 선택
   - Inspector에서 `_cellButtonPrefab` → SudokuCellButton Prefab 연결

#### 4.5. Prefab 저장

1. SudokuUIPanel GameObject를 Project 창으로 드래그
2. 저장 위치: `Assets/Resources/Prefabs/UI/Sudoku/SudokuUIPanel.prefab`
3. Addressables 그룹에 추가:
   - Addressable Name: `UI/SudokuUIPanel`
   - Group: Default Local Group (또는 UI 전용 그룹)

**Addressables 설정**:
- Window → Asset Management → Addressables → Groups
- SudokuUIPanel.prefab 우클릭 → Mark as Addressable
- Address: `UI/SudokuUIPanel`

---

## 5. 추가 UI 컴포넌트 Prefab 생성

### 5.1. NumPad 버튼 구성 (선택 사항)

NumPadUI는 1-9 숫자 버튼과 Clear 버튼을 자동으로 찾습니다. 수동으로 배치하려면:

1. NumPadPanel 하위에 1-9 버튼 생성
2. 각 버튼 이름: `Button1`, `Button2`, ..., `Button9`
3. Clear 버튼 생성: `ClearButton`
4. NumPadUI 컴포넌트가 자동으로 찾음 (`_autoSetupButtons = true`)

### 5.2. LoadingSpinner 회전 애니메이션 (선택 사항)

**방법 1: Animation Clip 사용 (권장)**
1. LoadingSpinner Image 선택
2. Window → Animation → Animation
3. Create New Clip: `LoadingSpinnerRotation.anim`
4. Rotation.Z 키프레임 추가:
   - 0초: 0도
   - 2초: -360도
5. Loop Time 체크

**방법 2: 스크립트 사용**
```csharp
// LoadingSpinner 오브젝트에 추가
public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 180f;

    private void Update()
    {
        transform.Rotate(0f, 0f, -_rotationSpeed * Time.deltaTime);
    }
}
```

---

## 6. GamePlayList에 Sudoku 추가

### MainMenuScene 열기

1. **Scenes/MainMenuScene.unity** 더블클릭
2. Hierarchy에서 `GamePlayList` GameObject 찾기
   - 없으면 빈 GameObject 생성 후 `GamePlayList` 스크립트 추가

### Inspector에서 Sudoku 추가

1. GamePlayList 컴포넌트 선택
2. **Game List** 배열 확장
3. **+** 버튼 클릭하여 새 항목 추가
4. 새 항목 설정:
   - **Game ID**: `Sudoku` (대소문자 정확히 입력)
   - **Is Playable**: ✅ 체크

### 검증

```
Game List 예시:
[0] GameID: "Tetris", IsPlayable: true
[1] GameID: "Sudoku", IsPlayable: true
[2] GameID: "UndeadSurvivor", IsPlayable: true
```

---

## 7. 게임 아이콘 스프라이트 배치

### 아이콘 이미지 준비

1. Sudoku 게임 아이콘 이미지 준비 (PNG, 512x512 권장)
2. 저장 위치: `Assets/Resources/Sprites/Sudoku_icon.png`

### Sprite 설정

1. 아이콘 이미지 선택
2. Inspector 설정:
   - **Texture Type**: Sprite (2D and UI)
   - **Sprite Mode**: Single
   - **Pixels Per Unit**: 100
   - **Filter Mode**: Bilinear
   - **Max Size**: 512 또는 1024
3. **Apply** 클릭

### Addressables 등록

1. Sudoku_icon.png 우클릭 → **Mark as Addressable**
2. Addressable Name 설정: `Sprite/Sudoku_icon`
3. Addressables Groups 창에서 확인

**경로 규칙**:
```
Resources/Sprites/Sudoku_icon.png
→ Addressables Path: "Sprite/Sudoku_icon"
```

---

## 8. 씬 빌드 설정에 추가

### Build Settings 열기

1. **File → Build Settings** 선택

### Sudoku 씬 추가

1. **Add Open Scenes** 버튼 클릭 (Sudoku.unity가 열린 상태에서)
2. 또는 Project 창에서 Sudoku.unity를 드래그하여 Scenes In Build에 추가

### 씬 순서 확인

```
Scenes In Build:
[0] MainMenuScene
[1] Tetris
[2] Sudoku
[3] UndeadSurvivor
[4] Loading (선택 사항)
```

**Scene Index는 중요하지 않습니다** - SceneID enum을 통해 로드하기 때문

---

## 9. 테스트 및 검증

### 씬 단독 테스트

1. Sudoku.unity 씬 열기
2. Play 버튼 클릭
3. 확인 사항:
   - ✅ SudokuScene 초기화 로그 출력
   - ✅ SudokuUIPanel 로드 성공
   - ✅ 게임 시작 메뉴 표시
   - ✅ 난이도 선택 버튼 동작
   - ✅ 게임 로직 정상 작동

### 통합 테스트 (MainMenuScene → Sudoku)

1. MainMenuScene.unity 씬 열기
2. Play 버튼 클릭
3. 게임 선택 메뉴에서 Sudoku 버튼 클릭
4. 확인 사항:
   - ✅ Sudoku 아이콘 정상 표시
   - ✅ 버튼 클릭 시 Sudoku 씬 전환
   - ✅ 게임 정상 실행
   - ✅ Back 버튼으로 메인 메뉴 복귀

### Console 로그 확인

정상 흐름:
```
[INFO] GameRegistry::Awake - 3 games auto-registered
[INFO] DataManager::Awake - 1 providers auto-registered
[INFO] SudokuScene::OnSceneLoaded - Initializing Sudoku scene
[INFO] SudokuScene::OnSceneLoaded - UI Panel opened
[INFO] SudokuScene::OnSceneLoaded - Sudoku game started successfully
[INFO] SudokuGame::Initialize - Sudoku game initialized
[INFO] SudokuGame::StartGame - Sudoku game started
```

### 디버깅 체크리스트

#### 문제: Sudoku 버튼이 메인 메뉴에 안 보임
- [ ] GamePlayList에 Sudoku 추가 확인
- [ ] GameID 철자 정확히 `"Sudoku"` 확인
- [ ] IsPlayable 체크 확인
- [ ] Sudoku_icon.png Addressables 등록 확인

#### 문제: 씬 전환이 안 됨
- [ ] Build Settings에 Sudoku.unity 추가 확인
- [ ] SceneID.cs에 Sudoku 정의 확인 (이미 되어 있음)
- [ ] GameRegistry에 Sudoku 등록 확인 (이미 되어 있음)

#### 문제: UI가 안 보임
- [ ] SudokuUIPanel Prefab Addressables 등록 확인
- [ ] Address 이름: `"UI/SudokuUIPanel"` 정확히 확인
- [ ] Canvas 설정 확인
- [ ] EventSystem 존재 확인

#### 문제: 게임 로직이 안 돌아감
- [ ] SudokuGame.cs 컴파일 에러 없는지 확인
- [ ] SudokuDataProvider 등록 확인 (DataManager.cs)
- [ ] InputManager 이벤트 구독 확인

---

## 추가 설정 (선택 사항)

### 게임별 리소스 폴더 구조

Sudoku 게임 전용 리소스 조직:

```
Assets/Resources/
├── Prefabs/
│   └── UI/
│       └── Sudoku/
│           ├── SudokuUIPanel.prefab
│           └── SudokuCellButton.prefab
├── Sprites/
│   └── Sudoku/
│       ├── Sudoku_icon.png
│       ├── cell_normal.png
│       ├── cell_selected.png
│       └── cell_error.png
├── Audio/
│   ├── BGM/
│   │   └── Sudoku/
│   │       └── sudoku_theme.mp3
│   └── SFX/
│       └── Sudoku/
│           ├── cell_select.wav
│           ├── number_place.wav
│           └── game_complete.wav
└── Data/
    └── Sudoku/
        └── ScriptableObjects/
            └── SudokuDifficultySettings.asset
```

### 사운드 설정

SudokuGame.cs에서 사운드 재생:

```csharp
// BGM 재생
SoundManager.Instance.PlayBGM("Audio/BGM/Sudoku/sudoku_theme");

// SFX 재생
SoundManager.Instance.PlaySFX("Audio/SFX/Sudoku/cell_select");
```

---

## 참고 문서

- **Manager API**: `Assets/Docs/MANAGERS_GUIDE.md`
- **PRD**: `Assets/Docs/sudoku_PRD.md`
- **진행 상황**: `Assets/Docs/Sudoku_Progress.md`
- **Tetris 참고**: `Assets/Scripts/Scenes/TetrisScene.cs`
- **UI 참고**: `Assets/Docs/GameSelectUI_Setup_Guide.md`

---

## 완료 체크리스트

Phase 5 씬 통합 완료 확인:

- [ ] Sudoku.unity 씬 생성 완료
- [ ] SudokuScene 컴포넌트 배치 완료
- [ ] SudokuUIPanel Prefab 생성 및 Addressables 등록 완료
- [ ] GamePlayList에 Sudoku 추가 완료
- [ ] Sudoku_icon.png Addressables 등록 완료
- [ ] Build Settings에 씬 추가 완료
- [ ] 단독 씬 테스트 통과
- [ ] MainMenu → Sudoku 통합 테스트 통과
- [ ] Sudoku → MainMenu 복귀 테스트 통과
- [ ] 게임 로직 정상 작동 확인

---

**마지막 업데이트**: 2025-11-14 (Phase 5 씬 통합)
**다음 단계**: Phase 6 - 테스트 및 검증
