# GameSelectUI 설정 가이드

게임 선택 UI를 GameSelectScene에 적용하기 위한 단계별 가이드입니다.

## 1. 필수 리소스 준비

### 1.1 게임 아이콘 스프라이트 준비
```
필요한 파일:
- Tetris_icon.png (또는 .jpg)
- 향후 추가될 게임들의 아이콘 (예: Sudoku_icon.png)

위치: Assets/Resources/Sprite/
```

**스프라이트 설정**:
1. 아이콘 이미지를 `Assets/Resources/Sprite/` 폴더에 추가
2. Inspector에서 Texture Type을 `Sprite (2D and UI)`로 설정
3. Sprite Mode를 `Single`로 설정
4. Apply 클릭

### 1.2 GameSelectButton 프리팹 생성

**단계**:
1. Hierarchy에서 우클릭 → UI → Button 생성
2. 버튼 이름을 `GameSelectButton`으로 변경
3. 버튼에 `GameSelectButton.cs` 스크립트 추가
4. Button 컴포넌트 설정:
   - Transition: `Color Tint`
   - Normal Color: White (255, 255, 255, 255)
   - Highlighted Color: Light Gray (230, 230, 230, 255)
   - Pressed Color: Dark Gray (180, 180, 180, 255)
   - Selected Color: White
5. 버튼 크기 조정 (RectTransform):
   - Width: 150
   - Height: 150
6. 버튼의 자식 Image 오브젝트 확인 (아이콘이 표시될 곳)
7. Prefab 저장: 버튼을 `Assets/Resources/Prefabs/UI/` 폴더로 드래그

## 2. Addressables 설정

### 2.1 Addressables 그룹 생성 (처음 사용 시)

**Window → Asset Management → Addressables → Groups**

기본 그룹이 없으면:
1. `Create → Group → Packed Assets` 클릭
2. 그룹 이름: `UI` (UI 관련 리소스용)
3. 그룹 이름: `Sprites` (스프라이트 리소스용)

### 2.2 리소스를 Addressables로 등록

**GameSelectButton 프리팹**:
1. `Assets/Resources/Prefabs/UI/GameSelectButton.prefab` 선택
2. Inspector에서 `Addressable` 체크박스 체크
3. Address 이름: `Prefabs/UI/GameSelectButton` (정확히 입력)
4. Group: `UI` 선택

**게임 아이콘 스프라이트**:
1. `Assets/Resources/Sprite/Tetris_icon` 선택
2. Inspector에서 `Addressable` 체크박스 체크
3. Address 이름: `Sprite/Tetris_icon` (정확히 입력)
4. Group: `Sprites` 선택

**추가 게임 아이콘도 동일하게 등록**:
- `Sprite/Sudoku_icon`
- `Sprite/Puzzle_icon`
- 등...

## 3. GameSelectScene 설정

### 3.1 GamePlayList 오브젝트 생성

1. Hierarchy에서 빈 GameObject 생성
2. 이름: `GamePlayList`
3. `GamePlayList.cs` 스크립트 추가
4. Inspector에서 게임 목록 설정:
   ```
   Game List:
   - Size: 1 (현재 Tetris만 있으므로)
   - Element 0:
     - Game ID: "Tetris"
     - Is Playable: ✓ (체크)
   ```

**향후 게임 추가 시**:
```
Size: 3
Element 0: Game ID = "Tetris", Is Playable = ✓
Element 1: Game ID = "Sudoku", Is Playable = ✓
Element 2: Game ID = "Puzzle", Is Playable = ☐ (준비 중)
```

### 3.2 Canvas 설정

**Canvas가 없으면**:
1. Hierarchy 우클릭 → UI → Canvas
2. Canvas Scaler 설정:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Screen Match Mode: `Match Width Or Height`
   - Match: `0.5` (중간)

### 3.3 GameSelectUIPanel 오브젝트 생성

1. Canvas 하위에 빈 GameObject 생성
2. 이름: `GameSelectUIPanel`
3. `GameSelectUIPanel.cs` 스크립트 추가
4. RectTransform 설정:
   - Anchor Presets: Stretch (전체 화면)
   - Left, Top, Right, Bottom: 0
5. Canvas 컴포넌트 추가 (자동으로 추가될 수 있음)
6. Canvas Group 컴포넌트 추가

**Inspector 설정**:
```
GameSelectUIPanel (Script):
- Game Play List: GamePlayList 오브젝트 드래그
- Spawn Position: 버튼 생성 위치 RectTransform (아래 참조)
- Button Prefab Address: "Prefabs/UI/GameSelectButton"
- Button Spacing: 150
- Max Buttons Per Row: 3
```

### 3.4 Spawn Position 설정

