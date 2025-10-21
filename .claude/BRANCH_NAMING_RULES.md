# Git 브랜치 네이밍 규칙

## 기본 형식

```
{type}/{description}
```

## 브랜치 타입

| Type | 용도 | 예시 |
|------|------|------|
| `feature` | 새로운 기능 개발 | `feature/enemy-spawner` |
| `fix` | 버그 수정 | `fix/player-movement` |
| `refactor` | 코드 리팩토링 | `refactor/inventory-system` |
| `hotfix` | 긴급 버그 수정 (main에서 직접 분기) | `hotfix/critical-crash` |
| `docs` | 문서 작업 | `docs/api-documentation` |
| `test` | 테스트 코드 추가/수정 | `test/unit-tests` |
| `chore` | 빌드, 설정 등 기타 작업 | `chore/update-dependencies` |
| `design` | UI/UX 디자인 변경 | `design/main-menu` |

## 네이밍 규칙

### ✅ DO (권장사항)

1. **소문자 사용**
   ```
   feature/enemy-ai          ✅
   Feature/Enemy-AI          ❌
   ```

2. **단어 구분은 하이픈(`-`) 사용**
   ```
   feature/player-inventory  ✅
   feature/player_inventory  ❌
   feature/playerInventory   ❌
   ```

3. **명확하고 설명적인 이름**
   ```
   feature/enemy-spawner           ✅
   feature/implement-enemy-spawn   ✅
   feature/stuff                   ❌
   feature/fix                     ❌
   ```

4. **동사 시작 가능**
   ```
   feature/add-health-system       ✅
   fix/resolve-collision-bug       ✅
   refactor/improve-performance    ✅
   ```

5. **이슈 번호 포함 (선택)**
   ```
   feature/enemy-ai-#123           ✅
   fix/player-jump-#456            ✅
   ```

### ❌ DON'T (피해야 할 사항)

1. **공백 사용 금지**
   ```
   feature/enemy ai          ❌
   feature/enemy-ai          ✅
   ```

2. **특수문자 사용 지양** (하이픈, 슬래시, 숫자, 샵 제외)
   ```
   feature/enemy@ai          ❌
   feature/enemy!ai          ❌
   feature/enemy-ai          ✅
   ```

3. **너무 긴 이름**
   ```
   feature/add-complete-enemy-spawning-system-with-waves   ❌
   feature/enemy-spawn-waves                               ✅
   ```

4. **모호한 이름**
   ```
   feature/update            ❌
   feature/new               ❌
   feature/temp              ❌
   ```

5. **한글 사용 지양** (Git 호환성 문제)
   ```
   feature/적생성             ❌
   feature/enemy-spawner     ✅
   ```

## 타입별 예시

### Feature (기능 개발)
```
feature/player-movement
feature/enemy-ai
feature/inventory-system
feature/weapon-upgrade
feature/save-load-system
feature/multiplayer-#234
```

### Fix (버그 수정)
```
fix/player-collision
fix/ui-button-click
fix/memory-leak
fix/animation-freeze
fix/crash-on-load-#567
```

### Refactor (리팩토링)
```
refactor/player-controller
refactor/enemy-manager
refactor/code-cleanup
refactor/optimize-rendering
```

### Hotfix (긴급 수정)
```
hotfix/critical-crash
hotfix/game-breaking-bug
hotfix/save-corruption
```

### Design (UI/UX)
```
design/main-menu
design/hud-layout
design/settings-screen
design/character-ui
```

### Docs (문서화)
```
docs/readme-update
docs/api-reference
docs/setup-guide
```

### Test (테스트)
```
test/unit-tests
test/integration-tests
test/enemy-ai-tests
```

### Chore (기타 작업)
```
chore/update-unity-version
chore/add-gitignore
chore/setup-ci-cd
chore/dependency-update
```

## 브랜치 생성 체크리스트

브랜치 생성 전 확인사항:

- [ ] main 브랜치가 최신 상태인가?
- [ ] 브랜치 타입이 올바른가?
- [ ] 브랜치명이 작업 내용을 명확히 설명하는가?
- [ ] 소문자와 하이픈만 사용했는가?
- [ ] 너무 길지 않은가? (50자 이하 권장)
- [ ] 관련 이슈 번호를 포함했는가? (있는 경우)

## 브랜치 생성 명령어

### 기본 생성
```bash
# main에서 최신 상태 확인
git checkout main
git pull origin main

# 새 브랜치 생성 및 전환
git checkout -b feature/enemy-spawner
```

### 이슈 번호 포함
```bash
git checkout -b fix/player-jump-#123
```

### 특정 커밋에서 분기
```bash
git checkout -b hotfix/critical-bug <commit-hash>
```

## 브랜치 삭제

### 로컬 브랜치 삭제
```bash
# 병합된 브랜치 삭제
git branch -d feature/enemy-spawner

# 강제 삭제 (병합 안 된 브랜치)
git branch -D feature/enemy-spawner
```

### 원격 브랜치 삭제
```bash
git push origin --delete feature/enemy-spawner
```

## 브랜치 네이밍과 커밋 메시지 연관

브랜치명과 커밋 메시지는 일관성을 유지해야 합니다:

| 브랜치명 | 주요 커밋 타입 |
|----------|---------------|
| `feature/*` | `feat:` |
| `fix/*` | `fix:` |
| `refactor/*` | `refactor:` |
| `hotfix/*` | `fix:` |
| `docs/*` | `docs:` |
| `test/*` | `test:` |
| `design/*` | `design:` |
| `chore/*` | `chore:` |

### 예시
```bash
# 브랜치: feature/enemy-spawner
git commit -m "feat: Add EnemySpawner class"
git commit -m "feat: Implement wave system"

# 브랜치: fix/player-jump
git commit -m "fix: Resolve double jump bug"

# 브랜치: refactor/inventory-system
git commit -m "refactor: Simplify item management logic"
```

## 장기 실행 브랜치

### Main 브랜치
- **용도**: 프로덕션 배포용 안정 버전
- **규칙**: 직접 커밋 금지, PR을 통한 병합만 허용
- **상태**: 항상 배포 가능한 상태 유지

### 작업 브랜치 수명
- **단기 브랜치**: feature, fix, hotfix 등은 작업 완료 후 즉시 삭제
- **병합 후 삭제**: PR 병합 완료 시 자동 삭제 권장
- **최대 수명**: 2주 이내 권장 (대규모 기능 제외)

## 브랜치 충돌 방지

### 주기적 동기화
```bash
# main의 최신 변경사항 가져오기
git checkout feature/my-feature
git fetch origin
git rebase origin/main

# 또는 merge 사용
git merge origin/main
```

### 작은 단위로 작업
- 브랜치는 가능한 한 작은 단위로 유지
- 하나의 브랜치는 하나의 기능/수정에 집중
- 여러 기능은 여러 브랜치로 분리

## 참고 자료

- **Github Flow**: `Assets/Github-Flow.md`
- **커밋 메시지 규칙**: `.claude/COMMIT_MESSAGE_RULES.md`
- **브랜치 작업 관리**: `.claude/BRANCH_WORKFLOW.md`
