# 3-Match Puzzle Game 개발 진행 상황

**프로젝트**: ClaudeCode-Game-Create
**게임 ID**: ThreeMatch
**작성일**: 2025-11-24
**현재 상태**: 구현 준비 완료 → 개발 시작 대기

---

## 📋 프로젝트 개요

### 기본 정보
- **장르**: 3-Match Puzzle
- **플랫폼**: PC (Unity 6)
- **렌더 파이프라인**: URP 2D
- **아키텍처**: IMiniGame 인터페이스 기반 플러그인 패턴
- **목표**: 교착 상태 없는 매치-3 퍼즐 게임

### 핵심 기능
- **매치 시스템**: 3개 이상 연속 매칭 (가로/세로)
- **점수 계산**: 매치 형태별 차등 점수 (100~2,000점)
- **콤보 시스템**: 연쇄 매치 시 점수 배율 증가
- **게임 모드**: 클래식(시간), 이동 횟수, 무한 모드
- **난이도**: Easy(6×6), Normal(7×7), Hard(8×8)

---

## 📊 전체 진행 상황

### Phase 별 진행도

| Phase | 작업 내용 | 상태 | 진행률 |
|-------|----------|------|--------|
| Phase 1 | 데이터 구조 및 Manager 통합 | ⏳ 대기 | 0% |
| Phase 2 | 핵심 게임 로직 | ⏳ 대기 | 0% |
| Phase 3 | ThreeMatchGame (IMiniGame 구현) | ⏳ 대기 | 0% |
| Phase 4 | Unity 씬 및 UI 통합 | ⏳ 대기 | 0% |
| Phase 5 | 리소스 및 에셋 통합 | ⏳ 대기 | 0% |
| Phase 6 | 게임 모드 및 난이도 | ⏳ 대기 | 0% |
| Phase 7 | 폴리싱 및 테스트 | ⏳ 대기 | 0% |

**전체 진행률**: 0% (준비 단계)

---

## 🎯 Phase 1: 데이터 구조 및 Manager 통합

### 목표
ScriptableObject 기반 게임 데이터 구조 및 DataManager 통합

### 작업 항목

#### 1.1 ThreeMatchDataProvider 구현
- [ ] `Assets/Scripts/ThreeMatch/Data/ThreeMatchDataProvider.cs` 생성
- [ ] `IGameDataProvider` 인터페이스 구현
- [ ] `GameID = "ThreeMatch"` 설정
- [ ] 데이터 딕셔너리 초기화 (PieceType, Difficulty, GameMode)
- [ ] `LoadData()`: Resources에서 ScriptableObject 로드
- [ ] `UnloadData()`: 메모리 정리

**예상 소요**: 1-2시간

#### 1.2 ThreeMatchGameData 구현
- [ ] `Assets/Scripts/ThreeMatch/ThreeMatchGameData.cs` 생성
- [ ] `IGameData` 인터페이스 구현
- [ ] 런타임 게임 상태 필드 정의 (점수, 콤보, 남은 시간/이동)
- [ ] `SaveState()`: 하이스코어 PlayerPrefs 저장
- [ ] `LoadState()`: 하이스코어 로드

**예상 소요**: 1시간

#### 1.3 ScriptableObject 데이터 생성
- [ ] `Assets/Resources/Data/ThreeMatch/ScriptableObjects/` 폴더 생성
- [ ] `PieceTypeData.cs` 스크립트 생성 (7종 퍼즐 정의)
- [ ] `DifficultyConfig.cs` 스크립트 생성 (Easy/Normal/Hard)
- [ ] `GameModeConfig.cs` 스크립트 생성 (Classic/MovesLimited/Endless)
- [ ] `PieceTypeDataList.asset` 생성 및 설정
- [ ] `DifficultyConfigList.asset` 생성 및 설정
- [ ] `GameModeConfigList.asset` 생성 및 설정

**예상 소요**: 2-3시간

#### 1.4 DataManager 등록
- [ ] `ThreeMatchDataProvider` 인스턴스 생성
- [ ] `DataManager.Instance.RegisterProvider()` 호출
- [ ] 로드/언로드 테스트

**예상 소요**: 30분

