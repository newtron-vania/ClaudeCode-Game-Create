# Undead Survivor 아이콘 스프라이트 가이드

## 📋 개요

이 문서는 LevelUpUI에서 사용하는 아이콘 스프라이트의 경로, 명명 규칙, 설정 방법을 설명합니다.

## 🗂️ 파일 구조

```
Assets/Resources/Sprites/UndeadSurvivor/
├── Icon_Weapon_1.png           # Fireball (화염구)
├── Icon_Weapon_2.png           # Scythe (낫)
├── Icon_Weapon_3.png           # (향후 추가 무기)
├── Icon_Weapon_4.png
├── Icon_Weapon_5.png
├── Icon_Weapon_6.png
├── Icon_Stat_Damage.png        # 공격력
├── Icon_Stat_MaxHp.png         # 최대 체력
├── Icon_Stat_Defense.png       # 방어력
├── Icon_Stat_MoveSpeed.png     # 이동 속도
├── Icon_Stat_Area.png          # 범위
├── Icon_Stat_Cooldown.png      # 쿨타임
├── Icon_Stat_Amount.png        # 투사체 개수
├── Icon_Stat_Pierce.png        # 관통력
├── Icon_Stat_ExpMultiplier.png # 경험치 획득
├── Icon_Stat_PickupRange.png   # 아이템 획득 범위
├── Icon_Stat_Luck.png          # 행운
└── Icon_Default.png            # 기본 아이콘 (로드 실패 시)
```

## 📝 명명 규칙

### 무기 아이콘
```
형식: Icon_Weapon_{WeaponID}.png
예시:
- Icon_Weapon_1.png  → Fireball (WeaponID: 1)
- Icon_Weapon_2.png  → Scythe (WeaponID: 2)
```

### 스탯 아이콘
```
형식: Icon_Stat_{StatType}.png
예시:
- Icon_Stat_Damage.png       → 공격력 (StatType: Damage)
- Icon_Stat_MaxHp.png         → 최대 체력 (StatType: MaxHp)
- Icon_Stat_Defense.png       → 방어력 (StatType: Defense)
- Icon_Stat_MoveSpeed.png     → 이동 속도 (StatType: MoveSpeed)
- Icon_Stat_Area.png          → 범위 (StatType: Area)
- Icon_Stat_Cooldown.png      → 쿨타임 (StatType: Cooldown)
- Icon_Stat_Amount.png        → 투사체 개수 (StatType: Amount)
- Icon_Stat_Pierce.png        → 관통력 (StatType: Pierce)
- Icon_Stat_ExpMultiplier.png → 경험치 획득 (StatType: ExpMultiplier)
- Icon_Stat_PickupRange.png   → 아이템 획득 범위 (StatType: PickupRange)
- Icon_Stat_Luck.png          → 행운 (StatType: Luck)
```

### 기본 아이콘
```
형식: Icon_Default.png
용도: 아이콘 로드 실패 시 대체 이미지
```

## 🎨 아이콘 사양

### 이미지 규격
```
해상도: 256x256 pixels
포맷: PNG (알파 채널 포함)
색상: True Color (32-bit RGBA)
배경: 투명 (Alpha)
용량: 100KB 이하 권장
```

### 디자인 가이드
```
아이콘 스타일:
- 심플하고 명확한 실루엣
- 80x80 픽셀로 축소되어도 알아볼 수 있는 디자인
- 배경 투명, 필요 시 테두리 추가

색상 팔레트:
- 무기 아이콘: 각 무기의 특징적인 색상 사용
  - Fireball: 주황색/붉은색
  - Scythe: 은색/회색
- 스탯 아이콘: 스탯 특성을 나타내는 색상
  - 공격력: 빨간색
  - 체력: 초록색
  - 방어력: 파란색
  - 속도: 노란색
```

## ⚙️ Unity Import 설정

### Texture Import Settings

각 아이콘 스프라이트의 Import Settings:

```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Packing Tag: UndeadSurvivorIcons (선택 사항)

Advanced:
├─ sRGB (Color Texture): ✓
├─ Alpha Source: Input Texture Alpha
├─ Alpha Is Transparency: ✓
└─ Mip Maps: ✗ (UI는 불필요)

Default:
├─ Max Size: 256
├─ Compression: None (또는 High Quality)
└─ Format: RGBA 32 bit
```

### Sprite 설정 스크립트

대량의 아이콘을 일괄 설정하려면 아래 에디터 스크립트 사용:

```csharp
// Assets/Editor/IconImportSettings.cs
using UnityEngine;
using UnityEditor;

public class IconImportSettings : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        // UndeadSurvivor 아이콘만 처리
        if (!assetPath.Contains("UndeadSurvivor") || !assetPath.Contains("Icon_"))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 256;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
```

## 📦 Addressables 설정

### Address 명명 규칙

**중요**: LevelUpOptionElement.cs에서 아래 경로로 로드하므로 반드시 일치해야 합니다.

