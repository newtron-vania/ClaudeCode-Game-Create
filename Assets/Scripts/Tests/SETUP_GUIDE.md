# ResourceManagerTest 실행 전 설정 가이드

ResourceManagerTest를 실행하기 전에 반드시 완료해야 할 작업들입니다.

## 1. 테스트용 프리팹 생성

### 1-1. Prefabs 폴더 확인
- `Assets/Prefabs/` 폴더가 이미 생성되어 있습니다

### 1-2. 기본 도형 프리팹 3개 생성

**TestCube 생성:**
1. Hierarchy 우클릭 → 3D Object → Cube
2. 이름: `TestCube`
3. Inspector → Add Component → MeshRenderer
4. Material 색상: 빨간색 (Red)
5. Hierarchy의 TestCube를 `Assets/Prefabs/`로 드래그
6. Hierarchy에서 삭제

**TestSphere 생성:**
1. Hierarchy 우클릭 → 3D Object → Sphere
2. 이름: `TestSphere`
3. Material 색상: 파란색 (Blue)
4. Hierarchy의 TestSphere를 `Assets/Prefabs/`로 드래그
5. Hierarchy에서 삭제

**TestCapsule 생성:**
1. Hierarchy 우클릭 → 3D Object → Capsule
2. 이름: `TestCapsule`
3. Material 색상: 초록색 (Green)
4. Hierarchy의 TestCapsule을 `Assets/Prefabs/`로 드래그
5. Hierarchy에서 삭제

## 2. Addressables 설정

### 2-1. Addressables Groups 창 열기
- Window → Asset Management → Addressables → Groups

### 2-2. 프리팹을 Addressable로 등록

**각 프리팹마다 다음 작업 수행:**

1. **TestCube 등록:**
   - Project 창에서 `Assets/Prefabs/TestCube.prefab` 선택
   - Inspector 상단에서 "Addressable" 체크박스 활성화
   - Address 필드에 정확히 입력: `Prefabs/TestCube`

2. **TestSphere 등록:**
   - `Assets/Prefabs/TestSphere.prefab` 선택
   - "Addressable" 체크박스 활성화
   - Address: `Prefabs/TestSphere`

3. **TestCapsule 등록:**
   - `Assets/Prefabs/TestCapsule.prefab` 선택
   - "Addressable" 체크박스 활성화
   - Address: `Prefabs/TestCapsule`

### 2-3. Addressables 빌드

**중요: 이 단계를 건너뛰면 리소스 로드가 실패합니다!**

1. Addressables Groups 창 열기
2. Build → New Build → Default Build Script
3. 빌드 완료 대기 (Console 확인)

## 3. 테스트 씬 설정

### 3-1. 빈 GameObject 생성
1. Hierarchy 우클릭 → Create Empty
2. 이름: `ResourceManagerTester`

### 3-2. 테스트 스크립트 추가
1. `ResourceManagerTester` GameObject 선택
2. Inspector → Add Component
3. `ResourceManagerTest` 검색 후 추가

### 3-3. 테스트 설정값 입력

Inspector의 ResourceManagerTest 컴포넌트에서 다음을 설정:

**Test Settings:**
- Run Test On Start: ✅ (체크)
- Test Sync Load: ✅ (체크)
- Test Async Load: ✅ (체크)
- Test Preload: ✅ (체크)
- Test Instantiate: ✅ (체크)
- Test Release: ✅ (체크)

**Test Addresses:**
- Size: `3`
- Element 0: `Prefabs/TestCube`
- Element 1: `Prefabs/TestSphere`
- Element 2: `Prefabs/TestCapsule`

## 4. 카메라 위치 조정 (선택사항)

생성된 오브젝트를 보기 위해:
1. Main Camera 선택
2. Position: (0, 2, -5)
3. Rotation: (15, 0, 0)

## 5. 테스트 실행

1. Unity 에디터에서 **Play 버튼** 클릭
2. **Console 창** 확인 (Window → General → Console)

### 예상 출력:
```
========== ResourceManager Test Start ==========
===== Test: Sync Load =====
[INFO] ResourceManager::Load - Loaded and cached resource: Prefabs/TestCube
[TEST] ✅ Sync load succeeded: Prefabs/TestCube
===== Test: Async Load =====
[TEST] ✅ Async load succeeded: Prefabs/TestSphere
===== Test: Preload =====
[TEST] ✅ Preload completed!
===== Test: Instantiate =====
[TEST] ✅ Sync instantiate succeeded: TestCube(Clone)
[TEST] ✅ Async instantiate succeeded: TestSphere(Clone)
===== Test: Release =====
[TEST] ✅ Release succeeded
========== ResourceManager Test End ==========
```

### Scene 뷰 확인:
- 왼쪽 (-2, 0, 0): 빨간 큐브
- 오른쪽 (2, 0, 0): 파란 구체

## 6. 문제 해결

### "Failed to load resource" 에러가 발생하면:

**원인 1: Addressables 빌드를 안 함**
- 해결: Addressables Groups → Build → New Build → Default Build Script

**원인 2: Address 이름이 잘못됨**
- 해결: Inspector에서 Address가 정확히 `Prefabs/TestCube` 형식인지 확인

**원인 3: 프리팹이 Addressable로 등록 안 됨**
- 해결: Inspector에서 "Addressable" 체크박스 확인

**원인 4: 프리팹 파일이 없음**
- 해결: `Assets/Prefabs/` 폴더에 프리팹 파일이 있는지 확인

### "NullReferenceException" 에러가 발생하면:

**원인: ResourceManager 인스턴스가 없음**
- 해결: ResourceManager는 Singleton이므로 자동 생성됨. 씬을 재시작하거나 Unity 에디터 재시작

## 7. 체크리스트

설정 완료 전 다음을 확인하세요:

- [ ] TestCube.prefab 생성 완료
- [ ] TestSphere.prefab 생성 완료
- [ ] TestCapsule.prefab 생성 완료
- [ ] 3개 프리팹 모두 Addressable로 등록
- [ ] Address 이름이 정확함 (`Prefabs/xxx`)
- [ ] Addressables 빌드 완료
- [ ] ResourceManagerTester GameObject 생성
- [ ] ResourceManagerTest 컴포넌트 추가
- [ ] Test Addresses 3개 설정 완료

모든 항목 체크 후 테스트를 실행하세요!