**Option 1: GameSelectUIPanel 자체를 사용**
- Spawn Position 필드를 비워두면 자동으로 GameSelectUIPanel의 RectTransform 사용
- 버튼들이 화면 중앙에 생성됨

**Option 2: 별도 위치 지정**
1. GameSelectUIPanel 하위에 빈 GameObject 생성
2. 이름: `ButtonSpawnPoint`
3. RectTransform 추가
4. 위치 설정 (예: 화면 중앙):
   - Anchor: Center
   - Pos X: 0
   - Pos Y: 0
5. GameSelectUIPanel의 `Spawn Position` 필드에 이 RectTransform 드래그

## 4. 씬 흐름 연결

### 4.1 GameSelectScene 초기화 스크립트

**현재 프로젝트에는 이미 `MainMenuScene.cs` (= `GameSelectScene`) 파일이 존재합니다.**

파일 위치: `Assets/Scripts/Scenes/MainMenuScene.cs`

**이미 구현된 내용**:
- UIManager를 통한 페이드 효과
- GameSelectUIPanel 자동 찾기 및 Open
- 씬 종료 시 패널 정리

**GameSelectScene에 연결**:
1. Hierarchy에서 빈 GameObject가 있는지 확인 (없으면 생성)
2. 이름: `SceneController` (또는 기존 이름 유지)
3. Inspector에서 `MainMenuScene.cs` 스크립트가 연결되어 있는지 확인
4. `Game Select Panel` 필드에 GameSelectUIPanel GameObject 드래그

### 4.2 Build Settings 확인

**File → Build Settings**:
```
Scenes In Build:
0. GameSelectScene
1. Tetris
```

씬 순서대로 Build Index가 할당됩니다.

## 5. 테스트

### 5.1 Play Mode 테스트

1. GameSelectScene 열기
2. Play 버튼 클릭
3. 확인 사항:
   - GameSelectUIPanel이 보이는가?
   - Tetris 버튼이 생성되었는가?
   - 버튼에 아이콘이 표시되는가?
   - 버튼 클릭 시 Tetris 씬으로 이동하는가?

### 5.2 디버그 로그 확인

**Console 창에서 다음 로그 확인**:
```
[INFO] GamePlayList::GetPlayableGames - Found 1 playable games
[INFO] GameSelectUIPanel::CreateGameButtons - Creating 1 game buttons
[INFO] ResourceManager::LoadAsync - Loaded and cached resource: Prefabs/UI/GameSelectButton
[INFO] GameSelectButton::Init - Initialized button for game 'Tetris'
[INFO] ResourceManager::LoadAsync - Loaded and cached resource: Sprite/Tetris_icon
[INFO] GameSelectButton::LoadIcon - Loaded icon for 'Tetris'
```

**오류가 발생하면**:
- Addressable 주소가 정확한지 확인
- 리소스가 Addressables에 등록되었는지 확인
- 스크립트 컴파일 오류가 없는지 확인

## 6. 문제 해결

### 버튼이 생성되지 않음
- GamePlayList의 게임 목록이 비어있지 않은지 확인
- `Is Playable`이 체크되어 있는지 확인
- Console에서 오류 메시지 확인

### 아이콘이 표시되지 않음
- 스프라이트 파일이 올바른 위치에 있는지 확인
- Addressable 주소가 정확한지 확인 (`Sprite/Tetris_icon`)
- 스프라이트 Import Settings이 올바른지 확인

### 버튼 클릭 시 씬 전환 안 됨
- Tetris 씬이 Build Settings에 추가되어 있는지 확인
- CustomSceneManager가 씬에 있는지 확인
- UIManager가 씬에 있는지 확인

### Addressables 관련 오류
- `Window → Asset Management → Addressables → Groups`에서 빌드
- `Build → New Build → Default Build Script` 실행

## 7. 향후 게임 추가 시

새 게임 추가 절차:
1. 게임 아이콘 준비 (`GameName_icon.png`)
2. Addressables에 등록 (`Sprite/GameName_icon`)
3. GamePlayList에 게임 추가:
   - Game ID: `"GameName"`
   - Is Playable: `✓`
4. 게임 씬 생성 (GameName.unity)
5. Build Settings에 씬 추가

자동으로 GameSelectScene에 버튼이 추가됩니다!

## 8. 참고 사항

### Addressables 주소 규칙
- 프리팹: `Prefabs/{카테고리}/{이름}`
- 스프라이트: `Sprite/{이름}`
- 오디오: `Audio/BGM/{이름}` 또는 `Audio/SFX/{이름}`

### 게임 ID 명명 규칙
- PascalCase 사용 (예: `Tetris`, `SlidingPuzzle`)
- 씬 이름과 동일하게 유지
- GameRegistry에 등록된 ID와 일치해야 함