### Phase 1 완료 조건
- ✅ ThreeMatchDataProvider가 DataManager에 등록됨
- ✅ ScriptableObject 데이터 로드/언로드 정상 작동
- ✅ ThreeMatchGameData 초기화/검증 테스트 통과

---

## 🎮 Phase 2: 핵심 게임 로직

### 목표
보드 관리, 매치 감지, 입력 처리 등 핵심 게임 메커니즘 구현

### 작업 항목

#### 2.1 BoardManager 구현
- [ ] `Assets/Scripts/ThreeMatch/Board/BoardManager.cs` 생성
- [ ] 보드 초기화 (`GenerateInitialBoard()`)
- [ ] 초기 생성 시 매치 방지 (`GetNoMatchRandomPiece()`)
- [ ] Deadlock 감지 (`IsDeadlocked()`)
- [ ] 보드 재생성 (`ShuffleBoard()`)
- [ ] 퍼즐 교체 (`SwapPieces()`)

**핵심 로직**:
```csharp
- int[,] _board: 보드 상태 (0: 빈칸, 1~N: 퍼즐 ID)
- IsDeadlocked(): 모든 위치에서 가상 스왑 시도 → 매치 가능 여부 확인
- ShuffleBoard(): Deadlock 해소될 때까지 재생성 (최대 10회)
```

**예상 소요**: 3-4시간

#### 2.2 MatchDetector 구현
- [ ] `Assets/Scripts/ThreeMatch/Board/MatchDetector.cs` 생성
- [ ] 매치 타입 enum 정의 (Basic3/4/5, Cross33/43/53)
- [ ] `FindAllMatches()`: 전체 보드에서 매치 찾기
- [ ] `FindMatchAt()`: 특정 위치에서 가로/세로 매치 확인
- [ ] `CalculateScore()`: 매치 타입별 점수 계산 + 콤보 배율

**점수 테이블**:
| 매치 형태 | 점수 |
|----------|------|
| 5개 + 3개 (크로스) | 2,000 |
| 4개 + 3개 (크로스) | 1,500 |
| 3개 + 3개 (크로스) | 1,000 |
| 5개 (일렬) | 1,000 |
| 4개 (일렬) | 500 |
| 3개 (일렬) | 100 |

**예상 소요**: 3-4시간

#### 2.3 InputController 구현
- [ ] `Assets/Scripts/ThreeMatch/Input/InputController.cs` 생성
- [ ] 퍼즐 선택 로직 (`SelectPiece()`)
- [ ] 인접 확인 (`IsAdjacent()`)
- [ ] 교체 요청 이벤트 (`OnSwapRequested`)
- [ ] 처리 중 상태 관리 (`IsProcessing`)

**예상 소요**: 2시간

#### 2.4 PuzzlePiece 구현
- [ ] `Assets/Scripts/ThreeMatch/Board/PuzzlePiece.cs` 생성
- [ ] `IPoolable` 인터페이스 구현
- [ ] 좌표 설정 (`SetCoord()`)
- [ ] 퍼즐 타입 및 스프라이트 설정 (`SetPieceType()`)
- [ ] 이동 애니메이션 (`MoveToPosition()`, `MoveRoutine()`)

**예상 소요**: 2시간

#### 2.5 ComboSystem 구현
- [ ] `Assets/Scripts/ThreeMatch/Systems/ComboSystem.cs` 생성
- [ ] 콤보 카운터 증가/리셋
- [ ] 콤보 배율 계산
- [ ] 콤보 UI 업데이트 이벤트

**예상 소요**: 1시간

### Phase 2 완료 조건
- ✅ 보드 생성 시 초기 매치 없음
- ✅ Deadlock 감지 및 Shuffle 정상 작동
- ✅ 매치 감지 정확도 100% (단위 테스트)
- ✅ 점수 계산 정확도 검증
- ✅ 퍼즐 이동 애니메이션 부드러움

---

## 🏗️ Phase 3: ThreeMatchGame (IMiniGame 구현)

### 목표
IMiniGame 인터페이스 구현 및 게임 상태 관리

### 작업 항목

