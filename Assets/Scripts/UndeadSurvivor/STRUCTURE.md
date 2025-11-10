# UndeadSurvivor 폴더 구조

```
UndeadSurvivor/
├── Character/               # 캐릭터 관련
│   ├── Player/             # 플레이어
│   │   ├── Player.cs
│   │   ├── PlayerController.cs
│   │   ├── PlayerHealth.cs
│   │   ├── PlayerExperience.cs
│   │   ├── PlayerHitbox.cs
│   │   ├── PlayerWeaponManager.cs
│   │   └── CharacterStat.cs
│   └── Enemy/              # 적
│       ├── Enemy.cs
│       └── EnemySpawner.cs
├── Weapon/                  # 무기 시스템
│   ├── Weapon.cs           # 무기 베이스 클래스
│   ├── Scythe.cs           # 근접 무기
│   └── Projectile/         # 투사체
│       ├── Projectile.cs   # 투사체 베이스 클래스
│       └── Fireball.cs     # 원거리 무기
├── UI/                      # 게임 전용 UI
│   ├── LevelUpUIPanel.cs
│   ├── LevelUpUIController.cs
│   ├── LevelUpOptionElement.cs
│   └── LevelUpOptionButton.cs
├── System/                  # 게임 시스템
│   ├── LevelUpManager.cs
│   ├── LevelUpOption.cs
│   ├── UndeadSurvivorInputAdapter.cs
│   ├── UndeadSurvivorInputEventData.cs
│   └── UndeadSurvivorInputType.cs
├── Data/                    # 데이터 구조
│   ├── UndeadSurvivorDataProvider.cs
│   ├── CharacterData.cs
│   ├── WeaponData.cs
│   ├── MonsterData.cs
│   └── ItemData.cs
├── ScriptableObjects/       # ScriptableObject 리스트
│   ├── CharacterDataList.cs
│   ├── WeaponDataList.cs
│   ├── MonsterDataList.cs
│   └── ItemDataList.cs
├── UndeadSurvivorGame.cs    # 게임 진입점
└── UndeadSurvivorGameData.cs # 게임 런타임 데이터
```

## 폴더 구조 원칙

### Character/ - 캐릭터 계층 구조
- **Player/**: 플레이어 관련 모든 컴포넌트
- **Enemy/**: 적 관련 모든 컴포넌트

### Weapon/ - 무기 계층 구조
- 무기 베이스 클래스와 구체 무기 구현
- **Projectile/**: 투사체 관련 (원거리 무기)

### UI/ - 게임 전용 UI
- UndeadSurvivor에서만 사용하는 UI 컴포넌트

### System/ - 게임 시스템
- 레벨업, 입력 처리 등 게임 시스템 로직

### Data/ - 데이터 계층
- DataProvider와 각종 데이터 구조체

### ScriptableObjects/
- Unity ScriptableObject 리스트 클래스

## 정리 일자
2024-11-10
