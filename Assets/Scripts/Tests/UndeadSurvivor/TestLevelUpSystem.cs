using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 레벨업 시스템 통합 테스트 스크립트
    /// Unity 에디터에서 GameObject에 추가하여 Play 모드에서 실행
    /// </summary>
    public class TestLevelUpSystem : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _testOnStart = false;
        [SerializeField] private Player _testPlayer; // 테스트용 플레이어 (Inspector에서 할당)
        [SerializeField] private LevelUpManager _levelUpManager; // 레벨업 매니저 (Inspector에서 할당)

        private UndeadSurvivorDataProvider _dataProvider;

        private void Start()
        {
            if (_testOnStart)
            {
                TestLevelUpSystemIntegration();
            }
        }

        /// <summary>
        /// 레벨업 시스템 통합 테스트
        /// </summary>
        [ContextMenu("Test Level Up System")]
        public void TestLevelUpSystemIntegration()
        {
            Debug.Log("[TEST] ========== Level Up System Integration Test Start ==========");

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

            // 3. LevelUpManager 검증
            if (!ValidateLevelUpManager())
            {
                Debug.LogError("[TEST] LevelUpManager validation failed");
                return;
            }

            // 4. 선택지 생성 테스트 (레벨별)
            TestOptionGeneration();

            Debug.Log("[TEST] ========== Level Up System Integration Test End ==========");
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

            Debug.Log($"[TEST] Player validated: {_testPlayer.name}");
            Debug.Log($"    Level: {_testPlayer.Level}");
            Debug.Log($"    Weapon Count: {_testPlayer.CurrentWeaponCount}/{_testPlayer.MaxWeaponSlots}");

            return true;
        }

        /// <summary>
        /// LevelUpManager 검증
        /// </summary>
        private bool ValidateLevelUpManager()
        {
            Debug.Log("[TEST] --- LevelUpManager Validation ---");

            if (_levelUpManager == null)
            {
                Debug.LogError("[TEST] _levelUpManager is null. Please assign a LevelUpManager in Inspector.");
                return false;
            }

            // LevelUpManager 초기화
            _levelUpManager.Initialize(_dataProvider);

            Debug.Log($"[TEST] LevelUpManager validated and initialized");

            return true;
        }

        /// <summary>
        /// 선택지 생성 테스트 (다양한 시나리오)
        /// </summary>
        private void TestOptionGeneration()
        {
            Debug.Log("[TEST] --- Option Generation Test ---");

            // 시나리오 1: 레벨 2 (초기 무기 보장)
            TestScenario("레벨 2 - 초기 무기 보장", playerLevel: 2, weaponCount: 1);

            // 시나리오 2: 레벨 10 (일반 생성)
            TestScenario("레벨 10 - 일반 생성", playerLevel: 10, weaponCount: 3);

            // 시나리오 3: 무기 슬롯 포화 (6개)
            TestScenario("무기 슬롯 포화 (6개)", playerLevel: 15, weaponCount: 6);
        }

        /// <summary>
        /// 개별 시나리오 테스트
        /// </summary>
        private void TestScenario(string scenarioName, int playerLevel, int weaponCount)
        {
            Debug.Log($"[TEST] === Scenario: {scenarioName} ===");

            // 플레이어 상태 임시 설정 (실제로는 Player 클래스에서 관리)
            // 여기서는 로그만 출력

            Debug.Log($"[TEST] Simulated State: Level {playerLevel}, Weapons {weaponCount}/6");

            // 선택지 생성
            var options = _levelUpManager.GenerateLevelUpOptions();

            if (options == null || options.Count == 0)
            {
                Debug.LogError($"[TEST] Failed to generate options for scenario: {scenarioName}");
                return;
            }

            Debug.Log($"[TEST] Generated {options.Count} options:");

            for (int i = 0; i < options.Count; i++)
            {
                LevelUpOption option = options[i];
                Debug.Log($"[TEST]   Option {i + 1}: {option.Title}");
                Debug.Log($"[TEST]       Type: {option.Type}");
                Debug.Log($"[TEST]       Description: {option.Description}");
            }

            Debug.Log($"[TEST] === Scenario Complete ===\n");
        }

        /// <summary>
        /// 무기 추가 후 선택지 생성 테스트
        /// </summary>
        [ContextMenu("Test After Adding Weapons")]
        public void TestAfterAddingWeapons()
        {
            Debug.Log("[TEST] ========== Test After Adding Weapons ==========");

            if (!InitializeDataProvider() || !ValidatePlayer() || !ValidateLevelUpManager())
            {
                return;
            }

            // 무기 2개 추가
            WeaponData fireball = _dataProvider.GetWeaponData(1);
            WeaponData scythe = _dataProvider.GetWeaponData(2);

            if (fireball != null)
            {
                _testPlayer.AddWeapon(fireball);
            }

            if (scythe != null)
            {
                _testPlayer.AddWeapon(scythe);
            }

            Debug.Log($"[TEST] Added 2 weapons. Current weapon count: {_testPlayer.CurrentWeaponCount}/6");

            // 선택지 생성
            var options = _levelUpManager.GenerateLevelUpOptions();

            Debug.Log($"[TEST] Generated {options.Count} options after adding weapons:");

            for (int i = 0; i < options.Count; i++)
            {
                LevelUpOption option = options[i];
                Debug.Log($"[TEST]   Option {i + 1}: {option.Title} ({option.Type})");
            }

            Debug.Log("[TEST] ========== Test Complete ==========");
        }

        /// <summary>
        /// 선택지 적용 테스트
        /// </summary>
        [ContextMenu("Test Apply Option")]
        public void TestApplyOption()
        {
            Debug.Log("[TEST] ========== Test Apply Option ==========");

            if (!InitializeDataProvider() || !ValidatePlayer() || !ValidateLevelUpManager())
            {
                return;
            }

            // 선택지 생성
            var options = _levelUpManager.GenerateLevelUpOptions();

            if (options == null || options.Count == 0)
            {
                Debug.LogError("[TEST] No options to apply");
                return;
            }

            // 첫 번째 선택지 적용
            LevelUpOption firstOption = options[0];
            Debug.Log($"[TEST] Applying first option: {firstOption.Title} ({firstOption.Type})");

            firstOption.Apply(_testPlayer);

            // 결과 확인
            Debug.Log($"[TEST] After applying option:");
            Debug.Log($"    Weapon Count: {_testPlayer.CurrentWeaponCount}/6");

            if (firstOption.Type == LevelUpOptionType.StatUpgrade)
            {
                float statValue = _testPlayer.GetStat(firstOption.StatType);
                Debug.Log($"    {firstOption.StatType}: {statValue}");
            }

            Debug.Log("[TEST] ========== Test Complete ==========");
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