#### 3.1 ThreeMatchGame 클래스 구현
- [ ] `Assets/Scripts/ThreeMatch/ThreeMatchGame.cs` 생성
- [ ] `IMiniGame` 인터페이스 구현
- [ ] 게임 상태 enum (StartMenu/Playing/Paused/GameOver)
- [ ] Activity Action 패턴 적용 (Sudoku 참고)
  - [ ] `StartMenuActivityAction`
  - [ ] `PlayingActivityAction`
  - [ ] `PausedActivityAction`
  - [ ] `GameOverActivityAction`

**예상 소요**: 3시간

#### 3.2 게임 로직 통합
- [ ] `Initialize()`: DataProvider 로드, 컴포넌트 생성
- [ ] `StartGame()`: InputManager 이벤트 구독, 보드 초기화
- [ ] `Update()`: 게임 루프 (시간/이동 횟수 감소, Deadlock 체크)
- [ ] `Cleanup()`: 이벤트 구독 해제, 데이터 언로드

**예상 소요**: 3시간

#### 3.3 매치 및 슬라이딩 로직
- [ ] 퍼즐 교체 시 매치 확인
- [ ] 유효하지 않은 교체 원위치
- [ ] 매치 파괴 및 점수 계산
- [ ] 슬라이딩 (빈칸 채우기)
- [ ] 연쇄 매치 감지 및 콤보 증가

**예상 소요**: 4-5시간

### Phase 3 완료 조건
- ✅ ThreeMatchGame이 GameRegistry에 등록됨
- ✅ 게임 상태 전환 정상 작동
- ✅ 매치 → 파괴 → 슬라이딩 → 연쇄 매치 플로우 완성
- ✅ InputManager 이벤트 구독/해제 정상

---

## 🎨 Phase 4: Unity 씬 및 UI 통합

### 목표
Unity 씬 설정 및 4-상태 UI 패널 구현

### 작업 항목

#### 4.1 ThreeMatchScene 구현
- [ ] `Assets/Scripts/ThreeMatch/Scenes/ThreeMatchScene.cs` 생성
- [ ] `BaseScene` 상속
- [ ] UI 이벤트 구독 (`SubscribeUIEvents()`)
- [ ] Activity Actions 등록 (게임 → UI 연결)

**예상 소요**: 2시간

#### 4.2 ThreeMatchUIPanel 구현
- [ ] `Assets/Scripts/ThreeMatch/UI/ThreeMatchUIPanel.cs` 생성
- [ ] `UIPanel` 상속
- [ ] 4-상태 패널 구현
  - [ ] `ShowStartMenuPanel()`: 난이도/모드 선택
  - [ ] `ShowPlayingPanel()`: 게임 보드, 점수, 콤보, 타이머
  - [ ] `ShowPausedPanel()`: 재개/재시작/메인으로
  - [ ] `ShowGameOverPanel()`: 결과 표시
- [ ] UI 이벤트 정의 (`OnDifficultySelected`, `OnGameModeSelected`, etc.)

**예상 소요**: 4-5시간

#### 4.3 Unity 씬 설정
- [ ] `Assets/Scenes/ThreeMatch.unity` 생성
- [ ] Canvas 및 EventSystem 설정
- [ ] ThreeMatchUIPanel 프리팹 생성 및 배치
- [ ] 보드 시각화 영역 설정
- [ ] 카메라 설정 (Orthographic, 2D)

**예상 소요**: 2시간

#### 4.4 보드 시각화
- [ ] 퍼즐 생성 위치 계산 (Grid Layout)
- [ ] PuzzlePiece 프리팹 인스턴스화 (PoolManager 사용)
- [ ] 보드 → UI 동기화

**예상 소요**: 2-3시간

### Phase 4 완료 조건
- ✅ ThreeMatch.unity 씬이 정상적으로 로드됨
- ✅ 4-상태 패널 전환 작동
- ✅ UI 이벤트 → 게임 로직 연결 확인
- ✅ 보드 시각화 정상 (퍼즐 배치 및 이동)

---

## 🎁 Phase 5: 리소스 및 에셋 통합

### 목표
퍼즐 스프라이트, 오디오, Addressables 설정

### 작업 항목

