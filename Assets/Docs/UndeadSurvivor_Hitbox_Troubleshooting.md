# PlayerHitbox 피격 이벤트 트러블슈팅 가이드

피격 이벤트가 실행되지 않는 경우 다음 항목들을 순서대로 확인하세요.

## 🔍 진단 체크리스트

### 1. PlayerHitbox GameObject 존재 확인

**Unity Hierarchy 확인**:
```
Player (GameObject)
└── PlayerHitbox (Child GameObject) ← 이것이 있어야 함!
```

**확인 방법**:
- Hierarchy에서 Player 확장
- PlayerHitbox 자식 GameObject 존재 확인

**없으면**:
1. Player 우클릭 → Create Empty
2. 이름을 "PlayerHitbox"로 변경
3. Add Component → Box Collider 2D
4. Add Component → Player Hitbox (스크립트)

---

### 2. PlayerHitbox 컴포넌트 설정 확인

**Inspector 체크**:
```
PlayerHitbox GameObject 선택 후:

Box Collider 2D:
├── Is Trigger: ✅ 체크 (필수!)
├── Size: X: 1, Y: 1 (적당한 크기)
└── Offset: X: 0, Y: 0

Player Hitbox (Script):
├── Hitbox Collider: [BoxCollider2D 자동 연결]
├── Hitbox Size: X: 1, Y: 1
└── Hitbox Offset: X: 0, Y: 0

Transform:
├── Position: X: 0, Y: 0, Z: 0 (부모 기준 로컬)
└── Layer: Player (필수!)
```

**⚠️ 중요**:
- `Is Trigger`가 체크되어 있어야 함
- Layer가 `Player`여야 함

---

### 3. Player GameObject Tag 확인

**Player Root GameObject 선택 후**:
```
Inspector 상단:
Tag: Player (필수!)
Layer: Player
```

**Tag가 없거나 다르면**:
1. Player GameObject 선택
2. Inspector 상단 Tag → Player 선택

---

### 4. Enemy GameObject Tag 및 Layer 확인

**Enemy Prefab 또는 생성된 Enemy 선택 후**:
```
Inspector 상단:
Tag: Enemy (필수!)
Layer: Enemy (필수!)

Box Collider 2D / Circle Collider 2D:
└── Is Trigger: ❌ 체크 해제 (Enemy끼리 충돌 필요)

Rigidbody 2D:
├── Body Type: Dynamic
├── Gravity Scale: 0
└── Constraints: Freeze Rotation Z
```

---

### 5. Unity Layer 설정 확인

**Edit → Project Settings → Tags and Layers**:
```
Layers:
├── Layer 6: Player
└── Layer 7: Enemy
```

**없으면**:
- User Layer 6에 "Player" 입력
- User Layer 7에 "Enemy" 입력

---

### 6. Physics2D Collision Matrix 확인

**Edit → Project Settings → Physics 2D**:

하단 **Layer Collision Matrix** 테이블에서:
```
         | Default | Player | Enemy |
---------|---------|--------|-------|
Default  |   ✅    |   ✅   |  ✅   |
Player   |   ✅    |   ❌   |  ❌   | ← 중요!
Enemy    |   ✅    |   ❌   |  ✅   | ← 중요!
```

**Player ↔ Enemy 충돌이 비활성화**되어야 합니다!
- PlayerHitbox의 Trigger와만 반응하기 위함
- Enemy끼리는 충돌 활성화 (서로 밀어냄)

---

### 7. Console 로그 확인

**Play Mode 진입 후 Console에서 확인**:

#### 정상 작동 시:
```
[DEBUG] PlayerHitbox::OnTriggerEnter2D - Triggered by: Enemy(Clone), Tag: Enemy, Layer: Enemy
[INFO] PlayerHitbox::OnTriggerEnter2D - Player hit by Zombie, Damage: 10
[INFO] PlayerHealth::TakeDamage - Damage: 10, HP: 90/100
```

#### PlayerHitbox GameObject가 없을 때:
```
(아무 로그도 출력되지 않음)
```

#### PlayerHitbox의 Trigger가 작동하지만 Tag가 다를 때:
```
[DEBUG] PlayerHitbox::OnTriggerEnter2D - Triggered by: Enemy(Clone), Tag: Untagged, Layer: Enemy
[DEBUG] PlayerHitbox::OnTriggerEnter2D - Not an Enemy tag: Untagged
```

#### Player 참조가 없을 때:
```
[ERROR] PlayerHitbox::OnTriggerEnter2D - Player reference is null!
```

#### Is Trigger가 체크되지 않았을 때:
```
(OnTriggerEnter2D가 호출되지 않아 아무 로그도 없음)
```

#### Layer 충돌 설정이 잘못되었을 때:
```
(Player ↔ Enemy 충돌이 활성화되면 물리 충돌 발생, Trigger 작동 안 함)
```

---

### 8. Scene View에서 Gizmos 확인

**PlayerHitbox GameObject 선택**:
- Scene View에서 **빨간 사각형**(Hitbox)이 표시되어야 함
- Enemy가 이 영역에 들어오면 트리거 발생

