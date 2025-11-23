# Undead Survivor Layer 및 충돌 매트릭스 설정 가이드

이 문서는 Player와 Enemy 간의 충돌/트리거 처리를 위한 Unity Layer 설정 방법을 설명합니다.

## 목표

- **Player ↔ Enemy**: Trigger로 처리 (피격 판정만, 물리 충돌 없음)
- **Enemy ↔ Enemy**: Collision으로 처리 (서로 밀어냄)
- **Player ↔ Player**: 필요 없음 (싱글 플레이어)

---

## 1. Unity Layer 생성

### Layer 추가

**Unity Editor → Edit → Project Settings → Tags and Layers**

다음 Layer를 추가:
- **Layer 6**: `Player`
- **Layer 7**: `Enemy`

---

## 2. GameObject Layer 할당

### Player GameObject

**Hierarchy → Player → Inspector → Layer → Player**

Player GameObject와 모든 자식 GameObject를 `Player` Layer로 설정:
- Player (Root)
- PlayerHitbox (Child) - Trigger Collider 담당

### Enemy GameObject (프리팹)

**Assets/Resources/Prefabs/Monster/UndeadSurvivor/Enemy.prefab**

Enemy 프리팹을 `Enemy` Layer로 설정:
- Enemy (Root)

**주의**: Enemy는 코드에서 `gameObject.layer = LayerMask.NameToLayer("Enemy");`로 자동 설정되지만, 프리팹에서도 미리 설정해두는 것을 권장합니다.

---

## 3. Physics2D Collision Matrix 설정

**Unity Editor → Edit → Project Settings → Physics 2D**

하단의 **Layer Collision Matrix** 설정:

| Layer   | Default | Player | Enemy |
|---------|---------|--------|-------|
| Default | ✅      | ✅     | ✅    |
| Player  | ✅      | ❌     | ❌    |
| Enemy   | ✅      | ❌     | ✅    |

**설정 의미**:
- `Player ↔ Player`: ❌ 충돌 없음 (싱글 플레이어)
- `Player ↔ Enemy`: ❌ 물리 충돌 없음 (Trigger로만 처리)
- `Enemy ↔ Enemy`: ✅ 물리 충돌 있음 (서로 밀어냄)

---

## 4. Collider 구성

### Player Collider 구조

Player GameObject는 **2개의 Collider**를 가집니다:

#### 4.1. Player Root Collider (물리 충돌용)
- **Component**: `CircleCollider2D`
- **Is Trigger**: `false`
- **용도**: 벽, 장애물 등과의 물리 충돌
- **Layer**: `Player`

#### 4.2. PlayerHitbox (피격 판정용)
- **GameObject**: Player의 자식 GameObject "PlayerHitbox"
- **Component**: `CircleCollider2D` + `PlayerHitbox.cs`
- **Is Trigger**: `true`
- **Radius**: `0.5f` (조정 가능)
- **용도**: Enemy와의 피격 판정 (Trigger)
- **Layer**: `Player`

### Enemy Collider 구조

Enemy GameObject는 **1개의 Collider**를 가집니다:

- **Component**: `CircleCollider2D`
- **Is Trigger**: `false`
- **용도**:
  - Enemy끼리 물리 충돌 (밀어냄)
  - Player Hitbox와 Trigger 충돌 (피격 판정)
- **Layer**: `Enemy`

---

## 5. 충돌 처리 플로우

### Enemy → Player 피해

```
1. Enemy (CircleCollider2D, isTrigger=false, Layer=Enemy)
   ↓ OnTriggerEnter2D/Stay2D
2. PlayerHitbox (CircleCollider2D, isTrigger=true, Layer=Player)
   ↓ PlayerHitbox.cs
3. player.TakeDamage(enemy.Damage)
   ↓ PlayerHealth.cs
4. 무적 시간 체크 → 피해 적용
```

**왜 Trigger가 발생하는가?**
- Physics2D Collision Matrix에서 `Player ↔ Enemy` 충돌이 **비활성화**되어 있지만
- **한쪽이 Trigger면 Trigger 이벤트는 발생**합니다
- PlayerHitbox의 `isTrigger=true`이므로 Enemy와 겹칠 때 `OnTriggerEnter2D` 발생

### Enemy ↔ Enemy 충돌

```
1. Enemy A (CircleCollider2D, isTrigger=false, Layer=Enemy)
   ↓ OnCollisionEnter2D/Stay2D
2. Enemy B (CircleCollider2D, isTrigger=false, Layer=Enemy)
   ↓ 물리 충돌
3. Rigidbody2D에 의해 서로 밀어냄
```

**왜 충돌이 발생하는가?**
- Physics2D Collision Matrix에서 `Enemy ↔ Enemy` 충돌이 **활성화**
- 양쪽 모두 `isTrigger=false`이므로 물리 충돌 발생

---

## 6. Player GameObject 설정 예시

```
Player (GameObject, Layer: Player)
├── SpriteRenderer
├── Rigidbody2D (Dynamic, Gravity: 0)
├── CircleCollider2D (isTrigger: false, Radius: 0.3f) - 물리 충돌용
├── Player.cs
├── PlayerController.cs
├── PlayerHealth.cs
├── PlayerExperience.cs
├── PlayerWeaponManager.cs
└── PlayerHitbox (Child GameObject, Layer: Player)
    ├── CircleCollider2D (isTrigger: true, Radius: 0.5f) - 피격 판정용
    └── PlayerHitbox.cs
```

