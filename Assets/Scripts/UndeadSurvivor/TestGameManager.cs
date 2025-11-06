using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 테스트용 게임 매니저
    /// Unity 에디터에서 플레이어 시스템을 테스트하기 위한 초기화 및 테스트 메서드 제공
    /// </summary>
    public class TestGameManager : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private int _testCharacterId = 1; // 1: Knight, 2: Mage
        [SerializeField] private bool _autoResumeAfterLevelUp = true;
        [SerializeField] private float _autoResumeDelay = 3f;

        private Player _player;
        private UndeadSurvivorDataProvider _dataProvider;

        private void Start()
        {
            InitializeDataManager();
            InitializePlayer();
        }

        /// <summary>
        /// DataManager 초기화 및 데이터 로드
        /// </summary>
        private void InitializeDataManager()
        {
            // UndeadSurvivorDataProvider 생성 및 등록
            _dataProvider = new UndeadSurvivorDataProvider();
            _dataProvider.Initialize();
            DataManager.Instance.RegisterProvider(_dataProvider);

            // 게임 데이터 로드
            DataManager.Instance.LoadGameData("UndeadSurvivor");

            Debug.Log("[INFO] TestGameManager::InitializeDataManager - DataManager initialized and data loaded");
        }

        /// <summary>
        /// Player 찾기 및 CharacterData 기반 초기화
        /// </summary>
        private void InitializePlayer()
        {
            _player = FindObjectOfType<Player>();

            if (_player == null)
            {
                Debug.LogError("[ERROR] TestGameManager::InitializePlayer - Player not found in scene!");
                return;
            }

            // CharacterData 로드
            var characterData = _dataProvider.GetCharacterData(_testCharacterId);

            if (characterData == null)
            {
                Debug.LogError($"[ERROR] TestGameManager::InitializePlayer - Character ID {_testCharacterId} not found");
                return;
            }

            // Player 초기화
            _player.Initialize(characterData);

            // 레벨업 이벤트 구독 (자동 재개용)
            if (_autoResumeAfterLevelUp)
            {
                _player.OnPlayerLevelUp += HandlePlayerLevelUp;
            }

            Debug.Log($"[INFO] TestGameManager::InitializePlayer - Player initialized with {characterData.Name}");
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (_player != null && _autoResumeAfterLevelUp)
            {
                _player.OnPlayerLevelUp -= HandlePlayerLevelUp;
            }

            // 게임 데이터 언로드
            if (DataManager.Instance != null)
            {
                DataManager.Instance.UnloadGameData("UndeadSurvivor");
            }
        }

        #region Event Handlers

        /// <summary>
        /// 플레이어 레벨업 이벤트 핸들러
        /// </summary>
        private void HandlePlayerLevelUp(int newLevel)
        {
            Debug.Log($"[INFO] TestGameManager::HandlePlayerLevelUp - Player leveled up to {newLevel}");

            if (_autoResumeAfterLevelUp)
            {
                Invoke(nameof(ResumePlayerMovement), _autoResumeDelay);
            }
        }

        /// <summary>
        /// 플레이어 이동 재개 (레벨업 후)
        /// </summary>
        private void ResumePlayerMovement()
        {
            if (_player != null && _player.IsAlive)
            {
                _player.ResumeMovement();
                Debug.Log("[INFO] TestGameManager::ResumePlayerMovement - Player movement resumed");
            }
        }

        #endregion

        #region Test Methods (Context Menu)

        /// <summary>
        /// 플레이어에게 10 피해
        /// </summary>
        [ContextMenu("Test: Damage Player 10")]
        private void TestDamage10()
        {
            if (_player != null)
            {
                _player.TakeDamage(10f);
                Debug.Log($"[TEST] Dealt 10 damage to player. Current HP: {_player.CurrentHp}/{_player.MaxHp}");
            }
        }

        /// <summary>
        /// 플레이어에게 50 피해
        /// </summary>
        [ContextMenu("Test: Damage Player 50")]
        private void TestDamage50()
        {
            if (_player != null)
            {
                _player.TakeDamage(50f);
                Debug.Log($"[TEST] Dealt 50 damage to player. Current HP: {_player.CurrentHp}/{_player.MaxHp}");
            }
        }

        /// <summary>
        /// 플레이어 20 체력 회복
        /// </summary>
        [ContextMenu("Test: Heal Player 20")]
        private void TestHeal20()
        {
            if (_player != null)
            {
                _player.Heal(20f);
                Debug.Log($"[TEST] Healed 20 HP. Current HP: {_player.CurrentHp}/{_player.MaxHp}");
            }
        }

        /// <summary>
        /// 플레이어 완전 회복
        /// </summary>
        [ContextMenu("Test: Heal Player Full")]
        private void TestHealFull()
        {
            if (_player != null)
            {
                _player.Heal(_player.MaxHp);
                Debug.Log($"[TEST] Fully healed. Current HP: {_player.CurrentHp}/{_player.MaxHp}");
            }
        }

        /// <summary>
        /// 경험치 50 획득
        /// </summary>
        [ContextMenu("Test: Gain 50 Exp")]
        private void TestGainExp50()
        {
            if (_player != null)
            {
                _player.GainExp(50);
                Debug.Log($"[TEST] Gained 50 exp. Current Level: {_player.Level}, Exp: {_player.CurrentExp}");
            }
        }

        /// <summary>
        /// 경험치 100 획득 (레벨업)
        /// </summary>
        [ContextMenu("Test: Gain 100 Exp")]
        private void TestGainExp100()
        {
            if (_player != null)
            {
                _player.GainExp(100);
                Debug.Log($"[TEST] Gained 100 exp. Current Level: {_player.Level}, Exp: {_player.CurrentExp}");
            }
        }

        /// <summary>
        /// 경험치 1000 획득 (다중 레벨업)
        /// </summary>
        [ContextMenu("Test: Gain 1000 Exp (Multi Level Up)")]
        private void TestGainExp1000()
        {
            if (_player != null)
            {
                _player.GainExp(1000);
                Debug.Log($"[TEST] Gained 1000 exp. Current Level: {_player.Level}, Exp: {_player.CurrentExp}");
            }
        }

        /// <summary>
        /// MaxHp 스탯 업그레이드 (+20)
        /// </summary>
        [ContextMenu("Test: Upgrade MaxHp +20")]
        private void TestUpgradeMaxHp()
        {
            if (_player != null)
            {
                _player.ApplyStatUpgrade(StatType.MaxHp, 20f);
                Debug.Log($"[TEST] Upgraded MaxHp +20. Current MaxHp: {_player.MaxHp}");
            }
        }

        /// <summary>
        /// MoveSpeed 스탯 업그레이드 (+0.5)
        /// </summary>
        [ContextMenu("Test: Upgrade MoveSpeed +0.5")]
        private void TestUpgradeMoveSpeed()
        {
            if (_player != null)
            {
                _player.ApplyStatUpgrade(StatType.MoveSpeed, 0.5f);
                Debug.Log($"[TEST] Upgraded MoveSpeed +0.5. Current MoveSpeed: {_player.GetStat(StatType.MoveSpeed)}");
            }
        }

        /// <summary>
        /// Damage 스탯 업그레이드 (+10%)
        /// </summary>
        [ContextMenu("Test: Upgrade Damage +10%")]
        private void TestUpgradeDamage()
        {
            if (_player != null)
            {
                _player.ApplyStatUpgrade(StatType.Damage, 10f);
                Debug.Log($"[TEST] Upgraded Damage +10%. Current Damage: {_player.GetStat(StatType.Damage)}%");
            }
        }

        /// <summary>
        /// 플레이어 상태 로그 출력
        /// </summary>
        [ContextMenu("Test: Print Player Status")]
        private void TestPrintPlayerStatus()
        {
            if (_player != null && _player.CharacterStat != null)
            {
                Debug.Log($"[TEST] Player Status:\n{_player.CharacterStat.ToString()}");
                Debug.Log($"[TEST] Level: {_player.Level}, IsAlive: {_player.IsAlive}");
            }
        }

        #endregion
    }
}