### 성능 고려사항
- 아이콘 스프라이트 크기: 512x512 권장
- 압축: RGBA Compressed ETC2 (모바일) 또는 DXT5 (PC)
- Max Size: 512

## 완료!

모든 설정이 완료되었으면 GameSelectScene을 Play하여 게임 선택 UI가 정상 작동하는지 확인하세요.




📝 Unity 작업 체크리스트

  1단계: 리소스 준비 (Assets/Resources/)

  아이콘 스프라이트

  - Assets/Resources/Sprite/Tetris_icon.png 준비
  - Inspector에서 Texture Type: Sprite (2D and UI) 설정
  - Sprite Mode: Single 설정
  - Apply 클릭

  GameSelectButton 프리팹

  - Hierarchy → UI → Button 생성
  - 이름: GameSelectButton으로 변경
  - GameSelectButton.cs 스크립트 컴포넌트 추가
  - Button 컴포넌트 설정:
    - Transition: Color Tint
    - Colors: Normal(White), Highlighted(Light Gray), Pressed(Dark Gray)
  - RectTransform: Width 150, Height 150
  - Prefab 저장: Assets/Resources/Prefabs/UI/GameSelectButton.prefab

  2단계: Addressables 설정

  Window → Asset Management → Addressables → Groups

  GameSelectButton 프리팹 등록:
  - GameSelectButton.prefab 선택
  - Addressable 체크박스 체크
  - Address: Prefabs/UI/GameSelectButton (정확히 입력!)

  Tetris 아이콘 등록:
  - Tetris_icon 스프라이트 선택
  - Addressable 체크박스 체크
  - Address: Sprite/Tetris_icon (정확히 입력!)

  Addressables 빌드:
  - Groups 창에서 Build → New Build → Default Build Script

  3단계: GameSelectScene (MainMenuScene) 구성

  GamePlayList 오브젝트

  - Hierarchy에 GameObject 생성
  - 이름: GamePlayList
  - GamePlayList.cs 스크립트 추가
  - Inspector에서 게임 목록 설정:
  Game List:
  - Size: 1
  - Element 0:
    - Game ID: "Tetris"
    - Is Playable: ✓

  Canvas 및 GameSelectUIPanel

  - Canvas 생성 (없으면)
  - Canvas Scaler:
    - UI Scale Mode: Scale With Screen Size
    - Reference Resolution: 1920 x 1080
  - Canvas 하위에 GameObject 생성
  - 이름: GameSelectUIPanel
  - GameSelectUIPanel.cs 스크립트 추가
  - RectTransform: Anchor Stretch (전체 화면)
  - Inspector 설정:
    - Game Play List: GamePlayList 오브젝트 드래그
    - Spawn Position: 비워두기 (자동으로 자신의 RectTransform 사용)
    - Button Prefab Address: "Prefabs/UI/GameSelectButton"
    - Button Spacing: 150
    - Max Buttons Per Row: 3

  SceneController

  - Hierarchy에 GameObject 생성 (또는 기존 SceneController 사용)
  - 이름: SceneController
  - MainMenuScene.cs 스크립트가 연결되어 있는지 확인
  - Inspector:
    - Game Select Panel: GameSelectUIPanel 드래그

  4단계: 테스트

  Console 로그 확인 (예상 로그)

  [INFO] MainMenuScene::Awake - ...
  [INFO] MainMenuScene::OnSceneLoaded - ...
  [INFO] GamePlayList::GetPlayableGames - Found 1 playable games
  [INFO] GameSelectUIPanel::CreateGameButtons - Creating 1 game buttons
  [INFO] ResourceManager::LoadAsync - Loaded and cached resource: Prefabs/UI/GameSelectButton
  [INFO] GameSelectButton::Init - Initialized button for game 'Tetris'
  [INFO] ResourceManager::LoadAsync - Loaded and cached resource: Sprite/Tetris_icon
  [INFO] GameSelectButton::LoadIcon - Loaded icon for 'Tetris'

  동작 확인

  - Play 모드 실행
  - 페이드 아웃 효과 확인
  - Tetris 버튼이 화면에 생성되는지 확인
  - 버튼에 아이콘이 표시되는지 확인
  - 버튼 클릭 시 Tetris 씬으로 전환되는지 확인

  5단계: 문제 해결

  버튼이 안 보이면:
  - Console에서 오류 메시지 확인
  - GamePlayList에 Tetris가 등록되고 IsPlayable이 체크되어 있는지 확인
  - Addressables가 빌드되었는지 확인

  아이콘이 안 보이면:
  - Addressable 주소가 정확한지 확인: Sprite/Tetris_icon
  - 스프라이트 Import Settings 확인

  씬 전환이 안 되면:
  - Build Settings에 Tetris 씬이 추가되어 있는지 확인
  - CustomSceneManager, UIManager가 씬에 있는지 확인