**Gizmos가 보이지 않으면**:
1. Scene View 상단 → Gizmos 버튼 클릭
2. PlayerHitbox 체크 확인

---

### 9. Rigidbody2D 확인

**Player GameObject**:
```
Rigidbody 2D (필수!):
├── Body Type: Dynamic
├── Gravity Scale: 0
└── Constraints: Freeze Rotation Z
```

**없으면 Trigger 이벤트가 발생하지 않을 수 있음!**

**Enemy GameObject**:
```
Rigidbody 2D (필수!):
├── Body Type: Dynamic
├── Gravity Scale: 0
└── Constraints: Freeze Rotation Z
```

---

## 🐛 자주 발생하는 문제들

### 문제 1: "아무 로그도 출력되지 않음"

**원인**:
- PlayerHitbox GameObject가 없음
- PlayerHitbox의 BoxCollider2D `Is Trigger`가 체크 해제됨
- Player 또는 Enemy에 Rigidbody2D가 없음

**해결**:
1. PlayerHitbox GameObject 생성 확인
2. BoxCollider2D의 `Is Trigger` 체크
3. Player와 Enemy에 Rigidbody2D 추가

---

### 문제 2: "Triggered by 로그는 나오지만 Not an Enemy tag"

**원인**:
- Enemy GameObject의 Tag가 "Enemy"가 아님

**해결**:
1. Enemy GameObject 또는 Prefab 선택
2. Inspector 상단 Tag → Enemy 선택
3. Prefab이면 Apply

---

### 문제 3: "Player reference is null!"

**원인**:
- PlayerHitbox가 Player의 자식이 아님
- Player.cs 컴포넌트가 부모에 없음

**해결**:
1. Hierarchy에서 PlayerHitbox를 Player 아래로 드래그
2. Player GameObject에 Player.cs 컴포넌트 확인

---

### 문제 4: "Trigger는 작동하지만 피해를 받지 않음"

**원인**:
- Player.TakeDamage()가 호출되지만 무적 상태
- PlayerHealth 컴포넌트가 없음

**해결**:
1. Console에서 "Invincible, damage ignored" 확인
2. 무적 시간 (0.5초) 대기 후 재테스트
3. Player GameObject에 PlayerHealth.cs 확인

---

### 문제 5: "Enemy와 Player가 물리 충돌하여 밀림"

**원인**:
- Physics2D Collision Matrix에서 Player ↔ Enemy 충돌이 활성화됨

**해결**:
1. Edit → Project Settings → Physics 2D
2. Layer Collision Matrix에서 Player ↔ Enemy 체크 해제

---

### 문제 6: "Enemy끼리 충돌하지 않음"

**원인**:
- Physics2D Collision Matrix에서 Enemy ↔ Enemy 충돌이 비활성화됨
- Enemy Collider의 `Is Trigger`가 체크됨

**해결**:
1. Layer Collision Matrix에서 Enemy ↔ Enemy 체크
2. Enemy의 BoxCollider2D `Is Trigger` 체크 해제

---

## ✅ 최종 확인 체크리스트

Unity 에디터에서 다음을 모두 확인하세요:

- [ ] PlayerHitbox GameObject 존재 (Player의 자식)
- [ ] PlayerHitbox의 BoxCollider2D `Is Trigger` 체크
- [ ] PlayerHitbox의 Layer: Player
- [ ] Player GameObject Tag: Player
- [ ] Enemy GameObject Tag: Enemy
- [ ] Enemy GameObject Layer: Enemy
- [ ] Enemy의 Collider `Is Trigger` 체크 해제
- [ ] Unity Layer 생성: Player (6), Enemy (7)
- [ ] Collision Matrix: Player ↔ Enemy 비활성화
- [ ] Collision Matrix: Enemy ↔ Enemy 활성화
- [ ] Player에 Rigidbody2D 존재
- [ ] Enemy에 Rigidbody2D 존재
- [ ] Scene View에서 PlayerHitbox Gizmos (빨간 사각형) 표시

---

## 🧪 테스트 방법

1. **Play Mode 진입**
2. **Player를 Enemy 쪽으로 이동**
3. **Console 확인**:
   ```
   [DEBUG] PlayerHitbox::OnTriggerEnter2D - Triggered by: Enemy(Clone), Tag: Enemy, Layer: Enemy
   [INFO] PlayerHitbox::OnTriggerEnter2D - Player hit by Zombie, Damage: 10
   [INFO] PlayerHealth::TakeDamage - Damage: 10, HP: 90/100
   ```

4. **무적 시간 확인** (0.5초 후):
   ```
   [INFO] PlayerHealth::TakeDamage - Invincible, damage ignored
   ```

5. **무적 해제 후 다시 피해**:
   ```
   [INFO] PlayerHealth::TakeDamage - Damage: 10, HP: 80/100
   ```

---

## 📞 추가 지원

여전히 문제가 해결되지 않으면:

1. Console의 **전체 로그**를 확인
2. PlayerHitbox GameObject의 **Inspector 스크린샷**
3. Enemy GameObject의 **Inspector 스크린샷**
4. **Physics 2D Collision Matrix 스크린샷**

위 정보를 제공하면 정확한 진단이 가능합니다.