```
무기 아이콘:
Address: Sprite/UndeadSurvivor/Icon_Weapon_{WeaponID}
예시: Sprite/UndeadSurvivor/Icon_Weapon_1

스탯 아이콘:
Address: Sprite/UndeadSurvivor/Icon_Stat_{StatType}
예시: Sprite/UndeadSurvivor/Icon_Stat_Damage

기본 아이콘:
Address: Sprite/UndeadSurvivor/Icon_Default
```

### Addressables 그룹 설정

1. **그룹 생성** (선택 사항):
   - 그룹 이름: `UndeadSurvivor_Icons`
   - Build Path: `ServerData/[BuildTarget]`
   - Load Path: `{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]`

2. **아이콘 추가**:
   - Project 창에서 아이콘 스프라이트 선택 (다중 선택 가능)
   - Inspector 창에서 `Addressable` 체크박스 활성화
   - Address 이름을 위의 규칙에 맞게 설정

3. **일괄 Address 설정 스크립트**:

```csharp
// Assets/Editor/SetIconAddresses.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class SetIconAddresses
{
    [MenuItem("Tools/Set Icon Addresses")]
    public static void SetAddresses()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables settings not found");
            return;
        }

        string iconPath = "Assets/Resources/Sprites/UndeadSurvivor";
        string[] guids = AssetDatabase.FindAssets("Icon_", new[] { iconPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            // Address 생성: Sprite/UndeadSurvivor/{fileName}
            string address = $"Sprite/UndeadSurvivor/{fileName}";

            // Addressables에 추가
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = address;

            Debug.Log($"Set address: {address} for {assetPath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Addressable addresses set for {guids.Length} icons");
    }
}
```

## 🔍 코드 참조

### LevelUpOptionElement.cs 아이콘 로드 로직

```csharp
private string GetIconPath(LevelUpOption option)
{
    switch (option.OptionType)
    {
        case LevelUpOptionType.NewWeapon:
            return $"Sprite/UndeadSurvivor/Icon_Weapon_{option.WeaponData.WeaponId}";

        case LevelUpOptionType.WeaponUpgrade:
            return $"Sprite/UndeadSurvivor/Icon_Weapon_{option.WeaponId}";

        case LevelUpOptionType.StatUpgrade:
            return $"Sprite/UndeadSurvivor/Icon_Stat_{option.StatType}";

        default:
            return "Sprite/UndeadSurvivor/Icon_Default";
    }
}
```

### ResourceManager 사용

```csharp
ResourceManager.Instance.LoadAsync<Sprite>(iconPath, (sprite) =>
{
    if (sprite != null && _icon != null)
    {
        _icon.style.backgroundImage = new StyleBackground(sprite);
    }
    else
    {
        // 기본 아이콘 로드
        LoadDefaultIcon();
    }
});
```

## ✅ 체크리스트

### 아이콘 추가 시
- [ ] 256x256 PNG 파일 생성 (알파 채널 포함)
- [ ] 명명 규칙에 맞게 파일명 설정
- [ ] `Assets/Resources/Sprites/UndeadSurvivor/` 폴더에 배치
- [ ] Import Settings 확인 (Sprite, 256px, RGBA 32bit)
- [ ] Addressables에 추가
- [ ] Address 이름 확인 (`Sprite/UndeadSurvivor/Icon_...`)
- [ ] Unity Play 모드에서 아이콘 로드 테스트

### 문제 해결
- [ ] Console 창에서 로드 실패 로그 확인
- [ ] Address 이름이 코드의 경로와 정확히 일치하는지 확인
- [ ] Addressables Groups 창에서 아이콘이 빌드에 포함되는지 확인
- [ ] 기본 아이콘(Icon_Default.png)이 있는지 확인

## 🎯 현재 상태

### 구현된 무기 (WeaponData.json 기준)
- ✅ Weapon ID 1: Fireball (화염구)
- ✅ Weapon ID 2: Scythe (낫)
- ⏳ Weapon ID 3-6: 향후 추가 예정

### 필요한 아이콘
**무기**:
- Icon_Weapon_1.png (Fireball)
- Icon_Weapon_2.png (Scythe)

**스탯** (11종):
- Icon_Stat_Damage.png
- Icon_Stat_MaxHp.png
- Icon_Stat_Defense.png
- Icon_Stat_MoveSpeed.png
- Icon_Stat_Area.png
- Icon_Stat_Cooldown.png
- Icon_Stat_Amount.png
- Icon_Stat_Pierce.png
- Icon_Stat_ExpMultiplier.png
- Icon_Stat_PickupRange.png
- Icon_Stat_Luck.png

**기본**:
- Icon_Default.png

**총 14개 아이콘 필요**

## 📚 참고 문서

- **LevelUpUI 설정 가이드**: `Assets/Docs/UndeadSurvivor_LevelUpUI_Setup_Guide.md`
- **LevelUpUI 아키텍처**: (이전 대화 내용 참조)
- **Addressables 공식 문서**: https://docs.unity3d.com/Packages/com.unity.addressables@latest
- **Sprite Import 공식 문서**: https://docs.unity3d.com/Manual/class-TextureImporter.html
