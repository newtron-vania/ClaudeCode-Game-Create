using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 무기 시스템 통합 테스트 스크립트
    /// Unity 에디터에서 GameObject에 추가하여 Play 모드에서 실행
    /// </summary>
    public class TestWeaponSystem : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _testOnStart = true;
        [SerializeField] private Player _testPlayer; // 테스트용 플레이어 (Inspector에서 할당)

        [Header("Weapon IDs to Test")]
        [SerializeField] private int _fireballWeaponId = 1; // Fireball
        [SerializeField] private int _scytheWeaponId = 2;   // Scythe

        private UndeadSurvivorDataProvider _dataProvider;

        private void Start()
        {
            if (_testOnStart)
            {
                TestWeaponSystemIntegration();
            }
        }

        /// <summary>
        /// 무기 시스템 통합 테스트
        /// </summary>
        [ContextMenu("Test Weapon System")]
        public void TestWeaponSystemIntegration()
        {
            Debug.Log("[TEST] ========== Weapon System Integration Test Start ==========");

            // 1. DataProvider 초기화
            if (!InitializeDataProvider())
            {
                Debug.LogError("[TEST] Failed to initialize DataProvider");
                return;
            }

            // 2. Player 검증
            if (!ValidatePlayer())
            {
                Debug.LogError("[TEST] Player validation failed");
                return;
            }

            // 3. WeaponData 로드 테스트
            TestWeaponDataLoad();

            // 4. 무기 추가 테스트
            TestAddWeapons();

            // 5. 무기 레벨업 테스트
            TestWeaponLevelUp();

            Debug.Log("[TEST] ========== Weapon System Integration Test End ==========");
        }

        /// <summary>
        /// DataProvider 초기화
        /// </summary>
        private bool InitializeDataProvider()
        {
            Debug.Log("[TEST] --- DataProvider Initialization ---");

            _dataProvider = new UndeadSurvivorDataProvider();
            _dataProvider.Initialize();
            _dataProvider.LoadData();

            if (!_dataProvider.IsLoaded)
            {
                Debug.LogError("[TEST] DataProvider load failed");
                return false;
            }

            Debug.Log($"[TEST] DataProvider loaded successfully");
            return true;
        }

        /// <summary>
        /// Player 검증
        /// </summary>
        private bool ValidatePlayer()
        {
            Debug.Log("[TEST] --- Player Validation ---");

            if (_testPlayer == null)
            {
                Debug.LogError("[TEST] _testPlayer is null. Please assign a Player in Inspector.");
                return false;
            }

            if (!_testPlayer.IsAlive)
            {
                Debug.LogWarning("[TEST] Player is not alive");
            }

            Debug.Log($"[TEST] Player validated: {_testPlayer.name}");
            Debug.Log($"    Level: {_testPlayer.Level}");
            Debug.Log($"    HP: {_testPlayer.CurrentHp}/{_testPlayer.MaxHp}");
            Debug.Log($"    Weapon Count: {_testPlayer.CurrentWeaponCount}/{_testPlayer.MaxWeaponSlots}");

            return true;
        }

        /// <summary>
        /// WeaponData 로드 테스트
        /// </summary>
        private void TestWeaponDataLoad()
        {
            Debug.Log("[TEST] --- WeaponData Load Test ---");

            // Fireball 데이터
            WeaponData fireballData = _dataProvider.GetWeaponData(_fireballWeaponId);
            if (fireballData != null)
            {
                Debug.Log($"[TEST] Fireball WeaponData loaded:");
                Debug.Log($"    ID: {fireballData.Id}, Name: {fireballData.Name}, Type: {fireballData.Type}");
                Debug.Log($"    Levels: {fireballData.LevelStats.Length}");

                for (int i = 0; i < fireballData.LevelStats.Length; i++)
                {
                    WeaponLevelStat stat = fireballData.LevelStats[i];
                    Debug.Log($"    Lv.{i + 1}: Damage={stat.Damage}, Cooldown={stat.Cooldown}s, Count={stat.CountPerCreate}, Penetrate={stat.Penetrate}");
                }
            }
            else
            {
                Debug.LogError($"[TEST] Failed to load Fireball WeaponData (ID {_fireballWeaponId})");
            }

            // Scythe 데이터
            WeaponData scytheData = _dataProvider.GetWeaponData(_scytheWeaponId);
            if (scytheData != null)
            {
                Debug.Log($"[TEST] Scythe WeaponData loaded:");
                Debug.Log($"    ID: {scytheData.Id}, Name: {scytheData.Name}, Type: {scytheData.Type}");
                Debug.Log($"    Levels: {scytheData.LevelStats.Length}");

                for (int i = 0; i < scytheData.LevelStats.Length; i++)
                {
                    WeaponLevelStat stat = scytheData.LevelStats[i];
                    Debug.Log($"    Lv.{i + 1}: Damage={stat.Damage}, Count={stat.CountPerCreate}, Speed={stat.Speed}");
                }
            }
            else
            {
                Debug.LogError($"[TEST] Failed to load Scythe WeaponData (ID {_scytheWeaponId})");
            }
        }

        /// <summary>
        /// 무기 추가 테스트
        /// </summary>
        private void TestAddWeapons()
        {
            Debug.Log("[TEST] --- Add Weapons Test ---");

            // Fireball 추가
            WeaponData fireballData = _dataProvider.GetWeaponData(_fireballWeaponId);
            if (fireballData != null)
            {
                bool added = _testPlayer.AddWeapon(fireballData);
                if (added)
                {
                    Debug.Log($"[TEST] ✅ Fireball added successfully");
                    Debug.Log($"    Current weapon count: {_testPlayer.CurrentWeaponCount}/{_testPlayer.MaxWeaponSlots}");
                }
                else
                {
                    Debug.LogWarning($"[TEST] ❌ Failed to add Fireball (already equipped or slots full)");
                }
            }

            // Scythe 추가
            WeaponData scytheData = _dataProvider.GetWeaponData(_scytheWeaponId);
            if (scytheData != null)
            {
                bool added = _testPlayer.AddWeapon(scytheData);
                if (added)
                {
                    Debug.Log($"[TEST] ✅ Scythe added successfully");
                    Debug.Log($"    Current weapon count: {_testPlayer.CurrentWeaponCount}/{_testPlayer.MaxWeaponSlots}");
                }
                else
                {
                    Debug.LogWarning($"[TEST] ❌ Failed to add Scythe (already equipped or slots full)");
                }
            }
        }

        /// <summary>
        /// 무기 레벨업 테스트
        /// </summary>
        private void TestWeaponLevelUp()
        {
            Debug.Log("[TEST] --- Weapon Level Up Test ---");

            // Fireball 레벨업
            if (_testPlayer.HasWeapon(_fireballWeaponId))
            {
                bool leveledUp = _testPlayer.LevelUpWeapon(_fireballWeaponId);
                if (leveledUp)
                {
                    Debug.Log($"[TEST] ✅ Fireball leveled up successfully");
                }
                else
                {
                    Debug.LogWarning($"[TEST] ❌ Fireball level up failed (max level or not equipped)");
                }
            }
            else
            {
                Debug.LogWarning($"[TEST] Fireball not equipped, cannot level up");
            }

            // Scythe 레벨업
            if (_testPlayer.HasWeapon(_scytheWeaponId))
            {
                bool leveledUp = _testPlayer.LevelUpWeapon(_scytheWeaponId);
                if (leveledUp)
                {
                    Debug.Log($"[TEST] ✅ Scythe leveled up successfully");
                }
                else
                {
                    Debug.LogWarning($"[TEST] ❌ Scythe level up failed (max level or not equipped)");
                }
            }
            else
            {
                Debug.LogWarning($"[TEST] Scythe not equipped, cannot level up");
            }
        }

        /// <summary>
        /// 무기 슬롯 포화 테스트 (6개 무기 추가)
        /// </summary>
        [ContextMenu("Test Weapon Slots Full")]
        public void TestWeaponSlotsFull()
        {
            Debug.Log("[TEST] ========== Weapon Slots Full Test Start ==========");

            if (!InitializeDataProvider() || !ValidatePlayer())
            {
                return;
            }

            // 무기 ID 1-6까지 추가 시도
            for (int weaponId = 1; weaponId <= 6; weaponId++)
            {
                WeaponData weaponData = _dataProvider.GetWeaponData(weaponId);
                if (weaponData != null)
                {
                    bool added = _testPlayer.AddWeapon(weaponData);
                    Debug.Log($"[TEST] Weapon {weaponData.Name} (ID {weaponId}): {(added ? "Added" : "Failed")}");
                }
                else
                {
                    Debug.LogWarning($"[TEST] WeaponData ID {weaponId} not found");
                }
            }

            Debug.Log($"[TEST] Final weapon count: {_testPlayer.CurrentWeaponCount}/{_testPlayer.MaxWeaponSlots}");
            Debug.Log($"[TEST] Slots full: {_testPlayer.IsWeaponSlotsFull}");

            Debug.Log("[TEST] ========== Weapon Slots Full Test End ==========");
        }

        private void OnDestroy()
        {
            // DataProvider 정리
            if (_dataProvider != null && _dataProvider.IsLoaded)
            {
                _dataProvider.UnloadData();
            }
        }
    }
}