---

## 7. Enemy 프리팹 설정 예시

```
Enemy (Prefab, Layer: Enemy)
├── SpriteRenderer
├── Rigidbody2D (Dynamic, Gravity: 0, Freeze Rotation: Z)
├── CircleCollider2D (isTrigger: false, Radius: 0.4f)
└── Enemy.cs
```

---

## 8. 설정 검증

### Unity 에디터에서 테스트

1. **Play Mode 진입**
2. **Console 로그 확인**:
   ```
   [INFO] PlayerHitbox::OnTriggerEnter2D - Player hit by Zombie, Damage: 10
   [INFO] PlayerHealth::TakeDamage - Damage: 10, HP: 90/100
   [INFO] PlayerHealth::TakeDamage - Invincible, damage ignored (무적 시간)
   ```

3. **Scene View에서 확인**:
   - Enemy들이 서로 밀어내는지 확인 (물리 충돌)
   - Player가 Enemy와 겹쳐도 밀리지 않는지 확인 (Trigger만)
   - Player가 피해를 받는지 Console에서 확인

### Gizmos 활용

PlayerHitbox는 Scene View에서 빨간 원으로 시각화됩니다:
- **Hierarchy → Player → PlayerHitbox 선택**
- **Scene View에서 빨간 원(Hitbox) 확인**

---

## 9. 트러블슈팅

### 문제 1: Player가 Enemy에게 피해를 받지 않음

**원인**:
- PlayerHitbox GameObject가 없거나 비활성화
- PlayerHitbox의 `isTrigger`가 `false`
- Player 또는 Enemy의 Layer가 잘못 설정됨

**해결**:
- PlayerHitbox GameObject 생성 및 활성화
- Inspector에서 `isTrigger` 체크
- Layer 설정 확인

### 문제 2: Enemy끼리 충돌하지 않음

**원인**:
- Physics2D Collision Matrix에서 `Enemy ↔ Enemy` 비활성화
- Enemy Collider의 `isTrigger`가 `true`

**해결**:
- Collision Matrix에서 `Enemy ↔ Enemy` 활성화
- Enemy의 `isTrigger`를 `false`로 설정

### 문제 3: Player가 Enemy에게 밀림

**원인**:
- Physics2D Collision Matrix에서 `Player ↔ Enemy` 활성화
- Player의 물리 Collider가 Enemy와 충돌 중

**해결**:
- Collision Matrix에서 `Player ↔ Enemy` 비활성화
- Player Root Collider와 PlayerHitbox 구분 확인

### 문제 4: 무적 시간이 작동하지 않음

**원인**:
- PlayerHealth의 무적 시간 설정이 너무 짧음 (기본 0.5초)
- OnTriggerStay2D가 매 프레임 호출되어 로그만 출력

**해결**:
- PlayerHealth의 `_invincibilityDuration` 값 확인 (0.5~1.0 권장)
- Console에서 "Invincible, damage ignored" 로그 확인

---

## 10. 권장 설정 값

### Player
- **Root Collider Radius**: `0.3f` (작게, 벽 충돌용)
- **Hitbox Radius**: `0.5f` (크게, 피격 판정 너그럽게)
- **Invincibility Duration**: `0.5f` ~ `1.0f`

### Enemy
- **Collider Radius**: `0.4f` (Enemy 크기에 따라 조정)
- **Rigidbody2D Mass**: `1.0f` (기본값)
- **Linear Drag**: `0f` (마찰 없음, 부드러운 이동)

---

## 11. 코드 자동 설정

다음은 **코드에서 자동으로 설정**되므로 수동 설정 불필요:

### Enemy.cs (Initialize 메서드)
```csharp
_rigidbody.bodyType = RigidbodyType2D.Dynamic;
_rigidbody.gravityScale = 0f;
_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
_collider.isTrigger = false;
gameObject.layer = LayerMask.NameToLayer("Enemy");
```

### PlayerHitbox.cs (Awake 메서드)
```csharp
_hitboxCollider.isTrigger = true;
gameObject.layer = LayerMask.NameToLayer("Player");
```

---

## 요약 체크리스트

Unity 에디터에서 다음을 확인하세요:

- [ ] **Layer 생성**: Player (Layer 6), Enemy (Layer 7)
- [ ] **Player GameObject**: Layer → Player
- [ ] **Enemy Prefab**: Layer → Enemy
- [ ] **Collision Matrix**: Player ↔ Enemy 충돌 비활성화
- [ ] **Collision Matrix**: Enemy ↔ Enemy 충돌 활성화
- [ ] **PlayerHitbox GameObject**: Player의 자식으로 생성, `PlayerHitbox.cs` 추가
- [ ] **PlayerHitbox Collider**: `isTrigger = true`, `Radius = 0.5f`
- [ ] **Player Root Collider**: `isTrigger = false`, `Radius = 0.3f`
- [ ] **Enemy Collider**: `isTrigger = false`, `Radius = 0.4f`
- [ ] **Play Mode 테스트**: 피격 이벤트 및 충돌 동작 확인

---

## 참고 자료

- [Unity Manual - 2D Physics](https://docs.unity3d.com/Manual/Physics2DReference.html)
- [Unity Manual - Layer-based Collision Detection](https://docs.unity3d.com/Manual/LayerBasedCollision.html)
- [Unity Scripting API - Physics2D](https://docs.unity3d.com/ScriptReference/Physics2D.html)