#### 5.1 퍼즐 스프라이트 준비
- [ ] 7종 퍼즐 스프라이트 에셋 준비
  - [ ] Piece_Red
  - [ ] Piece_Blue
  - [ ] Piece_Green
  - [ ] Piece_Yellow
  - [ ] Piece_Purple
  - [ ] Piece_Orange
  - [ ] Piece_Pink
- [ ] `Assets/Resources/Sprites/ThreeMatch/` 폴더에 배치
- [ ] Sprite 설정 (Pixels Per Unit, Filter Mode)

**예상 소요**: 1-2시간 (에셋 제작 시간 제외)

#### 5.2 Addressables 그룹 설정
- [ ] ThreeMatch Addressables 그룹 생성
- [ ] 퍼즐 프리팹 등록 (`Prefabs/ThreeMatch/Piece`)
- [ ] 스프라이트 등록 (`Sprites/ThreeMatch/Piece_*`)
- [ ] UI 패널 등록 (`UI/ThreeMatchUIPanel`)
- [ ] 오디오 등록 (BGM, SFX)

**주소 규칙**:
```
Prefabs/ThreeMatch/Piece
Prefabs/ThreeMatch/MatchEffect
Sprites/ThreeMatch/Piece_Red
Audio/BGM/ThreeMatch/Theme
Audio/SFX/ThreeMatch/Match3
```

**예상 소요**: 1시간

#### 5.3 오디오 클립 준비
- [ ] BGM: Theme (메인 BGM)
- [ ] SFX: Match3, Match4, Match5, Combo, Swap, InvalidMove
- [ ] `Assets/Resources/Audio/BGM/ThreeMatch/` 폴더에 배치
- [ ] `Assets/Resources/Audio/SFX/ThreeMatch/` 폴더에 배치

**예상 소요**: 1-2시간 (에셋 제작 시간 제외)

#### 5.4 풀 생성 및 프리로드
- [ ] ResourceManager를 통한 퍼즐 풀 생성
  ```csharp
  ResourceManager.Instance.CreatePool("Prefabs/ThreeMatch/Piece", 64, 200, true);
  ```
- [ ] 스프라이트 프리로드
- [ ] 오디오 프리로드

**예상 소요**: 1시간

### Phase 5 완료 조건
- ✅ 모든 리소스가 Addressables에 등록됨
- ✅ 퍼즐 풀링 정상 작동 (생성/반환)
- ✅ 스프라이트 로드 및 적용 확인
- ✅ BGM/SFX 재생 테스트 통과

---

## 🎮 Phase 6: 게임 모드 및 난이도

### 목표
3가지 게임 모드 및 3가지 난이도 구현

### 작업 항목

#### 6.1 클래식 모드 구현
- [ ] 시간 제한 로직 (60초/90초/120초)
- [ ] 타이머 UI 업데이트
- [ ] 목표 점수 달성 체크
- [ ] 시간 종료 시 GameOver

**예상 소요**: 2시간

#### 6.2 이동 횟수 모드 구현
- [ ] 이동 횟수 제한 (20회/30회/40회)
- [ ] 남은 이동 횟수 UI 업데이트
- [ ] 이동 횟수 소진 시 GameOver

**예상 소요**: 2시간

#### 6.3 무한 모드 구현
- [ ] 제한 없음
- [ ] Deadlock 감지 시 Shuffle (3회 실패 시 GameOver)
- [ ] 하이스코어 추적

**예상 소요**: 1시간

#### 6.4 난이도별 설정 적용
- [ ] Easy: 6×6 보드, 5종 퍼즐, 목표 1,000점
- [ ] Normal: 7×7 보드, 6종 퍼즐, 목표 2,000점
- [ ] Hard: 8×8 보드, 7종 퍼즐, 목표 3,500점

**예상 소요**: 1시간

### Phase 6 완료 조건
- ✅ 3가지 게임 모드 모두 정상 작동
- ✅ 3가지 난이도 선택 및 적용 확인
- ✅ 종료 조건 정상 작동 (시간/이동/Deadlock)

---

## ✨ Phase 7: 폴리싱 및 테스트

### 목표
게임 완성도 향상 및 통합 테스트

### 작업 항목

