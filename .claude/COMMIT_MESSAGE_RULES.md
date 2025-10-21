# Git Commit Message Rules

Based on the Github Flow documentation.

## Commit Message Structure

```
type(타입) : title(제목)

body(본문, 생략 가능)

Resolves : #issueNo, ...(해결한 이슈, 생략 가능)
Ref : #issueNo, ...(참고 이슈, 생략 가능)
```

## Basic Rules

### Format Rules
- ✅ **제목과 본문을 빈 행으로 구분**
- ✅ **제목은 영문 기준 50글자 이하**
- ✅ **첫 글자는 대문자로 작성**
- ❌ **제목 끝에 마침표 사용 금지**
- ✅ **제목은 명령문으로 사용, 과거형 금지**
  - Good: "Add feature", "Fix bug"
  - Bad: "Added feature", "Fixed bug"
- ✅ **본문의 각 행은 영문 기준 72글자 이하**

### Content Rules
- ✅ **"어떻게"보다는 "무엇"과 "왜"를 설명**
- ✅ **커밋은 단위별로 자주 이루어져야 함**
- ✅ **커밋 메시지는 명확하게 변경 사항을 설명해야 함**

## Commit Types

| Type | 사용 시점 |
|------|----------|
| `feat` | 새로운 기능 추가 |
| `fix` | 버그 수정 |
| `docs` | 문서 수정 |
| `style` | 코드 스타일 변경 (코드 포맷팅 등) <br>**(기능 수정이 없어야 함)** |
| `design` | UI 수정 <br>**(기능 수정이 없어야 함)** |
| `test` | 테스트 코드 추가 |
| `refactor` | 코드 리팩토링 |
| `chore` | git ignore 등 빌드 관련 수정 |
| `rename` | 파일 혹은 폴더명을 수정만 함 |
| `remove` | 파일 혹은 폴더를 삭제만 함 |

## Examples

### Example 1: Feature Addition
```
feat: Add Enemy Manager

-적을 관리하는 클래스를 추가함
```

### Example 2: Bug Fix with Issue Reference
```
fix: Enemy not move in specific Scenes

-적이 특정 신에서 움직이지 않는 버그를 고침

Resolves: #219
```

### Example 3: Documentation Update
```
docs: Update README with installation guide

-설치 가이드를 README에 추가
```

### Example 4: Style Changes
```
style: Format PlayerController code

-PlayerController의 코드 스타일을 정리함
```

### Example 5: Refactoring
```
refactor: Improve enemy spawn logic

-적 생성 로직을 개선하여 가독성을 높임
```

## Issue Reference Guidelines

### Resolves (해결한 이슈)
- 이 커밋으로 **완전히 해결된 이슈**를 명시
- 이슈가 자동으로 닫힘
```
Resolves: #123
Resolves: #123, #456
```

### Ref (참고 이슈)
- 이 커밋과 **관련있는 이슈**를 명시
- 이슈는 자동으로 닫히지 않음
```
Ref: #789
Ref: #789, #101
```

## Quick Checklist

Before committing, verify:
- [ ] Type이 올바른가? (feat, fix, docs 등)
- [ ] 제목은 50자 이하인가?
- [ ] 제목은 대문자로 시작하는가?
- [ ] 제목은 명령문인가? (과거형 X)
- [ ] 제목 끝에 마침표가 없는가?
- [ ] 본문은 "무엇"과 "왜"를 설명하는가?
- [ ] 본문 각 행은 72자 이하인가?
- [ ] 이슈 참조가 필요한가? (Resolves/Ref)

## Anti-Patterns (피해야 할 패턴)

### ❌ Bad Examples
```
# 과거형 사용
fix: Fixed bug in player movement

# 마침표 사용
feat: Add new weapon.

# 소문자로 시작
feat: add new weapon

# 불명확한 메시지
fix: bug fix

# 너무 긴 제목
feat: Add a completely new weapon system with multiple weapon types including swords and guns

# "어떻게"만 설명
refactor: Changed for loop to while loop
```

### ✅ Good Examples
```
# 명령문, 대문자, 명확
fix: Resolve player movement bug in Scene 2

# 간결하고 명확
feat: Add sword weapon

# "무엇"과 "왜" 설명
refactor: Improve weapon spawn logic

-for 루프를 while 루프로 변경하여 조건 검사를 명확하게 함
```

## Commit Workflow

### 1. Stage Changes
```bash
git add <file>
```

### 2. Commit with Message
```bash
git commit -m "type: Title

Body explaining what and why

Resolves: #123"
```

### 3. Push to Branch
```bash
git push origin <branch-name>
```

## Integration with Github Flow

### Branch Naming + Commit Message
- Branch: `feature/enemy-manager`
- Commit: `feat: Add Enemy Manager`

### Commit Frequency
- ✅ **단위별로 자주 커밋**
- ✅ **각 커밋은 논리적으로 독립적인 변경사항**
- ❌ 너무 많은 변경사항을 하나의 커밋에 포함

### Pull Request Title
- PR 제목은 주요 커밋 메시지를 반영
- 예: `feat: Implement Enemy Management System`
