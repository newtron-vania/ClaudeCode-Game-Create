# ThreeMatch 씬 설정 가이드

**작성일**: 2025-11-29
**대상**: Unity Editor에서 ThreeMatch.unity 씬 설정
**참고**: Sudoku_Scene_Setup_Guide.md 패턴 적용

---

## 📋 목차

1. [씬 생성 및 기본 설정](#1-씬-생성-및-기본-설정)
2. [ThreeMatchScene 컴포넌트 설정](#2-threematchscene-컴포넌트-설정)
3. [UI 구조 설정](#3-ui-구조-설정)
4. [ThreeMatchUIPanel 프리팹 생성](#4-threematchuipanel-프리팹-생성)
5. [BoardView 및 InputController 설정](#5-boardview-및-inputcontroller-설정)
6. [GamePlayList 등록](#6-gameplaylist-등록)
7. [테스트 및 검증](#7-테스트-및-검증)

---

## 1. 씬 생성 및 기본 설정

### 1.1 새 씬 생성

1. **Unity Editor** → **File** → **New Scene** 선택
2. **2D (URP)** 템플릿 선택
3. **Save As**: `Assets/Scenes/ThreeMatch.unity`

### 1.2 빌드 설정에 추가

1. **File** → **Build Settings**
2. **Add Open Scenes** 클릭하여 ThreeMatch 씬 추가
3. 씬 인덱스 확인 (MainMenu 다음)

### 1.3 카메라 설정

**Main Camera** 오브젝트 선택:
- **Projection**: Orthographic
- **Size**: 5 (또는 필요에 따라 조정)
- **Background**: Solid Color → 검정색 또는 어두운 색
- **Culling Mask**: Everything

---

## 2. ThreeMatchScene 컴포넌트 설정

### 2.1 씬 컨트롤러 오브젝트 생성

1. Hierarchy에서 **빈 GameObject 생성** (우클릭 → Create Empty)
2. 이름: `ThreeMatchSceneController`
3. **Add Component** → `ThreeMatchScene` 스크립트 추가

### 2.2 Inspector 설정

**ThreeMatchScene** 컴포넌트:
- `_boardView`: (나중에 설정)
- `_inputController`: (나중에 설정)

> ⚠️ **중요**: BoardView와 InputController는 3.5단계에서 설정합니다.

---

## 3. UI 구조 설정

### 3.1 Canvas 생성

1. Hierarchy 우클릭 → **UI** → **Canvas**
2. 이름: `ThreeMatchCanvas`

**Canvas 컴포넌트 설정**:
- **Render Mode**: Screen Space - Overlay
- **Canvas Scaler** 컴포넌트 설정:
  - **UI Scale Mode**: Scale With Screen Size
  - **Reference Resolution**: 1920 x 1080
  - **Screen Match Mode**: Match Width Or Height
  - **Match**: 0.5

### 3.2 EventSystem 확인

Canvas 생성 시 자동으로 EventSystem이 생성됩니다.
- 없으면 Hierarchy 우클릭 → **UI** → **Event System**

---

## 4. ThreeMatchUIPanel 프리팹 생성

### 4.1 UI Panel 루트 오브젝트 생성

1. **ThreeMatchCanvas** 하위에 **빈 GameObject** 생성
2. 이름: `ThreeMatchUIPanel`
3. **Add Component** → `ThreeMatchUIPanel` 스크립트 추가
4. **RectTransform** 설정:
   - **Anchors**: Stretch-Stretch (전체 화면)
   - **Left, Top, Right, Bottom**: 모두 0

### 4.2 5개 메인 패널 생성

**ThreeMatchUIPanel** 하위에 5개 패널 생성:

#### 4.2.1 StartMenuPanel

1. 우클릭 → **UI** → **Panel**
2. 이름: `StartMenuPanel`
3. **하위 UI 요소**:
   - `TitleText` (TextMeshPro - Text): "3-MATCH PUZZLE"
   - `DifficultyDropdown` (Dropdown - TextMeshPro): Easy/Normal/Hard
   - `GameModeDropdown` (Dropdown - TextMeshPro): Classic/MovesLimited/Endless
   - `StartButton` (Button - TextMeshPro): "START GAME"
   - `BackButton` (Button - TextMeshPro): "BACK"

**레이아웃 예시**:
```
StartMenuPanel
├── TitleText (중앙 상단)
├── DifficultyPanel (중앙)
│   ├── DifficultyLabel: "Difficulty"
│   └── DifficultyDropdown
├── GameModePanel (중앙 하단)
│   ├── GameModeLabel: "Game Mode"
│   └── GameModeDropdown
├── StartButton (하단)
└── BackButton (좌측 하단 모서리)
```

#### 4.2.2 PlayingPanel

1. 우클릭 → **UI** → **Panel**
2. 이름: `PlayingPanel`
3. **하위 UI 요소**:
   - `ScoreText` (TextMeshPro): "Score: 0"
   - `TargetScoreText` (TextMeshPro): "Target: 1000"
   - `ComboText` (TextMeshPro): "Combo: x2" (초기 비활성)
   - `TimerText` (TextMeshPro): "Time: 01:00"
   - `MovesText` (TextMeshPro): "Moves: 20"
   - `PauseButton` (Button): "||" (일시정지 아이콘)
   - `ProgressBar` (Slider): 목표 점수 진행도

**레이아웃 예시**:
```
PlayingPanel
├── TopBar (상단)
│   ├── ScoreText (좌측)
│   ├── TargetScoreText (중앙)
│   └── PauseButton (우측)
├── ProgressBar (상단 아래)
├── ComboText (중앙 상단, 콤보 발생 시)
├── TimerText (우측 중앙)
├── MovesText (우측 중앙, 모드별 표시)
└── BoardContainer (중앙, 여기에 보드가 배치됨)
```

#### 4.2.3 PausedPanel

1. 우클릭 → **UI** → **Panel**
2. 이름: `PausedPanel`
3. **하위 UI 요소**:
   - `PausedTitleText` (TextMeshPro): "PAUSED"
   - `ResumeButton` (Button): "RESUME"
   - `RestartButton` (Button): "RESTART"
   - `MainMenuButton` (Button): "MAIN MENU"

#### 4.2.4 GameClearPanel

1. 우클릭 → **UI** → **Panel**
2. 이름: `GameClearPanel`
3. **하위 UI 요소**:
   - `ClearTitleText` (TextMeshPro): "STAGE CLEAR!"
   - `FinalScoreText` (TextMeshPro): "Final Score: 0"
   - `MaxComboText` (TextMeshPro): "Max Combo: x0"
   - `ClearTimeText` (TextMeshPro): "Time: 00:00"
   - `PlayAgainButton` (Button): "PLAY AGAIN"
   - `ClearMainMenuButton` (Button): "MAIN MENU"

#### 4.2.5 GameOverPanel

1. 우클릭 → **UI** → **Panel**
2. 이름: `GameOverPanel`
3. **하위 UI 요소**:
   - `GameOverTitleText` (TextMeshPro): "GAME OVER"
   - `GameOverReasonText` (TextMeshPro): "Time's Up!"
   - `GameOverScoreText` (TextMeshPro): "Score: 0 / 1000"
   - `RetryButton` (Button): "RETRY"
   - `GameOverMainMenuButton` (Button): "MAIN MENU"

### 4.3 ThreeMatchUIPanel Inspector 설정

**ThreeMatchUIPanel** 스크립트 컴포넌트에서 모든 참조 연결:

#### Main Panels
- `_startMenuPanel` → StartMenuPanel 오브젝트
- `_playingPanel` → PlayingPanel 오브젝트
- `_pausedPanel` → PausedPanel 오브젝트
- `_gameClearPanel` → GameClearPanel 오브젝트
- `_gameOverPanel` → GameOverPanel 오브젝트

#### StartMenuPanel Elements
- `_titleText` → TitleText
- `_difficultyDropdown` → DifficultyDropdown
- `_gameModeDropdown` → GameModeDropdown
- `_startButton` → StartButton
- `_backButton` → BackButton

#### PlayingPanel Elements
- `_scoreText` → ScoreText
- `_targetScoreText` → TargetScoreText
- `_comboText` → ComboText
- `_timerText` → TimerText
- `_movesText` → MovesText
- `_pauseButton` → PauseButton
- `_progressBar` → ProgressBar (Slider)

#### PausedPanel Elements
- `_pausedTitleText` → PausedTitleText
- `_resumeButton` → ResumeButton
- `_restartButton` → RestartButton
- `_mainMenuButton` → MainMenuButton

#### GameClearPanel Elements
- `_clearTitleText` → ClearTitleText
- `_finalScoreText` → FinalScoreText
- `_maxComboText` → MaxComboText
- `_clearTimeText` → ClearTimeText
- `_playAgainButton` → PlayAgainButton
- `_clearMainMenuButton` → ClearMainMenuButton

#### GameOverPanel Elements
- `_gameOverTitleText` → GameOverTitleText
- `_gameOverReasonText` → GameOverReasonText
- `_gameOverScoreText` → GameOverScoreText
- `_retryButton` → RetryButton
- `_gameOverMainMenuButton` → GameOverMainMenuButton

### 4.4 초기 활성화 상태 설정

**중요**: 모든 패널을 비활성화 상태로 설정 (StartMenuPanel 제외)
- StartMenuPanel: ✅ Active
- PlayingPanel: ❌ Inactive
- PausedPanel: ❌ Inactive
- GameClearPanel: ❌ Inactive
- GameOverPanel: ❌ Inactive

### 4.5 프리팹으로 저장

1. `ThreeMatchUIPanel` 오브젝트를 `Assets/Resources/Prefabs/UI/ThreeMatch/` 폴더로 드래그
2. 프리팹 이름: `ThreeMatchUIPanel`

---

## 5. BoardView 및 InputController 설정

### 5.1 BoardContainer 생성

1. **PlayingPanel** 하위에 **빈 GameObject** 생성
2. 이름: `BoardContainer`
3. **RectTransform** 설정:
   - **Anchors**: Center
   - **Width**: 700, **Height**: 700 (7x7 보드 기준)
   - **Pos X, Y, Z**: 0, 0, 0

### 5.2 ThreeMatchBoardView 추가

1. **BoardContainer**에 **Add Component** → `ThreeMatchBoardView` 스크립트
2. **Inspector 설정**:

#### Board Layout
- `_boardContainer`: BoardContainer의 Transform
- `_cellSize`: 1.0 (퍼즐 조각 크기)
- `_spacing`: 0.1 (퍼즐 간 간격)

#### Animation Durations
- `_swapDuration`: 0.3 (교체 애니메이션)
- `_destroyDuration`: 0.4 (파괴 애니메이션)
- `_fallDuration`: 0.5 (낙하 애니메이션)
- `_spawnDuration`: 0.3 (생성 애니메이션)

#### Visual Settings
- `_matchEffectColor`: 흰색 또는 밝은 색
- `_highlightColor`: 노란색

### 5.3 InputController 추가

1. **BoardContainer**에 **Add Component** → `InputController` 스크립트
2. **Inspector 설정**:

#### Input Settings
- `_enableMouseInput`: ✅ true
- `_enableTouchInput`: ✅ true
- `_dragThreshold`: 0.5
- `_clickTimeThreshold`: 0.2

#### Visual Feedback
- `_selectionColor`: 노란색 또는 밝은 색
- `_highlightScale`: 1.1 (선택 시 크기 증가)

### 5.4 ThreeMatchScene에 참조 연결

1. **ThreeMatchSceneController** 오브젝트 선택
2. **ThreeMatchScene** 컴포넌트 Inspector:
   - `_boardView` → BoardContainer의 ThreeMatchBoardView 컴포넌트
   - `_inputController` → BoardContainer의 InputController 컴포넌트

---

## 6. GamePlayList 등록

### 6.1 GamePlayList ScriptableObject 찾기

1. **Project** 창에서 검색: `GamePlayList`
2. 경로: `Assets/ScriptableObjects/GamePlayList.asset` (또는 유사 경로)

### 6.2 ThreeMatch 게임 정보 추가

**GamePlayList Inspector**:
1. **Game Infos** 리스트 크기 증가 (+1)
2. 새 항목 설정:
   - **Game ID**: `ThreeMatch`
   - **Is Playable**: ✅ true
   - **Display Name**: "3-Match Puzzle"
   - **Description**: "매치-3 퍼즐 게임"

### 6.3 게임 아이콘 준비

**아이콘 리소스 경로**: `Assets/Resources/Sprites/ThreeMatch_icon.png`

**Addressables 설정**:
1. 아이콘 스프라이트 선택
2. **Inspector** → **Addressables** 체크
3. **Address**: `Sprite/ThreeMatch_icon`

> 📌 **임시**: 아이콘이 없으면 다른 게임 아이콘을 복사하여 임시로 사용 가능

---

## 7. 테스트 및 검증

### 7.1 씬 실행 전 체크리스트

#### ThreeMatchScene 설정
- [ ] ThreeMatchSceneController에 ThreeMatchScene 스크립트 추가됨
- [ ] `_boardView` 참조가 연결됨
- [ ] `_inputController` 참조가 연결됨

#### UI 설정
- [ ] ThreeMatchUIPanel의 모든 패널 참조가 연결됨
- [ ] 모든 버튼, 텍스트, 드롭다운 참조가 연결됨
- [ ] StartMenuPanel만 활성화, 나머지는 비활성화

#### BoardView 설정
- [ ] BoardContainer가 PlayingPanel 하위에 있음
- [ ] ThreeMatchBoardView 스크립트 추가됨
- [ ] 모든 Inspector 파라미터 설정됨

#### InputController 설정
- [ ] InputController 스크립트 추가됨
- [ ] 마우스/터치 입력 활성화됨

#### GamePlayList 등록
- [ ] ThreeMatch 게임 정보 추가됨
- [ ] GameID가 정확히 "ThreeMatch"임

### 7.2 씬 실행 테스트

1. **Play 버튼** 클릭
2. **기대 동작**:
   - StartMenuPanel이 표시됨
   - 난이도/게임모드 드롭다운이 작동함
   - START GAME 버튼 클릭 시 게임 시작
   - PlayingPanel로 전환됨

### 7.3 콘솔 로그 확인

**정상 로그 예시**:
```
[ThreeMatchScene] Initializing ThreeMatch scene
[ThreeMatchScene] UI Panel opened
[ThreeMatchScene] ThreeMatch game started and UI initialized
[ThreeMatchUIPanel] UI initialized
[ThreeMatchUIPanel] Showing panel: StartMenu
```

### 7.4 오류 해결

#### "BoardView is not assigned in Inspector"
→ ThreeMatchScene의 `_boardView` 참조 연결 확인

#### "InputController is not assigned in Inspector"
→ ThreeMatchScene의 `_inputController` 참조 연결 확인

#### UI 패널이 표시되지 않음
→ ThreeMatchUIPanel의 패널 참조 연결 확인

#### 버튼 클릭이 작동하지 않음
→ EventSystem이 씬에 있는지 확인

---

## 8. 메인 메뉴에서 접근 테스트

### 8.1 메인 메뉴 씬으로 이동

1. **MainMenuScene** 열기
2. **Play** 클릭
3. 3-Match Puzzle 버튼 확인

### 8.2 게임 전환 테스트

1. 3-Match Puzzle 버튼 클릭
2. ThreeMatch 씬으로 전환되는지 확인
3. StartMenuPanel이 표시되는지 확인

---

## 9. 완료 체크리스트

Phase 3.3 완료 조건:
- [ ] ThreeMatch.unity 씬 생성 완료
- [ ] ThreeMatchScene 컴포넌트 설정 완료
- [ ] ThreeMatchUIPanel 5-상태 패널 구조 완료
- [ ] BoardView 및 InputController 설정 완료
- [ ] GamePlayList에 ThreeMatch 등록 완료
- [ ] 씬 실행 시 정상 작동 확인
- [ ] 메인 메뉴에서 게임 접근 가능

---

## 10. 참고 자료

- **Sudoku 씬 설정 가이드**: `Assets/Docs/Sudoku_Scene_Setup_Guide.md`
- **ThreeMatch 아키텍처**: `Assets/Docs/3-match-architecture.md`
- **ThreeMatch 진행 상황**: `Assets/Docs/3-match-progress.md`
- **Manager 가이드**: `Assets/Docs/MANAGERS_GUIDE.md`

---

## 📝 작업 시 팁

1. **저장 자주하기**: Ctrl+S (Win) / Cmd+S (Mac)
2. **씬 저장**: File → Save Scene
3. **프리팹 업데이트**: 오브젝트 수정 후 프리팹에 적용
4. **콘솔 확인**: 오류 로그 주시
5. **참조 연결 확인**: Inspector에서 "None" 표시가 없는지 확인

---

**작성자**: Claude Code
**버전**: 1.0
**최종 수정**: 2025-11-29