#### 7.1 매치 이펙트 및 사운드
- [ ] 매치 파괴 이펙트 (파티클 또는 애니메이션)
- [ ] 매치 타입별 사운드 재생 (Match3/4/5)
- [ ] 콤보 사운드 및 이펙트

**예상 소요**: 3시간

#### 7.2 콤보 UI 이펙트
- [ ] 콤보 카운터 UI 애니메이션
- [ ] 콤보 배율 표시
- [ ] 콤보 초기화 피드백

**예상 소요**: 2시간

#### 7.3 Deadlock 처리
- [ ] Deadlock 감지 시 경고 UI
- [ ] Shuffle 애니메이션
- [ ] Shuffle 실패 시 GameOver 처리

**예상 소요**: 2시간

#### 7.4 슬라이딩 애니메이션 최적화
- [ ] 부드러운 이동 애니메이션 (SmoothStep)
- [ ] 동시 이동 최적화
- [ ] 애니메이션 중 입력 차단

**예상 소요**: 2-3시간

#### 7.5 통합 테스트
- [ ] 게임 시작 → 플레이 → GameOver 플로우 테스트
- [ ] 모든 게임 모드 + 난이도 조합 테스트
- [ ] Deadlock 감지 및 Shuffle 테스트
- [ ] 성능 테스트 (60 FPS 유지)
- [ ] 메모리 누수 확인

**예상 소요**: 3-4시간

#### 7.6 밸런스 조정
- [ ] 난이도별 목표 점수 밸런싱
- [ ] 시간 제한 조정
- [ ] 이동 횟수 조정
- [ ] Shuffle 빈도 확인

**예상 소요**: 2시간

### Phase 7 완료 조건
- ✅ 모든 이펙트 및 사운드 적용
- ✅ 60 FPS 유지 (8×8 보드 기준)
- ✅ 메모리 누수 없음
- ✅ 밸런스 테스트 완료
- ✅ QA 체크리스트 100% 통과

---

## 🧪 테스트 계획

### 단위 테스트
- [ ] `BoardManager.IsDeadlocked()` - 교착 상태 감지 정확도
- [ ] `MatchDetector.FindAllMatches()` - 매치 감지 정확도
- [ ] `MatchDetector.CalculateScore()` - 점수 계산 검증
- [ ] `BoardManager.GetNoMatchRandomPiece()` - 초기 생성 매치 방지

### 통합 테스트
- [ ] 게임 시작 → 플레이 → 게임 오버 전체 플로우
- [ ] 일시정지 → 재개 → 재시작
- [ ] Deadlock 감지 → Shuffle → 계속 플레이
- [ ] 콤보 시스템 - 연속 매치 점수 배율 적용

### 성능 테스트
- [ ] 60 FPS 유지 (8×8 보드 기준)
- [ ] 메모리 사용량 모니터링
- [ ] GC Alloc 최소화 확인 (풀링 효과)

---

## 📝 참고 문서

- **Manager 시스템**: `Assets/Docs/MANAGERS_GUIDE.md` ⚠️ **필수 읽기**
- **아키텍처 가이드**: `CLAUDE.md` - Multi-Minigame Platform Architecture
- **Sudoku 참고**: `Assets/Scripts/Sudoku/` - Activity Action 패턴 참고
- **Undead Survivor 참고**: `Assets/Docs/UndeadSurvivor_Reference.md` - DataProvider 패턴 참고
- **PRD 원본**: `Assets/Docs/3-match-prd.md`

---

## 🎯 다음 작업

### 즉시 시작 가능한 작업
1. **Phase 1 시작**: ThreeMatchDataProvider 구현
2. **ScriptableObject 생성**: PieceTypeData, DifficultyConfig, GameModeConfig
3. **폴더 구조 생성**: `Assets/Scripts/ThreeMatch/` 하위 폴더 생성

### 작업 우선순위
1. 🔴 **High**: Phase 1 (데이터 구조) - 모든 Phase의 기반
2. 🟡 **Medium**: Phase 2 (핵심 로직) - 게임 메커니즘
3. 🟢 **Low**: Phase 7 (폴리싱) - 완성도 향상

---

**마지막 업데이트**: 2025-11-24
**작성자**: Claude Code
**상태**: ✅ 진행 상황 문서 작성 완료
