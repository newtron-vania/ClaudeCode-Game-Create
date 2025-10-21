# 브랜치별 작업 관리 시스템

## 개요

각 Git 브랜치마다 독립적인 작업 사항 문서(`.md`)를 생성하여 작업 내용을 추적하고 관리합니다.

## 디렉토리 구조

```
.claude/
├── branches/                    # 브랜치별 작업 문서 저장소
│   ├── main.md                 # main 브랜치 작업 사항
│   ├── feature-enemy-ai.md     # feature/enemy-ai 브랜치 작업 사항
│   └── fix-player-movement.md  # fix/player-movement 브랜치 작업 사항
└── BRANCH_WORKFLOW.md          # 이 문서
```

## 작업 문서 형식

각 브랜치 작업 문서는 다음 형식을 따릅니다:

```markdown
# 브랜치: [브랜치명]

## 브랜치 정보
- **생성일**: YYYY-MM-DD
- **타입**: feature/fix/refactor/etc
- **목적**: [브랜치 생성 목적 한 줄 요약]
- **관련 이슈**: #123, #456

## 작업 목표
- [ ] 목표 1
- [ ] 목표 2
- [ ] 목표 3

## 작업 내역

### YYYY-MM-DD HH:MM
- 작업 내용 1
- 작업 내용 2

### YYYY-MM-DD HH:MM
- 작업 내용 3
- 작업 내용 4

## 커밋 기록
- `commit-hash` - type: Commit message
- `commit-hash` - type: Commit message

## 완료 조건
- [ ] 모든 기능 구현 완료
- [ ] 테스트 통과
- [ ] 코드 리뷰 완료
- [ ] 문서화 완료

## 참고 사항
- 특이사항이나 주의할 점
- 다른 브랜치와의 의존성
```

## 브랜치 전환 시 자동 작업

### 1. 새 브랜치 생성 시
```bash
git checkout -b feature/new-feature
```

자동으로 수행할 작업:
1. `.claude/branches/feature-new-feature.md` 파일 생성
2. 템플릿으로 초기화
3. 생성일, 브랜치명 자동 입력

### 2. 기존 브랜치로 전환 시
```bash
git checkout feature/existing-feature
```

자동으로 수행할 작업:
1. `.claude/branches/feature-existing-feature.md` 파일 존재 확인
2. 없으면 생성, 있으면 내용 표시
3. 마지막 작업 내역 요약 출력

### 3. 작업 완료 후
```bash
# 작업 문서에 커밋 기록 자동 추가
git commit -m "feat: Add new feature"
```

## Claude Code 통합

Claude Code는 다음 시점에 브랜치 작업 문서를 관리합니다:

### 트리거
- `git checkout` 명령 감지
- `git checkout -b` 명령 감지
- 사용자가 "브랜치 작업 상황 보여줘" 요청
- 커밋 전후

### 자동 작업
1. **브랜치 전환 감지**
   - 현재 브랜치명 확인
   - 해당 브랜치 문서 존재 여부 확인
   - 없으면 생성, 있으면 로드

2. **작업 기록 자동 업데이트**
   - 새 작업 시작 시 타임스탬프와 함께 기록
   - 커밋 시 커밋 메시지 자동 추가

3. **작업 상태 추적**
   - 체크리스트 항목 진행 상황 관리
   - 완료된 작업은 자동으로 체크

## 사용 예시

### 예시 1: 새 기능 브랜치
```bash
# 1. 브랜치 생성
git checkout -b feature/enemy-spawner

# Claude가 자동으로 .claude/branches/feature-enemy-spawner.md 생성

# 2. 작업 시작
# Claude에게 "적 스포너 시스템 구현 시작"이라고 알림

# 3. 작업 진행
# 파일 수정, 커밋 등

# 4. 커밋
git commit -m "feat: Add EnemySpawner class"
# Claude가 자동으로 커밋 기록 추가

# 5. 작업 완료
# Claude에게 "작업 완료 상태로 변경"
```

### 예시 2: 작업 재개
```bash
# 1. 기존 브랜치로 전환
git checkout feature/enemy-spawner

# Claude가 자동으로:
# - feature-enemy-spawner.md 로드
# - 마지막 작업 내역 표시
# - 남은 작업 목록 표시

# 2. 작업 계속
```

## 브랜치 병합 후 처리

브랜치가 main에 병합되면:
1. 해당 브랜치 문서에 "병합 완료" 상태 추가
2. 병합 날짜 기록
3. 문서를 `.claude/branches/archived/` 폴더로 이동

## 명령어 참조

### 브랜치 작업 문서 생성
```
"새 브랜치 [브랜치명] 작업 문서 생성"
"브랜치 작업 시작"
```

### 브랜치 작업 상태 확인
```
"현재 브랜치 작업 상황 보여줘"
"브랜치 작업 목록"
"남은 작업 보여줘"
```

### 작업 기록 추가
```
"작업 기록: [내용]"
"[내용] 완료"
```

### 작업 문서 업데이트
```
"작업 목표 추가: [목표]"
"완료 조건 추가: [조건]"
```

## 파일 명명 규칙

브랜치명을 파일명으로 변환할 때:
- `/` → `-` (슬래시를 대시로)
- 소문자 유지
- 특수문자 제거

예시:
- `feature/enemy-ai` → `feature-enemy-ai.md`
- `fix/player-movement` → `fix-player-movement.md`
- `main` → `main.md`

## 통합 체크리스트

브랜치 전환 시 Claude가 확인할 사항:
- [ ] Git 저장소 초기화 여부
- [ ] 현재 브랜치명 확인
- [ ] `.claude/branches/` 디렉토리 존재 여부
- [ ] 해당 브랜치 작업 문서 존재 여부
- [ ] 작업 문서 템플릿 적용
- [ ] 마지막 작업 내역 표시
